/* i18n engine
 *   - load I18N dict from <this-script-dir>/{lang}.js
 *   - replace <i18n>key</i18n> placeholders in-place
 *   - replace [i18n-attr="attr:key|attr:key"] attributes
 *   - watch DOM for newly added <i18n> nodes (loadpage injects HTML via jQuery .load)
 *   - exposes: window.t(key, vars), window.tn(key, n), window.setLang(lang), window.getLang()
 *
 *   The locale path is derived from the script's own URL, so this engine
 *   works under any mount point (e.g. /panel/, /panel3/, /login/).
 */
(function () {
    'use strict';

    const SUPPORTED = ['en', 'ru', 'uk'];
    const FALLBACK  = 'en';
    const VERSION   = 1;

    // resolve the directory of this very script — works for /panel/locales/i18n.js,
    // /panel3/locales/i18n.js, /login/locales/i18n.js, etc.
    function selfDir() {
        const src = (document.currentScript && document.currentScript.src) || '';
        return src.replace(/[^/]*$/, '');                 // strip file name → "…/locales/"
    }

    let dict = {};
    let lang = FALLBACK;

    function detectLang() {
        try {
            const s = localStorage.getItem('lang');
            if (s && SUPPORTED.indexOf(s) !== -1) return s;
        } catch (e) {}
        const m = document.cookie.match(/(?:^|;\s*)lang=([^;]+)/);
        if (m && SUPPORTED.indexOf(m[1]) !== -1) return m[1];
        return FALLBACK;
    }

    function pluralCategory(n) {
        const abs = Math.abs(parseInt(n, 10) || 0);
        if (lang !== 'ru' && lang !== 'uk') return abs === 1 ? 'one' : 'other';
        const m10 = abs % 10, m100 = abs % 100;
        if (m10 === 1 && m100 !== 11) return 'one';
        if (m10 >= 2 && m10 <= 4 && (m100 < 10 || m100 >= 20)) return 'few';
        return 'many';
    }

    window.t = function (key, vars) {
        let s = dict[key];
        if (s == null) return key;                               // fallback to key
        if (vars) for (const k in vars) s = s.split('{' + k + '}').join(vars[k]);
        return s;
    };

    window.tn = function (key, n) {
        return window.t(key + '.' + pluralCategory(n), { n: n });
    };

    function applyIn(root) {
        // <i18n>key</i18n>  ->  replaced with translated text (HTML-safe via <template>)
        const placeholders = root.querySelectorAll ? root.querySelectorAll('i18n') : [];
        placeholders.forEach(node => {
            const key = node.textContent.trim();
            if (!key) return;
            const html = window.t(key);
            const tmpl = document.createElement('template');
            tmpl.innerHTML = html;
            node.replaceWith(tmpl.content);
        });

        // [i18n-attr="placeholder:key|title:key2"]
        const attrNodes = root.querySelectorAll ? root.querySelectorAll('[i18n-attr]') : [];
        attrNodes.forEach(el => {
            const spec = el.getAttribute('i18n-attr');
            if (!spec) return;
            spec.split('|').forEach(pair => {
                const idx = pair.indexOf(':');
                if (idx < 0) return;
                const attr = pair.slice(0, idx).trim();
                const key  = pair.slice(idx + 1).trim();
                if (attr && key) el.setAttribute(attr, window.t(key));
            });
        });
    }

    function applyAll() {
        applyIn(document);
        const titleKey = document.documentElement.getAttribute('data-i18n-title');
        if (titleKey) document.title = window.t(titleKey);
        // auto-sync language switcher(s) with current lang
        document.querySelectorAll('[data-i18n-langselect]').forEach(sel => {
            if (sel.value !== lang) sel.value = lang;
        });
    }
    window.applyTranslations = applyAll;

    function startObserver() {
        if (typeof MutationObserver === 'undefined') return;
        const obs = new MutationObserver(muts => {
            for (const m of muts) {
                for (const n of m.addedNodes) {
                    if (n.nodeType !== 1) continue;
                    if (n.tagName === 'I18N') { applyAll(); return; }
                    if (n.querySelectorAll && (n.querySelectorAll('i18n').length ||
                        n.querySelectorAll('[i18n-attr]').length)) { applyAll(); return; }
                }
            }
        });
        obs.observe(document.body, { childList: true, subtree: true });
    }

    window.setLang = function (newLang) {
        if (SUPPORTED.indexOf(newLang) === -1) return;
        try { localStorage.setItem('lang', newLang); } catch (e) {}
        document.cookie = 'lang=' + newLang + ';path=/;max-age=31536000;samesite=lax';
        location.reload();
    };
    window.getLang = function () { return lang; };

    // boot
    lang = detectLang();
    document.documentElement.lang = lang;

    // expose a deferred promise for code that needs to wait for the dictionary
    window.i18nReady = {};
    window.i18nReady.promise = new Promise(function (resolve, reject) {
        window.i18nReady.resolve = resolve;
        window.i18nReady.reject  = reject;
    });

    const s = document.createElement('script');
    s.src = selfDir() + lang + '.js?v=' + VERSION;
    s.onload = function () {
        dict = window.I18N || {};
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                applyAll();
                startObserver();
                fireReady();
            });
        } else {
            applyAll();
            startObserver();
            fireReady();
        }
    };

    function fireReady() {
        // Promise form: window.i18nReady.then(() => …)
        if (window.i18nReady && typeof window.i18nReady.resolve === 'function') {
            window.i18nReady.resolve();
        }
        document.dispatchEvent(new CustomEvent('i18n:ready', { detail: { lang: lang, t: window.t, tn: window.tn } }));
    }
    s.onerror = function () {
        console.warn('[i18n] failed to load ' + selfDir() + lang + '.js');
    };
    document.head.appendChild(s);
})();
