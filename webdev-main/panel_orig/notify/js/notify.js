function headsel(el, t) {
    $(".themepage").hide();
    $(".securitypage").hide();
    setTimeout(() => {
        $(el).eq(0).show();
        $(".headselectorbutton").eq(0).removeClass("headselectorbuttonactive");
        $(".headselectorbutton").eq(1).removeClass("headselectorbuttonactive");
        $(t).addClass("headselectorbuttonactive");
    }, 1);
}

function getmethods() {
    $.ajax({
        url: '/api/notify/methods',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
        },
        success: function (data) {
            jdata = JSON.parse(data);
            window.notifymethods = JSON.parse(data).result;
            $('#meth').html('');
            if (jdata.result == "No auth") {
                notification("orange", window.t('common.warning'), window.t('settings.no_auth'));
                console.error("Redirecting to login");
                window.location.href = "/login/";
                return;
            }
            if (jdata.success == true) {
                datas = '';
                mthods = jdata.result;
                if(mthods.length == 0){
                    $('#createtrigbut').addClass('button1inact');
                }else{
                    $('#createtrigbut').removeClass('button1inact');
                }
                mthods.forEach(function (item) {
                    console.log(item);
                    switch (item.type) {
                        case 3:
                            // ds+log
                            item.type = '<svg xmlns="http://www.w3.org/2000/svg" height="23px" viewBox="0 0 640 512" fill="currentColor"><path d="M524.531,69.836a1.5,1.5,0,0,0-.764-.7A485.065,485.065,0,0,0,404.081,32.03a1.816,1.816,0,0,0-1.923.91,337.461,337.461,0,0,0-14.9,30.6,447.848,447.848,0,0,0-134.426,0,309.541,309.541,0,0,0-15.135-30.6,1.89,1.89,0,0,0-1.924-.91A483.689,483.689,0,0,0,116.085,69.137a1.712,1.712,0,0,0-.788.676C39.068,183.651,18.186,294.69,28.43,404.354a2.016,2.016,0,0,0,.765,1.375A487.666,487.666,0,0,0,176.02,479.918a1.9,1.9,0,0,0,2.063-.676A348.2,348.2,0,0,0,208.12,430.4a1.86,1.86,0,0,0-1.019-2.588,321.173,321.173,0,0,1-45.868-21.853,1.885,1.885,0,0,1-.185-3.126c3.082-2.309,6.166-4.711,9.109-7.137a1.819,1.819,0,0,1,1.9-.256c96.229,43.917,200.41,43.917,295.5,0a1.812,1.812,0,0,1,1.924.233c2.944,2.426,6.027,4.851,9.132,7.16a1.884,1.884,0,0,1-.162,3.126,301.407,301.407,0,0,1-45.89,21.83,1.875,1.875,0,0,0-1,2.611,391.055,391.055,0,0,0,30.014,48.815,1.864,1.864,0,0,0,2.063.7A486.048,486.048,0,0,0,610.7,405.729a1.882,1.882,0,0,0,.765-1.352C623.729,277.594,590.933,167.465,524.531,69.836ZM222.491,337.58c-28.972,0-52.844-26.587-52.844-59.239S193.056,219.1,222.491,219.1c29.665,0,53.306,26.82,52.843,59.239C275.334,310.993,251.924,337.58,222.491,337.58Zm195.38,0c-28.971,0-52.843-26.587-52.843-59.239S388.437,219.1,417.871,219.1c29.667,0,53.307,26.82,52.844,59.239C470.715,310.993,447.538,337.58,417.871,337.58Z"/></svg>'
                            try {
                                item.data = item.data.split("/")[5];
                            } catch {
                                item.data = "err";
                            }
                            break;
                        case 1:
                            // ds
                            item.type = '<svg xmlns="http://www.w3.org/2000/svg" height="23px" viewBox="0 0 640 512" fill="currentColor"><path d="M524.531,69.836a1.5,1.5,0,0,0-.764-.7A485.065,485.065,0,0,0,404.081,32.03a1.816,1.816,0,0,0-1.923.91,337.461,337.461,0,0,0-14.9,30.6,447.848,447.848,0,0,0-134.426,0,309.541,309.541,0,0,0-15.135-30.6,1.89,1.89,0,0,0-1.924-.91A483.689,483.689,0,0,0,116.085,69.137a1.712,1.712,0,0,0-.788.676C39.068,183.651,18.186,294.69,28.43,404.354a2.016,2.016,0,0,0,.765,1.375A487.666,487.666,0,0,0,176.02,479.918a1.9,1.9,0,0,0,2.063-.676A348.2,348.2,0,0,0,208.12,430.4a1.86,1.86,0,0,0-1.019-2.588,321.173,321.173,0,0,1-45.868-21.853,1.885,1.885,0,0,1-.185-3.126c3.082-2.309,6.166-4.711,9.109-7.137a1.819,1.819,0,0,1,1.9-.256c96.229,43.917,200.41,43.917,295.5,0a1.812,1.812,0,0,1,1.924.233c2.944,2.426,6.027,4.851,9.132,7.16a1.884,1.884,0,0,1-.162,3.126,301.407,301.407,0,0,1-45.89,21.83,1.875,1.875,0,0,0-1,2.611,391.055,391.055,0,0,0,30.014,48.815,1.864,1.864,0,0,0,2.063.7A486.048,486.048,0,0,0,610.7,405.729a1.882,1.882,0,0,0,.765-1.352C623.729,277.594,590.933,167.465,524.531,69.836ZM222.491,337.58c-28.972,0-52.844-26.587-52.844-59.239S193.056,219.1,222.491,219.1c29.665,0,53.306,26.82,52.843,59.239C275.334,310.993,251.924,337.58,222.491,337.58Zm195.38,0c-28.971,0-52.843-26.587-52.843-59.239S388.437,219.1,417.871,219.1c29.667,0,53.307,26.82,52.844,59.239C470.715,310.993,447.538,337.58,417.871,337.58Z"/></svg>'
                            try {
                                item.data = item.data.split("/")[5];
                            } catch {
                                item.data = "err";
                            }
                            break;
                        case 2:
                            // tg+log
                            datas = item.data;
                            item.type = '<svg xmlns="http://www.w3.org/2000/svg" height="23px" viewBox="0 0 496 512" fill="currentColor"><path d="M248,8C111.033,8,0,119.033,0,256S111.033,504,248,504,496,392.967,496,256,384.967,8,248,8ZM362.952,176.66c-3.732,39.215-19.881,134.378-28.1,178.3-3.476,18.584-10.322,24.816-16.948,25.425-14.4,1.326-25.338-9.517-39.287-18.661-21.827-14.308-34.158-23.215-55.346-37.177-24.485-16.135-8.612-25,5.342-39.5,3.652-3.793,67.107-61.51,68.335-66.746.153-.655.3-3.1-1.154-4.384s-3.59-.849-5.135-.5q-3.283.746-104.608,69.142-14.845,10.194-26.894,9.934c-8.855-.191-25.888-5.006-38.551-9.123-15.531-5.048-27.875-7.717-26.8-16.291q.84-6.7,18.45-13.7,108.446-47.248,144.628-62.3c68.872-28.647,83.183-33.623,92.511-33.789,2.052-.034,6.639.474,9.61,2.885a10.452,10.452,0,0,1,3.53,6.716A43.765,43.765,0,0,1,362.952,176.66Z"/></svg>'
                            break;
                        case 0:
                            // tg
                            try {
                                datas = item.data;
                                item.type = '<svg xmlns="http://www.w3.org/2000/svg" height="23px" viewBox="0 0 496 512" fill="currentColor"><path d="M248,8C111.033,8,0,119.033,0,256S111.033,504,248,504,496,392.967,496,256,384.967,8,248,8ZM362.952,176.66c-3.732,39.215-19.881,134.378-28.1,178.3-3.476,18.584-10.322,24.816-16.948,25.425-14.4,1.326-25.338-9.517-39.287-18.661-21.827-14.308-34.158-23.215-55.346-37.177-24.485-16.135-8.612-25,5.342-39.5,3.652-3.793,67.107-61.51,68.335-66.746.153-.655.3-3.1-1.154-4.384s-3.59-.849-5.135-.5q-3.283.746-104.608,69.142-14.845,10.194-26.894,9.934c-8.855-.191-25.888-5.006-38.551-9.123-15.531-5.048-27.875-7.717-26.8-16.291q.84-6.7,18.45-13.7,108.446-47.248,144.628-62.3c68.872-28.647,83.183-33.623,92.511-33.789,2.052-.034,6.639.474,9.61,2.885a10.452,10.452,0,0,1,3.53,6.716A43.765,43.765,0,0,1,362.952,176.66Z"/></svg>'
                            }
                            catch {
                                console.log(datas)
                                item.type = '<b>err</b>'
                            }
                            break;
                        default:
                            break;
                    }
                    const template = `
                        <div class="buildchiled" style="width: 380px;">
                            <div class="linebuildchiled2">
                            <div><b>${fixtext(item.name)}</b></div> <div style="cursor: pointer;" onclick='deletemethod(${item.id})'>
                            <div style="height: 18px; width: 18px;">
                                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512"><path fill="currentColor" d="M160.5 27.4c2-6.8 8.3-11.4 15.3-11.4l96.4 0c7.1 0 13.3 4.6 15.3 11.4l11 36.6-149 0 11-36.6zM116.1 64L16 64C7.2 64 0 71.2 0 80S7.2 96 16 96l416 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-100.1 0-13.7-45.8C312.1-2.1 293.4-16 272.2-16l-96.4 0c-21.2 0-39.9 13.9-46 34.2L116.1 64zM28.7 144L51.6 452.7c2.5 33.4 30.3 59.3 63.8 59.3l217.1 0c33.5 0 61.3-25.9 63.8-59.3l22.9-308.7-32.1 0-22.7 306.4c-1.2 16.7-15.2 29.6-31.9 29.6l-217.1 0c-16.8 0-30.7-12.9-31.9-29.6L60.8 144 28.7 144z"/></svg>
                            </div>
                        </div>
                            </div>
                            <div class="linebuildchiled" style="margin-top: 10px;">
                                <div style="margin-top: -3px; margin-bottom: 2px;">${item.type}
                                </div>
                                <div>
                                <b>${item.data}</b>
                                </div>
                                <div>
                                    ${window.t('builder.lbl_created')} <b>${item.created}</b>
                                </div>
                            </div>
                            </div>
                        </div>
                    `;
                    $('#meth').append(template);
                })
                console.log(data)
            }
        }
    })
}

function deletemethod(e) {
    $.ajax({
        url: '/api/notify/methods/remove',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            id: e
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success == true) {
                $('#meth').html("");
                getmethods();
            }
        }
    })
}

function deletetrigger(e) {
    $.ajax({
        url: '/api/notify/triggers/remove',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            id: e
        },
        success: function (data) {
            jdata = JSON.parse(data);
            if (jdata.success == true) {
                $('#trig').html("");
                gettriggers();
            }
        }
    })
}

function gettriggers() {
    $('#trig').html("");
    $.ajax({
        url: '/api/notify/triggers',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
        },
        success: function (data) {
            jdata = JSON.parse(data);
            console.log(jdata)
            if (jdata.success == true) {
                trigs = jdata.result;
                trigs.forEach(function (item) {
                    console.log(item);
                    switch (item.targetType) {
                        case 0:
                            method = window.t('notify.target_panel');
                            item.target = window.t('notify.target_all_users');
                            break;
                        case 1:
                            method = window.t('notify.tab_build');
                            break;
                        case 2:
                            method = window.t('notify.tab_user');
                            break;
                        default:
                            break;
                    }

                    switch (item.event) {
                        case 0:
                            ievent = window.t('builder.opt_install');
                            break;
                        case 1:
                            ievent = window.t('builder.opt_connect');
                            break;
                        case 2:
                            ievent = window.t('builder.opt_steal');
                            break;
                        case 3:
                            ievent = window.t('builder.opt_steal');
                            adding = window.t('notify.adding_log');
                            break;
                        case 4:
                            ievent = window.t('builder.opt_install');
                            adding = window.t('notify.adding_screen');
                            break;
                        case 5:
                            ievent = window.t('builder.opt_connect');
                            adding = window.t('notify.adding_screen');
                            break;
                        default:
                            break;
                    }


                    const template = `
                        <div class="buildchiled">
                            <div class="linebuildchiled2">
                            <div><b>${fixtext(method)}</b></div> <div style="cursor: pointer;" onclick='deletetrigger(${item.id})''>
                            <div style="height: 18px; width: 18px;">
                                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512"><path fill="currentColor" d="M160.5 27.4c2-6.8 8.3-11.4 15.3-11.4l96.4 0c7.1 0 13.3 4.6 15.3 11.4l11 36.6-149 0 11-36.6zM116.1 64L16 64C7.2 64 0 71.2 0 80S7.2 96 16 96l416 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-100.1 0-13.7-45.8C312.1-2.1 293.4-16 272.2-16l-96.4 0c-21.2 0-39.9 13.9-46 34.2L116.1 64zM28.7 144L51.6 452.7c2.5 33.4 30.3 59.3 63.8 59.3l217.1 0c33.5 0 61.3-25.9 63.8-59.3l22.9-308.7-32.1 0-22.7 306.4c-1.2 16.7-15.2 29.6-31.9 29.6l-217.1 0c-16.8 0-30.7-12.9-31.9-29.6L60.8 144 28.7 144z"/></svg>
                            </div>
                        </div>
                            </div>
                            <div>
                                ${window.t('notify.lbl_event')} <b>${ievent}</b>
                            </div>
                                <div>
                                    ${window.t('builder.lbl_created')} <b>${item.created}</b>
                                </div>
                            <div>
                                ${window.t('notify.lbl_target')} <b>${item.target}</b>
                            </div>
                        </div>
                        `;
                    $('#trig').append(template);
                })
            }
        }
    })
}



window.selmethtype = 0

function checkmethod() {
    if ((window.selmethtype == 0 && $('#notifytgtok').val() == "" && $('#notifytgchid').val() == "") || (window.selmethtype == 1 && $('#notifydshook').val() == "")) {
        notification('red', window.t('common.error'), window.t('notify.err_fill_blanks'))
        return
    }
    if (window.selmethtype == 0) {
        validateTelegramToken($('#notifytgtok').val())
    } else {
        if (!$('#notifydshook').val().startsWith('https://discord.com/api/webhooks/')) {
            if (!$('#notifydshook').val().startsWith('https://discordapp.com/api/webhooks/')) {
                notification('red', window.t('common.error'), window.t('notify.err_wrong_webhook'))
                return;
            }
        }
    }
    $.ajax({
        url: '/api/notify/methods/check',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            type: window.selmethtype,
            data: window.selmethtype == 0 ? JSON.stringify({ token: $('#notifytgtok').val(), chatid: $('#notifytgchid').val() }) : $('#notifydshook').val()
        },
        success: function (data) {
            jdata = JSON.parse(data);
            console.log(jdata)
            if (jdata.success == true) {
                notification('lime', window.t('notify.notif_sent_title'), window.t('notify.notif_sent_body'))
            } else {
                notification('red', window.t('notify.notif_rate_title'), window.t('notify.notif_rate_body'))
            }

        }
    })
}

function addmethod() {
    methtgname = $('#methtgname').val().replace(/\s+$/, '');
    methdsname = $('#methdsname').val().replace(/\s+$/, '');
    $.ajax({
        url: '/api/notify/methods/add',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            name: window.selmethtype == 0 ? methtgname : methdsname,
            type: window.selmethtype,
            data: window.selmethtype == 0 ? JSON.stringify({ token: $('#notifytgtok').val(), chatid: $('#notifytgchid').val() }) : $('#notifydshook').val()
        },
        success: function (data) {
            $('.mainpopup').hide();
            $('.backblur').hide();
            $('#methtgname').val('');
            $('#methdsname').val('');
            $('#notifytgtok').val('');
            $('#notifytgchid').val('');
            $('#notifydshook').val('');
            getmethods();
        }
    })
}

function validateTelegramToken(token) {
    $.ajax({
        url: `https://api.telegram.org/bot${token}/getMe`,
        method: 'GET',
        success: function (response) {
            if (response.ok) {
                const botName = response.result.username;
                console.log('Token valid. Bot name:', botName);
                if ($('#methtgname').val() == '') {
                    $('#methtgname').val(botName);
                }
                $('#tokencheckstatus').html('<div style="color: lime;"><b>' + window.t('notify.token_valid') + '</b></div>');
            } else {
                console.log('Token invalid.');
                $('#tokencheckstatus').html('<div style="color: red;"><b>' + window.t('notify.token_invalid') + '</b></div>');
            }
        },
        error: function () {
            console.log('Error check');
        }
    });
}

function shownewtrig() {
    $('#newtrig').show();
    $('.backblur').show();
    $('#methodsbuild, #notifymeth, #methodsuser').empty();
    window.notifymethods.forEach(function (item) {
        console.log(item);
        template = `<option>${item.name}</option>`
        $('#methodsbuild').append(template);
        $('#notifymeth').append(template);
        $('#methodsuser').append(template);
    });
    $('.textareatrig').val(`*WebRat notification*
-----------------------------
{user}
{build} {version}
{ip} {country}
admin: {adm}
-----------------------------
\`{uid}\``)

    getbuildsfornotify()
    $('.methsel').customSelect({
        modifier: 'custom-select--dark',
        placeholder: window.t('builder.placeholder_method')
    });
    $('.methsel').customSelect("reset");
}

function changesel(e, h) {
    $('.notifyselitem').css('border-bottom', '#adadad8f 2px solid');
    $(e).css('border-bottom', 'var(--color1) 2px solid');
    $('.methpage').hide();
    $(h).show();
    /*$('.methsel').customSelect({
        modifier: 'custom-select--dark',
        placeholder: 'method'
    });*/
}


$(function () {
    getmethods();
    gettriggers();
    $('.othersel').customSelect({
        modifier: 'custom-select--dark'
    });

    if (readCookie("bgmode") == "2") {
        $("body").css("background", readCookie('bgcolor'));
    }

    $.ajax({
        url: '/api/users',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
        },
        success: function (data) {
            jdata = JSON.parse(data)
            if (jdata.success == false) {
                if (jdata.result == "No license") {
                    window.location.href = "/panel#subs"
                } else {
                    notification("orange", window.t('common.warning'), window.t('settings.no_auth'));
                    console.error("Redirecting to login")
                    window.location.href = "/login/"
                }
            }
            window.userlist = jdata.result;
        }
    });
});

function getbuildsfornotify() {
    $.ajax({
        url: '/api/builds',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {

        },
        success: function (data) {
            window.buildslist = data;
            dataStr = data;
            builds = JSON.parse(dataStr);
            builds.forEach(function (item) {
                buildname = JSON.parse(item.config)["name"]
                if (buildname == '') {
                    buildname = "⠀";
                }
                console.log(buildname);
                template = `<option>${buildname}</option>`
                $('#notifybuild').append(template);
            })
            $('.buildsel').customSelect({
                modifier: 'custom-select--dark'
            });
        }
    })
}

function changenamefull() {
    if ($('#notifytarget').val() == window.t('builder.opt_install') || $('#notifytarget').val() == window.t('builder.opt_connect')) {
        $('#notifygalkafull').html(window.t('builder.attach_screen'));
    } else if ($('#notifytarget').val() == window.t('builder.opt_steal')) {
        $('#notifygalkafull').html(window.t('builder.attach_log'));
    }
}

function changenamebuild() {
    if ($('#notifyeventbuild').val() == window.t('builder.opt_install') || $('#notifyeventbuild').val() == window.t('builder.opt_connect')) {
        $('#notifygalkabuild').html(window.t('builder.attach_screen'));
    } else if ($('#notifyeventbuild').val() == window.t('builder.opt_steal')) {
        $('#notifygalkabuild').html(window.t('builder.attach_log'));
    }
}

window.trigsel = 0;

function addtrig() {
    targetType = 0;
    switch (window.trigsel) {
        case "build":
            targetType = 1;
            target = $('#buildhwid').val();
            pretext = $('#notifybuildtextarea').val();
            methodname = $('#methodsbuild').val();
            method = window.notifymethods.find(item => item.name === methodname);

            if (target == '' || methodname == null) {
                notification('red', window.t('common.error'), window.t('notify.err_fill_blanks'));
                return;
            }

            switch ($('#notifyeventbuild').val()) {
                case window.t('builder.opt_install'):
                    if ($('#Buildattach').is(':checked')) {
                        mainevent = 4;
                    } else {
                        mainevent = 0;
                    }
                    break;
                case window.t('builder.opt_connect'):
                    if ($('#Buildattach').is(':checked')) {
                        mainevent = 5;
                    } else {
                        mainevent = 1;
                    }

                    break;
                case window.t('builder.opt_steal'):
                    if ($('#Buildattach').is(':checked')) {
                        mainevent = 3;
                    } else {
                        mainevent = 2;
                    }
                    break;
                default:
                    break;
            }
            break;

        case "panel":
            targetType = 0;
            target = 0;
            methodname = $('#notifymeth').val();
            method = window.notifymethods.find(item => item.name === methodname);
            pretext = $('#notifyalltextarea').val();

            switch ($('#notifytarget').val()) {
                case window.t('builder.opt_install'):
                    if ($('#Fullattach').is(':checked')) {
                        mainevent = 4;
                    } else {
                        mainevent = 0;
                    }
                    break;
                case window.t('builder.opt_connect'):
                    if ($('#Fullattach').is(':checked')) {
                        mainevent = 5;
                    } else {
                        mainevent = 1;
                    }
                    break;
                case window.t('builder.opt_steal'):
                    if ($('#Fullattach').is(':checked')) {
                        mainevent = 3;
                    } else {
                        mainevent = 2;
                    }
                    break;

                default:
                    break;
            }
            break;

        case "user":
            console.log('efefef');
            targetType = 2;
            target = $('#userhwid').val();
            pretext = $('#notifyusertextarea').val();
            methodname = $('#methodsuser').val();
            method = window.notifymethods.find(item => item.name === methodname);
            if (target == '' || methodname == null) {
                notification('red', window.t('common.error'), window.t('notify.err_fill_blanks'));
                return;
            }
            if ($('#Userattach').is(':checked')) {
                mainevent = 5;
            } else {
                mainevent = 1;
            }
            break;
        default:
            break;
    }
    console.log(method);
    $.ajax({
        url: '/api/notify/triggers/add',
        method: 'post',
        dataType: 'html',
        headers: {
            'Authorization': 'Bearer ' + window.localStorage.getItem("auth_token"),
        },
        data: {
            method: method.id,
            targetType: targetType,
            target: target,
            event: mainevent,
            text: pretext
        },
        success: function (data) {
            jdata = JSON.parse(data)
            if (jdata.success == true) {
                $('.mainpopup').hide();
                $('.backblur').hide();
                setTimeout(gettriggers, 1000);
            } else {
                notification('red', window.t('common.error'), jdata.result);
            }

        }
    })

}