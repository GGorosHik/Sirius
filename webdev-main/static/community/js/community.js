window.messagesorder = "DESC";
if (window.newnews) {
    window.currentthread = 1006;
} else {
    window.currentthread = readCookie('lastThread');
}

if (!readCookie('lastThread')) {
    createCookie('lastThread', 1, 365);
}

function getPrimaryChatInput() {
    return document.getElementById('chatinput');
}

// Tracks the last-focused chat composer textarea so markdown/emoji tools
// can act on whichever one (#chatinput or #dmchatinput) the user was typing
// in. The tool buttons themselves can steal focus when clicked, so we
// record focus explicitly via the focusin handler below.
_chatComposerLastInput = null;

function getChatComposerInput() {
    if (_chatComposerLastInput && document.body.contains(_chatComposerLastInput)) {
        return _chatComposerLastInput;
    }
    return getPrimaryChatInput();
}

function closeToolsPicker() {
    $('#toolsPicker, #dmToolsPicker').removeClass('open');
}

function closeEmojiPicker() {
    $('#emojiPicker, #dmEmojiPicker').removeClass('open');
}

function closeChatPopups() {
    closeToolsPicker();
    closeEmojiPicker();
}

function updateChatInputState(input) {
    const targetInput = input || getChatComposerInput();
    if (!targetInput) {
        return;
    }

    const $input = $(targetInput);
    $input.trigger('input');

    setTimeout(() => {
        try {
            adjustChatboxHeight();
        } catch { }
    }, 0);
}

function insertIntoChatInput(prefix, suffix = '', placeholder = '') {
    const input = getChatComposerInput();
    if (!input) {
        return;
    }

    const start = input.selectionStart ?? input.value.length;
    const end = input.selectionEnd ?? input.value.length;
    const selectedText = input.value.slice(start, end);
    const content = selectedText || placeholder;
    const inserted = `${prefix}${content}${suffix}`;

    input.value = input.value.slice(0, start) + inserted + input.value.slice(end);

    const selectionOffsetStart = start + prefix.length;
    const selectionOffsetEnd = selectionOffsetStart + content.length;

    input.focus();
    if (selectedText || placeholder) {
        input.setSelectionRange(selectionOffsetStart, selectionOffsetEnd);
    } else {
        input.setSelectionRange(selectionOffsetStart, selectionOffsetStart);
    }

    updateChatInputState(input);
}

function insertLinePrefixIntoChatInput(prefix, fallback = '') {
    const input = getChatComposerInput();
    if (!input) {
        return;
    }

    const start = input.selectionStart ?? input.value.length;
    const end = input.selectionEnd ?? input.value.length;
    const selectedText = input.value.slice(start, end);
    const valueBefore = input.value.slice(0, start);
    const lineStart = valueBefore.lastIndexOf('\n') + 1;
    const baseText = selectedText || fallback;
    const prefixed = baseText
        .split('\n')
        .map(line => `${prefix}${line}`)
        .join('\n');

    input.value = input.value.slice(0, lineStart) + prefixed + input.value.slice(end);
    input.focus();
    input.setSelectionRange(lineStart + prefix.length, lineStart + prefixed.length);
    updateChatInputState(input);
}

function handleChatTool(tool) {
    switch (tool) {
        case 'bold':
            insertIntoChatInput('*', '*', 'bold text');
            closeToolsPicker();
            break;
        case 'strike':
            insertIntoChatInput('~', '~', 'text');
            closeToolsPicker();
            break;
        case 'header':
            insertLinePrefixIntoChatInput('# ', 'Title');
            closeToolsPicker();
            break;
        case 'code':
            insertIntoChatInput('```\n', '\n```', 'code');
            closeToolsPicker();
            break;
        default:
            break;
    }
}

function insertEmojiIntoChatInput(emoji) {
    const input = getChatComposerInput();
    if (!input) {
        return;
    }

    const start = input.selectionStart ?? input.value.length;
    const end = input.selectionEnd ?? input.value.length;
    const needsLeadingSpace = start > 0 && !/\s/.test(input.value[start - 1]);
    const needsTrailingSpace = end < input.value.length && !/\s/.test(input.value[end]);
    const emojiText = `${needsLeadingSpace ? ' ' : ''}${emoji}${needsTrailingSpace ? ' ' : ''}`;

    input.value = input.value.slice(0, start) + emojiText + input.value.slice(end);
    const caret = start + emojiText.length;

    input.focus();
    input.setSelectionRange(caret, caret);
    closeEmojiPicker();
    updateChatInputState(input);
}

function toggleToolsPicker(pickerId) {
    const id = pickerId || 'toolsPicker';
    const willOpen = !$('#' + id).hasClass('open');
    closeChatPopups();
    if (willOpen) {
        $('#' + id).addClass('open');
    }
}

function toggleEmojiPicker(pickerId) {
    const id = pickerId || 'emojiPicker';
    const willOpen = !$('#' + id).hasClass('open');
    closeChatPopups();
    if (willOpen) {
        $('#' + id).addClass('open');
    }
}

$(() => {
    if (readCookie("bgmode") == "2") {
        $("body").css("background", readCookie('bgcolor'));
    }

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#emojiPicker, #toolsPicker, #chatEmojiToggle, #chatToolsToggle, #dmEmojiPicker, #dmToolsPicker, #dmEmojiToggle, #dmToolsToggle').length) {
            closeChatPopups();
        }
    });

    // Track whichever composer textarea the user is typing in so markdown /
    // emoji tool actions operate on the right one.
    $(document).on('focusin', '#chatinput, #dmchatinput', function () {
        _chatComposerLastInput = this;
    });

    $('#chatinput').on('change', function () {
        this.style.height = '34px'; // минимальная высота
        this.style.height = (this.scrollHeight) + 'px'; // высота по содержимому
        if (window.adjustChatWait == false) {
            setTimeout(() => { adjustChatboxHeight(); }, 300);
        }
    });
    $('#chatinput').on('input', function () {
        if (window.oldcurrentthread != 7) { return; }
        $("#everychat").css("height", "calc(100% - " + (60 + (this.scrollHeight > 190 ? 190 : this.scrollHeight)) + "px)")
        setTimeout(() => { $("#everychat").css("height", "calc(100% - " + (60 + (this.scrollHeight > 190 ? 190 : this.scrollHeight)) + "px)") }, 1);
    })

    const url = new URL(window.location.href); // Получаем текущий URL
    const params = new URLSearchParams(url.search); // Извлекаем параметры

    thread = params.get('thread'); // Получаем значение параметра thread
    msg = params.get('msg');

    if (thread && msg) {
        getOldMessage(thread, msg)
    }

    if (msg != null) {
        $.ajax({
            url: '/api/threads/info',
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
            },
            data: {
                thread: window.currentthread ? window.currentthread : 1,
            },
            dataType: 'json'
        }).done(res => {
            if (res.success) {
                if (res.result.forum == 2) {
                    buttonid = 1006;
                } else if (res.result.forum == 1) {
                    buttonid = 7;
                } else if (res.result.forum == 3) {
                    buttonid = 8;
                } else {
                    buttonid = thread;
                }
                console.warn("clicked " + buttonid)
                if (thread) {
                    window.gomessagecounter = 0;
                    window.gomessageinterval = setInterval(() => {
                        window.gomessagecounter++;
                        if (window.gomessagecounter > 1000) {
                            clearInterval(window.gomessageinterval);
                        }
                        button = document.querySelector(`.headselectorbutton[threadid="${buttonid}"]`);
                        if (button) {
                            if (res.result.forum == 2) {
                                window.currentthreadisforum = true
                                window.currentthread = buttonid;
                                window.oldcurrentthread = buttonid;
                                initChat()
                            } else if (res.result.forum == 1) {
                                window.currentthreadisforum = true
                                window.currentthread = buttonid;
                                window.oldcurrentthread = buttonid;
                                initChat()
                            }
                            //window.currentthread = thread;
                            //window.oldcurrentthread = thread;
                            //initChat()
                            $('.headselectorbutton').removeClass('headselectorbuttonactive')
                            button.classList.add('headselectorbuttonactive');
                            // для прямой ссылки «back» должен возвращать к списку тем (7/1006/8),
                            // а не к предыдущему currentthread из куки
                            let backTarget = null;
                            if (res.result.forum == 1) backTarget = 7;
                            else if (res.result.forum == 2) backTarget = 1006;
                            else if (res.result.forum == 3) backTarget = 8;
                            openthread(thread, res.result.name, res.result.first_message_id, backTarget);
                            clearInterval(window.gomessageinterval);
                        } else {
                            console.warn(`${thread} not found`);
                        }
                    }, 10);
                }
            }
        })
    }


    $('#chatinput').on('keypress', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            e.stopPropagation();
            $('#sendmsgbutton').click()
            setTimeout(() => {
                adjustChatboxHeight();
            }, 500);
        }
    });
    $('#chatinput').on('input', function () {
        this.style.height = '34px'; // минимальная высота
        this.style.height = (this.scrollHeight) + 'px'; // высота по содержимому
        if (window.adjustChatWait == false) {
            adjustChatboxHeight();
        }
    });

    // Same handlers for the DM composer — Enter sends via #dmsendmsgbutton
    $('#dmchatinput').on('keypress', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            e.stopPropagation();
            $('#dmsendmsgbutton').click();
        }
    });
    $('#dmchatinput').on('input', function () {
        this.style.height = '34px';
        this.style.height = (this.scrollHeight) + 'px';
    });

    $('.fullscreen-image').on('wheel', function (e) {
        e.preventDefault();
        e.stopPropagation();

        delta = e.originalEvent.deltaY < 0 ? 1 : -1;
        newScale = scale + delta * scaleStep;

        if (newScale >= minScale && newScale <= maxScale) {
            scale = newScale;
            $(this).addClass('zoomed').css('--scale', scale);
        }
    });

    $(document).on('keydown', function (event) {
        if (event.key === 'Escape' || event.keyCode === 27) {
            try { $('.close-btn').click(); } catch { }
            try { $('.closebutton').click(); } catch { }
            try {
                cancelReply();
                $('#chatinput').val('');
                $('#chatinput').trigger("input");
                $('#chatinput')[0].style.height = '34px';
                $('#sendmsgbutton').html('Send');
                $('#sendmsgbutton').attr('onclick', 'sendmessage(window.currentthread)');
            } catch { }
        }
    });
    setTimeout(() => { $('#chatinput').focus(); }, 500);
    setInterval(() => { try { adjustChatboxHeight(); } catch { } }, 1000);

    // Стили для тегов фильтрации
    const tagStyles = `
        .comtag {
            cursor: pointer;
            padding: 6px 10px;
            margin: 2px;
            border-radius: 3px;
            background: #3030305d;
            border: 1px color-mix(in srgb, var(--color1), transparent 70%) solid;
            color: #ddd;
            transition: box-shadow 0.2s ease, border-color 0.2s ease, color 0.2s ease;
            display: inline-block;
        }
        .comtag:hover {
            border-color: color-mix(in srgb, var(--color1), transparent 40%);
            box-shadow: inset 0 0 10px color-mix(in srgb, var(--color1), transparent 65%);
            color: #fff;
        }
        .comtag-active {
            background: #30303080 !important;
            border-color: color-mix(in srgb, var(--color1), transparent 30%) !important;
            box-shadow: inset 0 0 12px color-mix(in srgb, var(--color1), transparent 55%) !important;
            color: #fff !important;
        }
    `;
    $('<style>').text(tagStyles).appendTo('head');

    $.ajax({
        url: '/api/threads',
        method: 'post',
        dataType: 'json',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
        },
        success: function (jres) {
            $('.headselectorbutton[threadid]').remove();
            window.threads = jres;
            jres.result.forEach(function (el) {
                addclass = ""
                if ((window.newnews && el.id == 4) || (el.id == readCookie('lastThread') && !window.newnews)) {
                    addclass = ' headselectorbuttonactive'
                }
                template = `
                <div threadid='${el.id}' class="headselectorbutton${addclass}" onclick="headselcom('.comscripts',this,${el.id});">
                    <b>${el.name}</b>
                </div>
                `
                console.warn(el.name);
                $('#communityheader').append(template);
            })
            window.newnews = false;
        }
    });

    renderSelectedTags();
})

window.adjustChatWait = false;
console.log(window.newnews)

window.ismarket = false;
window.selectedTag = 0; // Инициализация выбранного тега
window.previousThread = null;

function isGuidesForumView() {
    return Number(window.currentthread) === 8 && !window.currentthreadisforum;
}

function isGuidesThread() {
    if (window.currentThreadInfo && Number(window.currentThreadInfo.forum) === 3) {
        return true;
    }

    return window.currentthreadisforum && Number(window.oldcurrentthread) === 8;
}

function isInformationForumView() {
    return Number(window.currentthread) === 1006 && !window.currentthreadisforum;
}

function isInformationThread() {
    if (window.currentThreadInfo && Number(window.currentThreadInfo.forum) === 2) {
        return true;
    }
    return window.currentthreadisforum && Number(window.oldcurrentthread) === 1006;
}

function isTaglessSection() {
    return isGuidesForumView() || isGuidesThread() || isInformationForumView() || isInformationThread();
}

function syncForumCreateTagsVisibility() {
    const $tagsRow = $('#selected-tags').parent();
    if ($tagsRow.length === 0) {
        return;
    }

    if (isGuidesForumView() || isInformationForumView()) {
        window.threadSelectedTags = [];
        renderSelectedTags();
        $('#tagPopup').hide();
        $tagsRow.hide();
        return;
    }

    $tagsRow.show();
}

function initChat() {
    syncForumCreateTagsVisibility();

    if (window.currentthread == 7) {
        gettegs(); // Загружаем теги для фильтрации
        $('.marketlefeder').show();
        $('.chatbox').css('height', '100%');
        $('.chatbox').css('width', 'calc(100% - 70px)');
        $('#comchat').css('flex-direction', 'row');
        $('.chatinput-container').hide();
        $('#comchat').css('margin-top', '');
        $(".message").remove();
        $(".forummsg").remove();
        $(".fmessages").remove();
        $("#guidescreatebutton").remove();
    } else if (window.currentthread == 1006) {
        $('.marketlefeder').hide();
        $('.chatbox').css('height', '100%');
        $('.chatbox').css('width', '100%');
        $('#comchat').css('flex-direction', 'row');
        $('#comchat').css('margin-top', '');
        $('.chatinput-container').hide();
        $(".message").remove();
        $(".forummsg").remove();
        $(".fmessages").remove();
        $("#guidescreatebutton").remove();
    } else if (window.currentthread == 8) {
        $('.marketlefeder').hide();
        $('#everychat').html(`<div style="width: calc(100% - 24px);" id="guidescreatebutton" onclick="$('#forumcreate').show(); $('.backblur').show();" class="button1">create new guide</div>`);
        $('.chatbox').css('width', '100%');
        $('.chatbox').css('height', '100%');
        $('#comchat').css('flex-direction', 'row');
        $('#comchat').css('margin-top', '');
        $('.chatinput-container').hide();
        $(".message").remove();
        $(".forummsg").remove();
        $(".fmessages").remove();
    } else {
        if (window.oldcurrentthread == 7 && window.currentthreadisforum) {
            setTimeout(() => {
                $('#themetags').show();
                $('.chatbox').css('height', 'calc(100% - 90px)');
                $('#comchat').css('margin-top', '80px');
                writethemetags();
            }, 1000);

        } else if (window.oldcurrentthread == 8 && window.currentthreadisforum) {
            $('#themetags').hide();
            $('#changetags-wrap').hide();
            $('#comchat').css('margin-top', '40px');
            $('.chatbox').css('height', 'calc(100% - 50px)');

        } else if (window.oldcurrentthread == 1006 && window.currentthreadisforum) {
            $('#themetags').hide();
            $('#changetags-wrap').hide();
            $('#comchat').css('margin-top', '40px');
            $('.chatbox').css('height', 'calc(100% - 50px)');

        } else if (window.oldcurrentthread == 1006) {
            $('.chatbox').css('height', '');
            $('#comchat2').css('margin-top', '40px');
            $('#themetags').hide();
        } else {
            $('#themetags').hide();
            $('.chatbox').css('height', '');
            setTimeout(() => { writethemetags(); }, 500);
            // при входе в тред из произвольного места (прямая ссылка, переход с обычного чата)
            // нужно оставить место под фиксированный #headerthem, иначе он накрывает первое сообщение
            if (window.currentthreadisforum) {
                $('#comchat').css('margin-top', '50px');
                $('.chatbox').css('height', 'calc(100% - 60px)');
            } else {
                $('#comchat').css('margin-top', '0px');
            }
        }
        // $('.chatbox').css('height', '');
        $('.marketlefeder').hide();
        $('.chatbox').css('width', 'calc(100% - 20px)');
        $('#comchat').css('flex-direction', 'column');
        $('.chatinput-container').show();
        // При переходе из 1006/7 в реальный тред сюда приезжают пустые
        // postElement-обёртки (bg-gray-800 ... flex) от fmessage-карточек —
        // $(".fmessages").remove() убивает только внутренний .fmessages div,
        // а внешний враппер остаётся и выглядит как пустая серая карточка.
        $('#everychat').empty();
        $(".message").remove();
        $(".forummsg").remove();
        $(".fmessages").remove();
    }
    if (!window.currentthreadisforum) {
        window.down = true;
    }
    window.firstload = true;
    window.disablescroll = true;
    window.messages = [];
    window.oldmessages = [];
    window.currentOffset = 0;
    window.scrolldirectiondown = true;
    console.log('co3');
    window.currentlyLoadingHistory = false;
    window.lastUpdate = 0;
    window.await = false;
}
initChat();
function adjustChatboxHeight() {
    return;
    console.log("chat height recalculated.")
    window.adjustChatWait = true;
    setTimeout(() => {
        $('#chatinput').trigger("input")
        const chatInput = document.getElementById('chatinput');
        const chatbox = document.querySelector('.chatbox');
        const container = document.querySelector('.chat-area') || chatbox.parentElement;
        const containerHeight = container.clientHeight;
        const inputHeight = chatInput.offsetHeight;
        chatbox.style.height = (containerHeight - inputHeight - 8) + 'px';
        window.adjustChatWait = false;
    }, 100);
}

function getme() {
    $.ajax({
        url: '/api/profile/getMe',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {},
        dataType: 'json'
    }).done(res => {
        if (res.success) {
            window.currentUser = res.result;
            $('#profhead').text(res.result.name);
            window.await = true;
            getMessages(window.currentthread);
            setInterval(() => {
                if (window.currentthread != 7 && window.currentthread != 1006 && window.currentthread != 8) {
                    if (window.currentOffset == 0 && !window.await) {
                        window.await = true;
                        getMessages(window.currentthread);
                    }
                }
            }, 500);

        }
    });
    $.ajax({
        url: '/api/user/getMe',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        success: function (data) {
            window.currentUserInfo = JSON.parse(data);
            getThreadInfo();
        }
    })
}
getme();

function escapeHtml(unsafe) {
    return unsafe
        .replaceAll(/&/g, "&amp;")
        .replaceAll(/</g, "&lt;")
        .replaceAll(/>/g, "&gt;")
        .replaceAll(/"/g, "&quot;")
        .replaceAll(/'/g, "&#039;");
}

function getThreadInfo() {
    $.ajax({
        url: '/api/threads/info',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            thread: window.currentthread ? window.currentthread : 1,
        },
        dataType: 'json'
    }).done(res => {
        if (res.success) {

            window.currentThreadInfo = res.result;
            if (window.currentthreadisforum) {
                // доточим URL до полного вида, если openthread был вызван без firstMessageId
                setThreadUrl(window.currentthread, res.result.first_message_id, true);
                writethemetags();
                syncFirstForumMessageStyle();
            }
            if (window.currentThreadInfo.messages_count > 30) {
                // $('.chatbox').append(`
                //     <div style="position: absolute; bottom: 50; left: 48%; transform: translateX(50%); display: flex; background: rgba(173, 173, 173, 0.17);"> 
                //         <div style="padding: 5px; cursor: pointer;"><</div>
                //         <div style="padding: 5px; cursor: pointer;">1</div>
                //         <div style="padding: 5px; cursor: pointer;">2</div>
                //         <div style="padding: 5px; cursor: pointer;">3</div>
                //         <div style="padding: 5px; cursor: pointer;">></div>
                //     </div>
                //     `)
            }
            $('#everychat').show();
            $('#no-permission').css('display', 'none');
            $('#chatinput').prop('disabled', false);
            console.log(res.result.writeRole);
            $('#chatinput').attr('placeholder', 'message');
            $('#sendmsgbutton').attr('onclick', 'sendmessage(window.currentthread);');
            if (window.currentUserInfo.mail == null && window.currentUser.role == 0) {
                $('#chatinput').attr('placeholder', 'Need to verify email');
                $('#chatinput').prop('disabled', true);
                $('#sendmsgbutton').attr('onclick', 'return false;');
                return;
            }
            if (res.result.writeRole > window.currentUser.role) {
                console.log('You do not have permission to write in this thread');
                $('#chatinput').attr('placeholder', 'No permission to write in this thread');
                $('#chatinput').prop('disabled', true);

                $('#sendmsgbutton').attr('onclick', 'return false;');
            }
            if (window.currentThreadInfo.first_author_user == window.currentUser.id || window.currentUser.role > 9) {
                $('.dropdown').show();
                $('.nodropdown').hide();
                // Hide "change tags" for Information threads
                if (isInformationThread()) {
                    $('.dropdown-item').filter(function () {
                        return $(this).text().trim() === 'change tags';
                    }).hide();
                } else {
                    $('.dropdown-item').filter(function () {
                        return $(this).text().trim() === 'change tags';
                    }).show();
                }
            } else {
                $('.dropdown').hide();
                $('.nodropdown').show();
            }
        } else {
            $('#chatinput').val('');
            $('.chatinput-container').hide();
            $('#no-permission').css('display', 'flex');
            $('#everychat').hide();
        }
    });

}

function checknews() {

}

window.thismessages = []
defthread = window.currentthread;

// Функция рендера отфильтрованных тредов для раздела 7
function renderFilteredThreads() {
    const chatContainer = document.querySelector('#everychat');
    if (!chatContainer) return;

    chatContainer.innerHTML = ''; // Очищаем контейнер

    // Фильтруем треды на основе выбранных тегов (window.selectedTags — массив)
    let filteredThreads = [];

    if (window.threads && window.threads.result) {
        filteredThreads = window.threads.result.filter(thread => {
            // Всегда только форум 1
            if (thread.forum !== 1) return false;

            // Если выбран только "All" (0) или массив содержит 0 — показываем все
            if (window.selectedTags.includes(0)) {
                return true;
            }

            // Если ничего не выбрано — ничего не показываем (но по логике это не должно происходить)
            if (window.selectedTags.length === 0) {
                return false;
            }

            // Парсим теги треда
            const threadTags = thread.tags
                ? thread.tags.split(',').map(id => parseInt(id.trim())).filter(id => !isNaN(id))
                : [];

            // Показываем, если у треда есть хотя бы один из выбранных тегов
            return window.selectedTags.every(selectedId => threadTags.includes(selectedId));
        });
    }

    // Сортируем так, чтобы темы с тегом 111 шли первыми, сохраняя относительный порядок внутри групп
    const group110 = [];
    const group111 = [];
    const groupOther = [];

    filteredThreads.forEach(post => {
        const threadTags = post.tags
            ? post.tags.split(',').map(id => parseInt(id.trim())).filter(id => !isNaN(id))
            : [];

        if (threadTags.includes(110)) {
            group110.push(post);
        } else if (threadTags.includes(111)) {
            group111.push(post);
        } else {
            groupOther.push(post);
        }
    });

    // Сортируем группу 110 по id (по возрастанию)
    group110.sort((a, b) => (parseInt(a.id) || 0) - (parseInt(b.id) || 0));

    // Сортируем группу 111 по time_up (самые свежие первыми).
    // Если time_up отсутствует — используем first_message_time как запасной вариант.
    const timeValue = item => {
        const v = item.time_up ?? item.first_message_time ?? 0;
        return Number(v) || 0;
    };
    group111.sort((a, b) => timeValue(b) - timeValue(a)); // по убыванию времени

    // Всё, что после 111 — тоже сортируем по time_up (по убыванию, свежие первыми)
    groupOther.sort((a, b) => timeValue(b) - timeValue(a));

    filteredThreads = group110.concat(group111, groupOther);

    filteredThreads.forEach(post => {
        const postElement = document.createElement('div');
        postElement.className = 'bg-gray-800 p-4 rounded-lg shadow-md mb-4 flex';

        const avatarUrl = post.first_author_avatar || 'https://i.ibb.co/q3GCfzPm/besavatarochniy.png';
        const messageText = post.first_message_text
            ? (post.first_message_text.length > 100 ? post.first_message_text.substring(0, 100) + '...' : post.first_message_text)
            : '';
        const isOwner = window.currentUser && (
            post.first_author_user == window.currentUser.id ||
            post.first_author_id == window.currentUser.id ||
            post.first_author == window.currentUser.id
        );
        const bg = isOwner ? '#30303090' : '#3030305d';
        const hoverBg = isOwner ? '#303030b0' : '#30303080';
        // вместо opacity показываем серый замочек перед заголовком
        const opacityStyle = '';
        if (post.status != 0) {
            // вставляем HTML-safe заголовок с серым замком (не забудьте, что в шаблоне далее используется post.name)
            lockico = `<span style="color: red; margin-right:8px; align-items:center;"><svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" height="20px" width="20px" viewBox="0 0 640 640"><path d="M240 160L240 224L400 224L400 160C400 115.8 364.2 80 320 80C275.8 80 240 115.8 240 160zM192 224L192 160C192 89.3 249.3 32 320 32C390.7 32 448 89.3 448 160L448 224C483.3 224 512 252.7 512 288L512 512C512 547.3 483.3 576 448 576L192 576C156.7 576 128 547.3 128 512L128 288C128 252.7 156.7 224 192 224zM400 272L240 272L240 272L192 272L192 272C183.2 272 176 279.2 176 288L176 512C176 520.8 183.2 528 192 528L448 528C456.8 528 464 520.8 464 512L464 288C464 279.2 456.8 272 448 272L448 272L400 272L400 272z"/></svg></span>`;
        } else {
            lockico = "";
        }

        postElement.innerHTML = `
            <div class='fmessages' data-url="/panel/?thread=${post.id}&msg=${post.first_message_id}#community" style="cursor: pointer; display: flex; padding: 15px; margin: 10px 0; background: ${bg}; border: 1px color-mix(in srgb, var(--color1), transparent 75%) solid; border-radius: 3px; transition: border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;"
            onmouseover="this.style.background='${hoverBg}'; this.style.borderColor='color-mix(in srgb, var(--color1), transparent 45%)'; this.style.boxShadow='inset 0 0 12px color-mix(in srgb, var(--color1), transparent 65%)';"
            onmouseout="this.style.background='${bg}'; this.style.borderColor='color-mix(in srgb, var(--color1), transparent 75%)'; this.style.boxShadow='none';"
            onclick='openthread(${post.id},"${escapeHtml(post.name)}", ${post.first_message_id})'>
            <div style="margin-right: 15px;">
            <img src="${avatarUrl}" alt="${post.first_author_name}'s avatar"
             style="width: 60px; height: 60px; border-radius: 6px; object-fit: cover; border: 1px solid #444;">
            </div>
            <div style="flex: 1; display: flex; flex-direction: column;">
            <div style="display: flex; justify-content: space-between; align-items: center;">
            <div style="display: flex;"><div>${lockico}</div><h3 style="font-size: 20px; font-weight: bold; color: var(--color1); margin: 0;">${post.name}</h3></div>
            <span style="font-size: 12px; color: #aaa;">${new Date(post.first_message_time * 1000).toLocaleString()}</span>
            </div>
            <div style="font-size: 13px; color: #bbb; margin-top: 4px;">
            Autor: <span style="color: #ccc; font-weight: 500;">${post.first_author_name}</span>
            </div>
            <div style="font-size: 15px; color: #ddd; margin: 10px 0; line-height: 1.5;">
            ${messageText}
            </div>
            <div style="text-align: center; font-size: 14px; color: #999; border-top: 1px solid #333; padding-top: 10px; margin-top: auto;">
            <b>Messages: ${post.messages_count}</b>
            </div>
            </div>
            </div>
        `;

        chatContainer.appendChild(postElement);
    });

    // Обновляем активные классы для всех выбранных тегов (включая "All")
    $('.comtag').removeClass('comtag-active');
    window.selectedTags.forEach(id => {
        $(`.comtag[data-id="${id}"]`).addClass('comtag-active');
    });

    // Если мы только что вышли из темы — вернёмся туда, где были
    restoreForumScrollIfNeeded();
}

// Восстанавливает scrollTop списка тредов после выхода из темы.
// Учитывает то, что аватарки/превью догружаются и меняют высоту контейнера.
// ВАЖНО: не блокирует ручной скролл — как только пользователь сам прокрутил,
// мы прекращаем принудительно дёргать позицию.
function restoreForumScrollIfNeeded() {
    if (!window.__pendingRestoreForumScroll) return;
    if (typeof window.savedForumScroll !== 'number') return;
    const el = document.getElementById('everychat');
    if (!el) return;

    const target = window.savedForumScroll;
    window.__pendingRestoreForumScroll = false;
    window.savedForumScroll = null;

    let lastApplied = -1;
    let userTook = false;

    const apply = () => {
        if (userTook) return;
        const max = el.scrollHeight - el.clientHeight;
        const want = Math.max(0, Math.min(target, max));
        // если юзер уехал руками с нашей позиции — больше не трогаем
        if (lastApplied !== -1 && Math.abs(el.scrollTop - lastApplied) > 4) {
            userTook = true;
            cleanup();
            return;
        }
        el.scrollTop = want;
        lastApplied = want;
    };

    const onUserScroll = () => {
        if (lastApplied !== -1 && Math.abs(el.scrollTop - lastApplied) > 4) {
            userTook = true;
            cleanup();
        }
    };
    const onUserInput = () => { userTook = true; cleanup(); };

    let imgListeners = [];
    let iv = null;
    const cleanup = () => {
        if (iv) { clearInterval(iv); iv = null; }
        el.removeEventListener('wheel', onUserInput);
        el.removeEventListener('touchstart', onUserInput);
        el.removeEventListener('keydown', onUserInput);
        imgListeners.forEach(({ im, fn }) => {
            im.removeEventListener('load', fn);
            im.removeEventListener('error', fn);
        });
        imgListeners = [];
    };

    el.addEventListener('wheel', onUserInput, { once: true, passive: true });
    el.addEventListener('touchstart', onUserInput, { once: true, passive: true });
    el.addEventListener('keydown', onUserInput, { once: true });

    apply();
    requestAnimationFrame(apply);

    // Подвинуть позицию ещё раз когда догрузятся картинки превью
    const imgs = el.querySelectorAll('img');
    imgs.forEach(im => {
        if (!(im.complete && im.naturalHeight !== 0)) {
            const fn = () => { apply(); };
            im.addEventListener('load', fn, { once: true });
            im.addEventListener('error', fn, { once: true });
            imgListeners.push({ im, fn });
        }
    });

    // Короткая страховка — пара тиков пока вёрстка устаканится, и всё
    let ticks = 0;
    iv = setInterval(() => {
        apply();
        ticks++;
        if (userTook || ticks >= 6) cleanup();
    }, 80);
}

function getMessages() {
    var thread = window.currentthread;
    try { window.ajaxgetMessages.abort() } catch { }
    if (!$('.mainheader').is(':visible')) {
        return
    }
    if (thread == 7) {
        window.currentforum = 1;
        requrl = '/api/forum/threads';
        reqdata = { 'forum': window.currentforum };
        // Убираем передачу тега на сервер, фильтрация на фронте
    } else if (thread == 1006) {
        window.currentforum = 2;
        requrl = '/api/forum/threads';
        reqdata = { 'forum': window.currentforum }
    } else if (thread == 8) {
        window.currentforum = 3;
        requrl = '/api/forum/threads';
        reqdata = { 'forum': window.currentforum }
    } else {
        window.currentforum = 0;
        requrl = '/api/messages/get';
        reqdata = {
            thread: window.currentthread ? window.currentthread : 1,
            offset: window.currentOffset,
            time: window.lastUpdate,
            order: window.messagesorder
        }
    }
    console.error(reqdata);
    window.ajaxgetMessages = $.ajax({
        url: requrl,
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: reqdata,
        success: function (data) {
            let res;
            try {
                res = JSON.parse(data);
            } catch (e) {
                // Сервер прислал мусор вместо JSON — отпускаем await,
                // чтобы polling перезапустил запрос, иначе чат останется
                // пустым навсегда.
                console.error('getMessages: невалидный JSON в ответе', e, data);
                setTimeout(() => { window.await = false; }, 500);
                return;
            }
            if (!res || typeof res !== 'object') {
                console.error('getMessages: ожидался объект, пришло', res);
                setTimeout(() => { window.await = false; }, 500);
                return;
            }
            if (res.success == false) {
                if (res.result == "thread not found") {
                    createCookie('lastThread', 1, 365);
                    window.currentthread = 1;
                    setTimeout(() => {
                        getMessages();
                    }, 100);
                    return;
                }
            }
            if (window.currentthread != 7 && window.currentthread != 1006 && window.currentthread != 8) {
                if (window.currentthread == 4) {
                    window.lastMessageId = res.result[0].id;
                    createCookie('NewsLastMsgId', window.lastMessageId);
                }
                if (res.success) {
                    if (thread != window.currentthread) {
                        window.await = false;
                        return;
                    }
                    if (window.firstload) {
                        window.firstload = false;
                        window.__pendingInitialScroll = !window.currentthreadisforum;
                        if (res.result.length > 29) {
                            window.disablescroll = false;
                        }
                    }
                    if (res.result.length < 30) {
                        window.disablescroll = true;
                    }
                    window.lastUpdate = res.time;
                    window.currentlyLoadingHistory = false;
                    res.result = res.result.reverse();
                    var tempmsgs = []
                    JSON.parse(JSON.stringify(res.result)).forEach(item => {
                        tempmsgs[item.id] = item;
                    })
                    window.thismessages.forEach(item => {
                        tempmsgs[item.id] = item;
                    })

                    window.thismessages = Object.values(tempmsgs);
                    console.warn(window.thismessages);
                    if (currentthread != 7 || currentthread != 1006 || currentthread != 8) {
                        res.result.forEach(el => {
                            el.text = parseMessage(el.text);
                        });
                    }
                    if (window.currentOffset != 0) {
                        window.messagese = window.messages;
                        window.oldmessages = window.messages;
                        window.messages = window.messages.concat(res.result);
                        btm = everychat.scrollHeight - everychat.scrollTop - everychat.clientHeight;
                        renderMessages();
                        if (!window.scrolldirectiondown) {
                            everychat.scrollTop = everychat.scrollHeight - everychat.clientHeight - btm;
                        }
                        window.await = false;
                        return;
                    }
                    // Merge delta with existing messages instead of replacing —
                    // when sending `time: lastUpdate` the server returns only new
                    // messages, so replacing would wipe the visible history.
                    const __mergedMap = {};
                    window.messages.forEach(m => { __mergedMap[m.id] = m; });
                    res.result.forEach(m => { __mergedMap[m.id] = m; });
                    const __mergedArr = Object.values(__mergedMap).sort((a, b) => Number(a.id) - Number(b.id));

                    if (JSON.stringify(__mergedArr) != JSON.stringify(window.messages)) {
                        window.oldmessages = window.messages;
                        window.messages = __mergedArr;
                        window.messagese = window.messages;
                        const __everychatEl = document.getElementById('everychat');
                        const __doInitialPin = window.__pendingInitialScroll && __everychatEl;
                        if (__doInitialPin) {
                            __everychatEl.style.visibility = 'hidden';
                        }
                        renderMessages();
                        if (__doInitialPin) {
                            // Запоминаем scrollTop, на который мы только что
                            // поставили юзера. Дальше: пока он сидит в этой
                            // позиции (контент под ним подрастает из-за
                            // догружающихся картинок) — сдвигаем за ним.
                            // Стоило ему уехать вверх больше чем на 10px —
                            // считаем, что он взял управление на себя,
                            // больше не тащим.
                            let __pinnedScrollTop = 0;
                            let __userTookOver = false;
                            const pinBottom = () => {
                                if (__userTookOver) return;
                                if (Math.abs(__everychatEl.scrollTop - __pinnedScrollTop) > 10) {
                                    __userTookOver = true;
                                    return;
                                }
                                __everychatEl.scrollTop = __everychatEl.scrollHeight;
                                __pinnedScrollTop = __everychatEl.scrollTop;
                            };
                            pinBottom();
                            const imgs = __everychatEl.querySelectorAll('img');
                            imgs.forEach(im => {
                                if (!(im.complete && im.naturalHeight !== 0)) {
                                    const done = () => pinBottom();
                                    im.addEventListener('load', done, { once: true });
                                    im.addEventListener('error', done, { once: true });
                                }
                            });
                            requestAnimationFrame(() => {
                                pinBottom();
                                __everychatEl.style.visibility = '';
                            });
                            window.__pendingInitialScroll = false;
                        }
                        if (window.oldcurrentthread == 1006) {
                            setTimeout(() => {
                                everychat.scrollTop = everychat.scrollHeight;
                            }, 100);
                        }
                        if (window.down && window.currentthreadisforum == false) {
                            everychat.scrollTop = everychat.scrollHeight;
                            setTimeout(() => {
                                // За 500мс юзер мог уйти вверх. Пиним
                                // повторно только если он всё ещё у дна —
                                // иначе сразу утащим его обратно вниз.
                                if (window.down && window.currentthreadisforum == false) {
                                    everychat.scrollTop = everychat.scrollHeight;
                                }
                            }, 500);
                        }
                    }
                    window.await = false;
                } else {
                    setTimeout(() => {
                        window.await = false;
                    }, 3000);
                }
            } else {
                // Для форумов 7 и 1006 сохраняем данные и рендерим
                if (thread == 7) {
                    window.threads = res; // Сохраняем полный список тредов
                    renderFilteredThreads(); // Рендерим с учетом фильтра
                } else {
                    // Для 1006 рендерим как обычно
                    chatContainer = document.querySelector('#everychat');
                    // innerHTML='' вместо $(".fmessages").remove() — иначе при
                    // повторных рендерах 1006 в #everychat копятся пустые
                    // postElement-обёртки (внутренний .fmessages удаляется, а
                    // внешний bg-gray-800...flex остаётся).
                    chatContainer.innerHTML = '';
                    res.result.forEach(post => {
                        const postElement = document.createElement('div');
                        postElement.className = 'bg-gray-800 p-4 rounded-lg shadow-md mb-4 flex';

                        const avatarUrl = post.first_author_avatar || 'https://i.ibb.co/q3GCfzPm/besavatarochniy.png';
                        const messageText = post.first_message_text.length > 100
                            ? post.first_message_text.substring(0, 100) + '...'
                            : post.first_message_text;

                        // Замочек для закрытых тем — как в разделе 7, только для гайдов (thread == 8)
                        let lockico = "";
                        if (thread == 8 && post.status != 0) {
                            lockico = `<span style="color: red; margin-right:8px; align-items:center;"><svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" height="20px" width="20px" viewBox="0 0 640 640"><path d="M240 160L240 224L400 224L400 160C400 115.8 364.2 80 320 80C275.8 80 240 115.8 240 160zM192 224L192 160C192 89.3 249.3 32 320 32C390.7 32 448 89.3 448 160L448 224C483.3 224 512 252.7 512 288L512 512C512 547.3 483.3 576 448 576L192 576C156.7 576 128 547.3 128 512L128 288C128 252.7 156.7 224 192 224zM400 272L240 272L240 272L192 272L192 272C183.2 272 176 279.2 176 288L176 512C176 520.8 183.2 528 192 528L448 528C456.8 528 464 520.8 464 512L464 288C464 279.2 456.8 272 448 272L448 272L400 272L400 272z"/></svg></span>`;
                        }

                        postElement.innerHTML = `
                            <div class='fmessages' data-url="/panel/?thread=${post.id}&msg=${post.first_message_id}#community" style="cursor: pointer; display: flex; padding: 15px; margin: 10px 0; background: #3030305d; border: 1px color-mix(in srgb, var(--color1), transparent 75%) solid; border-radius: 3px; transition: border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;"
                                onmouseover="this.style.background='#30303080'; this.style.borderColor='color-mix(in srgb, var(--color1), transparent 45%)'; this.style.boxShadow='inset 0 0 12px color-mix(in srgb, var(--color1), transparent 65%)';"
                                onmouseout="this.style.background='#3030305d'; this.style.borderColor='color-mix(in srgb, var(--color1), transparent 75%)'; this.style.boxShadow='none';" onclick='openthread(${post.id},"${post.name.replace(/"/g, '&quot;')}", ${post.first_message_id})'>
                            <!-- остальной HTML как раньше -->
                            <div style="margin-right: 15px;">
                                <img src="${avatarUrl}" alt="${post.first_author_name}'s avatar"
                                    style="width: 60px; height: 60px; border-radius: 6px; object-fit: cover; border: 1px solid #444;">
                            </div>
                            <div style="flex: 1; display: flex; flex-direction: column;">
                                <div style="display: flex; justify-content: space-between; align-items: center;">
                                    <div style="display: flex;"><div>${lockico}</div><h3 style="font-size: 20px; font-weight: bold; color: var(--color1); margin: 0;">${post.name}</h3></div>
                                    <span style="font-size: 12px; color: #aaa;">${new Date(post.first_message_time * 1000).toLocaleString()}</span>
                                </div>
                                <div style="font-size: 13px; color: #bbb; margin-top: 4px;">
                                    Autor: <span style="color: #ccc; font-weight: 500;">${post.first_author_name}</span>
                                </div>
                                <div style="font-size: 15px; color: #ddd; margin: 10px 0; line-height: 1.5;">
                                    ${messageText}
                                </div>
                                <div style="text-align: center; font-size: 14px; color: #999; border-top: 1px solid #333; padding-top: 10px; margin-top: auto;">
                                    <b>Messages: ${post.messages_count}</b>
                                </div>
                            </div>
                        </div>
                        `;
                        chatContainer.appendChild(postElement);
                    });
                    restoreForumScrollIfNeeded();
                }
                // 1006/7/8 ветка не сбрасывала await — пока юзер сидит в
                // форумном списке, polling для 7/1006/8 скипается и залипания
                // не видно, но при переходе в реальный тред initChat делает
                // await=false, и если к этому моменту 1006-ответ ещё не
                // вернулся — он придёт позже и без сброса await залипнет.
                window.await = false;
            }
        },
        error: function (xhr, status, error) {
            // Если нас прервал следующий getMessages (polling перезапустил
            // запрос) — это нормальный сценарий, await выставит новый запрос.
            if (status === 'abort') {
                return;
            }
            console.log('getMessages error:', xhr.status, error, 'status:', status);
            // На любой реальной ошибке (таймаут, 5xx, 4xx, сеть) обязательно
            // отпускаем await, иначе polling залипнет и новые сообщения
            // никогда не подгрузятся. Пауза — чтобы не зациклить при
            // перманентной ошибке.
            setTimeout(() => {
                window.await = false;
            }, xhr.status === 502 ? 1000 : 500);
        }
    })
}


function gettegs() {
    // Сбрасываем выбор к дефолтному состоянию перед загрузкой тегов
    window.selectedTags = [0];

    $.ajax({
        url: '/api/forum/tags',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {},
        success: function (data) {
            try {
                const jres = JSON.parse(data);
                window.forumtags = jres.result;

                if (jres.success && Array.isArray(jres.result)) {
                    const $container = $('#comtags');
                    if ($container.length === 0) {
                        $('.marketlefeder').append('<div id="comtags" style="margin: 10px 0;"></div>');
                        $container = $('#comtags');
                    }
                    $container.empty();

                    // Функция обновления активных классов
                    function updateActiveClasses() {
                        $('.comtag').removeClass('comtag-active');
                        window.selectedTags.forEach(id => {
                            $(`.comtag[data-id="${id}"]`).addClass('comtag-active');
                        });
                    }

                    // Добавляем тег All
                    $container.append(`<span class="comtag" data-id="0">All</span>`);

                    // Добавляем остальные теги
                    jres.result.forEach(tag => {
                        if (tag.id != 111 && tag.id != 110) {
                            $container.append(`<span class="comtag" data-id="${tag.id}">${tag.name}</span>`);
                        }
                    });

                    // Сразу ставим активный класс на All (до обработчика)
                    updateActiveClasses();

                    // Обработчик клика
                    $container.off('click', '.comtag').on('click', '.comtag', function (e) {
                        e.preventDefault();
                        const tagId = parseInt($(this).data('id'));

                        if (tagId === 0) {
                            window.selectedTags = [0];
                        } else {
                            // Убираем All, если был
                            const allIndex = window.selectedTags.indexOf(0);
                            if (allIndex !== -1) {
                                window.selectedTags.splice(allIndex, 1);
                            }

                            // Тоггл текущего тега
                            const index = window.selectedTags.indexOf(tagId);
                            if (index === -1) {
                                window.selectedTags.push(tagId);
                            } else {
                                window.selectedTags.splice(index, 1);
                            }

                            // Если ничего не выбрано — возвращаем All
                            if (window.selectedTags.length === 0) {
                                window.selectedTags = [0];
                            }
                        }

                        updateActiveClasses();
                        renderFilteredThreads();
                    });

                    // Важно: сразу после загрузки тегов рендерим все треды (All выбран)
                    renderFilteredThreads();

                } else {
                    console.error('Ошибка: Неверный формат ответа или success=false', jres);
                }
            } catch (e) {
                console.error('Ошибка парсинга JSON:', e);
            }
        },
        error: function (xhr, status, error) {
            console.error('Ошибка запроса:', status, error);
        }
    });
}

window.currentthreadisforum = false;
window.oldcurrentthread = null;

function writethemetags() {
    const themetags = $('#themetags');
    themetags.empty();

    if (!window.currentThreadInfo) {
        return;
    }

    if (isGuidesThread() || isInformationThread()) {
        themetags.hide();
        return;
    }

    themetags.show();

    if (!window.currentThreadInfo.tags || !Array.isArray(window.forumtags)) {
        themetags.append('<span class="tag">No tags</span>');
        return;
    }

    const tags = window.currentThreadInfo.tags.split(',').map(id => parseInt(id.trim()));
    const tagslist = window.forumtags;

    if (Array.isArray(tags) && tags.length > 0 && JSON.stringify(tags) != "[null]") {
        console.error(JSON.stringify(tags));
        tags.forEach(tagId => {
            const tag = tagslist.find(t => t.id === tagId);
            if (tag) {
                const tagElement = `<span class="tag" data-id="${tag.id}">${tag.name}</span>`;
                themetags.append(tagElement);
            }
        });
    } else {
        themetags.append('<span class="tag">No tags</span>');
    }
}

function getCurrentThreadFirstMessageId() {
    if (!window.currentThreadInfo || window.currentThreadInfo.first_message_id == null) {
        return null;
    }

    const firstMessageId = Number(window.currentThreadInfo.first_message_id);
    return Number.isNaN(firstMessageId) ? null : firstMessageId;
}

function syncFirstForumMessageStyle() {
    if (!window.currentthreadisforum) {
        return;
    }

    const firstMessageId = getCurrentThreadFirstMessageId();
    if (firstMessageId == null) {
        return;
    }

    $('#everychat').children('[id^="message-"]').each(function () {
        const $message = $(this);
        const messageId = Number($message.data('id'));
        const isFirstMessage = messageId === firstMessageId;

        $message.addClass('forummsg');
        $message.toggleClass('first-forummsg', isFirstMessage);
        $message.toggleClass('message', !isFirstMessage);
    });
}

function openthread(thread, name, firstMessageId, backTarget) {
    // backTarget — куда возвращаться по «back». Если задан (прямая ссылка по URL),
    // используем его, иначе — текущий currentthread (случай клика по карточке из списка).
    window.oldcurrentthread = (backTarget != null) ? backTarget : window.currentthread;
    // remember where we were in the forum list so we can come back to the same spot
    try {
        const __ec = document.getElementById('everychat');
        if (__ec && !window.currentthreadisforum) {
            window.savedForumScroll = __ec.scrollTop;
        }
    } catch (e) { }
    window.currentthread = thread;
    window.currentThreadInfo = null;
    window.currentthreadisforum = true;
    // отражаем тред в адресной строке, чтобы ссылку можно было скопировать и переслать
    setThreadUrl(thread, firstMessageId, false);
    $('#headerthem').show();
    $('#comchat').addClass('comchat2');
    $('#themename').text(name);
    $("#guidescreatebutton").remove();
    initChat();
    setTimeout(firsmsginthem, 1000);
    window.messagesorder = "ASC";
    getThreadInfo();
    // Сразу дёргаем getMessages, не дожидаясь 500мс-polling — иначе
    // первая отрисовка реального треда приходит с задержкой, и всё это
    // время юзер видит хвост от 1006-вью (или пустой чат). Тот же
    // паттерн, что и в headselcom через setTimeout(getMessages, 200).
    setTimeout(getMessages, 200);
}

function closethread() {
    window.messagesorder = "DESC";
    $('#themetags').empty();
    $('#comchat').css('margin-top', '');
    $(".message").remove();
    $(".fmessages").remove();
    $('#comchat').removeClass('comchat2');
    if (window.currentthreadisforum) {
        $('#headerthem').hide();
        window.currentthread = window.oldcurrentthread;
        window.currentthreadisforum = false;
        // при выходе из треда адресная строка должна вернуться к «чистому» URL
        clearThreadUrl();
        // flag for the renderer: restore the previously saved forum scroll instead of pinning to bottom
        if (typeof window.savedForumScroll === 'number') {
            window.__pendingRestoreForumScroll = true;
        }
        initChat();
        getMessages();
    }
}

function firsmsginthem() {
    // $('.message').eq(0).css({'max-width': '100%', 'border-bottom': '5px solid var(--color1)','height':'200px','background':'none'});
}

function getOldMessage(thr, mesid) {
    window.currentthread = thr;
    window.down = false;
    window.currentOffset = -1
    $.ajax({
        url: '/api/messages/get',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            thread: window.currentthread,
            message: mesid
        },
        success: function (data) {
            jres = JSON.parse(data);
            if (jres.success) {
                console.log(jres);
                window.oldmsgshowed = false;
                try { window.ajaxgetMessages.abort() } catch { }
                window.oldmessages = window.messages;
                window.messages = jres.result;
                window.currentOffset = jres.offset;
                window.showoldmsg = false;
                window.down = false;
                setTimeout(() => { window.showoldmsg = true; window.currentOffset = jres.offset; }, 1000)
                console.error(jres.offset);
                if ($(".scroll-down-button").length == 0 && !window.currentthreadisforum) {
                    $('#everychat').append('<div class="scroll-down-button" onclick="scrollDown()">▽</div>');
                }
                renderMessages()
                setTimeout(() => {
                    container = document.querySelector('#everychat');
                    targetElement = container.querySelector('#message-' + mesid + '');
                    if (container && targetElement) {
                        // scrollIntoView + scroll-margin-top в style.css даёт зазор под фиксированный #headerthem
                        targetElement.scrollIntoView({ block: 'start', behavior: 'instant' });
                    }
                    if (!targetElement.classList.contains('first-forummsg')) {
                        targetElement.classList.add('msgsel');
                        setTimeout(() => {
                            targetElement.classList.remove('msgsel');
                        }, 600);
                    }
                }, 1000)
            }
        }
    })
}

function headselcom(el, t, thread) {
    // Если открыт DM, currentthread не отражает то, что реально видит пользователь
    // (он остался от прошлой вкладки/куки) — не считаем совпадение основанием для return.
    const inDmView = window.dmState && window.dmState.open;
    if (!inDmView && window.currentthread == thread) {
        return;
    }
    $("#guidescreatebutton").remove();
    window.oldcurrentthread = 0;
    window.messagesorder = "DESC";
    $('#headerthem').hide();
    $('#comchat').removeClass('comchat2');
    if (typeof closeDmView === 'function') {
        try { closeDmView(); } catch (e) { }
    }
    try { createCookie('lastDmUserId', '', -1); } catch (e) { }
    setTimeout(() => { $('#chatinput').focus(); }, 500);
    window.currentthread = thread;
    if (window.currentthread == 7) {
        window.ismarket = true;
        window.selectedTag = 0;
    } else {
        window.ismarket = false;
    }
    disableeditmode();
    createCookie('lastThread', thread, 365);
    getThreadInfo();
    window.currentthreadisforum = false;
    $(".comscripts").hide();
    $(".comchat").hide();
    $(".comprog").hide();
    $(".comfaq").hide();
    $(".comsuggest").hide();
    $(".combugs").hide();
    setTimeout(() => {
        $(el).eq(0).show();
        $(".headselectorbutton").removeClass("headselectorbuttonactive");
        $(t).addClass("headselectorbuttonactive");
    }, 100);
    initChat();
    setTimeout(getMessages, 200);
}

function scrollup(event) {
    if (event.wheelDelta) {
        return event.wheelDelta > 0;
    }
    return event.deltaY < 0;
}

function scrollDown() {
    window.showoldmsg = false;
    everychat.scrollTop = everychat.scrollHeight;
    $(".scroll-down-button").remove();
    window.currentOffset = 0;
}

$('#everychat').on("wheel", function (e) {
    if (scrollup(e.originalEvent)) {
        if ($(".scroll-down-button").length == 0 && !window.currentthreadisforum) {
            $('#everychat').append('<div class="scroll-down-button" onclick="scrollDown()">▽</div>');
        }
        console.log("SCROLLUP");
        window.down = false;
        window.scrolldirectiondown = false;
        if (everychat.scrollTop == 0) {
            e.preventDefault();
            e.stopPropagation();
        }
    } else {
        console.log("SCROLLDOWN");
        window.scrolldirectiondown = true;
        if ((everychat.scrollHeight - everychat.scrollTop <= everychat.clientHeight + 100) && window.currentOffset == 0) {
            window.down = true;
            if (window.showoldmsg) {
                return;
            }
            window.currentOffset = 0;
            console.log('co1');
            $(".scroll-down-button").remove();
        }
    }
});
$('#everychat').on("scroll", function (e) {
    if ((everychat.scrollHeight - everychat.scrollTop <= everychat.clientHeight + 100) && window.currentOffset == 0) {
        window.down = true;
        if (window.showoldmsg) {
            scrollHandler();
            return;
        }
        window.currentOffset = 0;
        console.log('co1');
        $(".scroll-down-button").remove();
    } else {
        // Юзер ушёл от дна (скроллбаром/тачем/клавиатурой — wheel-хэндлер
        // сам сбрасывает window.down, но эти события wheel не вызывают).
        // Без этого polling раз в 500мс пинит его обратно вниз.
        window.down = false;
    }
    scrollHandler();
    if (everychat.scrollTop == 0) {
        e.preventDefault();
        e.stopPropagation();
    }
});

function scrollHandler() {
    if (window.currentlyLoadingHistory) return;
    if (window.disablescroll && currentOffset == 0) { return; }
    if (!window.down && window.showoldmsg && (everychat.scrollHeight - everychat.scrollTop - everychat.clientHeight < 500)) {
        window.lastUpdate = 0;

        window.currentOffset -= 30;
        if (window.currentOffset < 0) { window.currentOffset = 0; return; }
        console.log('co2');
        getMessages();
        window.currentlyLoadingHistory = true;
    }
    if (window.disablescroll) { return; }
    if (everychat.scrollTop <= 500 && !window.currentthreadisforum) {
        window.lastUpdate = 0;
        window.currentOffset += 30;
        console.log('co2');
        getMessages();
        window.currentlyLoadingHistory = true;
    }

    if (window.currentthreadisforum) {
        if (everychat.scrollHeight - everychat.scrollTop <= everychat.clientHeight + 100) {
            window.lastUpdate = 0;
            window.currentOffset += 30;
            console.log('co2');
            getMessages();
            window.currentlyLoadingHistory = true;
        }
    }
}


function showrename(iconElement) {
    $('#prname').html(`
        <input style='width: calc(100% - 70px);' type="text" id="newname" placeholder="New name" value="${window.currentUser.name}">
    `);

    window.originalIcon = $(iconElement).clone();

    $(iconElement).replaceWith(`
        <svg id='savenameicon' class="check-icon" onclick="saveName(this)" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 512 512">
            <path d="M173.9 439.4l-166.4-166.4c-12.5-12.5-12.5-32.8 0-45.3l45.3-45.3c12.5-12.5 32.8-12.5 45.3 0L192 276.3 413.9 54.4c12.5-12.5 32.8-12.5 45.3 0l45.3 45.3c12.5 12.5 12.5 32.8 0 45.3L218.1 431.4c-12.5 12.5-32.8 12.5-45.3 0z"/>
        </svg>
    `);
    window.oldName = $('#newname').val();
    window.saveName = function (checkIcon) {
        const newName = $('#newname').val().trim();
        if (newName) {
            renameuser(newName);
            $(checkIcon).replaceWith(originalIcon);// Возвращаем иконку редактирования
        }
    };
}

function renameuser(newname) {
    $.ajax({
        url: '/api/profile/edit',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            field: 'name',
            value: newname
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                window.currentUser.name = newname; // Обновляем имя пользователя
                $('#prname').text(newname); // Обновляем отображаемое имя
            } else {
                notification('red', 'error', jdata.error || jdata.message || jdata.result)
                $('#prname').text(window.oldName); // Обновляем отображаемое имя
            }
        }

    });

}

function showrebio(iconElement) {
    $('#prbio').html(`
        <textarea class='textbox1' style='resize: none; overflow: scroll; margin-top: -10px; width: calc(100% - 70px); height: calc(100% - 20px); text-align: center;' id="newbio" placeholder="New bio">${escapeHtml(window.currentUser.bio)}</textarea>
    `);

    // Сохраняем оригинальную иконку редактирования
    window.originalIcon = $(iconElement).clone();

    // Заменяем иконку редактирования на галочку
    $(iconElement).replaceWith(`
        <svg id='savebioicon' class="check-icon" onclick="saveBio(this)" style="width: 30px; flex-shrink: 0; margin-right: 10px; position: absolute; right: 0; top: 10%; transform: translateY(-10%); pointer-events: auto; color: azure;" onclick="" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 512 512">
            <path d="M173.9 439.4l-166.4-166.4c-12.5-12.5-12.5-32.8 0-45.3l45.3-45.3c12.5-12.5 32.8-12.5 45.3 0L192 276.3 413.9 54.4c12.5-12.5 32.8-12.5 45.3 0l45.3 45.3c12.5 12.5 12.5 32.8 0 45.3L218.1 431.4c-12.5 12.5-32.8 12.5-45.3 0z"/>
        </svg>
    `);
    window.oldbio = $('#prbio').text();
    window.saveBio = function (checkIcon) {
        rebiouser = $('#newbio').val().trim();
        if (rebiouser) {
            rebiouser2(rebiouser);
            $(checkIcon).replaceWith(originalIcon);
        }
    };
}

function tsToDateStr(timestamp) {
    return new Intl.DateTimeFormat(navigator.language || 'en-US', {
        year: 'numeric',
        month: 'numeric',
        day: 'numeric'
    }).format(new Date(timestamp) * 1000);
}

function rebiouser2(newbio) {
    $.ajax({
        url: '/api/profile/edit',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            field: 'bio',
            value: newbio
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                window.currentUser.bio = rebiouser; // Обновляем имя пользователя
                $('#prbio').text(escapeHtml(rebiouser)); // Обновляем отображаемое имя
            } else {
                notification('red', 'error', jdata.error || jdata.message || jdata.result)
                $('#prbio').text(escapeHtml(window.oldbio)); // Обновляем отображаемое имя
            }
        }
    });

}

function isNumber(value) {
    return typeof value === 'number' && isFinite(value);
}


function openminiprofile(id, name) {
    window.selecteduserid = id;
    if ($('#savebioicon').length > 0) {
        try { $('#savebioicon').replaceWith(window.originalIcon); } catch { }
    }
    if ($('#savenameicon').length > 0) {
        try { $('#savenameicon').replaceWith(window.originalIcon); } catch { }
    }

    window.currentUser.avatar = window.currentUser.avatar || "https://i.ibb.co/q3GCfzPm/besavatarochniy.png";
    if (id == 'my' || id == window.currentUser.id) {
        role = window.currentUser.role;
        if (window.currentUser.bio == "") {
            bio = "No bio";
        } else {
            bio = window.currentUser.bio;
        }
        switch (role) {
            case 1:
                linerole = "CORE";
                break;
            case 2:
                linerole = "PRO";
                break;
            case 3:
                linerole = "PRO";
                break;
            case 11:
                linerole = "Administrator";
                break;
            default:
                linerole = "Newbie";
                break;
        }
        window.currentUser.created

        if (window.currentUser.deposit == 0) {
            dep = "no deposit";
            $('#prdep').css('color', 'red');
        } else {
            dep = "$ " + window.currentUser.deposit;
            $('#prdep').css('color', 'lime');
        }
        try {
            window.bantimeRaw = window.currentUser.bantime * 1000;
            nowMs = Date.now();

            let displayNameHtml = escapeHtml(window.currentUser.name);

            if (bantimeRaw > nowMs) {
                displayNameHtml = `<s>${displayNameHtml}</s>`;
            }

            setTimeout(() => {
                $('#prname').html(displayNameHtml);
            }, 10);
        } catch (e) {
            console.error('Error processing ban display:', e);
        }
        $('#prava').attr('src', window.currentUser.avatar);
        $('#prbio').html("<div>" + escapeHtml(bio) + "</div>");
        $('#prcreated').text(tsToDateStr(window.currentUser.created));
        $('#prrole').text(linerole);
        $('#prdep').text(dep);
        $('#comprofilepop, .backblur').show();
        $('#prava').attr('onclick', "changeAvatar()");
        $('.edit-icon').show();
        // Own profile — hide the mini toolbar (DM + ban history).
        $('#prminitools').hide();
    } else {
        const data = {};
        if (id !== undefined && id !== null) {
            data.id = id;
        } else if (name !== undefined && name !== null) {
            data.name = name;
        } else {

        }
        $.ajax({
            url: '/api/profile/get',
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
            },
            data: data,
            dataType: 'json'
        }).done(res => {
            role = res.result.role;
            deposit = res.result.deposit;
            if (res.result.bio == "") {
                bio = "No bio";
            } else {
                bio = res.result.bio;
            }
            switch (role) {
                case 1:
                    linerole = "CORE";
                    break;
                case 2:
                    linerole = "PRO";
                    break;
                case 3:
                    linerole = "PRO";
                    break;
                case 11:
                    linerole = "Administrator";
                    break;
                default:
                    linerole = "Newbie";
                    break;
            }
            if (deposit == 0) {
                dep = "no deposit";
                $('#prdep').css('color', 'red');
            } else {
                dep = "$ " + deposit;
                $('#prdep').css('color', 'lime');
            }

            // Подготовим отображаемое имя с зачёркиванием если bantime > текущего времени


            if (res.success) {
                try {
                    window.bantimeRaw = res.result && res.result.bantime * 1000;
                    nowMs = Date.now();

                    let displayNameHtml = escapeHtml(res.result.name);

                    if (bantimeRaw > nowMs) {
                        displayNameHtml = `<s>${displayNameHtml}</s>`;
                    }

                    setTimeout(() => {
                        if (res && res.success) {
                            $('#prname').html(displayNameHtml);
                        }
                    }, 10);
                } catch (e) {
                    console.error('Error processing ban display:', e);
                }
                $('#prava').attr('src', res.result.avatar || "https://i.ibb.co/q3GCfzPm/besavatarochniy.png");
                $('#prbio').html("<div>" + escapeHtml(bio) + "</div>");
                $('#prcreated').text(tsToDateStr(res.result.created));
                $('#prrole').text(linerole);
                $('#prdep').text(dep);
                $('#prava').attr('onclick', "");
                $('.edit-icon').hide();

                // Show the mini toolbar (DM + ban history) when viewing
                // someone else's profile. The ban history icon only appears
                // when the API actually returned a non-empty banhistory.
                $('#prminitools').show();
                if (res.result.banhistory != null && String(res.result.banhistory).length) {
                    $('#prbanhistoryMini').show();
                } else {
                    $('#prbanhistoryMini').hide();
                }

                $('#comprofilepop, .backblur').show();
                window.selecteduser = res.result.id;
                window.selecteduserinfo = res.result;
            } else {
                // $('#prname').text('User not found');
                // $('#prava').attr('src', 'https://i.ibb.co/q3GCfzPm/besavatarochniy.png');
                // $('#prbio').text('User not found');
                // $('#prcreated').text('User not found');
                // $('#prrole').text('User not found');
                // $('#prava').attr('onclick', "");
                // $('.edit-icon').hide();
                notification('red', 'error', 'user not found')
            }
            if (window.currentUser.role >= 10) {
                $('#prcreated').attr('onclick', "$('#banmenu, .backblur').show();");
            }
        });
    }
}

// Render the selected user's ban history into the popup. The data shape
// varies server-side, so we handle the common cases: null, string, array
// of objects, single object.
function openBanHistory() {
    const info = window.selecteduserinfo || {};
    const $content = $('#banhistorycontent');
    $content.empty();

    // Title with the user's name (escaped)
    const name = info.name ? escapeHtml(info.name) : ('User ' + (info.id || ''));
    $('#banhistorytitle').html('Ban history: <span style="color:#888;">' + name + '</span>');

    const list = normalizeBanList(info.banhistory);
    if (list.length === 0) {
        $content.append(emptyBansHtml());
    } else {
        // Newest first.
        list.sort((a, b) => (b._ts || 0) - (a._ts || 0));

        const wrap = document.createElement('div');
        wrap.style.cssText = 'padding: 14px; display: flex; flex-direction: column; gap: 10px;';
        list.forEach((entry, i) => {
            wrap.appendChild(buildBanCard(entry, list.length - i));
        });
        $content.append(wrap);
    }

    $('#banhistorypopup, .backblur').show();
}

// Close the ban history popup, but only hide the shared backdrop if no
// other main popup is still on screen — otherwise we'd kill the dim/blur
// for, say, the profile popup sitting behind us.
function closeBanHistory() {
    $('#banhistorypopup').hide();
    if ($('.mainpopup:visible').length === 0) {
        $('.backblur').hide();
    }
}

$(document).on('keydown', function (e) {
    if (e.key === 'Escape' && $('#banhistorypopup').is(':visible')) {
        e.stopPropagation();
        closeBanHistory();
    }
});

// Coerce the various banhistory shapes the server might hand us into a
// uniform array of plain objects with the fields we care about lifted to
// the top level (reason, by, until, duration, ...).
function normalizeBanList(bh) {
    if (bh == null) return [];
    if (typeof bh === 'string') {
        // Try to parse JSON; otherwise treat as a single free-form note.
        try { bh = JSON.parse(bh); } catch { return [normalizeBanEntry({ note: bh })]; }
    }
    if (Array.isArray(bh)) {
        const out = [];
        bh.forEach(e => { const n = normalizeBanEntry(e); if (n) out.push(n); });
        return out;
    }
    if (typeof bh === 'object') {
        const n = normalizeBanEntry(bh);
        return n ? [n] : [];
    }
    return [];
}

function normalizeBanEntry(entry) {
    if (entry == null) return null;
    if (typeof entry === 'string') {
        try { entry = JSON.parse(entry); } catch { entry = { reason: entry }; }
    }
    if (typeof entry !== 'object') {
        return { reason: String(entry) };
    }

    const ts = pickTimeField(entry);
    const from = pickField(entry, ['from', 'time', 'date', 'created', 'at', 'ts', 'timestamp', 'banned_at']);
    const until = pickField(entry, ['until', 'to', 'expire', 'expires', 'expire_at', 'expiretime']);
    const by = pickField(entry, ['by', 'admin', 'banned_by', 'moderator', 'mod', 'author']);
    const reason = pickField(entry, ['reason', 'reason_text', 'text', 'message', 'comment', 'note']);
    const duration = pickField(entry, ['duration', 'hours', 'time_hours', 'length']);

    return {
        ts: isLikelyTimestamp(ts) ? ts : (isLikelyTimestamp(from) ? from : null),
        from, until, by, reason, duration,
        _ts: isLikelyTimestamp(ts) ? ts : (isLikelyTimestamp(from) ? from : 0),
        raw: entry
    };
}

function emptyBansHtml() {
    return '<div style="padding: 80px 20px; text-align: center;">'
        + '<svg xmlns="http://www.w3.org/2000/svg" width="72" height="72" fill="#3a3a3a" viewBox="0 0 512 512" style="display:block;margin:0 auto 14px;">'
        + '<path d="M318.6 9.4c-12.5-12.5-32.8-12.5-45.3 0l-127 127c-12.5 12.5-12.5 32.8 0 45.3l16 16L8.4 380.2c-21.9 21.9-21.9 57.3 0 79.2l32 32c21.9 21.9 57.3 21.9 79.2 0L281 329.4l16 16c12.5 12.5 32.8 12.5 45.3 0l127-127c12.5-12.5 12.5-32.8 0-45.3l-16-16 152.7-152.7c21.9-21.9 21.9-57.3 0-79.2l-32-32c-21.9-21.9-57.3-21.9-79.2 0L342.6 25.4l-16-16c-12.5-12.5-32.8-12.5-45.3 0zM219.3 192l-50.4 50.4 152 152 50.4-50.4-152-152z"/>'
        + '</svg>'
        + '<div style="color: #aaa; font-size: 16px; font-weight: 500;">No bans on record</div>'
        + '<div style="color: #666; font-size: 12px; margin-top: 6px;">This user has a clean slate</div>'
        + '</div>';
}

// Render one ban record as a card with a status badge, italic reason
// quote, and a tidy meta grid for the rest of the well-known fields.
function buildBanCard(entry, index) {
    const isActive = isLikelyTimestamp(entry.until) && (entry.until * 1000) > Date.now();
    const accent = isActive ? '#e66' : '#5a5a5a';

    const card = document.createElement('div');
    card.style.cssText = 'background: linear-gradient(180deg, #1f1f1f 0%, #1a1a1a 100%);'
        + 'border: 1px solid #2a2a2a;'
        + 'border-left: 3px solid ' + accent + ';'
        + 'border-radius: 8px;'
        + 'padding: 14px 16px;'
        + 'box-shadow: 0 1px 0 rgba(255,255,255,0.02) inset;';

    // Header: "Ban #N" + status badge
    const header = document.createElement('div');
    header.style.cssText = 'display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;';

    const num = document.createElement('div');
    num.style.cssText = 'font-size: 12px; color: #888; letter-spacing: 0.5px; text-transform: uppercase; font-weight: 600;';
    num.textContent = 'Ban #' + index;
    header.appendChild(num);

    // Status badge — always rendered, even when there's no until field.
    let badgeLabel, badgeColor, badgeBg, badgeBorder;
    if (entry.until != null && isLikelyTimestamp(entry.until)) {
        if (isActive) {
            badgeLabel = '● Active';
            badgeColor = '#ff6b6b';
            badgeBg = 'rgba(238,102,102,0.18)';
            badgeBorder = 'rgba(238,102,102,0.5)';
        } else {
            badgeLabel = 'Expired';
            badgeColor = '#888';
            badgeBg = 'rgba(150,150,150,0.1)';
            badgeBorder = 'rgba(150,150,150,0.25)';
        }
    } else if (entry.until != null) {
        // Non-timestamp "until" — still treat as "set to expire later"
        badgeLabel = '● Active';
        badgeColor = '#ff6b6b';
        badgeBg = 'rgba(238,102,102,0.18)';
        badgeBorder = 'rgba(238,102,102,0.5)';
    } else {
        // No expiry info — permanent ban
        badgeColor = '#ffb84d00';
        badgeBg = 'rgba(255, 184, 77, 0)';
        badgeBorder = 'rgba(255, 184, 77, 0)';
    }
    const badge = document.createElement('span');
    badge.style.cssText = 'background: ' + badgeBg + '; color: ' + badgeColor + ';'
        + 'font-size: 11px; padding: 3px 10px; border-radius: 10px;'
        + 'border: 1px solid ' + badgeBorder + '; font-weight: 700;'
        + 'text-transform: uppercase; letter-spacing: 0.7px;';
    badge.textContent = badgeLabel;
    header.appendChild(badge);
    card.appendChild(header);

    // Reason — the main content, displayed as a quote
    if (entry.reason != null && String(entry.reason).length) {
        const reasonEl = document.createElement('div');
        reasonEl.style.cssText = 'color: #e6e6e6; font-size: 14px; line-height: 1.5;'
            + 'margin-bottom: 12px; padding: 10px 12px;'
            + 'background: rgba(255,255,255,0.025);'
            + 'border-left: 2px solid #444;'
            + 'border-radius: 0 4px 4px 0;'
            + 'font-style: italic;'
            + 'word-break: break-word;'
            + 'white-space: pre-wrap;';
        reasonEl.textContent = String(entry.reason);
        card.appendChild(reasonEl);
    } else {
        const noReason = document.createElement('div');
        noReason.style.cssText = 'color: #777; font-size: 13px; font-style: italic; margin-bottom: 12px; padding: 10px 12px; background: rgba(255,255,255,0.015); border-radius: 4px;';
        noReason.textContent = 'No reason given';
        card.appendChild(noReason);
    }

    // Meta grid: date / duration / banned by / expires
    const meta = document.createElement('div');
    meta.style.cssText = 'display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 8px 16px; font-size: 12px;';

    if (entry._ts) {
        const tsMs = entry._ts * 1000;
        meta.appendChild(metaRow('📅', 'Issued', relativeTime(tsMs, Date.now())));
    } else if (entry.from != null) {
        meta.appendChild(metaRow('📅', 'Issued', isLikelyTimestamp(entry.from) ? relativeTime(entry.from * 1000, Date.now()) : String(entry.from)));
    }

    if (entry.duration != null && String(entry.duration).length) {
        meta.appendChild(metaRow('⏱', 'Duration', humanDuration(entry.duration)));
    }

    if (entry.by != null && String(entry.by).length) {
        meta.appendChild(metaRow('👤', 'Banned by', String(entry.by)));
    }

    if (entry.until != null && String(entry.until).length) {
        if (isLikelyTimestamp(entry.until)) {
            const tsMs = entry.until * 1000;
            meta.appendChild(metaRow(isActive ? '⏳' : '⌛', isActive ? 'Expires' : 'Expired', relativeTime(tsMs, Date.now())));
        } else {
            meta.appendChild(metaRow('⌛', 'Until', String(entry.until)));
        }
    }

    if (meta.childElementCount > 0) {
        card.appendChild(meta);
    }

    return card;
}

function metaRow(icon, label, value) {
    const row = document.createElement('div');
    row.style.cssText = 'display: flex; align-items: baseline; gap: 6px; min-width: 0;';

    const ic = document.createElement('span');
    ic.textContent = icon;
    ic.style.opacity = '0.7';
    ic.style.flexShrink = '0';

    const lbl = document.createElement('span');
    lbl.style.cssText = 'color: #888; flex-shrink: 0;';
    lbl.textContent = label + ':';

    const val = document.createElement('span');
    val.style.cssText = 'color: #ddd; word-break: break-word; min-width: 0;';
    val.textContent = value;

    row.appendChild(ic);
    row.appendChild(lbl);
    row.appendChild(val);
    return row;
}

function pickField(obj, keys) {
    if (!obj || typeof obj !== 'object') return null;
    for (const k of keys) {
        for (const key of Object.keys(obj)) {
            if (key.toLowerCase() === k) {
                const v = obj[key];
                if (v != null && !(typeof v === 'string' && v.length === 0)) return v;
            }
        }
    }
    return null;
}

function pickTimeField(obj) {
    if (!obj || typeof obj !== 'object') return null;
    const keys = ['time', 'from', 'date', 'created', 'at', 'ts', 'timestamp', 'banned_at'];
    for (const k of keys) {
        for (const key of Object.keys(obj)) {
            if (key.toLowerCase() === k) {
                const v = obj[key];
                if (isLikelyTimestamp(v)) return v;
            }
        }
    }
    return null;
}

function isLikelyTimestamp(v) {
    return typeof v === 'number' && v > 1000000000 && v < 99999999999;
}

// "3 days ago" / "in 2 hours" — the kind of thing a human would actually
// say out loud, as opposed to "1717353600000 - Date.now()".
function relativeTime(targetMs, nowMs) {
    const diff = targetMs - nowMs;
    const abs = Math.abs(diff);
    const future = diff > 0;

    const minute = 60 * 1000;
    const hour = 60 * minute;
    const day = 24 * hour;
    const week = 7 * day;
    const month = 30 * day;
    const year = 365 * day;

    if (abs < minute) return future ? 'in a moment' : 'just now';
    if (abs < hour) {
        const v = Math.max(1, Math.round(abs / minute));
        return future ? 'in ' + v + ' min' : v + ' min ago';
    }
    if (abs < day) {
        const v = Math.max(1, Math.round(abs / hour));
        return future ? 'in ' + v + ' hour' + (v === 1 ? '' : 's') : v + ' hour' + (v === 1 ? '' : 's') + ' ago';
    }
    if (abs < week) {
        const v = Math.max(1, Math.round(abs / day));
        return future ? 'in ' + v + ' day' + (v === 1 ? '' : 's') : v + ' day' + (v === 1 ? '' : 's') + ' ago';
    }
    if (abs < month) {
        const v = Math.max(1, Math.round(abs / week));
        return future ? 'in ' + v + ' week' + (v === 1 ? '' : 's') : v + ' week' + (v === 1 ? '' : 's') + ' ago';
    }
    if (abs < year) {
        const v = Math.max(1, Math.round(abs / month));
        return future ? 'in ' + v + ' month' + (v === 1 ? '' : 's') : v + ' month' + (v === 1 ? '' : 's') + ' ago';
    }
    const v = Math.max(1, Math.round(abs / year));
    return future ? 'in ' + v + ' year' + (v === 1 ? '' : 's') : v + ' year' + (v === 1 ? '' : 's') + ' ago';
}

function humanDuration(v) {
    if (typeof v === 'string') return v;
    if (typeof v !== 'number') return String(v);

    // Sub-hour
    if (v > 0 && v < 1) {
        const minutes = Math.max(1, Math.round(v * 60));
        return minutes + ' min';
    }
    // Hours
    if (v < 24) {
        const h = Math.round(v);
        return h + ' hour' + (h === 1 ? '' : 's');
    }
    // Days + leftover hours
    const d = Math.floor(v / 24);
    const h = v % 24;
    if (h === 0) return d + ' day' + (d === 1 ? '' : 's');
    return d + ' day' + (d === 1 ? '' : 's') + ' ' + h + ' hour' + (h === 1 ? '' : 's');
}

function parseMessage(text) {
    // Картинки
    let imgstyle;
    if (window.currentforum) {
        imgstyle = 'max-width: 600px; max-height: 600px;';
    } else {
        imgstyle = 'max-width: 700px; max-height: 700px;';
    }
    text = text.replace(
        /https?:\/\/i\.imgur\.com\/([a-zA-Z0-9]+)\.(jpg|jpeg|png|gif|webp)/gi,
        '<img onclick="showbigpicture(this)" src="https://i.imgur.com/$1.$2" alt="Loading Image..." style="' + imgstyle + '">'
    );
    text = text.replace(
        /https?:\/\/i\.ibb\.co\/([a-zA-Z0-9]+)\/([a-zA-Z0-9]+)\.(jpg|jpeg|png|gif|webp)/gi,
        '<img onclick="showbigpicture(this)" src="https://i.ibb.co/$1/$2.$3" alt="Loading Image..." style="' + imgstyle + '">'
    );

    // Видео
    text = text.replace(
        /https?:\/\/i\.imgur\.com\/([a-zA-Z0-9]+)\.(mp4|webm)/gi,
        '<video controls style="max-width:400px; max-height:400px;"><source src="https://i.imgur.com/$1.$2" type="video/$2">Your browser does not support the video tag.</video>'
    );

    // Альбомы
    text = text.replace(
        /https?:\/\/(www\.)?imgur\.com\/(a|gallery)\/([a-zA-Z0-9]+)/gi,
        '<a href="https://imgur.com/$2/$3"> Imgur Album [$3]</a>'
    );



    return text;
}

function checkOverflow($message) {
    setTimeout(() => {
        const $text = $message.find('.message-text');
        const $expandBtn = $message.find('.expand-btn');

        if (!$text.length || !$expandBtn.length) return;

        // Сбрасываем стили для точного измерения полной высоты
        $text.css({
            'max-height': 'none',
            'overflow': 'visible',
            'height': 'auto',
            'display': 'block'
        });

        // Ждём полной загрузки всех изображений внутри текста
        const $images = $text.find('img');
        let imagesLoaded = 0;
        const totalImages = $images.length;

        function tryCheck() {
            imagesLoaded++;
            if (imagesLoaded >= totalImages) {
                performCheck();
            }
        }

        if (totalImages === 0) {
            performCheck();
        } else {
            $images.each(function () {
                if (this.complete && this.naturalHeight !== 0) {
                    tryCheck();
                } else {
                    $(this).one('load error', tryCheck);
                }
            });
        }

        function performCheck() {
            const lineHeight = parseFloat($text.css('line-height')) || 20;
            const maxLines = 20; // подбери нужное количество строк
            const maxHeight = lineHeight * maxLines;

            const realHeight = $text[0].scrollHeight;

            if (realHeight > maxHeight + 10) { // погрешность
                $text.css({
                    'max-height': maxHeight + 'px',
                    'overflow': 'hidden'
                });
                $expandBtn.show();
            } else {
                $text.css({
                    'max-height': 'none',
                    'overflow': 'visible'
                });
                $expandBtn.hide();
            }
        }
    }, 100);
}

function safetext(text) {
    safeText = text.replace(/\r?\n/g, "\\n");
    safeText = safeText.replace(/'/g, "\\'");
    return safeText;
}

function renderMessages() {
    const firstThreadMessageId = getCurrentThreadFirstMessageId();

    window.oldmessages.filter(i => !new Set(window.messages.map(ii => ii.id)).has(i.id)).forEach(oldmsg => {
        //console.log("REMOVING", oldmsg)
        $(`#message-${oldmsg.id}`).remove();
    })
    window.messages.filter(i => !new Set(window.oldmessages.map(ii => ii.id)).has(i.id)).forEach(msg => {

        if (msg.text.match(new RegExp(`\\B@${currentUser.name}\\b`))) {
            marketmsgstyle2 = 'border: 2px solid var(--color1)';
        } else {
            marketmsgstyle2 = '';
        }

        if (window.currentUser.id == msg.boss) {
            right = "message-sel";
        } else {
            right = "";
        }

        if (msg.status == 2) {
            statedit = '[EDITED]'
        } else {
            statedit = ''
        }

        if (window.currentthread == 7 || window.currentthread == 1006 || window.currentthread == 8) {
            marketmsgstyle = 'min-height: 90px'
            marketmsgtextstyle = 'font-size: 20px; '
            marketshowfeed = 'display: block;'
        } else {
            marketmsgstyle = ''
            marketmsgtextstyle = ''
            marketshowfeed = 'display: none;'
        }

        forumthread = '';
        if (window.currentthreadisforum) {
            forumthread = 'forummsg';
            if (firstThreadMessageId !== null && Number(msg.id) === firstThreadMessageId) {
                forumthread += ' first-forummsg';
            }
            if (window.lastmarket) {
                $('#themetags').show();
            }

        }

        classString = forumthread;
        if (!forumthread.includes('first-forummsg')) {
            classString += ' message';
        }
        classString += ' ' + right;

        //console.log("ADDING", msg)
        let currentMsgDiv = $(`
                <div class="${classString}" style='${marketmsgstyle, marketmsgstyle2}; display: flex; flex-direction: column; justify-content: space-between;' data-id=${msg.id} id="message-${msg.id}">
                    <div style="display: flex;">
                        <img onclick="openminiprofile(${msg.boss});" src="${msg.avatar || 'https://i.ibb.co/q3GCfzPm/besavatarochniy.png'}" class="message-avatar">
                        <div class='msgcont'>
                            <div class="message-content">
                                <div class="message-header">
                                    <span onclick="openminiprofile(${msg.boss});" class="message-username">${msg.name}</span>
                                    <span class="message-time">${new Date(msg.time * 1000).toLocaleString()}</span>
                                    <span class="message-status">${statedit}</span>
                                </div>
                                <span style='${marketmsgtextstyle}' class="message-text">${processMarkdown(msg.text, msg.role)}</span>
                                <div class="expand-btn" onclick="showexpandpop($(this).parent().children().eq(1).html(), '${msg.role}')" style="display: none;">
                                    <center><b>Expand Message</b></center>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="feedback-btn" style="${marketshowfeed}" onclick="openthread(${msg.id},${msg.subthread}, null); $('.backblur').show(); ">
                        Messages ${msg.messages_count}
                    </div>
                </div>
                `)
        let inserted = false;
        let newId = parseInt(msg.id);
        if (window.currentUser.id == msg.boss || window.currentUser.role >= 10) {
            currentMsgDiv.find('.message-header').append(`
                <div class='message-right'>
                            <svg class="delete-icon" onclick="editemessageshow(${msg.id})" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 448 512" style="cursor: pointer; margin-right: 10px; opacity: 0; transition: opacity 0.3s;">
                                <path d="M395.8 39.6c9.4-9.4 24.6-9.4 33.9 0l42.6 42.6c9.4 9.4 9.4 24.6 0 33.9L417.6 171 341 94.4l54.8-54.8zM318.4 117L395 193.6 159.6 428.9c-7.6 7.6-16.9 13.1-27.2 16.1L39.6 472.4l27.3-92.8c3-10.3 8.6-19.6 16.1-27.2L318.4 117zM452.4 17c-21.9-21.9-57.3-21.9-79.2 0L60.4 329.7c-11.4 11.4-19.7 25.4-24.2 40.8L.7 491.5c-1.7 5.6-.1 11.7 4 15.8s10.2 5.7 15.8 4l121-35.6c15.4-4.5 29.4-12.9 40.8-24.2L495 138.8c21.9-21.9 21.9-57.3 0-79.2L452.4 17z"/>
                                </svg>
                            <svg class="delete-icon" onclick="deletemessage(${msg.id})" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 448 512" style="cursor: pointer; margin-right: 10px; opacity: 0; transition: opacity 0.3s;">
                                <path d="M135.2 17.7C140.5 7.4 150.9 0 162.7 0H285.3C297.1 0 307.5 7.4 312.8 17.7L336 64H432C440.8 64 448 71.2 448 80V112C448 120.8 440.8 128 432 128H416V432C416 476.2 380.2 512 336 512H112C67.8 512 32 476.2 32 432V128H16C7.2 128 0 120.8 0 112V80C0 71.2 7.2 64 16 64H112L135.2 17.7zM80 128V432C80 449.7 94.3 464 112 464H336C353.7 464 368 449.7 368 432V128H80zM192 224C200.8 224 208 231.2 208 240V368C208 376.8 200.8 384 192 384C183.2 384 176 376.8 176 368V240C176 231.2 183.2 224 192 224zM272 224C280.8 224 288 231.2 288 240V368C288 376.8 280.8 384 272 384C263.2 384 256 376.8 256 368V240C256 231.2 263.2 224 272 224z"/>
                            </svg>
                </div>
                        `);

            currentMsgDiv.hover(
                function () {
                    $(this).find('.delete-icon, .reply-icon').css('opacity', '1');
                },
                function () {
                    $(this).find('.delete-icon, .reply-icon').css('opacity', '0');
                }
            );
        }

        currentMsgDiv.find('.message-header').each(function () {
            if (!$(this).find('.reply-icon').length) {
                const replyBtn = $(`<svg class="reply-icon" onclick="replyToMessage(${msg.id})" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 512 512" style="cursor: pointer; margin-right: 10px; opacity: 0; transition: opacity 0.3s;">
                    <path d="M205.7 14.3c3.4-3.4 8-5.3 12.8-5.3c9.9 0 18 8.1 18 18l0 56 0 37.5 37.5 0c109.6 0 198.4 88.8 198.4 198.4l0 9.8c0 22-6.9 43.5-19.7 61.4l-4.2 5.9c-7.3 10.2-21.6 11.5-30.6 2.5l-4.8-4.8c-27.3-27.3-63.9-42.8-102.3-43.7l-36.9-.8-37.5 0 0 37.5 0 56c0 9.9-8.1 18-18 18c-4.8 0-9.4-1.9-12.8-5.3L9.4 259.7C3.4 253.7 0 245.5 0 237s3.4-16.7 9.4-22.6L205.7 14.3z"/>
                </svg>`);
                if ($(this).find('.message-right').length) {
                    $(this).find('.message-right').prepend(replyBtn);
                } else {
                    const wrapper = $(`<div class='message-right'></div>`);
                    wrapper.append(replyBtn);
                    $(this).append(wrapper);
                }
            }
        });

        currentMsgDiv.hover(
            function () {
                $(this).find('.reply-icon, .delete-icon').css('opacity', '1');
            },
            function () {
                $(this).find('.reply-icon, .delete-icon').css('opacity', '0');
            }
        );

        if (!currentMsgDiv.hasClass('first-forummsg')) {
            checkOverflow(currentMsgDiv);
        }

        $('.message').each(function () {
            let existingId = parseInt($(this).data('id'));

            if (newId < existingId) {
                $(this).before(currentMsgDiv);
                inserted = true;
                return false;
            }
        });

        if (!inserted) {
            $('#everychat').append(currentMsgDiv);
        }
        if (window.currentthreadisforum) {
            // currentThreadInfo поднимается асинхронно в getThreadInfo;
            // если renderMessages успел вызваться раньше (гонка при открытии
            // прямой ссылки ?thread=X&msg=Y), пропускаем проверку депозита,
            // иначе forEach крашнется на первой же итерации и оставит в DOM
            // ровно одно (уже вставленное) сообщение.
            if (window.currentThreadInfo && window.currentThreadInfo.first_author_deposit == 0) {
                const $first = $('.first-forummsg');
                if ($first.length && $first.find('.no-deposit-warning').length === 0) {
                    $first.append('<div class="no-deposit-warning" style="box-shadow: inset 0 0 10px color-mix(in srgb, black, transparent 30%); margin-top: 5px; margin-bottom: 5px; padding: 5px; width: calc(100% - 10px); color: azure; text-align: center; cursor: pointer;"><h4><b>Warning!!! User has NO security deposit. Use a <u onclick="window.location.href = `/panel/?thread=1093&msg=13364#community`">guarantor</u>.</b></h4></div>');
                }
            }
        }
    })



    window.messages.forEach(msg => {
        window.oldmessages.filter(i => i.id == msg.id).forEach(oldmsg => {
            if (oldmsg.text != msg.text) {
                $(`#message-${msg.id} .message-text`).html(processMarkdown(msg.text, msg.role));
            }
            if (oldmsg.status != msg.status) {
                $(`#message-${msg.id} .message-header .message-status`).text('[EDITED]')
            }
        })
    });

    syncFirstForumMessageStyle();
}

function processMarkdown(text, role) {
    if (role > 10) {
        text = text
            .replace(/\[([^\]]*)\]\(([^)]*)\)/g, '<a style="color: var(--color1);" href="$2">$1</a>');
    }
    text = text
        .replace(/\*(.*?)\*/g, '<strong>$1</strong>')
        .replace(/~(.*?)~/g, '<del>$1</del>')
        .replace(/```([\s\S]*?)```/g, '<div style="position:relative; min-width: 35px;"><pre><code style="display:block;padding:8px;margin:1px 0;white-space:pre;overflow-x:auto;background-color:rgba(0,0,0,0.2);border-radius:3px;font-family:monospace;">$1</code></pre><button class="copy-button" onclick="copyToClipboard(this)" style="position:absolute;top:6px;right:6px;background:rgba(255,255,255,0.1);border:none;color:#fff;padding:2px 8px;border-radius:3px;cursor:pointer;font-size:12px;">Copy</button></div>')
        .replace(/# (.*?)\n/g, '<h3>$1</h3>\n')
        .replace(/(^@)([а-яА-ЯёЁa-zA-Z0-9_]+)/g, '<span class="mention" onclick="openminiprofile(null,`$2`)" style="color: var(--color1); cursor: pointer;">@$2</span>')
        .replace(/\s@([а-яА-ЯёЁa-zA-Z0-9_]+)/g, ' <span class="mention" onclick="openminiprofile(null,`$1`)" style="color: var(--color1); cursor: pointer;">@$1</span>')
        .replace(
            /(?<!["'])https?:\/\/[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]/gi,
            (url) => {
                const safeUrl = escapeHtml(url);
                return `<a style="color: var(--color1);" href="${safeUrl}" target="_blank" rel="noopener noreferrer">${safeUrl}</a>`;
            })
    return text;
}

function copyToClipboard(button) {
    const code = button.parentElement.querySelector('pre').textContent;
    navigator.clipboard.writeText(code).then(() => {
        button.textContent = 'Copied!';
        button.style.background = 'rgba(46, 160, 67, 0.4)';
        setTimeout(() => {
            button.textContent = 'Copy';
            button.style.background = 'rgba(255,255,255,0.1)';
        }, 2000);
    }).catch(() => {
        button.textContent = 'Error';
        button.style.background = 'rgba(248, 81, 73, 0.4)';
        setTimeout(() => {
            button.textContent = 'Copy';
            button.style.background = 'rgba(255,255,255,0.1)';
        }, 2000);
    });
}

function copyTextToClipboard(text) {
    if (text === undefined || text === null) return Promise.resolve(false);
    text = String(text);
    if (navigator.clipboard && navigator.clipboard.writeText) {
        return navigator.clipboard.writeText(text).then(() => {
            try { notification('lime', 'clipboard', 'copied'); } catch { }
            return true;
        }).catch(() => {
            return fallbackCopy(text);
        });
    } else {
        return Promise.resolve(fallbackCopy(text));
    }
}

function fallbackCopy(text) {
    try {
        const ta = document.createElement('textarea');
        ta.value = text;
        ta.setAttribute('readonly', '');
        ta.style.position = 'absolute';
        ta.style.left = '-9999px';
        document.body.appendChild(ta);

        const selected = document.getSelection();
        const range = document.createRange();
        range.selectNodeContents(ta);
        selected.removeAllRanges();
        selected.addRange(range);

        const ok = document.execCommand('copy');
        try { notification(ok ? 'lime' : 'red', 'clipboard', ok ? 'copied' : 'copy failed'); } catch { }
        selected.removeAllRanges();
        document.body.removeChild(ta);
        return !!ok;
    } catch (e) {
        console.error(e);
        try { notification('red', 'clipboard', 'copy error'); } catch { }
        return false;
    }
}

function sendmessage(thread) {
    if ($('#finputmessage').is(':visible')) {
        $input = $('#finputmessage');
    } else {
        $input = $('#chatinput');
    }
    let text = $input.val().trim();
    //var word = '';
    //var regex = new RegExp('\\b' + word + '\\b', 'i');
    var inputValue = text;
    if (inputValue.includes('[Image Uploading...]')) {
        console.error(inputValue);
        notification('red', 'chat', 'image loading');
        return
    }
    if (window.msginway == true) {
        notification('red', 'error', 'message in progress');
        return;
    }
    window.msginway = true;
    $.ajax({
        url: '/api/messages/send',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            text, thread
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                $('#chatinput').val('').css('height', '34px');
                window.msginway = false;
                $('#chatinput').trigger("input");
                getMessages();
                setTimeout(() => { everychat.scrollTop = everychat.scrollHeight; }, 300)
            } else {
                notification('red', 'error', jdata.message || jdata.result)
                window.msginway = false;
            }
        }
    })
}


$('#everychat, #chatinput, #finputmessage, #dmeverychat, #dmchatinput').on({
    dragover: function (e) {
        e.preventDefault();
        e.stopPropagation();
        e.originalEvent.dataTransfer.dropEffect = 'copy';
        $(this).css('background', '#2226');
    },
    dragleave: function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).css('background', '');
    },
    drop: function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).css('background', '');
        const file = e.originalEvent.dataTransfer.files[0];
        if (!file || !(file.type === 'image/jpeg' || file.type === 'image/png' || file.type === 'image/gif')) {
            //alert('Only images (JPEG, PNG или GIF)');
            return;
        }

        console.log('File (drop):', file, 'MIME Type:', file.type);
        processImage(file);
    },
    paste: function (e) {

        if ($('#finputmessage').is(':visible')) {
            $input = $('#finputmessage');
        } else if ($('#dmchatinput').is(':visible') && (this.id === 'dmchatinput' || this.id === 'dmeverychat')) {
            $input = $('#dmchatinput');
        } else {
            $input = $('#chatinput');
        }
        const items = (e.originalEvent || e).clipboardData.items;
        let file = null;

        // Проверяем, есть ли изображение в буфере обмена
        for (const item of items) {
            if (item.type === 'image/jpeg' || item.type === 'image/png' || item.type === 'image/gif') {
                file = item.getAsFile();
                break;
            }
        }

        if (file) {
            // Если есть изображение, обрабатываем его
            e.preventDefault();
            e.stopPropagation();
            console.log('File (paste):', file, 'MIME Type:', file.type);
            processImage(file);
        } else {
            console.log('File (paste):', file);
            setTimeout(() => {
                adjustChatboxHeight();
            }, 500);
        }
    }
});

// Helper function to process the image (shared between drop and paste)
function processImage(file) {
    if ($('#finputmessage').is(':visible')) {
        $input = $('#finputmessage');
    } else if ($('#dminputcontainer').is(':visible')) {
        $input = $('#dmchatinput');
    } else {
        $input = $('#chatinput');
    }
    const oldVal = $input.val();
    $input.val(oldVal + '[Image Uploading...]');
    window.riba = file; // Maintain existing global variable
    const reader = new FileReader();
    reader.onload = () => {
        console.log('Reader result:', reader.result);
        uploadImageToImgBB(reader.result.split(',')[1]);
    };
    reader.readAsDataURL(file);
}

async function uploadImageToImgBB(file) {
    const apiKey = '14da68ae1862e5b24c2736d589f8ceeb'; // Замените на ваш API-ключ ImgBB
    const url = 'https://api.imgbb.com/1/upload';
    console.log(file);
    // Создаем объект FormData для отправки файла
    const formData = new FormData();
    formData.append('image', file);
    formData.append('key', apiKey);

    try {
        const response = await fetch(url, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        if (data.success) {
            // console.log('Изображение успешно загружено:', data.data.url);
            if ($('#finputmessage').is(':visible')) {
                $input = $('#finputmessage');
            } else if ($('#dminputcontainer').is(':visible')) {
                $input = $('#dmchatinput');
            } else {
                $input = $('#chatinput');
            }
            const oldVal = $input.val();
            $input.val(oldVal.replace('[Image Uploading...]', `${data.data.url}`));

            return data.data.url; // Возвращаем URL загруженного изображения
        } else {
            throw new Error('Ошибка загрузки: ' + data.error.message);
        }
    } catch (error) {
        console.error('Ошибка при загрузке изображения:', error);
        return null;
    }
}

function changeAvatar() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.style.display = 'none';

    input.onchange = function (event) {
        const file = event.target.files[0];
        if (!file) return;
        if (!file.type.match(/^image\//)) {
            alert('Можно загружать только изображения');
            return;
        }
        const reader = new FileReader();
        reader.onload = async function () {
            const base64 = reader.result.split(',')[1];
            const apiKey = 'a5147d4ffd7b7b5450b1a87afe9a4f5d'; // ваш ключ imgbb
            const formData = new FormData();
            formData.append('image', base64);
            formData.append('key', apiKey);

            try {
                const response = await fetch('https://api.imgbb.com/1/upload', {
                    method: 'POST',
                    body: formData
                });
                const data = await response.json();
                if (data.success) {
                    // Обновить аватарку на сервере (пример)
                    $.ajax({
                        url: '/api/profile/edit',
                        method: 'POST',
                        headers: {
                            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
                        },
                        data: {
                            field: 'avatar',
                            value: data.data.url
                        }
                    }).done(() => {
                        $('#prava').attr('src', data.data.url);
                        window.currentUser.avatar = data.data.url;
                    });
                } else {
                    alert('Ошибка загрузки: ' + data.error.message);
                }
            } catch (e) {
                console.error(e);
            }
        };
        reader.readAsDataURL(file);
    };

    document.body.appendChild(input);
    input.click();
    setTimeout(() => document.body.removeChild(input), 1000);
}


function deletemessage(id, thread = window.currentthread) {
    $.ajax({
        url: '/api/messages/delete',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: { id, thread }
    }).done(() => {
        $(`#message-${id}`).remove();
    }).fail((jqXHR, textStatus, errorThrown) => {
        console.error("Error deleting message:", textStatus, errorThrown);
    });
}

scale = 1;
scaleStep = 0.2;
minScale = 0.5;
maxScale = 3;

function showbigpicture(e) {
    let scale = 1;
    let dragging = false;
    let startX = 0, startY = 0, imgX = 0, imgY = 0;
    let overlayClicked = false;

    const $overlay = $('.fullscreen-overlay');
    const $image = $('.fullscreen-image');
    const $close = $('.close-btn');
    const src = $(e).attr('src');

    // Устанавливаем картинку в начальное состояние (невидимую)
    $image
        .attr('src', src)
        .attr('draggable', false)
        .off('dragstart.sbp').on('dragstart.sbp', ev => ev.preventDefault())
        .removeClass('visible') // сброс анимации
        .addClass('zoomed')
        .css({ '--scale': 1, '--x': '0px', '--y': '0px' });

    // Показ оверлея
    $overlay.fadeIn(300, () => {
        $overlay.css('display', 'grid');
        // небольшой timeout нужен, чтобы CSS-анимация сработала
        requestAnimationFrame(() => $image.addClass('visible'));
    });

    // === Закрытие ===
    $close.off('.sbp').on('click.sbp', closeOverlay);

    $overlay.off('.sbp')
        .on('mousedown.sbp', function (ev) {
            overlayClicked = ev.target === this;
        })
        .on('mouseup.sbp', function (ev) {
            if (overlayClicked && ev.target === this) closeOverlay();
            overlayClicked = false;
        });

    $image.off('.sbpClick').on('click.sbpClick', ev => ev.stopPropagation());

    // === Перетаскивание ===
    $image.off('mousedown.sbpDrag').on('mousedown.sbpDrag', ev => {
        if (ev.button !== 0) return;
        dragging = true;
        startX = ev.clientX - imgX;
        startY = ev.clientY - imgY;
        $('body').addClass('no-select');

        $(document)
            .off('.sbpDrag')
            .on('mousemove.sbpDrag', onMove)
            .on('mouseup.sbpDrag', endDrag);
    });

    function onMove(ev) {
        if (!dragging) return;
        imgX = ev.clientX - startX;
        imgY = ev.clientY - startY;
        $image.css({ '--x': imgX + 'px', '--y': imgY + 'px' });
    }

    function endDrag() {
        if (!dragging) return;
        dragging = false;
        $('body').removeClass('no-select');
        $(document).off('.sbpDrag');
    }

    function closeOverlay() {
        endDrag();
        $image.removeClass('visible'); // плавно исчезает
        $overlay.fadeOut(300, () => {
            $image.removeClass('zoomed').css({ '--scale': 1, '--x': '0px', '--y': '0px' });
            imgX = imgY = 0;
            scale = 1;
        });
    }
}






function disableeditmode() {
    $('#chatinput').val('');
    $('#chatinput').trigger("input");
    $('#chatinput')[0].style.height = '34px';
    $('#sendmsgbutton').html('Send');
    $('#sendmsgbutton').attr('onclick', 'sendmessage(window.currentthread)');
}



function editemessageshow(id) {
    window.lasteditmsgid = id;
    message = window.thismessages.find(msg => msg.id === id);
    window.lasteditmsg = message.text;
    $('#chatinput').val(message.text);
    $('#sendmsgbutton').html('Edit');
    $('#sendmsgbutton').attr('onclick', 'editemessage(' + id + ')');
    $('#chatinput')[0].style.height = ($('#chatinput')[0].scrollHeight) + 'px';
    $('#chatinput').trigger("input");
    if (window.adjustChatWait == false) {
        adjustChatboxHeight();
    }
}

function editemessage(message, thread = window.currentthread) {
    text = $('#chatinput').val();
    if (window.lasteditmsg == text) {
        disableeditmode();
        return;
    }
    msgid = window.lasteditmsgid;
    massage2 = window.thismessages.find(msg => msg.id === msgid);
    if (massage2) {
        massage2.text = text;
    }
    $.ajax({
        url: '/api/messages/edit',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: { message, thread, text }
    }).done(() => {
        $('#chatinput').val('');
        $('#chatinput').trigger("input");
        setTimeout(() => {
            disableeditmode();
        }, 500)
    }).fail((jqXHR, textStatus, errorThrown) => {
        console.error("Error deleting message:", textStatus, errorThrown);
    });
}

function showexpandpop(text, role) {
    const $content = $(event.target).closest('.message-content');
    const $text = $content.find('.message-text');
    const $btn = $content.find('.expand-btn');

    $text.css({
        'max-height': 'none',
        'overflow': 'visible'
    });
    $btn.hide();
}

function showbanmenu() {

}

function communityban(id, reason, clean, hours) {
    $.ajax({
        url: '/api/profile/ban',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            id: id,
            reason: reason,
            clean: clean,
            hours: hours
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                notification('lime', 'user control', 'ban success');
            } else {
                notification('lime', 'user control', 'no permission');
            }
        }

    });
}


function addthread(title, text, forum) {
    let tags = '';

    if (parseInt(forum) !== 8 && parseInt(forum) !== 1006) {
        // Собираем все выбранные теги: берём либо id элемента, либо data-id, и если id вида "tag-<num>" — извлекаем число
        tags = $('.selectedtags').map((_, el) => {
            const $el = $(el);
            let id = $el.attr('id') || $el.attr('data-id') || $el.data('id') || '';
            if (!id) return '';
            if (typeof id === 'string' && id.startsWith('tag-')) return id.substring(4);
            return id.toString();
        }).get().filter(Boolean).join(',');
    } else {
        window.threadSelectedTags = [];
        renderSelectedTags();
    }

    switch (parseInt(forum)) {
        case 7:
            forum = 1;
            break;

        case 8:
            forum = 3;
            break;

        case 2:
            forum = 228;
            break;

        default:
            break;
    }


    $.ajax({
        url: '/api/forum/threads/add',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            title: title,
            text: text,
            forum: forum,
            tags: tags
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                notification('lime', 'forum', 'thread added');
                $('#forumcreate').hide();
                $('.backblur').hide();
                $('#tagPopup').hide();
                setTimeout(getMessages, 200);
            } else {
                notification('red', 'forum', 'error: ' + (jdata.error || jdata.message || jdata.result));
            }
        }

    });
}




function showeditthredname() {
    $('#themename').html(`
        <input type="text" id="threadname" value="${escapeHtml(window.currentThreadInfo.name)}" style="flex: 1; padding: 5px; font-size: 16px; border: 1px solid var(--color1); border-radius: 4px; background-color: var(--bg2); color: var(--text1);"><div class="save-icon" id="savethreadnameicon" onclick="editthredname($('#threadname').val())">
            <svg id="savenameicon" class="check-icon" onclick="saveName(this)" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 512 512">
            <path d="M173.9 439.4l-166.4-166.4c-12.5-12.5-12.5-32.8 0-45.3l45.3-45.3c12.5-12.5 32.8-12.5 45.3 0L192 276.3 413.9 54.4c12.5-12.5 32.8-12.5 45.3 0l45.3 45.3c12.5 12.5 12.5 32.8 0 45.3L218.1 431.4c-12.5 12.5-32.8 12.5-45.3 0z"></path>
        </svg>`);
    $('#themename').css('display', 'flex');
}

function editthredname(newname) {
    $.ajax({
        url: '/api/forum/threads/edit',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            field: 'title',
            message: window.currentThreadInfo.first_message_id,
            thread: window.currentthread,
            value: newname
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                notification('lime', 'forum', 'thread edited');
                $('#themename').text(newname);
                window.currentThreadInfo.name = newname;
            } else {
                notification('red', 'forum', 'error: ' + (jdata.error || jdata.message || jdata.result));
            }
        }
    });
}

function editthredstatus(status) {
    $.ajax({
        url: '/api/forum/threads/edit',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            field: 'status',
            message: window.currentThreadInfo.first_message_id,
            thread: window.currentthread,
            value: status
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                notification('lime', 'forum', 'thread edited');
            } else {
                notification('red', 'forum', 'error: ' + (jdata.error || jdata.message || jdata.result));
            }
        }
    });
}













// Theme management functions
function toggleDropdown() {
    const dropdown = document.getElementById('themeDropdown');
    dropdown.style.display = dropdown.style.display === 'none' ? 'block' : 'none';
}

function renameTheme() {
    const newName = prompt('Enter new theme name:', document.getElementById('themename').innerText);
    if (newName) {
        document.getElementById('themename').innerText = newName;
        // TODO: Add server-side update logic
    }
}

function hideTheme() {
    document.getElementById('headerthem').style.display = 'none';
    // TODO: Add server-side update logic
}

function deleteTheme() {
    id = $('.first-forummsg').attr('data-id');
    deletemessage(id);
    closethread();
}

// Close dropdown when clicking outside
document.addEventListener('click', function (e) {
    if (!e.target.closest('.dropdown')) {
        try { document.getElementById('themeDropdown').style.display = 'none'; } catch { }
    }
});

function getAvailableTags() {
    if (isGuidesForumView()) return [];
    if (!window.forumtags || !Array.isArray(window.forumtags)) return [];
    const userRole = (window.currentUser && typeof window.currentUser.role === 'number') ? window.currentUser.role : 0;
    return window.forumtags
        .filter(tag => {
            const tagRole = (typeof tag.role === 'number') ? tag.role : 0;
            return tagRole < userRole;
        })
        .map(tag => tag.name);
}

function renderTagOptions() {
    const search = $('#tag-search').val().toLowerCase();
    const container = $('#available-tags');
    container.empty();
    // ensure composer-specific tag array exists (separate from community filters)
    window.threadSelectedTags = window.threadSelectedTags || [];

    getAvailableTags().forEach(tag => {
        // use threadSelectedTags for the composer so it doesn't conflict with comtags
        if (tag.toLowerCase().includes(search) && !window.threadSelectedTags.includes(tag)) {
            container.append(
                $('<span>').addClass('button11').css({
                    padding: '5px 10px', margin: '3px', cursor: 'pointer'
                }).text(tag).click(function () { addTag(tag); })
            );
        }
    });
}



function renderSelectedTags() {
    const container = $('#selected-tags');
    container.empty();
    // use a separate array for tags selected in the "create thread" composer
    window.threadSelectedTags = window.threadSelectedTags || [];

    window.threadSelectedTags.forEach(tag => {
        const tagObj = (window.forumtags || []).find(t => t.name === tag) || null;
        const tagId = tagObj && typeof tagObj.id !== 'undefined' ? tagObj.id : '';

        const $tagEl = $('<span>')
            .addClass('selectedtags')
            .css({
                padding: '5px 10px',
                background: '#363636',
                fontSize: '13px',
                display: 'flex',
                alignItems: 'center'
            })
            .text(tag)
            .attr('data-id', tagId);

        if (tagId !== '') {
            $tagEl.attr('id', 'tag-' + tagId);
        }

        const $close = $('<span>')
            .css({
                marginLeft: '5px',
                cursor: 'pointer',
                color: 'rgba(255, 0, 0, 0.79)',
                fontWeight: 'bold'
            })
            .text('×')
            .click(function () { removeTag(tag); });

        $tagEl.append($close);
        container.append($tagEl);
    });
}

function addTag(tag) {
    window.threadSelectedTags = window.threadSelectedTags || [];
    if (!window.threadSelectedTags.includes(tag)) {
        window.threadSelectedTags.push(tag);
        renderSelectedTags();
        renderTagOptions();
    }
}

function removeTag(tag) {
    window.threadSelectedTags = window.threadSelectedTags || [];
    window.threadSelectedTags = window.threadSelectedTags.filter(t => t !== tag);
    renderSelectedTags();
    renderTagOptions();
}

function openTagPopup() {
    if (isGuidesForumView() || isInformationForumView()) {
        return;
    }

    $('.backblur').show();
    $('#tagPopup').show();
    $('#tag-search').val('');
    renderTagOptions();
}

function getSelectedTags() { window.threadSelectedTags = window.threadSelectedTags || []; return window.threadSelectedTags; }



// Инициализация UI для изменения тегов: слева активные теги треда (changetags1),
// справа — все доступные теги (changetags2). Поддержка drag'n'drop и клика.
function initChangeTagsUI() {
    // Создать контейнеры, если их нет
    if ($('#changetags-wrap').length === 0) {
        $('#themetags').after('<div id="changetags-wrap" style="display:flex;gap:12px;margin:8px 0;"><div id="changetags1" style="flex:1"></div><div id="changetags2" style="flex:1"></div></div>');
    }
    renderChangeTags();

    // Переменная-резерв для браузеров, где getData может быть пустым
    window._dragTagId = null;

    // обработчики для зон (drop)
    $('#changetags1, #changetags2')
        .off('dragenter dragover dragleave drop')
        .on('dragenter', function (e) {
            e.preventDefault();
            e.stopPropagation();
            $(this).css('outline', '2px dashed var(--color1)');
        })
        .on('dragover', function (e) {
            e.preventDefault();
            e.stopPropagation();
            const ev = e.originalEvent || e;
            try {
                ev.dataTransfer.dropEffect = 'move';
            } catch { }
            $(this).css('outline', '2px dashed var(--color1)');
        })
        .on('dragleave', function (e) {
            e.preventDefault();
            e.stopPropagation();
            $(this).css('outline', '');
        })
        .on('drop', function (e) {
            e.preventDefault();
            e.stopPropagation();
            $(this).css('outline', '');
            const ev = e.originalEvent || e;
            let tagIdRaw = null;
            try { tagIdRaw = ev.dataTransfer.getData('text/plain'); } catch (err) { tagIdRaw = null; }
            // fallback на временную переменную, если getData пустой
            if ((!tagIdRaw || tagIdRaw === '') && window._dragTagId) tagIdRaw = String(window._dragTagId);
            const tagId = parseInt(tagIdRaw, 10);
            if (isNaN(tagId)) return;
            if (this.id === 'changetags1') {
                addTagToThread(tagId);
            } else {
                removeTagFromThread(tagId);
            }
            // сброс
            window._dragTagId = null;
        });
}

// Рендер списков тегов
function renderChangeTags() {
    if (isGuidesThread() || isInformationThread()) {
        $('#changetags-wrap').hide();
        return;
    }

    // Одна панель для всех тегов — активировать/деактивировать кликом
    const allTags = Array.isArray(window.forumtags) ? window.forumtags : [];
    const threadTagsStr = (window.currentThreadInfo && typeof window.currentThreadInfo.tags === 'string') ? window.currentThreadInfo.tags : '';
    const selectedIds = new Set(threadTagsStr.split(',').map(s => parseInt(s.trim(), 10)).filter(n => !isNaN(n) && n > 0));

    // роль текущего пользователя для проверки доступности тегов
    const userRole = (window.currentUser && typeof window.currentUser.role === 'number') ? window.currentUser.role : 0;

    // Убедимся, что контейнеры есть (инициализация может не создавать их)
    if ($('#changetags1').length === 0) {
        $('#themetags').after('<div id="changetags-wrap" style="margin:8px 0;"><div id="changetags1"></div></div>');
    }
    $('#changetags-wrap').show();
    // Скрываем правую колонку, если она есть — мы используем только одну колонку
    $('#changetags2').hide && $('#changetags2').hide();

    const $container = $('#changetags1');
    $container.empty().append('<div style="font-weight:600;margin-bottom:6px">Tags</div>');

    // Фильтруем теги по доступности (role на теге <= role пользователя)
    const availableTags = allTags.filter(tag => {
        const tagRole = (typeof tag.role === 'number') ? tag.role : 0;
        return tagRole <= userRole;
    });

    if (availableTags.length === 0) {
        $container.append('<div style="color:#999">No tags available or insufficient permissions</div>');
        return;
    }

    // Создать элемент тега
    availableTags.forEach(tag => {
        const isActive = selectedIds.has(tag.id);
        const $el = $(`<span class="change-tag" data-id="${tag.id}" style="
            display:inline-block;
            padding:6px 10px;
            margin:6px 6px 0 0;
            border-radius:6px;
            cursor:pointer;
            user-select:none;
            transition:all .15s;
            "></span>`);

        // Стили активного/неактивного состояния
        if (isActive) {
            $el.css({ background: '', color: '#fff', border: '2px solid var(--color1)' });
        } else {
            $el.css({ background: '', color: '#ddd', border: '2px solid #333' });
        }

        $el.text(tag.name);

        // Клик — переключение: вызывает addTagToThread / removeTagFromThread,
        // которые обновляют currentThreadInfo и снова вызовут renderChangeTags()
        $el.on('click', function (e) {
            e.preventDefault();
            const id = parseInt($(this).data('id'), 10);
            if (isNaN(id)) return;
            // Временно показать состояние загрузки, чтобы предотвратить множественные клики
            $el.css('opacity', '0.6').css('pointer-events', 'none');
            if (selectedIds.has(id)) {
                removeTagFromThread(id);
            } else {
                addTagToThread(id);
            }
            // Вызов renderChangeTags будет выполнен в callback add/remove функции,
            // но если их нет или они не перерисуют, сбросим блокировку через 1.5s
            setTimeout(() => {
                $el.css('opacity', '').css('pointer-events', '');
            }, 1500);
        });

        // Двойной клик — то же самое (для удобства)
        $el.on('dblclick', function (e) {
            e.preventDefault();
            $(this).trigger('click');
        });

        $container.append($el);
    });
}

// Добавить тег в текущий тред (UI + отправка на сервер) — использует action=add
function addTagToThread(tagId) {
    if (!window.currentThreadInfo) return;
    const existing = (window.currentThreadInfo.tags || '').split(',').map(s => parseInt(s.trim(), 10)).filter(n => !isNaN(n) && n > 0);
    if (existing.includes(tagId)) {
        renderChangeTags();
        return;
    }

    // Отправляем запрос action=add + value=tagId
    saveThreadTags(window.currentthread, null, () => {
        // обновим локально
        const set = new Set(existing);
        set.add(tagId);
        window.currentThreadInfo.tags = Array.from(set).join(',');
        renderChangeTags();
        writethemetags();
    }, 'add', tagId);
}

// Убрать тег из треда — использует action=remove
function removeTagFromThread(tagId) {
    if (!window.currentThreadInfo) return;
    const existing = (window.currentThreadInfo.tags || '').split(',').map(s => parseInt(s.trim(), 10)).filter(n => !isNaN(n) && n > 0);
    if (!existing.includes(tagId)) {
        renderChangeTags();
        return;
    }

    // Отправляем запрос action=remove + value=tagId
    saveThreadTags(window.currentthread, null, () => {
        const newIds = existing.filter(id => id !== tagId);
        window.currentThreadInfo.tags = newIds.join(',');
        renderChangeTags();
        writethemetags();
    }, 'remove', tagId);
}

// Сохраняем теги на сервере. При успешном ответе вызываем callback.
// Поддерживает API: POST /api/forum/threads/edit с полями:
// field=tags, message=first_message_id, action = "clear/add/remove", value (tag id при add/remove)
function saveThreadTags(threadId, tagsCsv, callback, action, value) {
    const data = {
        field: 'tags',
        thread: threadId
    };
    if (window.currentThreadInfo && window.currentThreadInfo.first_message_id) {
        data.message = window.currentThreadInfo.first_message_id;
    }
    if (typeof action === 'string' && action.length > 0) {
        data.action = action; // "add" / "remove" / "clear"
        if (typeof value !== 'undefined' && value !== null) {
            data.value = String(value);
        }
    } else if (typeof tagsCsv === 'string') {
        // Если action не передан, отправим value как csv (на случай старой реализации)
        data.value = tagsCsv;
    }

    $.ajax({
        url: '/api/forum/threads/edit',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: data,
        success: function (resp) {
            let j;
            try { j = typeof resp === 'string' ? JSON.parse(resp) : resp; } catch { j = resp; }
            if (j && j.success) {
                if (typeof callback === 'function') callback();
                try { notification('lime', 'tags', 'saved'); } catch { }
            } else {
                try { notification('red', 'tags', j && (j.error || j.message || j.result) ? (j.error || j.message || j.result) : 'save failed'); } catch { }
            }
        },
        error: function () {
            try { notification('red', 'tags', 'network error'); } catch { }
        }
    });
}


function themeup() {
    $.ajax({
        url: '/api/forum/threads/up',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            message: $('.first-forummsg').attr('data-id'),
            thread: window.currentthread
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                notification('green', 'up system', 'success')
            } else {
                notification('red', 'up system', jdata.result)
            }
        }
    });
}


function copyMarketLink() {
    link = "https://" + location.hostname + "/panel/?thread=" + window.currentthread + "&msg=" + $('.first-forummsg').attr('data-id') + "#community";
    copyTextToClipboard(link);
}

// Синхронизирует URL в адресной строке с текущим тредом, чтобы им можно было поделиться.
// replace=true используем для «доточки» URL после getThreadInfo, чтобы не плодить записи в истории.
function setThreadUrl(threadId, firstMessageId, replace) {
    if (threadId == null) return;
    try {
        const url = new URL(window.location.href);
        const params = new URLSearchParams(url.search);
        params.set('thread', String(threadId));
        if (firstMessageId != null && firstMessageId !== 0 && firstMessageId !== '0') {
            params.set('msg', String(firstMessageId));
        } else {
            params.delete('msg');
        }
        url.search = params.toString();
        if (!url.hash) {
            url.hash = '#community';
        }
        if (replace) {
            history.replaceState({}, '', url.toString());
        } else {
            history.pushState({}, '', url.toString());
        }
    } catch (e) { }
}

// Снимает thread/msg с URL, оставляя остальные параметры и #community.
function clearThreadUrl() {
    try {
        const url = new URL(window.location.href);
        const params = new URLSearchParams(url.search);
        const hadThread = params.has('thread') || params.has('msg');
        params.delete('thread');
        params.delete('msg');
        url.search = params.toString();
        if (!url.hash) {
            url.hash = '#community';
        }
        if (hadThread) {
            history.pushState({}, '', url.toString());
        }
    } catch (e) { }
}

function closetheme() {
    $.ajax({
        url: '/api/forum/threads/edit',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            field: 'status',
            thread: window.currentthread,
            message: $('.first-forummsg').attr('data-id'),
            value: 1
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                notification('green', 'thread edit', 'thread is close');
                window.currentThreadInfo.status = 1
            } else {
                notification('red', 'thread edit', jdata.result)
            }
        }
    });
}

function opentheme() {
    $.ajax({
        url: '/api/forum/threads/edit',
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            field: 'status',
            thread: window.currentthread,
            message: $('.first-forummsg').attr('data-id'),
            value: 0
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success) {
                notification('green', 'thread edit', 'thread is open');
                window.currentThreadInfo.status = 0
            } else {
                notification('red', 'thread edit', jdata.result)
            }
        }
    });
}

function checkthemestatus() {
    const openBtn = document.getElementById('butthemeopen');
    const closeBtn = document.getElementById('butthemeclose');
    const info = window.currentThreadInfo || null;
    const status = info && typeof info.status !== 'undefined' ? Number(info.status) : null;

    // Helper to set neutral look
    function neutral(btn) {
        if (!btn) return;
        btn.disabled = true;
        btn.style.background = '#3a3a3a';
        btn.style.color = '#cfcfcf';
        btn.style.border = '1px solid #333';
        btn.setAttribute('aria-pressed', 'false');
        btn.classList && btn.classList.remove('active');
    }

    // Helper to set active look
    function active(btn, color) {
        if (!btn) return;
        btn.disabled = false;
        btn.style.background = color;
        btn.style.color = '#fff';
        btn.style.border = '1px solid rgba(0,0,0,0.2)';
        btn.setAttribute('aria-pressed', 'true');
        btn.classList && btn.classList.add('active');
    }

    // If no info, just neutralize both
    if (status === null) {
        neutral(openBtn);
        neutral(closeBtn);
        return;
    }

    // status: 0 = open, 1 = closed (as used elsewhere in code)
    if (status === 1) {
        // thread is closed -> highlight "close" as active, "open" neutral
        active(closeBtn, '#e74c3c'); // red for closed state
        neutral(openBtn);
    } else {
        // thread is open -> highlight "open" as active, "close" neutral
        active(openBtn, '#2ecc71'); // green for open state
        neutral(closeBtn);
    }
}
// Выполнить сразу, чтобы состояние применилось при загрузке/обновлении информации
// Я зашёл в лифт и начал дико срать
function checkthemestatus() {
    const openBtn = document.getElementById('butthemeopen');
    const closeBtn = document.getElementById('butthemeclose');
    const info = window.currentThreadInfo || null;
    const status = info && typeof info.status !== 'undefined' ? Number(info.status) : null;

    function neutral(btn) {
        if (!btn) return;
        btn.disabled = false;
        btn.style.background = '#3a3a3a';
        btn.style.color = '#cfcfcf';
        btn.style.border = '1px solid #333';
        btn.setAttribute('aria-pressed', 'false');
        btn.classList && btn.classList.remove('active');
    }

    function active(btn, color) {
        if (!btn) return;
        btn.disabled = false;
        btn.style.background = color;
        btn.style.color = '#fff';
        btn.style.border = '1px solid rgba(0,0,0,0.2)';
        btn.setAttribute('aria-pressed', 'true');
        btn.classList && btn.classList.add('active');
    }

    if (status === null) {
        neutral(openBtn);
        neutral(closeBtn);
        return;
    }

    // Now highlight the button that reflects the CURRENT status:
    // status: 0 = open, 1 = closed
    if (status === 1) {
        // thread is closed -> highlight "close" as active, "open" neutral
        active(closeBtn, '#e74c3c'); // red for closed state
        neutral(openBtn);
    } else {
        // thread is open -> highlight "open" as active, "close" neutral
        active(openBtn, '#2ecc71'); // green for open state
        neutral(closeBtn);
    }
}

// Immediately apply once
try { checkthemestatus(); } catch (e) { /* ignore */ }

// Watch for reassignment to window.currentThreadInfo and for in-place changes
(function () {
    let _val = window.currentThreadInfo || null;
    let lastStatus = _val && typeof _val.status !== 'undefined' ? Number(_val.status) : null;

    Object.defineProperty(window, 'currentThreadInfo', {
        configurable: true,
        enumerable: true,
        get() { return _val; },
        set(v) {
            _val = v;
            try { checkthemestatus(); } catch (e) { /* ignore */ }
            lastStatus = _val && typeof _val.status !== 'undefined' ? Number(_val.status) : null;
            return _val;
        }
    });

    setInterval(function () {
        try {
            const info = window.currentThreadInfo;
            const status = info && typeof info.status !== 'undefined' ? Number(info.status) : null;
            if (status !== lastStatus) {
                lastStatus = status;
                checkthemestatus();
            }
        } catch (e) { /* ignore */ }
    }, 500);
})();

function renderUserThreads() {
    const chatContainer = document.querySelector('#everychat');
    if (!chatContainer) return;
    chatContainer.innerHTML = '';

    const uid = window.currentUser && window.currentUser.id;
    if (!uid) {
        chatContainer.innerHTML = `<div class="fmessages" style="padding:15px;color:#bbb">You must be signed in to see your threads.</div>`;
        return;
    }

    if (!window.threads || !Array.isArray(window.threads.result)) {
        chatContainer.innerHTML = `<div class="fmessages" style="padding:15px;color:#bbb">No threads loaded.</div>`;
        return;
    }

    const my = window.threads.result.filter(t => {
        return t.first_author_user == uid || t.first_author_id == uid || t.first_author == uid;
    });

    if (my.length === 0) {
        chatContainer.innerHTML = `<div class="fmessages" style="padding:15px;color:#bbb">You have no threads.</div>`;
        return;
    }

    my.forEach(post => {
        const avatarUrl = post.first_author_avatar || 'https://i.ibb.co/q3GCfzPm/besavatarochniy.png';
        const messageText = post.first_message_text
            ? (post.first_message_text.length > 120 ? post.first_message_text.substring(0, 120) + '...' : post.first_message_text)
            : '';
        const isOwner = true; // by filter it's the user's thread
        const bg = isOwner ? '#30303090' : '#3030305d';
        const lockico = post.status != 0
            ? `<span style="color: red; margin-right:8px; align-items:center;"><svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" height="20px" width="20px" viewBox="0 0 640 640"><path d="M240 160L240 224L400 224L400 160C400 115.8 364.2 80 320 80C275.8 80 240 115.8 240 160zM192 224L192 160C192 89.3 249.3 32 320 32C390.7 32 448 89.3 448 160L448 224C483.3 224 512 252.7 512 288L512 512C512 547.3 483.3 576 448 576L192 576C156.7 576 128 547.3 128 512L128 288C128 252.7 156.7 224 192 224zM400 272L240 272L240 272L192 272L192 272C183.2 272 176 279.2 176 288L176 512C176 520.8 183.2 528 192 528L448 528C456.8 528 464 520.8 464 512L464 288C464 279.2 456.8 272 448 272L448 272L400 272L400 272z"/></svg></span>`
            : '';

        const timeMs = Number(post.first_message_time || post.time_up || 0) * 1000;
        const timeStr = timeMs ? new Date(timeMs).toLocaleString() : '';

        const el = document.createElement('div');
        el.className = 'fmessages';
        el.style.cursor = 'pointer';
        el.style.display = 'flex';
        el.style.padding = '15px';
        el.style.marginTop = '20px';
        el.style.background = bg;
        el.style.border = '1px solid #333';
        el.style.borderRadius = '8px';
        el.style.transition = 'background 0.2s ease';
        el.onmouseover = function () { this.style.background = '#2a2a2a'; };
        el.onmouseout = function () { this.style.background = bg; };
        el.onclick = function () { openthread(post.id, escapeHtml(post.name), post.first_message_id); };

        el.innerHTML = `
            <div style="margin-right:15px">
                <img src="${avatarUrl}" alt="avatar" style="width:60px;height:60px;border-radius:6px;object-fit:cover;border:1px solid #444;">
            </div>
            <div style="flex:1;display:flex;flex-direction:column">
                <div style="display:flex;justify-content:space-between;align-items:center">
                    <div style="display:flex;align-items:center"><div>${lockico}</div><h3 style="font-size:20px;font-weight:bold;color:var(--color1);margin:0">${escapeHtml(post.name)}</h3></div>
                    <span style="font-size:12px;color:#aaa">${timeStr}</span>
                </div>
                <div style="font-size:13px;color:#bbb;margin-top:4px">Autor: <span style="color:#ccc;font-weight:500">${escapeHtml(post.first_author_name || '')}</span></div>
                <div style="font-size:15px;color:#ddd;margin:10px 0;line-height:1.5">${escapeHtml(messageText)}</div>
                <div style="text-align:center;font-size:14px;color:#999;border-top:1px solid #333;padding-top:10px;margin-top:auto"><b>Messages: ${post.messages_count || 0}</b></div>
            </div>
        `;
        chatContainer.appendChild(el);
    });
}

function showbanhistory() {

}

window.replyingTo = null;

function replyToMessage(msgId) {
    const msg = window.thismessages.find(m => m.id === msgId);
    if (!msg) return;

    const input = getChatComposerInput();
    if (!input) return;

    const mention = `@${msg.name} `;
    const curVal = input.value;
    input.value = mention + curVal;
    input.focus();
    input.setSelectionRange(mention.length + curVal.length, mention.length + curVal.length);
    updateChatInputState(input);
}

function cancelReply() {
    window.replyingTo = null;
    $('#reply-preview').hide();
}

$(document).on('mousedown', '.fmessages', function (e) {
    // 1 = левая, 2 = средняя (колёсико), 3 = правая
    if (e.which === 2) {
        e.preventDefault();           // предотвращаем скролл/вставку
        e.stopPropagation();          // на всякий случай

        let url = $(this).attr('href') || $(this).data('url') || $(this).find('a').first().attr('href');
        console.log(url)
        if (url) {
            window.open(url, '_blank');   // открываем в новой вкладке
        }
    }
});