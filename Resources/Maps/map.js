'use strict';
// Web Mercator slippy map. No remote scripts, geolocation, geocoder or native host objects.
const surface=document.getElementById('map'), tiles=document.getElementById('tiles'), markers=document.getElementById('markers');
let coverage=null;
let zoom=12, center={lat:-12.055,lon:-77.045}, stores=[], candidate=null, customerPoint=null, choosePoint=false, drag=null, moved=false, tileDeadline;
const send=m=>window.chrome.webview.postMessage(m);
const clamp=(n,a,b)=>Math.min(b,Math.max(a,n));
function checkTiles(expired=false){
 const visible=Array.from(tiles.children);if(!visible.length)return;
 if(visible.some(t=>t.dataset.status==='ok'))send({type:'tilesReady'});
 else if(expired||visible.every(t=>t.dataset.status==='error'))send({type:'offline'});
}
function project(p){const n=256*2**zoom,s=Math.sin(clamp(p.lat,-85.0511,85.0511)*Math.PI/180);return{x:(p.lon+180)/360*n,y:(.5-Math.log((1+s)/(1-s))/(4*Math.PI))*n};}
function unproject(p){const n=256*2**zoom;return{lon:((p.x/n*360)%360+360)%360-180,lat:Math.atan(Math.sinh(Math.PI*(1-2*clamp(p.y,0,n)/n)))*180/Math.PI};}
function render(){
 const c=project(center), w=surface.clientWidth,h=surface.clientHeight,left=c.x-w/2,top=c.y-h/2,n=2**zoom;
 const needed=new Set();
 for(let y=Math.floor(top/256);y<=Math.floor((top+h)/256);y++)for(let x=Math.floor(left/256);x<=Math.floor((left+w)/256);x++){
  if(y<0||y>=n)continue; const tx=(x%n+n)%n,key=`${zoom}/${x}/${y}`;needed.add(key);
  let tile=tiles.querySelector(`[data-key="${key}"]`);
  if(!tile){tile=document.createElement('img');tile.dataset.key=key;tile.dataset.status='loading';tile.alt='';tile.draggable=false;tile.referrerPolicy='strict-origin-when-cross-origin';tile.onload=()=>{tile.dataset.status='ok';checkTiles();};tile.onerror=()=>{tile.dataset.status='error';checkTiles();};tile.src=`https://tile.openstreetmap.org/${zoom}/${tx}/${y}.png`;tiles.append(tile);}
  tile.style.left=`${x*256-left}px`;tile.style.top=`${y*256-top}px`;
 }
 for(const tile of Array.from(tiles.children))if(!needed.has(tile.dataset.key))tile.remove();
 clearTimeout(tileDeadline);tileDeadline=setTimeout(()=>checkTiles(true),12000);checkTiles();
 const overlay=document.getElementById('coverage');overlay.replaceChildren();
 if(coverage)for(const polygon of coverage){const shape=document.createElementNS('http://www.w3.org/2000/svg','polygon');shape.setAttribute('points',polygon.map(v=>{const p=project(v);return `${p.x-left},${p.y-top}`}).join(' '));shape.setAttribute('fill','#188653');shape.setAttribute('fill-opacity','.22');shape.setAttribute('stroke','#188653');shape.setAttribute('stroke-width','2');overlay.append(shape);}
 markers.replaceChildren();
 for(const s of stores){const p=project(s),b=document.createElement('button');b.className='marker'+(coverage?' coverage-store':s.id===candidate?' selected':'');b.textContent='⌂ '+s.name.replace('Sucursal ','');b.setAttribute('aria-label',s.name);b.style.left=`${p.x-left}px`;b.style.top=`${p.y-top}px`;b.onpointerdown=e=>e.stopPropagation();b.onclick=e=>{e.stopPropagation();send({type:'store',id:s.id});};markers.append(b);}
 if(customerPoint){const p=project(customerPoint),dot=document.createElement('div');dot.className='customer'+(coverage?' delivery':'');dot.title='Tu punto de referencia';dot.style.left=`${p.x-left}px`;dot.style.top=`${p.y-top}px`;markers.append(dot);}
 const s=stores.find(s=>s.id===candidate),info=document.getElementById('info');info.hidden=!s;if(s)info.textContent=s.name+' · '+s.address+' · Punto ficticio de demostración';
}
surface.onpointerdown=e=>{if(e.button!==0)return;surface.setPointerCapture(e.pointerId);drag={x:e.clientX,y:e.clientY,center:project(center)};moved=false;};
surface.onpointermove=e=>{if(!drag)return;const dx=e.clientX-drag.x,dy=e.clientY-drag.y;if(Math.abs(dx)+Math.abs(dy)>4)moved=true;center=unproject({x:drag.center.x-dx,y:drag.center.y-dy});render();};
surface.onpointerup=e=>{if(!drag)return;if(choosePoint&&!moved){const r=surface.getBoundingClientRect(),c=project(center),p=unproject({x:c.x+e.clientX-r.left-r.width/2,y:c.y+e.clientY-r.top-r.height/2});send({type:'point',lat:p.lat,lon:p.lon});choosePoint=false;document.getElementById('point').setAttribute('aria-pressed','false');}drag=null;};
surface.onpointercancel=()=>drag=null;
function changeZoom(delta){zoom=clamp(zoom+delta,3,18);render();}
surface.onwheel=e=>{e.preventDefault();changeZoom(e.deltaY<0?1:-1);};
surface.onkeydown=e=>{const p=project(center);if(e.key==='+')changeZoom(1);else if(e.key==='-')changeZoom(-1);else if(['ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.key)){e.preventDefault();p.x+=e.key==='ArrowLeft'?-100:e.key==='ArrowRight'?100:0;p.y+=e.key==='ArrowUp'?-100:e.key==='ArrowDown'?100:0;center=unproject(p);render();}};
document.getElementById('zoomIn').onclick=()=>changeZoom(1);document.getElementById('zoomOut').onclick=()=>changeZoom(-1);
document.getElementById('center').onclick=()=>{center=customerPoint||stores.find(s=>s.id===candidate)||center;render();};
document.getElementById('showAll').onclick=()=>{
 if(!stores.length)return;
 const latitudes=stores.map(s=>s.lat),longitudes=stores.map(s=>s.lon);
 center={lat:(Math.min(...latitudes)+Math.max(...latitudes))/2,lon:(Math.min(...longitudes)+Math.max(...longitudes))/2};
 for(zoom=18;zoom>3;zoom--){const points=stores.map(project);if(Math.max(...points.map(p=>p.x))-Math.min(...points.map(p=>p.x))<Math.max(1,surface.clientWidth-160)&&Math.max(...points.map(p=>p.y))-Math.min(...points.map(p=>p.y))<Math.max(1,surface.clientHeight-160))break;}
 render();
};
document.getElementById('point').onclick=e=>{choosePoint=!choosePoint;e.currentTarget.setAttribute('aria-pressed',String(choosePoint));};
window.chrome.webview.addEventListener('message',e=>{const d=e.data;
 if(d.type==='centerStore'){const s=stores.find(s=>s.id===d.id);if(s){center={lat:s.lat,lon:s.lon};render();}return;}
 if(d.type!=='state'||!Array.isArray(d.stores))return;const previous=candidate;stores=d.stores;coverage=d.coverage||null;candidate=d.candidate;customerPoint=d.location;if(candidate!==previous){const s=stores.find(s=>s.id===candidate);if(s)center={lat:s.lat,lon:s.lon};}render();});
new ResizeObserver(render).observe(surface);send({type:'ready'});
window.addEventListener('offline',()=>send({type:'offline'}));


