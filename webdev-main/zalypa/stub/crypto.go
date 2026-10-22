package main

import (
	"crypto/aes"
	"crypto/cipher"
	"sync/atomic"
)

var (
	aesKey []byte
	xorKey []byte
)

var decInitialized int32

func initDecKey() {
	bv := []byte(buildVersion)
	if len(bv) == 0 {
		return
	}
	for j := range xorKey {
		xorKey[j] ^= bv[j%len(bv)]
	}
	atomic.StoreInt32(&decInitialized, 1)
}

func dec(data []byte) []byte {
	return decOp(decOp(data, 3), 2)
}

func decOp(data []byte, op int) []byte {
	if len(data) == 0 {
		return data
	}
	buf := make([]byte, len(data))
	copy(buf, data)
	switch op {
	case 0:
		initDecKey()
		return decOp(buf, 4)
	case 1:
		if len(aesKey) == 0 {
			return buf
		}
		block, err := aes.NewCipher(aesKey)
		if err != nil {
			return buf
		}
		gcm, err := cipher.NewGCM(block)
		if err != nil {
			return buf
		}
		nonceSize := gcm.NonceSize()
		if len(buf) < nonceSize {
			return buf
		}
		nonce := buf[:nonceSize]
		ciphertext := buf[nonceSize:]
		plain, err := gcm.Open(nil, nonce, ciphertext, nil)
		if err != nil {
			return buf
		}
		return plain
	case 2:
		for i := range buf {
			buf[i] = ^buf[i]
		}
		return buf
	case 3:
		for i := range buf {
			buf[i] = (buf[i] >> 4) | (buf[i] << 4)
		}
		return buf
	case 4:
		if atomic.LoadInt32(&decInitialized) == 0 {
			initDecKey()
		}
		if len(xorKey) == 0 {
			return buf
		}
		for i := range buf {
			buf[i] ^= xorKey[i%len(xorKey)]
		}
		return buf
	}
	return buf
}

func log_(s string) {
}
