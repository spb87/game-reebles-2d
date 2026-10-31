/* Living-painting demo — REEB-157/158
 * Scene coords: logical 1376x768. Backdrop drawn at 1.06x so the camera can drift.
 */
const W = 1376, H = 768;
const SCENE_SCALE = 1.06;
const canvas = document.getElementById('game');
const ctx = canvas.getContext('2d');

const load = src => new Promise((res, rej) => {
  const i = new Image(); i.onload = () => res(i); i.onerror = rej; i.src = src;
});

const assets = {};
Promise.all([
  load('assets/scene.png'),
  load('assets/fg_foliage.png'),
  load('assets/fog.png'),
  load('assets/reeble_front.png'),
  load('assets/reeble_side.png'),
]).then(([scene, fg, fog, front, side]) => {
  Object.assign(assets, { scene, fg, fog, front, side });
  fit();
  requestAnimationFrame(tick);
});

function fit() {
  const s = Math.min(innerWidth / W, innerHeight / H);
  canvas.style.width = W * s + 'px';
  canvas.style.height = H * s + 'px';
}
addEventListener('resize', fit);

// ---------- input ----------
const keys = {};
addEventListener('keydown', e => { keys[e.key.toLowerCase()] = true; hint.t = 9; });
addEventListener('keyup',   e => { keys[e.key.toLowerCase()] = false; });

// ---------- player ----------
const player = {
  x: 640, y: 525, dir: 'side', flip: false, moving: false, phase: 0,
};
const SPEED = 165;               // world px/s
const BOUNDS = { x0: 70, x1: 1306, y0: 445, y1: 724 };

// ---------- camera ----------
const cam = { x: 0, y: 0 };      // world-space offset applied to all world layers

// ---------- lantern glow points (matched to painted lantern strings) ----------
const lanterns = [
  [222, 348], [262, 356], [355, 382], [398, 388], [458, 382], [502, 388],
  [560, 380], [600, 376], [652, 370], [800, 316], [838, 324], [878, 330],
  [918, 344], [962, 352], [1008, 356], [1052, 348], [1102, 330],
].map(([x, y], i) => ({ x, y, seed: i * 7.13 }));

// ---------- fog wisps ----------
const wisps = [
  { x: 200,  y: 468, par: 0.92, vx: 6,  s: 0.9,  a: 0.35 },
  { x: 900,  y: 505, par: 0.95, vx: -4, s: 1.1,  a: 0.30 },
  { x: 500,  y: 640, par: 1.12, vx: 8,  s: 0.75, a: 0.28 },
  { x: 1150, y: 700, par: 1.15, vx: -7, s: 0.9,  a: 0.32 },
];

// ---------- particles: fireflies + pollen ----------
const rnd = (a, b) => a + Math.random() * (b - a);
const fireflies = Array.from({ length: 46 }, () => ({
  x: rnd(40, W - 40), y: rnd(240, H - 60),
  r: rnd(1.1, 2.4), t: rnd(0, 100),
  vx: rnd(-4, 4), vy: rnd(-3, 3), ph: rnd(0, 6.28),
}));
const motes = Array.from({ length: 26 }, () => ({
  x: rnd(0, W), y: rnd(120, H), r: rnd(1.5, 3.5),
  vx: rnd(4, 12), vy: rnd(2, 7), t: rnd(0, 100),
}));

// ---------- fountain ripples ----------
const FOUNTAIN = { x: 482, y: 452 };

// ---------- god rays ----------
const rays = [
  { x: 240, y: 60, w: 130, len: 700, ang: 0.62, a: 0.05, ph: 0 },
  { x: 420, y: 40, w: 90,  len: 640, ang: 0.62, a: 0.04, ph: 2.1 },
  { x: 1080, y: 60, w: 110, len: 660, ang: -0.60, a: 0.035, ph: 4.2 },
];

const hint = { t: 0 };
let last = 0;

function tick(ms) {
  const dt = Math.min((ms - last) / 1000 || 0.016, 0.05);
  last = ms;
  const t = ms / 1000;

  // --- update player ---
  let dx = (keys['d'] || keys['arrowright'] ? 1 : 0) - (keys['a'] || keys['arrowleft'] ? 1 : 0);
  let dy = (keys['s'] || keys['arrowdown'] ? 1 : 0) - (keys['w'] || keys['arrowup'] ? 1 : 0);
  player.moving = !!(dx || dy);
  if (player.moving) {
    const l = Math.hypot(dx, dy); dx /= l; dy /= l;
    player.x = Math.max(BOUNDS.x0, Math.min(BOUNDS.x1, player.x + dx * SPEED * dt));
    player.y = Math.max(BOUNDS.y0, Math.min(BOUNDS.y1, player.y + dy * SPEED * dt));
    player.phase += dt * 9;
    player.dir = Math.abs(dx) > Math.abs(dy) ? 'side' : 'front';
    if (dx) player.flip = dx < 0;
  } else {
    player.phase += dt * 2;
  }

  // --- camera: follow + idle sway ---
  const drift = Math.sin(t * 0.11) * 8;
  const tx = (player.x - W / 2) * 0.22 + drift;
  const ty = (player.y - H / 2) * 0.12 + Math.cos(t * 0.09) * 5;
  cam.x += (tx - cam.x) * 0.04;
  cam.y += (ty - cam.y) * 0.04;

  draw(t, dt);
  requestAnimationFrame(tick);
}

function worldToScreen(wx, wy, par = 1) {
  return [wx * SCENE_SCALE - cam.x * par, wy * SCENE_SCALE - cam.y * par];
}

function draw(t, dt) {
  ctx.clearRect(0, 0, W, H);
  const S = SCENE_SCALE, OX = -((S - 1) * W) / 2, OY = -((S - 1) * H) / 2;

  // 1) backdrop
  const sx = OX - cam.x, sy = OY - cam.y;
  ctx.drawImage(assets.scene, sx, sy, W * S, H * S);

  // 2) far fog (behind everything but sky)
  drawFog(t, w => w.par < 1);

  // 3) fountain ripples
  ctx.save();
  const [fx, fy] = worldToScreen(FOUNTAIN.x, FOUNTAIN.y);
  ctx.translate(fx + OX, fy + OY);
  for (let i = 0; i < 3; i++) {
    const p = (t * 0.5 + i / 3) % 1;
    ctx.strokeStyle = `rgba(235,225,255,${0.25 * (1 - p)})`;
    ctx.lineWidth = 1.6;
    ctx.beginPath();
    ctx.ellipse(0, 26, 20 + p * 42, 6 + p * 12, 0, 0, Math.PI * 2);
    ctx.stroke();
  }
  // sparkle on the water jet
  const tw = 0.5 + 0.5 * Math.sin(t * 6);
  ctx.fillStyle = `rgba(255,250,235,${0.25 + 0.35 * tw})`;
  ctx.beginPath(); ctx.arc(0, -46, 2.2, 0, 7); ctx.fill();
  ctx.restore();

  // 4) lantern glows (world-anchored, flickering)
  ctx.save();
  ctx.globalCompositeOperation = 'lighter';
  for (const L of lanterns) {
    const [lx, ly] = worldToScreen(L.x, L.y);
    const flick = 0.72 + 0.28 * Math.sin(t * 3.1 + L.seed) * Math.sin(t * 1.7 + L.seed * 2);
    const r = 30 + 9 * flick;
    const g = ctx.createRadialGradient(lx + OX, ly + OY, 0, lx + OX, ly + OY, r);
    g.addColorStop(0, `rgba(255,196,110,${0.34 * flick})`);
    g.addColorStop(0.55, `rgba(255,160,80,${0.10 * flick})`);
    g.addColorStop(1, 'rgba(255,140,60,0)');
    ctx.fillStyle = g;
    ctx.fillRect(lx + OX - r, ly + OY - r, r * 2, r * 2);
  }
  ctx.restore();

  // 5) god rays
  ctx.save();
  ctx.globalCompositeOperation = 'screen';
  for (const r of rays) {
    const pulse = 0.6 + 0.4 * Math.sin(t * 0.5 + r.ph);
    ctx.save();
    ctx.translate(r.x + OX - cam.x * 0.9, r.y + OY - cam.y * 0.9);
    ctx.rotate(r.ang);
    const g = ctx.createLinearGradient(0, 0, 0, r.len);
    g.addColorStop(0, `rgba(255,214,150,${r.a * pulse})`);
    g.addColorStop(1, 'rgba(255,214,150,0)');
    ctx.fillStyle = g;
    ctx.fillRect(-r.w / 2, 0, r.w, r.len);
    ctx.restore();
  }
  ctx.restore();

  // 6) reeble: shadow + sprite (scale with depth)
  const depth = (player.y - BOUNDS.y0) / (BOUNDS.y1 - BOUNDS.y0);
  const scale = (0.34 + depth * 0.30);
  const img = player.dir === 'side' ? assets.side : assets.front;
  const [px, py] = worldToScreen(player.x, player.y);
  const dw = img.width * scale * 0.22, dh = img.height * scale * 0.22;
  const bob = player.moving ? Math.abs(Math.sin(player.phase)) * dh * 0.05
                            : Math.sin(player.phase) * dh * 0.012;
  const squash = player.moving ? 1 + Math.sin(player.phase * 2) * 0.04 : 1;

  ctx.save();
  ctx.translate(px + OX, py + OY);
  // blob shadow
  ctx.fillStyle = 'rgba(40,25,40,0.34)';
  ctx.beginPath();
  ctx.ellipse(0, dh * 0.34, dw * 0.42, dh * 0.10, 0, 0, Math.PI * 2);
  ctx.fill();
  // sprite
  ctx.scale(player.flip ? -1 : 1, 1);
  ctx.scale(1, squash);
  ctx.drawImage(img, -dw / 2, -dh * 0.62 - bob, dw, dh);
  ctx.restore();

  // 7) near fog (in front of character)
  drawFog(t, w => w.par >= 1);

  // 8) foreground foliage — horizontal parallax only, pinned to frame bottom
  {
    const fw = W * S * 1.1, fh = assets.fg.height * (fw / assets.fg.width);
    const fx2 = OX - cam.x * 1.18 - (fw - W * S) / 2;
    const fy2 = H - fh * 0.72;
    ctx.drawImage(assets.fg, fx2, fy2, fw, fh);
  }

  // 9) particles
  ctx.save();
  ctx.globalCompositeOperation = 'lighter';
  for (const f of fireflies) {
    f.t += dt; f.x += f.vx * dt; f.y += f.vy * dt + Math.sin(f.t + f.ph) * 6 * dt;
    if (f.x < 20 || f.x > W - 20) f.vx *= -1;
    if (f.y < 220 || f.y > H - 40) f.vy *= -1;
    const [fx2, fy2] = worldToScreen(f.x, f.y, 1.05);
    const a = 0.25 + 0.75 * Math.max(0, Math.sin(f.t * 1.9 + f.ph));
    const g = ctx.createRadialGradient(fx2 + OX, fy2 + OY, 0, fx2 + OX, fy2 + OY, f.r * 4);
    g.addColorStop(0, `rgba(255,230,140,${0.75 * a})`);
    g.addColorStop(1, 'rgba(255,200,80,0)');
    ctx.fillStyle = g;
    ctx.fillRect(fx2 + OX - f.r * 4, fy2 + OY - f.r * 4, f.r * 8, f.r * 8);
  }
  for (const m of motes) {
    m.t += dt; m.x += m.vx * dt; m.y += m.vy * dt;
    if (m.x > W + 20) { m.x = -20; m.y = rnd(120, H); }
    if (m.y > H + 20) { m.y = -20; }
    ctx.fillStyle = 'rgba(255,235,190,0.10)';
    ctx.beginPath(); ctx.arc(m.x + OX, m.y + OY, m.r, 0, 7); ctx.fill();
  }
  ctx.restore();

  // 10) vignette + grade
  const vg = ctx.createRadialGradient(W / 2, H * 0.55, H * 0.42, W / 2, H * 0.55, H * 0.95);
  vg.addColorStop(0, 'rgba(0,0,0,0)');
  vg.addColorStop(1, 'rgba(30,12,30,0.42)');
  ctx.fillStyle = vg;
  ctx.fillRect(0, 0, W, H);
  const warm = ctx.createLinearGradient(0, 0, 0, H);
  warm.addColorStop(0, 'rgba(255,170,90,0.05)');
  warm.addColorStop(0.5, 'rgba(0,0,0,0)');
  warm.addColorStop(1, 'rgba(60,20,50,0.10)');
  ctx.fillStyle = warm;
  ctx.fillRect(0, 0, W, H);

  // 11) hint
  hint.t += dt;
  if (hint.t < 6) {
    ctx.fillStyle = `rgba(255,246,230,${Math.min(0.85, 6 - hint.t)})`;
    ctx.font = '15px Georgia, serif';
    ctx.textAlign = 'center';
    ctx.fillText('WASD or arrow keys — walk the square', W / 2, H - 22);
  }
}

function drawFog(t, pick) {
  ctx.save();
  for (const w of wisps) {
    if (!pick(w)) continue;
    w.x += w.vx * 0.016;
    if (w.x > W + 350) w.x = -350;
    if (w.x < -350) w.x = W + 350;
    const [fx, fy] = worldToScreen(w.x, w.y, w.par);
    const fw = assets.fog.width * w.s, fh = assets.fog.height * w.s;
    const S = SCENE_SCALE, OX = -((S - 1) * W) / 2, OY = -((S - 1) * H) / 2;
    ctx.globalAlpha = w.a * (0.8 + 0.2 * Math.sin(t * 0.4 + w.x * 0.01));
    ctx.drawImage(assets.fog, fx + OX - fw / 2, fy + OY - fh / 2, fw, fh);
  }
  ctx.restore();
}
