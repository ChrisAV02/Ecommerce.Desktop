'use strict';
window.addEventListener('error', e => {
 if (e.filename && e.filename.endsWith('/map.js')) window.chrome.webview.postMessage({type:'offline'});
});
