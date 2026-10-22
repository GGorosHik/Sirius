function scriptsappend(){for(var t in $("#scripts").html(""),window.scripts){t=`
                <div class="buildchiled" style="display:flex; justify-content:space-between; align-items:center; padding: 12px 20px; margin-bottom: 8px;">
                    <div style="font-size: 16px; font-weight: bold; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 65%;">
                        ${fixtext(t)}
                    </div>
                    <div class="linebuildchiled" style="display:flex; gap:10px; width: auto;">
                        <div class="button1" style="min-width: 70px; padding: 4px 12px;" onclick="confirm('delite script','${fixtext(t)}','delscript(\`${fixtext(t)}\`)')">Delete</div>
                        <div class="button1" style="min-width: 70px; padding: 4px 12px;" name="${fixtext(t)}" onclick="showeditor(window.scripts['${fixtext(t)}'],this);">Edit</div>
                    </div>
                </div>
        `;$("#scripts").append(t)}}function showeditor(t,i){null!=i&&(window.lastedit=$(i).attr("name"),$("#scriptname").html($(i).attr("name"))),$("#editorPopup").show(),$(".backblur").show(),null==window.editor?($("#container").html("Loading..."),setTimeout(()=>{"undefined"!=typeof monaco?window.monacowait=setInterval(()=>{"undefined"!=typeof monaco&&(clearInterval(window.monacowait),$("#container").html(""),window.editor=monaco.editor.create($("#container")[0],{value:t,language:"powershell",theme:"vs-dark"}),window.editor.setValue(t))},100):($("#container").html(""),window.editor=monaco.editor.create($("#container")[0],{value:t,language:"powershell",theme:"vs-dark"}),window.editor.setValue(t))},100)):window.editor.setValue(t)}function delscript(t){delete window.scripts[t],writeScripts(dataStr=JSON.stringify(window.scripts)),scriptsappend()}function addScriptButton(t){addscript(t,"",!1)||(console.log("tut dolzhen bit popup"),confirm("udalit","ovverride "+t+" ?","addscript('"+t+"','',true)"))}function savescriptbutton(){window.lastedit&&(console.log("Saving script: "+window.lastedit),addscript(window.lastedit,window.editor.getValue(),!0))}function addscript(t,i,e){return scripts=window.scripts,console.log(i),void 0===scripts[t]||e||(t=(t=>{let i=t,e=1;for(;void 0!==scripts[i];)i=t+" "+e,e++;return i})(t)),scripts[t]=i,writeScripts(dataStr=JSON.stringify(scripts)),scriptsappend(),$("#scriptname").html(t),!0}function showaddscript(){var t,i="#PowerShell";addscript(t=(t=>{var i=Object.keys(window.scripts||{});let e=t,n=1;for(;i.includes(e);)e=t+" "+n,n++;return e})("New script"),i,!1),scriptsappend(),window.lastedit=t,showeditor(i,null),$("#scriptname").html(t)}function renameclose(){$("#scriptnamediv").html(""),$("#scriptnamediv").html("<b>"+window.lastedit+"</b>")}function renamescript(){console.log("renaming script"),delscript(window.lastedit),scripts=window.scripts;for(var t=fixtext($("#renscriptinp").val());null!=scripts[t];)t+=" 2";$("#renscriptinp").val(t),addscript(t,window.editor.getValue(),!1),setTimeout(()=>{window.lastedit=t,$("#scriptnamediv").html(""),$("#scriptnamediv").html('<b id="scriptname">'+fixtext(window.lastedit)+"</b>"),scriptsappend()},200),window.openrenamescript=!1}function renamescriptshow(){console.log("renaming show"),$("#renscriptinp").is(":visible")||($("#scriptnamediv").html('<input id="renscriptinp" type="text" value="" placeholder="name"><div class="buttoncfg" onclick="renamescript();" style="margin-top: -1px; height: 18px; width: 20px; text-align: center; border-bottom: var(--color1) 2px solid;">✓</div>'),$("#renscriptinp").val(window.lastedit),console.log("ren true"),window.openrenamescript=!0)}function writeScripts(i){
    var payload = typeof i === 'string' ? i : JSON.stringify(i);
    $.ajax({
        url: '/api/scripts/save',
        method: 'POST',
        contentType: 'application/json',
        data: payload
    });
    (request=indexedDB.open("scriptDatabase",5)).onupgradeneeded=function(t){t.target.result.createObjectStore("scripts",{keyPath:"id"})},request.onsuccess=function(t){t.target.result.transaction(["scripts"],"readwrite").objectStore("scripts").put({id:"scriptsJson",data:payload})},request.onerror=function(t){console.log(t)}
}
function initScripts(){writeScripts(JSON.stringify(window.scripts)),scriptsappend()}
window.scripts={"no info":"plz update scripts list"},window.editor=null,window.openrenamescript=!1,$(document).ready(function(){
    "2"==readCookie("bgmode")&&$("body").css("background",readCookie("bgcolor"));
    $.ajax({
        url: '/api/scripts',
        method: 'POST',
        success: function(resp){
            if(resp && resp.success && resp.result && Object.keys(resp.result).length > 0){
                window.scripts = resp.result;
                scriptsappend();
            }
        }
    });
    (request=indexedDB.open("scriptDatabase",5)).onupgradeneeded=function(t){t.target.result.createObjectStore("scripts",{keyPath:"id"})},request.onsuccess=function(t){let i=t.target.result.transaction(["scripts"],"readonly").objectStore("scripts").get("scriptsJson");i.onsuccess=function(){console.log(i.result),i.result&&(window.scripts=JSON.parse(i.result.data),scriptsappend())},i.onerror=initScripts},request.onerror=initScripts
});
