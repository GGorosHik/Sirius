// DM (private messages) view — sits in the community page header as a tab.
// Left column: thread list (loaded from /api/dm/threads + offset, 20 per page)
// Right column: messages of the selected thread (loaded from /api/dm/messages + thread, offset, 50 per page)
// Sending: POST /api/dm/send with { target_user, text }
// Deleting own messages: POST /api/dm/delete with { message: id }
// Default per spec: chat with user id 2.

window.dmState = {
    threads: [],
    threadOffset: 0,
    threadHasMore: true,
    threadLoading: false,
    threadsLoaded: false,

    messages: [],
    messageOffset: 0,
    messageHasMore: true,
    messageLoading: false,
    // Таймстамп самого свежего сообщения, которое мы уже видели.
    // Используется как `time` в /api/dm/messages — чтобы poll тянул
    // только дельту, а не всю страницу (как в чате). Для history-подгрузки
    // (offset > 0) передаём 0, чтобы сервер не фильтровал по времени.
    lastMessageTime: 0,

    currentTarget: null,        // companion_id (user_id of the other person)
    currentThreadId: null,      // thread id from the /api/dm/threads list — what /api/dm/threads + thread expects
    currentTargetName: null,
    currentTargetAvatar: null,

    sending: false,
    initialScroll: true,
    pollTimer: null,
    // Периодический рефреш левого списка тредов (раз в 5 секунд), чтобы
    // новые чаты и изменения `time_up` подхватывались без переоткрытия
    // вкладки. Жизненный цикл привязан к openDmView / closeDmView.
    threadsPollTimer: null,
    open: false,

    // When openDmWithUser is called before the thread list is loaded,
    // the desired companion is stashed here so autoSelectDefaultDmThread
    // can pick it up on the next loadDmThreads success.
    pendingTarget: null
};

const DM_DEFAULT_USER_ID = 2;
const DM_THREADS_PAGE = 20;
const DM_MESSAGES_PAGE = 50;
const DM_FALLBACK_AVATAR = 'https://i.ibb.co/q3GCfzPm/besavatarochniy.png';

function dmAuthHeaders() {
    return {
        'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
    };
}

// Open a DM chat with the given user — used by the "Message" button in
// the mini-profile popup. Closes the profile popup, switches to the DM
// tab, and selects (or pre-selects) the user. If the thread list is not
// yet loaded, we stash the user in pendingTarget and the auto-select will
// honor it once the response arrives.
function openDmWithUser(userId, name, avatar) {
    if (userId == null) return;
    const target = {
        id: Number(userId),
        name: name || '',
        avatar: avatar || ''
    };

    // Close the profile popup (and any other open modals / backdrop)
    try { $('.mainpopup, .backblur').hide(); } catch (e) { }

    // Open DM view first — that handles tab switching and loading the
    // thread list if needed.
    openDmView();

    if (window.dmState.threadsLoaded) {
        // List is already in memory — select the user directly. If their
        // thread isn't in the list yet, selectDmThread still sets state
        // (currentThreadId will be null until the first /api/dm/send).
        const thread = (window.dmState.threads || [])
            .find(t => Number(t.companion_id) === target.id);
        if (thread) {
            selectDmThread(
                target.id,
                thread.name || target.name,
                thread.avatar || target.avatar
            );
        } else {
            selectDmThread(target.id, target.name, target.avatar);
        }
    } else {
        // Thread list is being loaded — stash the target so
        // autoSelectDefaultDmThread picks it once the response arrives.
        window.dmState.pendingTarget = target;
    }
}

function openDmView() {
    if (window.dmState.open) {
        return;
    }
    window.dmState.open = true;
    window.isDmView = true;

    // Switch active state in the header
    $('.headselectorbutton').removeClass('headselectorbuttonactive');
    $('#dmtab').addClass('headselectorbuttonactive');

    // Hide the regular chat and any open thread header
    $('#comchat').hide();
    $('#headerthem').hide();

    // Show DM view
    $('#dmview').css('display', 'flex');

    // Per request: don't auto-select a chat on entering the DM tab.
    // Reset the right-pane state and show the "Select a chat" placeholder.
    // The user picks a thread from the list (or "Message" on a profile) to open one.
    window.dmState.currentTarget = null;
    window.dmState.currentThreadId = null;
    window.dmState.currentTargetName = null;
    window.dmState.currentTargetAvatar = null;
    window.dmState.messages = [];
    window.dmState.messageOffset = 0;
    window.dmState.messageHasMore = true;
    // Закрываем вкладку DM — сбрасываем маркер «что мы уже видели», чтобы
    // при следующем заходе первый poll уехал с time=0 и дотянул всю
    // актуальную историю, а не считал, что всё уже подгружено.
    window.dmState.lastMessageTime = 0;
    window.dmState.initialScroll = true;
    $('#dmeverychat').empty().hide();
    $('#dmthreadheader').hide();
    $('#dminputcontainer').hide();
    $('#dmnochat').show();
    // Re-render the left list to clear the previous active state
    renderDmThreads();

    // Load the thread list for the left sidebar. Don't auto-select — pass
    // false so the default user (2) doesn't get picked on entry. The
    // openDmWithUser flow still works via the pendingTarget mechanism in
    // autoSelectDefaultDmThread.
    if (!window.dmState.threadsLoaded) {
        loadDmThreads(false);
    }

    // Start polling for new messages in the active thread. The polling
    // itself is a no-op until the user picks a chat (currentTarget is null).
    if (window.dmState.pollTimer) {
        clearInterval(window.dmState.pollTimer);
    }
    window.dmState.pollTimer = setInterval(() => {
        if (window.dmState.open && window.dmState.currentTarget && window.dmState.messageOffset === 0) {
            loadDmMessages();
        }
    }, 5000);

    // Авто-рефреш списка тредов (левая колонка) раз в 5 секунд — чтобы
    // новые чаты и изменения `time_up` подхватывались без переоткрытия
    // вкладки. Пропускаем тик, если юзер ушёл в пагинацию (проскроллил
    // список вниз) — замена списка выдернула бы его обратно наверх.
    if (window.dmState.threadsPollTimer) {
        clearInterval(window.dmState.threadsPollTimer);
    }
    window.dmState.threadsPollTimer = setInterval(() => {
        if (!window.dmState.open) return;
        if (window.dmState.threadLoading) return;
        const list = document.getElementById('dmthreadlist');
        if (list && list.scrollHeight > list.clientHeight && list.scrollTop > 50) {
            return;
        }
        window.dmState.threadOffset = 0;
        loadDmThreads(false);
    }, 5000);
}

function closeDmView() {
    if (!window.dmState.open) {
        return;
    }
    window.dmState.open = false;
    window.isDmView = false;
    closeDmProfile();
    $('#dmview').hide();
    $('#comchat').show();
    if (window.dmState.pollTimer) {
        clearInterval(window.dmState.pollTimer);
        window.dmState.pollTimer = null;
    }
    if (window.dmState.threadsPollTimer) {
        clearInterval(window.dmState.threadsPollTimer);
        window.dmState.threadsPollTimer = null;
    }
    // Абортим висящие запросы, чтобы их success не прилетел уже после
    // выхода и не переписал dmState.messages / не дёрнул selectDmThread.
    // На случай гонки (ответ пришёл до того, как abort дошёл) success
    // в loadDmMessages / loadDmThreads всё равно гейтчит по dmState.open.
    try { if (window.dmState.ajaxLoadMessages) window.dmState.ajaxLoadMessages.abort(); } catch (e) { }
    try { if (window.dmState.ajaxLoadThreads) window.dmState.ajaxLoadThreads.abort(); } catch (e) { }
    window.dmState.ajaxLoadMessages = null;
    window.dmState.ajaxLoadThreads = null;
}

// ── DM cookie (mirrors how community.js persists `lastThread`) ────────────
function saveDmCookie(userId) {
    if (userId != null && Number.isFinite(Number(userId))) {
        try { createCookie('lastDmUserId', String(userId), 365); } catch (e) { }
    }
}

function clearDmCookie() {
    try { createCookie('lastDmUserId', '', -1); } catch (e) { }
}

// Sort DM threads by `time_up` DESC — most recently active chat first.
// `time_up` приходит строкой вида "2026-06-16 00:00:00"; приводим к ms
// через `new Date(...)` (пробел → 'T' для ISO-совместимости). Невалидные
// или отсутствующие значения падают в 0 и уезжают в конец списка. Не
// мутирует входной массив — возвращает новую отсортированную копию.
function sortDmThreadsByTimeUp(threads) {
    const toMs = (s) => {
        if (!s) return 0;
        const t = new Date(String(s).replace(' ', 'T')).getTime();
        return isNaN(t) ? 0 : t;
    };
    return threads.slice().sort((a, b) => toMs(b && b.time_up) - toMs(a && a.time_up));
}

// Polling не должен пересоздавать одинаковый список тредов каждые 5 секунд:
// иначе браузер заново назначает src всем аватаркам и они постоянно мигают.
function dmThreadsSignature(threads) {
    return JSON.stringify((threads || []).map(t => [
        t && t.id,
        t && t.time_up,
        t && t.companion_id,
        t && t.name,
        t && t.avatar
    ]));
}

// Fetch the LEFT-SIDEBAR chat list (20 items per page).
// Endpoint: POST /api/dm/threads  with  { offset }
// Real response shape:
//   { success, result: [ { id, time_up, companion_id, name, avatar } ] }
// Optional onComplete runs after the thread list is merged/rendered and
// (if applicable) autoSelectDefaultDmThread has been called — used by
// sendDmMessage to chain a follow-up loadDmMessages once the freshly
// created thread id is known.
function loadDmThreads(withAutoSelect, onComplete) {
    if (window.dmState.threadLoading) return;
    window.dmState.threadLoading = true;
    window.dmState.ajaxLoadThreads = $.ajax({
        url: '/api/dm/threads',
        method: 'post',
        dataType: 'json',
        headers: dmAuthHeaders(),
        data: {
            offset: window.dmState.threadOffset
        },
        success: function (res) {
            window.dmState.threadLoading = false;
            window.dmState.ajaxLoadThreads = null;
            if (res && res.success) {
                const incoming = res.result || [];
                const previousThreads = window.dmState.threads || [];
                let nextThreads;
                if (window.dmState.threadOffset === 0) {
                    nextThreads = sortDmThreadsByTimeUp(incoming);
                } else {
                    // При пагинации (offset > 0) склеиваем и пересортировываем,
                    // чтобы подгруженные старые треды не ломали DESC-порядок
                    // относительно первой страницы.
                    nextThreads = sortDmThreadsByTimeUp(previousThreads.concat(incoming));
                }
                const threadsChanged = dmThreadsSignature(previousThreads) !== dmThreadsSignature(nextThreads);
                window.dmState.threads = nextThreads;
                window.dmState.threadHasMore = incoming.length >= DM_THREADS_PAGE;
                window.dmState.threadsLoaded = true;

                const threadItems = document.getElementById('dmthreaditems');
                if (threadsChanged || !threadItems || !threadItems.hasChildNodes()) {
                    renderDmThreads();
                }

                // If we already have a selected target, sync its thread id
                // from the freshly-loaded list. This covers the case where
                // the thread was just created server-side (first message to
                // a brand-new companion) — the previous load didn't have it,
                // so loadDmMessages would early-return on a null currentThreadId.
                if (window.dmState.currentTarget) {
                    const t = (window.dmState.threads || []).find(
                        th => Number(th.companion_id) === Number(window.dmState.currentTarget)
                    );
                    if (t) {
                        window.dmState.currentThreadId = Number(t.id);
                    }
                }

                // First load after opening: pick the default thread from the list.
                // Пропускаем при фоновом префетче (withAutoSelect === false) и
                // при открытии вкладки DM (openDmView передаёт false) — там
                // побочный эффект не нужен, юзер должен сам кликнуть чат.
                // pendingTarget из openDmWithUser всегда уважаем, даже если
                // авто-выбор подавлен.
                if ((withAutoSelect !== false || window.dmState.pendingTarget)
                    && !window.dmState.currentTarget) {
                    autoSelectDefaultDmThread();
                }

                if (typeof onComplete === 'function') {
                    try { onComplete(); } catch (e) { console.error('loadDmThreads onComplete', e); }
                }
            }
        },
        error: function () {
            window.dmState.threadLoading = false;
            window.dmState.ajaxLoadThreads = null;
        }
    });
}

// Render the LEFT-SIDEBAR chat list.
// Real row shape: { id, time_up, companion_id, name, avatar }
function renderDmThreads() {
    const $items = $('#dmthreaditems');
    $items.empty();
    const threads = window.dmState.threads || [];
    if (threads.length === 0) {
        $items.append('<div style="padding: 15px; color: #888; text-align: center; font-size: 13px;">No chats</div>');
        return;
    }
    threads.forEach(t => {
        const userId = Number(t.companion_id);
        const isActive = Number(window.dmState.currentTarget) === userId;
        const name = t.name || ('User ' + userId);
        const avatar = t.avatar || DM_FALLBACK_AVATAR;
        const timeStr = t.time_up || '';
        const html = `
            <div class="dmthreaditem ${isActive ? 'dmthreaditem-active' : ''}"
                 data-userid="${userId}"
                 onclick="selectDmThread(${userId}, '${escapeHtmlAttr(name)}', '${escapeHtmlAttr(avatar)}')">
                <img class="dmthreadavatar" src="${escapeHtmlAttr(avatar)}" alt=""
                     onerror="this.onerror=null; this.src='${DM_FALLBACK_AVATAR}';">
                <div class="dmthreadinfo">
                    <div class="dmthreadname"><b>${escapeHtml(name)}</b></div>
                    <div class="dmthreadtime">${escapeHtml(timeStr)}</div>
                </div>
            </div>
        `;
        $items.append(html);
    });
}

function autoSelectDefaultDmThread() {
    const threads = window.dmState.threads || [];
    const pending = window.dmState.pendingTarget;
    window.dmState.pendingTarget = null;

    // Honor a pending target (set by openDmWithUser) first — this is the
    // "open chat with user X" flow triggered from the mini-profile.
    if (pending && pending.id) {
        const t = threads.find(th => Number(th.companion_id) === Number(pending.id));
        if (t) {
            selectDmThread(
                Number(t.companion_id),
                t.name || pending.name,
                t.avatar || pending.avatar
            );
        } else {
            // No thread for this user yet — selectDmThread handles the
            // "first message will create the thread" case.
            selectDmThread(pending.id, pending.name, pending.avatar);
        }
        return;
    }

    if (threads.length === 0) {
        // No threads yet — the user can still type; the first /api/dm/send
        // will create a thread on the server. Show the empty chat UI.
        return;
    }
    // Pick the saved DM user from the cookie, fall back to the default (2)
    const saved = readCookie('lastDmUserId');
    const targetUserId = saved && /^\d+$/.test(saved) ? Number(saved) : DM_DEFAULT_USER_ID;

    // Find the thread for that user; if none, fall back to the first thread
    const thread = threads.find(t => Number(t.companion_id) === targetUserId) || threads[0];
    if (!thread) return;

    // Forward the name/avatar so the chat header and per-message fallback
    // resolve to the real user, not "User 2".
    selectDmThread(Number(thread.companion_id), thread.name || '', thread.avatar || '');
}

function selectDmThread(userId, name, avatar) {
    const newId = Number(userId);
    const isNew = Number(window.dmState.currentTarget) !== newId;
    if (isNew) {
        window.dmState.messages = [];
        window.dmState.messageOffset = 0;
        window.dmState.messageHasMore = true;
        // Перешли на другой чат — старые `time`-метки от прошлого собеседника
        // не имеют смысла, иначе первый poll с time>0 может прийти пустым
        // (у нового треда нет сообщений с time >= старой метки).
        window.dmState.lastMessageTime = 0;
        window.dmState.initialScroll = true;
        $('#dmeverychat').empty();
        // Switch to a different companion — close the profile drawer so
        // the avatar/name shown match the active chat.
        closeDmProfile();
    }
    window.dmState.currentTarget = newId;

    // Look up the thread id for this companion — the messages endpoint
    // expects a thread id, not the companion_id. If no thread exists yet
    // (e.g. first message to a brand-new user), the first /api/dm/send
    // will create one and we'll refresh the list on success.
    const thread = (window.dmState.threads || []).find(t => Number(t.companion_id) === newId);
    window.dmState.currentThreadId = thread ? Number(thread.id) : null;
    window.dmState.currentTargetName = name || ('User ' + newId);
    window.dmState.currentTargetAvatar = avatar || DM_FALLBACK_AVATAR;
    saveDmCookie(newId);

    // Update right-side header
    $('#dmthreadheader').show();
    $('#dmheadname').text(window.dmState.currentTargetName);
    $('#dmnochat').hide();
    // openDmView прячет #dmeverychat (показывает плейсхолдер "Select a chat")
    // — здесь возвращаем его видимость, чтобы только что загруженные
    // сообщения было где отрисовать.
    $('#dmeverychat').show();

    // Show input
    $('#dminputcontainer').show();
    $('#dmchatinput').prop('disabled', false).attr('placeholder', 'message');

    // Update active state in the thread list (if the item exists)
    $('.dmthreaditem').removeClass('dmthreaditem-active');
    const $item = $(`.dmthreaditem[data-userid="${newId}"]`);
    if ($item.length) {
        $item.addClass('dmthreaditem-active');
    } else {
        // Thread not in the loaded list — still allow chatting (default user 2 case)
        renderDmThreads();
        $(`.dmthreaditem[data-userid="${newId}"]`).addClass('dmthreaditem-active');
    }

    if (isNew || window.dmState.messages.length === 0) {
        loadDmMessages();
    } else {
        // Same thread — refresh from server (poll for new messages)
        window.dmState.messageOffset = 0;
        loadDmMessages();
    }

    // Profile panel is always visible — refresh it whenever the companion changes
    // (or on first select, so the panel doesn't stay on the placeholder).
    if (isNew) {
        loadDmProfile(newId);
    }
}

function loadDmMessages() {
    if (window.dmState.messageLoading) return;
    if (!window.dmState.currentThreadId) {
        // No thread yet for this companion (e.g. brand-new chat). Just
        // render an empty state — a /api/dm/send will create the thread.
        window.dmState.messages = [];
        window.dmState.messageHasMore = false;
        renderDmMessages();
        return;
    }
    window.dmState.messageLoading = true;
    window.dmState.ajaxLoadMessages = $.ajax({
        url: '/api/dm/messages',
        method: 'post',
        dataType: 'json',
        headers: dmAuthHeaders(),
        data: {
            thread: window.dmState.currentThreadId,
            offset: window.dmState.messageOffset,
            // Логика портирована из community.js → getMessages:
            //  - order фиксируем DESC, клиент всё равно сортирует по id ASC
            //  - time = lastMessageTime для полла (дельта), 0 для history
            order: 'DESC',
            time: window.dmState.messageOffset > 0 ? 0 : (window.dmState.lastMessageTime || 0)
        },
        success: function (res) {
            window.dmState.ajaxLoadMessages = null;
            // Если юзер уже вышел из вкладки DM — игнорим ответ.
            // Иначе in-flight запрос перепишет список сообщений, который
            // юзер не видит (вкладка скрыта), и при следующем заходе он
            // увидит «призрачный» апдейт.
            if (!window.dmState.open) {
                window.dmState.messageLoading = false;
                return;
            }
            if (res && res.success) {
                const incoming = res.result || [];
                if (window.dmState.messageOffset === 0) {
                    // Merge by id — keep locally cached entries (poll/append-safe)
                    const map = {};
                    window.dmState.messages.forEach(m => { if (m && m.id != null) map[m.id] = m; });
                    incoming.forEach(m => { if (m && m.id != null) map[m.id] = m; });
                    window.dmState.messages = Object.values(map).sort((a, b) => Number(a.id) - Number(b.id));
                } else {
                    // History page: prepend (older messages), keep unique by id
                    const map = {};
                    window.dmState.messages.forEach(m => { if (m && m.id != null) map[m.id] = m; });
                    incoming.forEach(m => { if (m && m.id != null) map[m.id] = m; });
                    window.dmState.messages = Object.values(map).sort((a, b) => Number(a.id) - Number(b.id));
                }
                // Стопоримся только по фактически пустому ответу. Раньше
                // было `>= DM_MESSAGES_PAGE` — и если сервер отдавал
                // меньше полной страницы (например, 20 при DM_MESSAGES_PAGE=50),
                // флаг сразу становился false и подгрузка вставала навсегда.
                window.dmState.messageHasMore = incoming.length > 0;
                // Подтягиваем lastMessageTime до максимального time в ответе —
                // следующий poll уйдёт уже с этим значением и прилетит только
                // реально новое. Безопасно и для offset>0 (history): max из
                // старой страницы всё равно <= текущего lastMessageTime.
                incoming.forEach(m => {
                    if (m && m.time != null) {
                        const t = Number(m.time);
                        if (!isNaN(t) && (!window.dmState.lastMessageTime || t > window.dmState.lastMessageTime)) {
                            window.dmState.lastMessageTime = t;
                        }
                    }
                });
                renderDmMessages();
            }
            // Держим messageLoading=true до конца рендера. Иначе во время
            // prepend'а успевает стрельнуть scroll-event, scroll-хэндлер
            // видит scrollTop<=50 и !messageLoading и дёргает ещё один
            // load — каскад подгрузок и визуальный «прыжок» в начало.
            window.dmState.messageLoading = false;
        },
        error: function () {
            window.dmState.messageLoading = false;
            window.dmState.ajaxLoadMessages = null;
        }
    });
}

function renderDmMessages() {
    const chat = document.getElementById('dmeverychat');
    if (!chat) return;
    const $chat = $(chat);

    const isHistoryLoad = window.dmState.messageOffset > 0;
    if (isHistoryLoad) {
        // Keep visual scroll position when prepending older messages.
        // Capture size/position BEFORE touching the DOM — browser scroll
        // anchoring would otherwise try to "fix" things for us and the
        // result is unpredictable.
        const prevHeight = chat.scrollHeight;
        const prevTop = chat.scrollTop;

        // Собираем HTML всех ещё-не-в-DOM сообщений **в правильном
        // порядке** (по возрастанию id) и prepend'им одним батчем.
        // Старый код prepend'ил по одному в цикле по возрастанию — jQuery
        // prepend пушит существующее вниз, так что последний
        // отрендеренный оказывался на самом верху, и порядок новых
        // сообщений разворачивался (msg_new_50 наверху вместо msg_new_1).
        const newHtml = window.dmState.messages
            .filter(msg => msg && msg.id != null && !document.getElementById('dmmessage-' + msg.id))
            .map(renderDmMessageHtml)
            .join('');
        if (newHtml) {
            $chat.prepend(newHtml);
        }

        // Форсим reflow (чтение scrollHeight), затем восстанавливаем
        // визуальную позицию: разница высот + старая позиция.
        const newHeight = chat.scrollHeight;
        chat.scrollTop = newHeight - prevHeight + prevTop;
        return;
    }

    // offset == 0 — render missing messages at the bottom
    // First, drop the "No messages yet" placeholder if it's there from a
    // previous render with an empty list.
    const $placeholder = $chat.children('.dm-empty-placeholder');
    if ($placeholder.length && window.dmState.messages.length > 0) {
        $placeholder.remove();
    }
    window.dmState.messages.forEach(msg => {
        if (!msg || msg.id == null) return;
        if (!document.getElementById('dmmessage-' + msg.id)) {
            $chat.append(renderDmMessageHtml(msg));
        }
    });

    // Empty state — only show it when the chat area is otherwise blank
    // (i.e. no messages rendered yet and no header offset).
    if (window.dmState.messages.length === 0 && !$chat.children('.dm-empty-placeholder').length) {
        $chat.append('<div class="dm-empty-placeholder" style="padding: 20px; color: #888; text-align: center; font-size: 13px;">No messages yet</div>');
    }

    // Pin to bottom: на initial scroll (первая загрузка, после отправки)
    // — всегда вниз. В остальных случаях (poll принёс новые сообщения) —
    // только если юзер уже был внизу, иначе не дёргать скролл, чтобы
    // читающий не вылетал из прочитанной позиции.
    if (window.dmState.initialScroll) {
        window.dmState.initialScroll = false;
        chat.scrollTop = chat.scrollHeight;
    } else {
        // Порог 50px — за край чата не цепляемся пиксель-в-пиксель, иначе
        // на длинных сообщениях sub-pixel округления дают ложный
        // «не у дна» даже когда юзер физически внизу.
        const wasAtBottom = (chat.scrollHeight - chat.scrollTop - chat.clientHeight) < 50;
        if (wasAtBottom) {
            chat.scrollTop = chat.scrollHeight;
        }
    }
}

function deleteDmMessage(messageId) {
    const id = String(messageId);
    if (!/^\d+$/.test(id)) return;

    const $message = $('#dmmessage-' + id);
    const $button = $message.find('.dm-message-delete');
    if ($button.data('deleting')) return;

    $button.data('deleting', true).prop('disabled', true);

    const restoreButton = () => {
        $button.data('deleting', false).prop('disabled', false);
    };
    const showError = (message) => {
        restoreButton();
        try { notification('red', 'error', message || 'delete failed'); }
        catch (e) { alert(message || 'delete failed'); }
    };

    $.ajax({
        url: '/api/dm/delete',
        method: 'post',
        dataType: 'json',
        headers: dmAuthHeaders(),
        data: {
            message: Number(id)
        },
        success: function (res) {
            if (!res || !res.success) {
                showError((res && (res.message || res.result)) || 'delete failed');
                return;
            }

            window.dmState.messages = (window.dmState.messages || [])
                .filter(msg => !msg || String(msg.id) !== id);
            $message.remove();

            const $chat = $('#dmeverychat');
            if (!$chat.children('.message').length && !$chat.children('.dm-empty-placeholder').length) {
                $chat.append('<div class="dm-empty-placeholder" style="padding: 20px; color: #888; text-align: center; font-size: 13px;">No messages yet</div>');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error deleting DM message:', textStatus, errorThrown);
            showError('delete failed');
        }
    });
}

// DM-специфичный рендер текста сообщения. В отличие от общего чата, текст
// экранируется ДО markdown-обработки: parseMessage сначала матчит URL
// известных хостингов и превращает их в <img>/<video>/<a>, но его регулярки
// безопасны только на сыром вводе — на экранированном он бы не нашёл ни
// одной картинки. Поэтому план такой:
//   1) выносим URL картинок/видео/альбомов в плейсхолдеры,
//   2) экранируем остальной текст (плейсхолдеры выживают — в них только [a-z0-9_]),
//   3) прогоняем безопасный markdown (тот же набор, что в processMarkdown, но
//      без bare-URL-ветки с двойным escapeHtml и без admin-ссылок, чтобы
//      processMarkdown со своими quirks не сломал query string в URL),
//   4) подставляем URL обратно через parseMessage — для одиночного проверенного
//      URL он возвращает только безопасный тег (alphanumerics + хардкод).
function renderDmMessageText(rawText, role) {
    if (!rawText) return '';

    const stashed = [];
    function stash(re) {
        rawText = rawText.replace(re, (m) => {
            const i = stashed.length;
            stashed.push(m);
            return `__DM_IMG_${i}__`;
        });
    }
    // Порядок важен — сначала более специфичные паттерны (видео/альбомы).
    stash(/https?:\/\/i\.imgur\.com\/[a-zA-Z0-9]+\.(mp4|webm)/gi);
    stash(/https?:\/\/i\.imgur\.com\/[a-zA-Z0-9]+\.(jpg|jpeg|png|gif|webp)/gi);
    stash(/https?:\/\/i\.ibb\.co\/[a-zA-Z0-9]+\/[a-zA-Z0-9]+\.(jpg|jpeg|png|gif|webp)/gi);
    stash(/https?:\/\/(www\.)?imgur\.com\/(a|gallery)\/[a-zA-Z0-9]+/gi);

    // Экранируем. __DM_IMG_N__ состоит только из [a-zA-Z0-9_] — escapeHtml их
    // не трогает, так что плейсхолдеры переживут и попадут в финальный текст.
    let html = escapeHtml(rawText);

    // Безопасный inline-markdown. $1 уже экранирован, класть в тело тега
    // безопасно. Набор правил — тот же, что в processMarkdown, минус
    // bare-URL и admin-ссылки (URL у нас либо в плейсхолдерах, либо
    // обрабатываются собственной веткой ниже без двойного escapeHtml).
    html = html
        .replace(/\*(.*?)\*/g, '<strong>$1</strong>')
        .replace(/~(.*?)~/g, '<del>$1</del>')
        .replace(/```([\s\S]*?)```/g, '<div style="position:relative; min-width: 35px;"><pre><code style="display:block;padding:8px;margin:1px 0;white-space:pre;overflow-x:auto;background-color:rgba(0,0,0,0.2);border-radius:3px;font-family:monospace;">$1</code></pre><button class="copy-button" onclick="copyToClipboard(this)" style="position:absolute;top:6px;right:6px;background:rgba(255,255,255,0.1);border:none;color:#fff;padding:2px 8px;border-radius:3px;cursor:pointer;font-size:12px;">Copy</button></div>')
        .replace(/# (.*?)\n/g, '<h3>$1</h3>\n')
        .replace(/(^@)([а-яА-ЯёЁa-zA-Z0-9_]+)/g, '<span class="mention" onclick="openminiprofile(null,`$2`)" style="color: var(--color1); cursor: pointer;">@$2</span>')
        .replace(/\s@([а-яА-ЯёЁa-zA-Z0-9_]+)/g, ' <span class="mention" onclick="openminiprofile(null,`$1`)" style="color: var(--color1); cursor: pointer;">@$1</span>')
        // Голые URL — вход уже экранирован, $1 в href и тексте безопасен.
        .replace(
            /(?<!["'])https?:\/\/[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]/gi,
            (url) => `<a style="color: var(--color1);" href="${url}" target="_blank" rel="noopener noreferrer">${url}</a>`);

    if (typeof parseMessage === 'function') {
        for (let i = 0; i < stashed.length; i++) {
            html = html.split(`__DM_IMG_${i}__`).join(parseMessage(stashed[i]));
        }
    }

    return html;
}

function renderDmMessageHtml(msg) {
    // Real message shape: { id, boss, text, time, status, is_read }
    // — `boss` is the sender user id. There is no per-message name/avatar,
    // so we derive them from window.currentUser (mine) and dmState (other).
    const myId = window.currentUser ? Number(window.currentUser.id) : 0;
    const isMine = myId > 0 && Number(msg.boss) === myId;
    const me = window.currentUser || {};
    const name = isMine
        ? (me.name || me.username || 'me')
        : (window.dmState.currentTargetName || ('User ' + (msg.boss || '')));
    const avatar = isMine
        ? (me.avatar || DM_FALLBACK_AVATAR)
        : (window.dmState.currentTargetAvatar || DM_FALLBACK_AVATAR);
    const timeStr = msg.time ? new Date(Number(msg.time) * 1000).toLocaleString() : '';
    // DM-специфичный безопасный рендер: экранирует XSS, сохраняет вставку
    // картинок/видео/альбомов через parseMessage (см. renderDmMessageText).
    const rawText = msg.text || '';
    const textHtml = (typeof processMarkdown === 'function' || typeof parseMessage === 'function')
        ? renderDmMessageText(rawText, msg.role)
        : escapeHtml(rawText).replace(/\n/g, '<br>');
    const deleteButton = isMine ? `
                    <div class="message-right">
                        <button type="button" class="dm-message-delete" data-message-id="${escapeHtmlAttr(msg.id)}" title="Delete message" aria-label="Delete message">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 448 512" aria-hidden="true">
                                <path d="M135.2 17.7C140.5 7.4 150.9 0 162.7 0H285.3C297.1 0 307.5 7.4 312.8 17.7L336 64H432C440.8 64 448 71.2 448 80V112C448 120.8 440.8 128 432 128H416V432C416 476.2 380.2 512 336 512H112C67.8 512 32 476.2 32 432V128H16C7.2 128 0 120.8 0 112V80C0 71.2 7.2 64 16 64H112L135.2 17.7zM80 128V432C80 449.7 94.3 464 112 464H336C353.7 464 368 449.7 368 432V128H80zM192 224C200.8 224 208 231.2 208 240V368C208 376.8 200.8 384 192 384C183.2 384 176 376.8 176 368V240C176 231.2 183.2 224 192 224zM272 224C280.8 224 288 231.2 288 240V368C288 376.8 280.8 384 272 384C263.2 384 256 376.8 256 368V240C256 231.2 263.2 224 272 224z"/>
                            </svg>
                        </button>
                    </div>` : '';
    return `
<div class="message ${isMine ? 'message-sel' : ''}" id="dmmessage-${msg.id}" data-id="${msg.id}">
    <div style="display: flex;">
        <img class="message-avatar" src="${escapeHtmlAttr(avatar)}" alt=""
             onerror="this.onerror=null; this.src='${DM_FALLBACK_AVATAR}';">
        <div class="msgcont">
            <div class="message-content">
                <div class="message-header">
                    <span class="message-username">${escapeHtml(name)}</span>
                    <span class="message-time">${escapeHtml(timeStr)}</span>${deleteButton}
                </div>
                <span class="message-text">${textHtml}</span>
            </div>
        </div>
    </div>
</div>`;
}

// The right-side profile panel is always visible now (no slide-in), so the
// old open/close toggles are no-ops kept only for backward compatibility
// with any leftover callers. Profile data is loaded automatically from
// selectDmThread when a companion is chosen.
function openDmProfile() {
    const id = window.dmState.currentTarget;
    if (id == null) return;
    loadDmProfile(Number(id));
}

function closeDmProfile() {
    // no-op: panel is always open
}

// Fetch the companion's full profile and render it in the slide-out.
// Same endpoint as the existing /comprofilepop in community.js, so we
// get bio, role, deposit, created, mail etc.
function loadDmProfile(userId) {
    const $loading = $('#dmprofile-loading');
    const $content = $('#dmprofile-content');
    const $error = $('#dmprofile-error');

    $loading.show();
    $content.hide();
    $error.hide();

    // Pre-fill name/avatar from what we already have so the panel isn't
    // blank while the request is in flight.
    $('#dmprofilename').text(window.dmState.currentTargetName || ('User ' + userId));
    $('#dmprofileavatar').attr('src', window.dmState.currentTargetAvatar || DM_FALLBACK_AVATAR);

    $.ajax({
        url: '/api/profile/get',
        method: 'post',
        dataType: 'json',
        headers: dmAuthHeaders(),
        data: { id: userId }
    }).done(function (res) {
        if (!res || !res.success || !res.result) {
            $loading.hide();
            $error.show();
            return;
        }
        const p = res.result;

        $('#dmprofileavatar').attr('src', p.avatar || DM_FALLBACK_AVATAR);

        // Strike through name if user is currently banned (bantime > now)
        let nameHtml = escapeHtml(p.name || ('User ' + userId));
        try {
            const banMs = (p.bantime || 0) * 1000;
            if (banMs > Date.now()) {
                nameHtml = '<s>' + nameHtml + '</s>';
            }
        } catch (e) { }
        $('#dmprofilename').html(nameHtml);

        $('#dmprofilerole').text(dmRoleLabel(p.role));

        // pins — custom rank/tag the user pinned to themselves (e.g. "admin").
        // Empty / null → hide the badge entirely.
        const $pins = $('#dmprofilepins');
        if (p.pins && String(p.pins).length) {
            $pins.text(p.pins).show();
        } else {
            $pins.hide();
        }

        $('#dmprofilecreated').text(p.created ? tsToDateStr(Number(p.created)) : '—');
        $('#dmprofiledeposit').text(p.deposit ? '$ ' + p.deposit : 'no deposit');
        $('#dmprofilebio').text((p.bio && p.bio.length) ? p.bio : 'No bio');

        // banhistory — скрыт в DM-профиле по требованию: даже если сервер
        // вернул историю банов, юзер её тут не видит. Всю диагностику
        // можно посмотреть через основной comprofilepop.
        $('#dmprofilebanhistory-section').hide();

        $loading.hide();
        $content.show();
    }).fail(function () {
        $loading.hide();
        $error.show();
    });
}

// Mirror the role→label mapping used by openminiprofile in community.js.
function dmRoleLabel(role) {
    switch (Number(role)) {
        case 1: return 'CORE';
        case 2: return 'PRO';
        case 3: return 'PRO';
        case 11: return 'Administrator';
        default: return 'Newbie';
    }
}

function sendDmMessage() {
    const $input = $('#dmchatinput');
    const text = $input.val().trim();
    if (!text) return;
    // Mirrors the chat composer: don't send while an image is still being
    // uploaded to imgbb (the URL placeholder is still in the textarea).
    if (text.includes('[Image Uploading...]')) {
        try { notification('red', 'chat', 'image loading'); } catch (e) { alert('image loading'); }
        return;
    }
    if (window.dmState.sending) return;
    if (!window.dmState.currentTarget) return;
    window.dmState.sending = true;
    $.ajax({
        url: '/api/dm/send',
        method: 'post',
        dataType: 'json',
        headers: dmAuthHeaders(),
        data: {
            target_user: window.dmState.currentTarget,
            text: text
        },
        success: function (res) {
            window.dmState.sending = false;
            if (res && res.success) {
                $input.val('').css('height', '34px');
                $input.trigger('input');

                // Reload latest messages (offset 0, scroll to bottom).
                const reloadMessages = () => {
                    window.dmState.messageOffset = 0;
                    window.dmState.initialScroll = true;
                    loadDmMessages();
                };

                if (!window.dmState.currentThreadId) {
                    // A new thread was just created server-side (first message
                    // to this companion). The old code called loadDmThreads()
                    // and loadDmMessages() back-to-back — but the second call
                    // saw a stale null currentThreadId and bailed out, so the
                    // chat never refreshed. Chain them via onComplete so the
                    // thread-id sync inside loadDmThreads runs first.
                    loadDmThreads(undefined, reloadMessages);
                } else {
                    reloadMessages();
                }
            } else {
                const err = (res && (res.message || res.result)) || 'send failed';
                try { notification('red', 'error', err); } catch (e) { alert(err); }
            }
        },
        error: function () {
            window.dmState.sending = false;
            try { notification('red', 'error', 'send failed'); } catch (e) { alert('send failed'); }
        }
    });
}

// ── helpers (mirror community.js style) ───────────────────────────────────
function escapeHtml(unsafe) {
    if (unsafe === undefined || unsafe === null) return '';
    return String(unsafe)
        .replaceAll(/&/g, "&amp;")
        .replaceAll(/</g, "&lt;")
        .replaceAll(/>/g, "&gt;")
        .replaceAll(/"/g, "&quot;")
        .replaceAll(/'/g, "&#039;");
}

function escapeHtmlAttr(unsafe) {
    return escapeHtml(unsafe);
}

// ── wiring (runs once the DOM is ready) ───────────────────────────────────
$(() => {
    // Make the DM tab visible only after the dynamic thread tabs have been
    // appended by community.js — keeps the order predictable.
    const showDmTab = () => {
        const $tab = $('#dmtab');
        if ($tab.length) $tab.show();
    };
    // community.js fetches /api/threads synchronously enough; just defer one tick
    setTimeout(showDmTab, 200);
    setTimeout(showDmTab, 800);

    // Delete button is rendered only for the current user's own messages.
    $(document).on('click', '.dm-message-delete', function (e) {
        e.preventDefault();
        e.stopPropagation();
        deleteDmMessage($(this).attr('data-message-id'));
    });

    // Enter to send (in the DM input only — don't steal Enter from other inputs)
    $(document).on('keypress', '#dmchatinput', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            e.stopPropagation();
            sendDmMessage();
        }
    });

    // Auto-resize the DM input as the user types
    $(document).on('input', '#dmchatinput', function () {
        this.style.height = '34px';
        this.style.height = (this.scrollHeight) + 'px';
    });

    // Scroll up — load older messages. Логика портирована из чата
    // (community.js → scrollHandler, ветка «close to top»): никакого
    // `messageHasMore` / `offset < length` нет, только лок через
    // `messageLoading`. Сервер сам вернёт пусто, когда сообщений больше
    // нет — renderDmMessages просто ничего не добавит.
    $(document).on('scroll', '#dmeverychat', function () {
        // Не дёргаем подгрузку в коротких чатах, где скроллить
        // нечего (scrollHeight <= clientHeight). Без этого гарда
        // `scrollTop <= 50` срабатывает и в неработающем скролле —
        // каскад лишних запросов и «прыжки» в начало.
        if (this.scrollTop <= 50
            && this.scrollHeight > this.clientHeight
            && !window.dmState.messageLoading
            && window.dmState.currentTarget) {
            window.dmState.messageOffset += DM_MESSAGES_PAGE;
            loadDmMessages();
        }
    });

    // Scroll to the bottom of the thread list — load more threads
    $(document).on('scroll', '#dmthreadlist', function () {
        if (this.scrollTop + this.clientHeight >= this.scrollHeight - 50
            && window.dmState.threadHasMore
            && !window.dmState.threadLoading) {
            window.dmState.threadOffset += DM_THREADS_PAGE;
            loadDmThreads();
        }
    });

    // window.currentUser is loaded asynchronously by community.js → getme().
    // Once it arrives, re-render the chat so the "isMine" side is correct.
    const _userWait = setInterval(() => {
        if (window.currentUser) {
            clearInterval(_userWait);
            if (window.dmState.open && window.dmState.currentTarget) {
                $('#dmeverychat').empty();
                renderDmMessages();
            }
        }
    }, 200);

    // If the user was in DM mode last time, restore it on page load.
    // We wait a tick so community.js finishes wiring up #communityheader first.
    const tryRestoreDm = () => {
        const saved = readCookie('lastDmUserId');
        if (saved && /^\d+$/.test(saved)) {
            try { openDmView(); } catch (e) { console.error('dm restore', e); }
        }
    };
    setTimeout(tryRestoreDm, 300);

    // Фоновый префетч списка DM-тредов: когда пользователь откроет вкладку DM,
    // список уже будет в памяти и отрисуется мгновенно. Не трогаем currentTarget
    // и не подгружаем сообщения — это произойдёт при реальном открытии вкладки.
    setTimeout(() => {
        if (!window.dmState.threadsLoaded) {
            loadDmThreads(false);
        }
    }, 1500);
});
