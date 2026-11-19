// Pure map regression tests. No network, location lookup or personal coordinates.
const fs = require('node:fs'), vm = require('node:vm'), assert = require('node:assert/strict');
function element() {
 return {clientWidth:900,clientHeight:500,children:[],style:{},dataset:{},textContent:'',
 append(child){this.children.push(child);child.remove=()=>{this.children=this.children.filter(c=>c!==child)}},
 replaceChildren(){this.children=[]},querySelector(selector){const key=selector.match(/data-key="([^"]+)"/)[1];return this.children.find(c=>c.dataset.key===key)},
 attributes:{},setAttribute(name,value){this.attributes[name]=value},setPointerCapture(){},getBoundingClientRect(){return{left:0,top:0,width:900,height:500}}};
}
const nodes=new Map(),messages=[],handlers={};
const context={document:{getElementById(id){if(!nodes.has(id))nodes.set(id,element());return nodes.get(id)},createElement:element,createElementNS:element},
 window:{chrome:{webview:{postMessage:m=>messages.push(m),addEventListener:(name,callback)=>handlers[name]=callback}},addEventListener:(name,callback)=>handlers[name]=callback},
 ResizeObserver:class{constructor(cb){this.cb=cb}observe(){this.cb()}},setTimeout(){},clearTimeout(){}};
vm.createContext(context);vm.runInContext(fs.readFileSync(require('node:path').join(__dirname,'../Resources/Maps/map.js'),'utf8'),context);
const run=source=>vm.runInContext(source,context);
for(const p of [{lat:-12.0464,lon:-77.0428},{lat:0,lon:0},{lat:50,lon:179.9},{lat:-50,lon:-179.9}]){
 context.testPoint=p;const result=run('unproject(project(testPoint))');
 assert.ok(Math.abs(result.lat-p.lat)<1e-8&&Math.abs(result.lon-p.lon)<1e-8,'projection roundtrip '+JSON.stringify(p));
}
const first={id:'one',name:'Sucursal Uno',address:'Referencia ficticia',lat:-12.04,lon:-77.04};
const second={id:'two',name:'Sucursal Dos',address:'Referencia ficticia',lat:-12.08,lon:-77.08};
handlers.message({data:{type:'state',stores:[first,second],candidate:'one',location:null}});
assert.equal(nodes.get('markers').children.length,2,'renders both branch markers');
nodes.get('markers').children[1].onclick({stopPropagation(){}});
assert.ok(messages.some(m=>m.type==='store'&&m.id==='two'),'marker sends stable store id');
run('center={lat:25,lon:25}'); handlers.message({data:{type:'centerStore',id:'one'}});
assert.equal(run('center.lon'),first.lon,'explicit recenter even if already candidate');
run('center={lat:40,lon:40}');nodes.get('showAll').onclick();
assert.ok(Math.abs(run('center.lon')-(first.lon+second.lon)/2)<1e-8,'show all centers branch extent');
for(const branch of [first,second]){context.testPoint=branch;const distance=run('({x:project(testPoint).x-project(center).x,y:project(testPoint).y-project(center).y})');assert.ok(Math.abs(distance.x)<370&&Math.abs(distance.y)<170,'show all fits visible branch extent');}
handlers.message({data:{type:'centerStore',id:'one'}});
nodes.get('map').onpointerdown({button:0,pointerId:1,clientX:450,clientY:250});
nodes.get('map').onpointermove({clientX:470,clientY:250}); nodes.get('map').onpointerup({clientX:470,clientY:250});
assert.ok(Math.abs(run('center.lon')-first.lon)<.1,'short pan remains near original area');
run('changeZoom(100)');assert.equal(run('zoom'),18,'zoom upper limit');run('changeZoom(-100)');assert.equal(run('zoom'),3,'zoom lower limit');
// Simulate a loaded view followed by failure of every tile in the next area.
for(const tile of nodes.get('tiles').children){tile.onload();}
assert.ok(messages.some(m=>m.type==='tilesReady'),'reports tiles available');
run('center={lat:40,lon:40};render()');
for(const tile of [...nodes.get('tiles').children])tile.onerror();
assert.equal(messages.at(-1).type,'offline','failure of new view triggers fallback after earlier success');
console.log('PASS: map projection, panning, zoom, markers, recenter and offline regression checks.');


handlers.message({data:{type:'state',stores:[first],candidate:'one',location:{lat:first.lat,lon:first.lon},coverage:[[{lat:-12.03,lon:-77.05},{lat:-12.05,lon:-77.05},{lat:-12.05,lon:-77.03}]]}});
assert.equal(nodes.get('coverage').children.length,1,'coverage polygon rendered separately from markers');
assert.equal(nodes.get('coverage').children[0].attributes.fill,'#188653','coverage fill is green');
assert.equal(nodes.get('markers').children[0].className,'marker coverage-store','store stays blue in coverage mode');
assert.equal(nodes.get('markers').children[1].className,'customer delivery','customer gets yellow delivery class');
const shapeBefore=nodes.get('coverage').children[0].attributes.points;run('changeZoom(1)');
assert.notEqual(nodes.get('coverage').children[0].attributes.points,shapeBefore,'polygon reprojects when zoom changes');
handlers.message({data:{type:'state',stores:[first],candidate:'one',location:null}});
assert.equal(nodes.get('coverage').children.length,0,'normal store map removes optional coverage layer');
console.log('PASS: optional coverage polygons, colors, zoom projection and original store map isolation.');
