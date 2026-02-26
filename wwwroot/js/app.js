// ═══════════════════════════════════════════════════════════════════════════
//  State
// ═══════════════════════════════════════════════════════════════════════════
let tourney          = null;
let users            = [];
let editUser         = null;
let ranking          = null;
let mode             = null;              // 'admin' | 'user'
let currentUser      = null;              // User object when mode === 'user'
let isBettingLocked  = false;             // true when dataset != 'vor_turnier'
let selectedDataset  = 'vor_turnier';     // 'vor_turnier' | 'nach_gruppe' | 'nach_turnier'

// ═══════════════════════════════════════════════════════════════════════════
//  Bootstrap – load tournament data, then show login
// ═══════════════════════════════════════════════════════════════════════════
async function init() {
  showSpinner();
  try {
    tourney = await fetchJSON('/api/tournament');
  } catch(e) {
    showToast('Fehler beim Laden der Turnierdaten: ' + e.message, true);
  }
  hideSpinner();
}

// ═══════════════════════════════════════════════════════════════════════════
//  Login
// ═══════════════════════════════════════════════════════════════════════════
async function doLogin(e) {
  e.preventDefault();
  const q = document.getElementById('login-input').value.trim();
  if (!q) return;

  selectedDataset = document.getElementById('login-dataset-select').value || 'vor_turnier';
  document.getElementById('login-error').style.display = 'none';

  // Turnierdaten für das gewählte Dataset (neu) laden
  showSpinner();
  try {
    tourney = await fetchJSON('/api/tournament?dataset=' + encodeURIComponent(selectedDataset));
  } catch(e) {
    showToast('Fehler beim Laden der Turnierdaten: ' + e.message, true);
    hideSpinner();
    return;
  }

  if (q.toLowerCase() === 'admin') {
    try {
      users = await fetchJSON('/api/users');
      activateAdminMode();
    } catch(err) { showToast('Fehler: ' + err.message, true); }
    hideSpinner();
    return;
  }

  try {
    const found = await fetchJSON('/api/users/lookup?q=' + encodeURIComponent(q));
    activateUserMode(found);
  } catch(err) {
    const errEl = document.getElementById('login-error');
    errEl.textContent = 'Tipper "' + q + '" nicht gefunden. Bitte Namen oder Tipper-ID pr\u00fcfen.';
    errEl.style.display = 'block';
  }
  hideSpinner();
}

function doLogout() {
  mode = null;
  currentUser = null;
  isBettingLocked = false;
  selectedDataset = 'vor_turnier';
  ranking = null;
  document.getElementById('login-input').value = '';
  document.getElementById('login-dataset-select').value = 'vor_turnier';
  document.getElementById('login-error').style.display = 'none';
  document.getElementById('login-overlay').style.display = '';
  document.getElementById('main-header').style.display = 'none';
  document.getElementById('main-nav').style.display = 'none';
  document.querySelectorAll('.main-page').forEach(p => p.style.display = 'none');
}

function activateAdminMode() {
  mode = 'admin';
  isBettingLocked = selectedDataset !== 'vor_turnier';
  document.getElementById('login-overlay').style.display = 'none';
  document.getElementById('main-header').style.display = '';
  document.getElementById('header-sub').textContent = 'Admin-Modus \u2013 Verwaltung der Tipper & Auswertung';
  document.getElementById('main-nav').style.display = '';
  document.getElementById('main-nav').innerHTML =
    '<button class="nav-btn active" onclick="switchMain(this,\'page-users\')">&#128101; Tipper</button>' +
    '<button class="nav-btn" onclick="switchMain(this,\'page-ranking\')">&#127942; Auswertung</button>';

  document.getElementById('admin-calc-bar').style.display = '';
  document.getElementById('user-calc-bar').style.display  = 'none';

  showMainPage('page-users');
  renderUserList();
  autoRecalculate();
}

function activateUserMode(user) {
  mode        = 'user';
  currentUser = user;

  isBettingLocked = selectedDataset !== 'vor_turnier';

  document.getElementById('login-overlay').style.display = 'none';
  document.getElementById('main-header').style.display = '';
  document.getElementById('header-sub').textContent =
    '\uD83D\uDC4B ' + user.Name + ' (' + user.UserId + ')';
  document.getElementById('main-nav').style.display = '';
  document.getElementById('main-nav').innerHTML =
    '<button class="nav-btn active" onclick="switchMain(this,\'page-my-tips\')">&#9997; Meine Tipps</button>' +
    '<button class="nav-btn" onclick="switchMain(this,\'page-ranking\')">&#127942; Auswertung</button>';

  document.getElementById('admin-calc-bar').style.display = 'none';
  document.getElementById('user-calc-bar').style.display  = '';

  showMainPage('page-my-tips');
  renderMyTips(currentUser);
  autoRecalculate();
}

function showMainPage(pageId) {
  document.querySelectorAll('.main-page').forEach(p => p.style.display = 'none');
  const el = document.getElementById(pageId);
  if (el) el.style.display = 'block';
  document.querySelectorAll('.nav-btn').forEach(b => {
    const fn = b.getAttribute('onclick') || '';
    b.classList.toggle('active', fn.includes("'" + pageId + "'"));
  });
}

// ═══════════════════════════════════════════════════════════════════════════
//  Auto-recalculate (silent) & ranking loader
// ═══════════════════════════════════════════════════════════════════════════
async function autoRecalculate() {
  try {
    const url = '/api/ranking/recalculate' +
      (selectedDataset ? '?dataset=' + encodeURIComponent(selectedDataset) : '');
    const resp = await fetch(url, { method: 'POST' });
    if (resp.ok) {
      ranking = await resp.json();
      renderRanking();
    } else {
      await tryLoadRanking();
    }
  } catch(_) {
    await tryLoadRanking();
  }
}

async function tryLoadRanking() {
  try {
    ranking = await fetchJSON('/api/ranking');
    renderRanking();
  } catch(_) { /* noch kein Ranking */ }
}

// ═══════════════════════════════════════════════════════════════════════════
//  Main page switching
// ═══════════════════════════════════════════════════════════════════════════
function switchMain(btn, pageId) {
  document.querySelectorAll('.nav-btn').forEach(b => b.classList.remove('active'));
  document.querySelectorAll('.main-page').forEach(p => p.style.display = 'none');
  btn.classList.add('active');
  document.getElementById(pageId).style.display = 'block';
}

// ═══════════════════════════════════════════════════════════════════════════
//  Admin: User List
// ═══════════════════════════════════════════════════════════════════════════
function renderUserList() {
  const wrap = document.getElementById('users-table-wrap');
  if (!users.length) {
    wrap.innerHTML = '<div class="empty-state">Noch keine Tipper. Klicke &bdquo;+ Neuer Tipper&ldquo;.</div>';
    return;
  }
  let html = '<table><thead><tr><th>#</th><th>ID</th><th>Name</th><th>Tipps</th><th>Bingo</th><th>Aktionen</th></tr></thead><tbody>';
  users.forEach((u, i) => {
    const bets = u.BetData?.GroupMatchBets?.length ?? 0;
    html += `<tr>
      <td>${i+1}</td>
      <td><span class="badge badge-blue">${esc(u.UserId)}</span></td>
      <td><strong>${esc(u.Name)}</strong></td>
      <td>${bets} Spieltipps</td>
      <td>${u.BetData?.BingoCard?.Cells?.length ?? 0} Felder</td>
      <td style="display:flex;gap:.4rem;">
        <button class="btn btn-ghost btn-sm" onclick="editUserById('${esc(u.UserId)}')">Bearbeiten</button>
        <button class="btn btn-danger btn-sm" onclick="deleteUser('${esc(u.UserId)}','${esc(u.Name)}')">L\u00f6schen</button>
      </td>
    </tr>`;
  });
  html += '</tbody></table>';
  wrap.innerHTML = html;
}

// ═══════════════════════════════════════════════════════════════════════════
//  Admin: User Form
// ═══════════════════════════════════════════════════════════════════════════
async function newUser() {
  showSpinner();
  try {
    const tmpl = await fetchJSON('/api/user-template');
    editUser = null;
    showForm(JSON.parse(JSON.stringify(tmpl)));
  } catch(e) { showToast('Fehler: ' + e.message, true); }
  hideSpinner();
}

function editUserById(id) {
  const u = users.find(x => x.UserId === id);
  if (u) { editUser = id; showForm(JSON.parse(JSON.stringify(u))); }
}

function showForm(userData) {
  document.getElementById('view-list').style.display = 'none';
  document.getElementById('view-form').style.display = 'block';
  document.getElementById('form-title').textContent = editUser
    ? ('Tipper bearbeiten \u2013 ' + userData.Name) : 'Neuer Tipper';

  document.querySelectorAll('#view-form .tab-btn').forEach((b,i) => b.classList.toggle('active', i===0));
  document.querySelectorAll('#view-form .tab-panel').forEach((p,i) => p.classList.toggle('active', i===0));

  document.getElementById('f-id').value   = userData.UserId || '';
  document.getElementById('f-name').value = userData.Name || '';

  const wc = document.getElementById('f-worldchamp');
  wc.innerHTML = '<option value="">-- Team ausw\u00e4hlen --</option>';
  tourney.Teams.forEach(t => {
    const opt = document.createElement('option');
    opt.value = t.TeamId;
    opt.textContent = t.DisplayName + ' (' + t.TeamId + ')';
    if (userData.BetData?.SpecialBets?.WorldChampionTeamId === t.TeamId) opt.selected = true;
    wc.appendChild(opt);
  });
  document.getElementById('f-topscorer').value = userData.BetData?.SpecialBets?.TopScorerName || '';

  renderGroupMatches(userData);
  renderKORounds(userData);
  renderBingoCard(userData);
  document.getElementById('view-form').dataset.userJson = JSON.stringify(userData);
}

function showList() {
  document.getElementById('view-list').style.display = 'block';
  document.getElementById('view-form').style.display = 'none';
}

// ─── Group match bets (admin) ──────────────────────────────────────────────
function renderGroupMatches(userData) {
  const container = document.getElementById('group-matches-container');
  const betMap = {};
  (userData.BetData?.GroupMatchBets || []).forEach(b => { betMap[b.MatchId] = b; });
  const teamMap = {};
  tourney.Teams.forEach(t => { teamMap[t.TeamId] = t; });
  const grouped = {};
  tourney.MatchResults.forEach(m => {
    if (!grouped[m.GroupName]) grouped[m.GroupName] = [];
    grouped[m.GroupName].push(m);
  });
  let html = '';
  Object.entries(grouped).forEach(([g, matches]) => {
    html += `<div class="group-section">
      <div class="group-header" onclick="toggleGroup(this)">
        <span>Gruppe ${g}</span><span style="color:var(--muted);font-size:.8rem;">&#9660;</span>
      </div><div class="group-body">`;
    matches.forEach(m => {
      const home = teamMap[m.HomeTeamId] || { DisplayName: m.HomeTeamId, FlagCode: '' };
      const away = teamMap[m.AwayTeamId] || { DisplayName: m.AwayTeamId, FlagCode: '' };
      const bet  = betMap[m.MatchId] || { HomeGoals: 0, AwayGoals: 0 };
      html += `<div class="match-bet" data-matchid="${m.MatchId}">
        <span class="team-name home-name">${flagImg(home.FlagCode)} ${esc(home.DisplayName)}</span>
        <span></span>
        <input type="number" class="score-input" min="0" max="20" value="${bet.HomeGoals}" data-role="home">
        <span class="vs">:</span>
        <input type="number" class="score-input" min="0" max="20" value="${bet.AwayGoals}" data-role="away">
        <span></span>
        <span class="team-name">${esc(away.DisplayName)} ${flagImg(away.FlagCode)}</span>
      </div>`;
    });
    html += '</div></div>';
  });
  container.innerHTML = html;
}

function toggleGroup(header) {
  const body = header.nextElementSibling;
  const open = body.style.display !== 'none';
  body.style.display = open ? 'none' : '';
  header.querySelector('span:last-child').textContent = open ? '\u25b6' : '\u25bc';
}

// ─── KO rounds (admin) ────────────────────────────────────────────────────
const KO_ROUNDS = [
  { key: 'RoundOf32',    label: 'Sechzehntelfinale', count: 32, pts: 2 },
  { key: 'RoundOf16',    label: 'Achtelfinale',       count: 16, pts: 4 },
  { key: 'QuarterFinal', label: 'Viertelfinale',      count:  8, pts: 6 },
  { key: 'SemiFinal',    label: 'Halbfinale',         count:  4, pts: 8 },
  { key: 'Final',        label: 'Finale',             count:  2, pts:10 },
];

function renderKORounds(userData) {
  const container = document.getElementById('ko-rounds-container');
  const bets = userData.BetData?.KnockoutBets || {};
  let html = '';
  KO_ROUNDS.forEach(r => {
    const selected = new Set(bets[r.key] || []);
    html += `<div class="card" id="ko-${r.key}">
      <div class="section-title">${r.label} <span class="badge badge-blue">${r.pts} Pkt/Team</span>
        <span class="round-count" id="cnt-${r.key}">(${selected.size}/${r.count})</span>
      </div><div class="team-grid" id="grid-${r.key}">`;
    tourney.Teams.forEach(t => {
      const sel = selected.has(t.TeamId);
      html += `<div class="team-chip ${sel?'selected':''}" data-team="${t.TeamId}"
        onclick="toggleTeam('${r.key}','${t.TeamId}',${r.count},this)">
        ${flagImg(t.FlagCode)} ${esc(t.DisplayName)}</div>`;
    });
    html += `</div></div>`;
  });
  container.innerHTML = html;
  KO_ROUNDS.forEach(r => updateRoundCount(r.key, r.count));
}

function toggleTeam(roundKey, teamId, maxCount, chip) {
  if (chip.classList.contains('selected')) {
    chip.classList.remove('selected');
  } else {
    const cnt = document.querySelectorAll('#grid-' + roundKey + ' .team-chip.selected').length;
    if (cnt >= maxCount) { showToast('Max. ' + maxCount + ' Teams!', true); return; }
    chip.classList.add('selected');
  }
  updateRoundCount(roundKey, maxCount);
}

function updateRoundCount(roundKey, maxCount) {
  const grid = document.getElementById('grid-' + roundKey);
  if (!grid) return;
  const cnt = grid.querySelectorAll('.team-chip.selected').length;
  const el  = document.getElementById('cnt-' + roundKey);
  if (!el) return;
  el.textContent = '(' + cnt + '/' + maxCount + ')';
  el.className = 'round-count ' + (cnt === maxCount ? 'ok' : cnt > maxCount ? 'over' : '');
}

// ─── Bingo card (admin) ───────────────────────────────────────────────────
function renderBingoCard(userData) {
  const container = document.getElementById('bingo-grid-container');
  const cells = userData.BetData?.BingoCard?.Cells || [];
  const cellMap = {};
  cells.forEach(c => { cellMap[c.Position] = c; });
  const eventMap = {};
  tourney.BingoEventCatalog.forEach(e => { eventMap[e.EventId] = e; });
  let html = '';
  for (let pos = 0; pos < 16; pos++) {
    const cell = cellMap[pos] || { Position: pos, EventId: '', IsFulfilled: false };
    const currentId = cell.EventId || '';
    const desc = eventMap[currentId]?.Description || '';
    html += `<div class="bingo-cell">
      <span class="pos-badge">${pos}</span>
      <select class="bingo-select" data-pos="${pos}" onchange="onBingoChange(this)">
        <option value="">-- w\u00e4hlen --</option>
        ${tourney.BingoEventCatalog.map(e =>
          `<option value="${e.EventId}" ${e.EventId===currentId?'selected':''}>${esc(e.Description)}</option>`
        ).join('')}
      </select>
      <div class="event-desc" id="bdesc-${pos}">${esc(desc)}</div>
    </div>`;
  }
  container.innerHTML = html;
  refreshBingoSelects();
}

function onBingoChange(sel) {
  const pos = sel.dataset.pos;
  const eventMap = {};
  tourney.BingoEventCatalog.forEach(e => { eventMap[e.EventId] = e; });
  document.getElementById('bdesc-' + pos).textContent = eventMap[sel.value]?.Description || '';
  refreshBingoSelects();
}

function refreshBingoSelects() {
  const used = {};
  document.querySelectorAll('#bingo-grid-container .bingo-select').forEach(s => {
    if (s.value) used[s.value] = (used[s.value] || 0) + 1;
  });
  document.querySelectorAll('#bingo-grid-container .bingo-select').forEach(sel => {
    const myVal = sel.value;
    Array.from(sel.options).forEach(opt => {
      if (!opt.value) return;
      opt.disabled = used[opt.value] > 1 || (used[opt.value] === 1 && opt.value !== myVal);
    });
  });
}

// ─── Tab switching ─────────────────────────────────────────────────────────
function switchTab(btn, panelId) {
  const parent = btn.closest('.tab-bar').parentElement;
  parent.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
  parent.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
  btn.classList.add('active');
  document.getElementById(panelId).classList.add('active');
}

// ─── Collect form data (admin) ──────────────────────────────────────────────
function collectFormData() {
  const base = JSON.parse(document.getElementById('view-form').dataset.userJson || '{}');
  base.UserId = document.getElementById('f-id').value.trim();
  base.Name   = document.getElementById('f-name').value.trim();
  if (!base.Name) { showToast('Name ist erforderlich!', true); return null; }

  const bets = [];
  document.querySelectorAll('#group-matches-container .match-bet').forEach(row => {
    bets.push({ MatchId: row.dataset.matchid,
      HomeGoals: parseInt(row.querySelector('[data-role=home]').value) || 0,
      AwayGoals: parseInt(row.querySelector('[data-role=away]').value) || 0 });
  });
  base.BetData.GroupMatchBets = bets;

  const koBets = {};
  KO_ROUNDS.forEach(r => {
    koBets[r.key] = Array.from(document.querySelectorAll('#grid-' + r.key + ' .team-chip.selected')).map(c => c.dataset.team);
  });
  base.BetData.KnockoutBets = koBets;
  base.BetData.SpecialBets = {
    WorldChampionTeamId: document.getElementById('f-worldchamp').value,
    TopScorerName:       document.getElementById('f-topscorer').value.trim(),
  };

  const bingoCells = [];
  for (let pos = 0; pos < 16; pos++) {
    const sel = document.querySelector('#bingo-grid-container .bingo-select[data-pos="' + pos + '"]');
    bingoCells.push({ Position: pos, EventId: sel ? sel.value : '', IsFulfilled: false, FulfilledAt: null });
  }
  const evtIds = bingoCells.filter(c => c.EventId).map(c => c.EventId);
  if (evtIds.filter((v,i,a) => a.indexOf(v) !== i).length) { showToast('Bingo: Doppelte Ereignisse!', true); return null; }
  base.BetData.BingoCard = { Cells: bingoCells };
  return base;
}

async function saveUser() {
  const data = collectFormData();
  if (!data) return;
  showSpinner();
  try {
    const isNew = !editUser;
    const resp = await fetch(isNew ? '/api/users' : '/api/users/' + encodeURIComponent(editUser),
      { method: isNew ? 'POST' : 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) });
    if (!resp.ok) throw new Error(await resp.text());
    users = await fetchJSON('/api/users');
    renderUserList();
    showList();
    showToast(isNew ? 'Tipper angelegt!' : 'Tipper gespeichert!');
  } catch(e) { showToast('Fehler: ' + e.message, true); }
  hideSpinner();
}

async function deleteUser(id, name) {
  if (!confirm('Tipper "' + name + '" wirklich l\u00f6schen?')) return;
  showSpinner();
  try {
    const resp = await fetch('/api/users/' + encodeURIComponent(id), { method: 'DELETE' });
    if (!resp.ok) throw new Error(await resp.text());
    users = await fetchJSON('/api/users');
    renderUserList();
    showToast('Tipper gel\u00f6scht.');
  } catch(e) { showToast('Fehler: ' + e.message, true); }
  hideSpinner();
}

// ═══════════════════════════════════════════════════════════════════════════
//  USER MODE – «Meine Tipps»
// ═══════════════════════════════════════════════════════════════════════════
function renderMyTips(userData) {
  document.getElementById('my-lock-banner').style.display = isBettingLocked ? ''     : 'none';
  document.getElementById('my-save-bar').style.display    = isBettingLocked ? 'none' : '';

  // Reset to first tab
  const bar = document.getElementById('my-tab-bar');
  bar.querySelectorAll('.tab-btn').forEach((b, i) => b.classList.toggle('active', i === 0));
  ['mt-groups','mt-ko','mt-special','mt-bingo'].forEach((id, i) => {
    const el = document.getElementById(id);
    if (el) el.classList.toggle('active', i === 0);
  });

  if (isBettingLocked) {
    renderMyGroupMatchesReadOnly(userData);
    renderMyKORoundsReadOnly(userData);
    renderMySpecialReadOnly(userData);
    renderMyBingoCardReadOnly(userData);
  } else {
    renderMyGroupMatches(userData);
    renderMyKORounds(userData);
    renderMySpecial(userData);
    renderMyBingoCard(userData);
  }
  document.getElementById('page-my-tips').dataset.userJson = JSON.stringify(userData);
}

// ─── My Group Matches – editable ──────────────────────────────────────────
function renderMyGroupMatches(userData) {
  const container = document.getElementById('my-group-matches-container');
  const betMap = {};
  (userData.BetData?.GroupMatchBets || []).forEach(b => { betMap[b.MatchId] = b; });
  const teamMap = {};
  tourney.Teams.forEach(t => { teamMap[t.TeamId] = t; });
  const grouped = {};
  tourney.MatchResults.forEach(m => {
    if (!grouped[m.GroupName]) grouped[m.GroupName] = [];
    grouped[m.GroupName].push(m);
  });
  let html = '';
  Object.entries(grouped).forEach(([g, matches]) => {
    html += `<div class="group-section">
      <div class="group-header" onclick="toggleGroup(this)">
        <span>Gruppe ${g}</span><span style="color:var(--muted);font-size:.8rem;">&#9660;</span>
      </div><div class="group-body">`;
    matches.forEach(m => {
      const home = teamMap[m.HomeTeamId] || { DisplayName: m.HomeTeamId, FlagCode: '' };
      const away = teamMap[m.AwayTeamId] || { DisplayName: m.AwayTeamId, FlagCode: '' };
      const bet  = betMap[m.MatchId] || { HomeGoals: 0, AwayGoals: 0 };
      html += `<div class="match-bet" data-matchid="${m.MatchId}">
        <span class="team-name home-name">${flagImg(home.FlagCode)} ${esc(home.DisplayName)}</span>
        <span></span>
        <input type="number" class="score-input" min="0" max="20" value="${bet.HomeGoals}" data-role="home">
        <span class="vs">:</span>
        <input type="number" class="score-input" min="0" max="20" value="${bet.AwayGoals}" data-role="away">
        <span></span>
        <span class="team-name">${esc(away.DisplayName)} ${flagImg(away.FlagCode)}</span>
      </div>`;
    });
    html += '</div></div>';
  });
  container.innerHTML = html;
}

// ─── My Group Matches – read-only with points ─────────────────────────────
function renderMyGroupMatchesReadOnly(userData) {
  const container = document.getElementById('my-group-matches-container');
  const betMap = {};
  (userData.BetData?.GroupMatchBets || []).forEach(b => { betMap[b.MatchId] = b; });
  const teamMap = {};
  tourney.Teams.forEach(t => { teamMap[t.TeamId] = t; });
  const matchMap = {};
  tourney.MatchResults.forEach(m => { matchMap[m.MatchId] = m; });
  const grouped = {};
  tourney.MatchResults.forEach(m => {
    if (!grouped[m.GroupName]) grouped[m.GroupName] = [];
    grouped[m.GroupName].push(m);
  });

  let html = '';
  Object.entries(grouped).forEach(([g, matches]) => {
    html += `<div class="group-section">
      <div class="group-header" onclick="toggleGroup(this)">
        <span>Gruppe ${g}</span><span style="color:var(--muted);font-size:.8rem;">&#9660;</span>
      </div><div class="group-body">
      <div class="match-ro-header">
        <span class="ro-home-label">Heim</span>
        <span class="ro-col-label">Tipp</span>
        <span class="ro-col-label">Ergebnis</span>
        <span class="ro-col-label">Pkt</span>
        <span class="ro-away-label">Ausw&auml;rts</span>
      </div>`;
    matches.forEach(m => {
      const home   = teamMap[m.HomeTeamId] || { DisplayName: m.HomeTeamId, FlagCode: '' };
      const away   = teamMap[m.AwayTeamId] || { DisplayName: m.AwayTeamId, FlagCode: '' };
      const bet    = betMap[m.MatchId];
      const result = matchMap[m.MatchId];

      const betStr    = bet
        ? `${bet.HomeGoals}:${bet.AwayGoals}`
        : '<span class="muted-txt">\u2013</span>';
      const actualStr = result && result.IsFinished
        ? `<strong>${result.HomeGoals}:${result.AwayGoals}</strong>`
        : '<span class="muted-txt">ausst.</span>';

      let ptsHtml = '<span class="muted-txt">\u2013</span>';
      if (bet && result && result.IsFinished) {
        const pts = calcMatchPoints(bet, result);
        const cls = pts === 4 ? 'pts-exact' : pts >= 2 ? 'pts-ok' : 'pts-zero';
        ptsHtml = `<span class="pts-badge ${cls}">${pts}</span>`;
      } else if (!bet && result && result.IsFinished) {
        ptsHtml = `<span class="pts-badge pts-zero">0</span>`;
      }

      html += `<div class="match-ro">
        <span class="team-name ro-home">${flagImg(home.FlagCode)} ${esc(home.DisplayName)}</span>
        <span class="ro-score">${betStr}</span>
        <span class="ro-score">${actualStr}</span>
        <span class="ro-pts">${ptsHtml}</span>
        <span class="team-name ro-away">${esc(away.DisplayName)} ${flagImg(away.FlagCode)}</span>
      </div>`;
    });
    html += '</div></div>';
  });
  container.innerHTML = html;
}

// Points calculation: mirrors EvaluatorBase.CalculateMatchPoints
// Exakt: 4 | Differenz: 3 | Tendenz: 2 | Falsch: 0
function calcMatchPoints(bet, result) {
  const betDiff    = bet.HomeGoals    - bet.AwayGoals;
  const actualDiff = result.HomeGoals - result.AwayGoals;
  if (bet.HomeGoals === result.HomeGoals && bet.AwayGoals === result.AwayGoals) return 4;
  if (betDiff === actualDiff) return 3;
  if (Math.sign(betDiff) === Math.sign(actualDiff)) return 2;
  return 0;
}

// ─── My KO Rounds – editable ──────────────────────────────────────────────
function renderMyKORounds(userData) {
  const container = document.getElementById('my-ko-rounds-container');
  const bets = userData.BetData?.KnockoutBets || {};
  let html = '';
  KO_ROUNDS.forEach(r => {
    const selected = new Set(bets[r.key] || []);
    html += `<div class="card">
      <div class="section-title">${r.label} <span class="badge badge-blue">${r.pts} Pkt/Team</span>
        <span class="round-count" id="my-cnt-${r.key}">(${selected.size}/${r.count})</span>
      </div><div class="team-grid" id="my-grid-${r.key}">`;
    tourney.Teams.forEach(t => {
      const sel = selected.has(t.TeamId);
      html += `<div class="team-chip ${sel?'selected':''}" data-team="${t.TeamId}"
        onclick="toggleMyTeam('${r.key}','${t.TeamId}',${r.count},this)">
        ${flagImg(t.FlagCode)} ${esc(t.DisplayName)}</div>`;
    });
    html += `</div></div>`;
  });
  container.innerHTML = html;
  KO_ROUNDS.forEach(r => updateMyRoundCount(r.key, r.count));
}

function toggleMyTeam(roundKey, teamId, maxCount, chip) {
  if (chip.classList.contains('selected')) {
    chip.classList.remove('selected');
  } else {
    const cnt = document.querySelectorAll('#my-grid-' + roundKey + ' .team-chip.selected').length;
    if (cnt >= maxCount) { showToast('Max. ' + maxCount + ' Teams!', true); return; }
    chip.classList.add('selected');
  }
  updateMyRoundCount(roundKey, maxCount);
}

function updateMyRoundCount(roundKey, maxCount) {
  const grid = document.getElementById('my-grid-' + roundKey);
  if (!grid) return;
  const cnt = grid.querySelectorAll('.team-chip.selected').length;
  const el  = document.getElementById('my-cnt-' + roundKey);
  if (!el) return;
  el.textContent = '(' + cnt + '/' + maxCount + ')';
  el.className = 'round-count ' + (cnt === maxCount ? 'ok' : cnt > maxCount ? 'over' : '');
}

// ─── My KO Rounds – read-only with result indicators ─────────────────────
function renderMyKORoundsReadOnly(userData) {
  const container = document.getElementById('my-ko-rounds-container');
  const bets    = userData.BetData?.KnockoutBets || {};
  const actuals = tourney.ActualKnockoutTeams    || {};
  const teamMap = {};
  tourney.Teams.forEach(t => { teamMap[t.TeamId] = t; });

  let html = '';
  KO_ROUNDS.forEach(r => {
    const bettedSet = new Set((bets[r.key] || []).map(s => s.toLowerCase()));
    const actualSet = new Set((actuals[r.key] || []).map(s => s.toLowerCase()));
    const known     = actualSet.size > 0;
    let pts = 0;
    bettedSet.forEach(t => { if (actualSet.has(t)) pts += r.pts; });

    html += `<div class="card">
      <div class="section-title">${r.label}
        <span class="badge badge-blue">${r.pts} Pkt/Team</span>
        ${known ? `<span class="pts-badge ${pts > 0 ? 'pts-ok' : 'pts-zero'}" style="margin-left:.4rem;">${pts} Pkt</span>` : ''}
      </div>`;

    if (bettedSet.size === 0) {
      html += '<div class="muted-txt" style="padding:.4rem 0;">Keine Teams getippt.</div>';
    } else {
      html += '<div class="team-grid">';
      tourney.Teams.forEach(t => {
        if (!bettedSet.has(t.TeamId.toLowerCase())) return;
        const correct = known && actualSet.has(t.TeamId.toLowerCase());
        const cls  = known ? (correct ? ' chip-correct' : ' chip-wrong') : '';
        const icon = known ? (correct ? ' <span style="color:var(--success)">&#10003;</span>' : ' <span style="color:var(--danger)">&#10007;</span>') : '';
        html += `<div class="team-chip-ro${cls}">${flagImg(t.FlagCode)} ${esc(t.DisplayName)}${icon}</div>`;
      });
      html += '</div>';
    }

    if (known) {
      html += `<details style="margin-top:.6rem;font-size:.8rem;color:var(--muted);">
        <summary style="cursor:pointer;">Alle tats\u00e4chlichen Teams einblenden</summary>
        <div class="team-grid" style="margin-top:.4rem;">`;
      tourney.Teams.forEach(t => {
        if (!actualSet.has(t.TeamId.toLowerCase())) return;
        html += `<div class="team-chip-ro chip-actual">${flagImg(t.FlagCode)} ${esc(t.DisplayName)}</div>`;
      });
      html += '</div></details>';
    }
    html += '</div>';
  });
  container.innerHTML = html;
}

// ─── My Special Bets – editable ───────────────────────────────────────────
function renderMySpecial(userData) {
  const container = document.getElementById('my-special-container');
  const bets = userData.BetData?.SpecialBets || {};
  let wcOptions = '<option value="">-- Team ausw\u00e4hlen --</option>';
  tourney.Teams.forEach(t => {
    const sel = bets.WorldChampionTeamId === t.TeamId ? 'selected' : '';
    wcOptions += `<option value="${t.TeamId}" ${sel}>${esc(t.DisplayName)} (${t.TeamId})</option>`;
  });
  container.innerHTML = `<div class="card">
    <div class="section-title">Sondertipps (je 20 Punkte)</div>
    <div class="row">
      <div class="field">
        <label>Weltmeister (Team)</label>
        <select id="my-f-worldchamp">${wcOptions}</select>
      </div>
      <div class="field">
        <label>Torsch\u00fctzenk\u00f6nig (vollst. Name)</label>
        <input type="text" id="my-f-topscorer" placeholder="z.B. Jamal Musiala" value="${esc(bets.TopScorerName || '')}">
      </div>
    </div>
  </div>`;
}

// ─── My Special Bets – read-only ──────────────────────────────────────────
function renderMySpecialReadOnly(userData) {
  const container = document.getElementById('my-special-container');
  const bets    = userData.BetData?.SpecialBets || {};
  const teamMap = {};
  tourney.Teams.forEach(t => { teamMap[t.TeamId] = t; });

  const wcTeam    = teamMap[bets.WorldChampionTeamId];
  const wcDisplay = wcTeam
    ? `${flagImg(wcTeam.FlagCode)} ${esc(wcTeam.DisplayName)}`
    : '<span class="muted-txt">Nicht getippt</span>';

  const actualWC     = tourney.ActualWorldChampionTeamId;
  const actualWCTeam = actualWC ? teamMap[actualWC] : null;
  const wcActual     = actualWCTeam
    ? `${flagImg(actualWCTeam.FlagCode)} ${esc(actualWCTeam.DisplayName)}`
    : '<span class="muted-txt">noch offen</span>';
  const wcCorrect = actualWC && bets.WorldChampionTeamId &&
    actualWC.toLowerCase() === bets.WorldChampionTeamId.toLowerCase();
  const wcPts = (actualWC != null && actualWC !== '') ? (wcCorrect ? 20 : 0) : null;

  const tsDisplay = bets.TopScorerName
    ? esc(bets.TopScorerName)
    : '<span class="muted-txt">Nicht getippt</span>';
  const actualTS  = tourney.ActualTopScorerName;
  const tsCorrect = actualTS && bets.TopScorerName &&
    actualTS.toLowerCase() === bets.TopScorerName.toLowerCase();
  const tsPts = (actualTS != null && actualTS !== '') ? (tsCorrect ? 20 : 0) : null;

  container.innerHTML = `<div class="card">
    <div class="section-title">Sondertipps (je 20 Punkte)</div>
    <table>
      <thead><tr><th>Tipp</th><th>Dein Tipp</th><th>Tats\u00e4chlich</th><th>Punkte</th></tr></thead>
      <tbody>
        <tr>
          <td><strong>Weltmeister</strong></td>
          <td>${wcDisplay}</td>
          <td>${wcActual}</td>
          <td>${wcPts !== null
            ? `<span class="pts-badge ${wcPts > 0 ? 'pts-exact' : 'pts-zero'}">${wcPts}</span>`
            : '<span class="muted-txt">\u2013</span>'}</td>
        </tr>
        <tr>
          <td><strong>Torsch\u00fctzenkönig</strong></td>
          <td>${tsDisplay}</td>
          <td>${actualTS ? esc(actualTS) : '<span class="muted-txt">noch offen</span>'}</td>
          <td>${tsPts !== null
            ? `<span class="pts-badge ${tsPts > 0 ? 'pts-exact' : 'pts-zero'}">${tsPts}</span>`
            : '<span class="muted-txt">\u2013</span>'}</td>
        </tr>
      </tbody>
    </table>
  </div>`;
}

// ─── My Bingo Card – editable ─────────────────────────────────────────────
function renderMyBingoCard(userData) {
  const container = document.getElementById('my-bingo-container');
  const cells  = userData.BetData?.BingoCard?.Cells || [];
  const cellMap = {};
  cells.forEach(c => { cellMap[c.Position] = c; });
  const eventMap = {};
  tourney.BingoEventCatalog.forEach(e => { eventMap[e.EventId] = e; });

  let html = '<div class="card"><div class="section-title">Bingo-Karte (4\u00d74)</div>';
  html += '<p style="font-size:.85rem;color:var(--muted);margin-bottom:1rem;">';
  html += 'W\u00e4hle f\u00fcr jedes Feld ein Ereignis. Jedes Ereignis kann nur einmal gew\u00e4hlt werden.</p>';
  html += '<div class="bingo-grid">';

  for (let pos = 0; pos < 16; pos++) {
    const cell = cellMap[pos] || { Position: pos, EventId: '', IsFulfilled: false };
    const currentId = cell.EventId || '';
    const desc = eventMap[currentId]?.Description || '';
    html += `<div class="bingo-cell">
      <span class="pos-badge">${pos}</span>
      <select class="my-bingo-select" data-pos="${pos}" onchange="onMyBingoChange(this)">
        <option value="">-- w\u00e4hlen --</option>
        ${tourney.BingoEventCatalog.map(e =>
          `<option value="${e.EventId}" ${e.EventId===currentId?'selected':''}>${esc(e.Description)}</option>`
        ).join('')}
      </select>
      <div class="event-desc" id="my-bdesc-${pos}">${esc(desc)}</div>
    </div>`;
  }
  html += '</div></div>';
  container.innerHTML = html;
  refreshMyBingoSelects();
}

function onMyBingoChange(sel) {
  const pos = sel.dataset.pos;
  const eventMap = {};
  tourney.BingoEventCatalog.forEach(e => { eventMap[e.EventId] = e; });
  const el = document.getElementById('my-bdesc-' + pos);
  if (el) el.textContent = eventMap[sel.value]?.Description || '';
  refreshMyBingoSelects();
}

function refreshMyBingoSelects() {
  const used = {};
  document.querySelectorAll('#my-bingo-container .my-bingo-select').forEach(s => {
    if (s.value) used[s.value] = (used[s.value] || 0) + 1;
  });
  document.querySelectorAll('#my-bingo-container .my-bingo-select').forEach(sel => {
    const myVal = sel.value;
    Array.from(sel.options).forEach(opt => {
      if (!opt.value) return;
      opt.disabled = used[opt.value] > 1 || (used[opt.value] === 1 && opt.value !== myVal);
    });
  });
}

// ─── My Bingo Card – read-only with fulfilled status ─────────────────────
function renderMyBingoCardReadOnly(userData) {
  const container = document.getElementById('my-bingo-container');
  const cells  = userData.BetData?.BingoCard?.Cells || [];
  const cellMap = {};
  cells.forEach(c => { cellMap[c.Position] = c; });
  const eventMap = {};
  tourney.BingoEventCatalog.forEach(e => { eventMap[e.EventId] = e; });
  const occurredSet = new Set((tourney.OccurredBingoEvents || []).map(s => s.toLowerCase()));

  let html = '<div class="card"><div class="section-title">Bingo-Karte (4\u00d74)</div>';
  html += '<div class="bingo-grid">';

  for (let pos = 0; pos < 16; pos++) {
    const cell = cellMap[pos] || { Position: pos, EventId: '', IsFulfilled: false };
    const eventId   = cell.EventId || '';
    const desc      = eventMap[eventId]?.Description || (eventId ? eventId : '\u2013 kein Ereignis \u2013');
    const fulfilled = eventId ? occurredSet.has(eventId.toLowerCase()) : false;
    html += `<div class="bingo-cell ${fulfilled ? 'fulfilled-cell' : 'unfulfilled-cell'}">
      <span class="pos-badge">${pos}</span>
      <div style="font-size:.72rem;margin-top:.3rem;word-break:break-word;">${esc(desc)}</div>
      ${fulfilled
        ? '<span class="bingo-pts-badge">&#10003; 3 Pkt</span>'
        : '<span class="bingo-open-badge">&#9711; offen</span>'}
    </div>`;
  }
  html += '</div></div>';
  container.innerHTML = html;
}

// ─── Collect My Form Data ─────────────────────────────────────────────────
function collectMyFormData() {
  const base = JSON.parse(document.getElementById('page-my-tips').dataset.userJson || '{}');

  const bets = [];
  document.querySelectorAll('#my-group-matches-container .match-bet').forEach(row => {
    bets.push({
      MatchId:   row.dataset.matchid,
      HomeGoals: parseInt(row.querySelector('[data-role=home]').value) || 0,
      AwayGoals: parseInt(row.querySelector('[data-role=away]').value) || 0
    });
  });
  base.BetData.GroupMatchBets = bets;

  const koBets = {};
  KO_ROUNDS.forEach(r => {
    koBets[r.key] = Array.from(
      document.querySelectorAll('#my-grid-' + r.key + ' .team-chip.selected')
    ).map(c => c.dataset.team);
  });
  base.BetData.KnockoutBets = koBets;

  base.BetData.SpecialBets = {
    WorldChampionTeamId: document.getElementById('my-f-worldchamp')?.value || '',
    TopScorerName:       (document.getElementById('my-f-topscorer')?.value || '').trim(),
  };

  const bingoCells = [];
  for (let pos = 0; pos < 16; pos++) {
    const sel = document.querySelector('#my-bingo-container .my-bingo-select[data-pos="' + pos + '"]');
    bingoCells.push({ Position: pos, EventId: sel ? sel.value : '', IsFulfilled: false, FulfilledAt: null });
  }
  const evtIds = bingoCells.filter(c => c.EventId).map(c => c.EventId);
  if (evtIds.filter((v, i, a) => a.indexOf(v) !== i).length) {
    showToast('Bingo: Doppelte Ereignisse!', true);
    return null;
  }
  base.BetData.BingoCard = { Cells: bingoCells };
  return base;
}

async function saveMyTips() {
  const data = collectMyFormData();
  if (!data) return;
  showSpinner();
  try {
    const resp = await fetch('/api/users/' + encodeURIComponent(currentUser.UserId), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
    if (!resp.ok) throw new Error(await resp.text());
    currentUser = data;
    document.getElementById('page-my-tips').dataset.userJson = JSON.stringify(data);
    showToast('Tipps gespeichert!');
  } catch(e) { showToast('Fehler beim Speichern: ' + e.message, true); }
  hideSpinner();
}

// ═══════════════════════════════════════════════════════════════════════════
//  Auswertung / Ranking
// ═══════════════════════════════════════════════════════════════════════════
async function recalculate() {
  showSpinner();
  try {
    const url = '/api/ranking/recalculate' +
      (selectedDataset ? '?dataset=' + encodeURIComponent(selectedDataset) : '');
    const resp = await fetch(url, { method: 'POST' });
    if (!resp.ok) throw new Error(await resp.text());
    ranking = await resp.json();
    renderRanking();
    const dsLabels = {
      vor_turnier:  'Vor dem Turnier',
      nach_gruppe:  'Nach der Gruppenphase',
      nach_turnier: 'Nach dem Turnier'
    };
    const label = dsLabels[selectedDataset] || selectedDataset;
    showToast('Auswertung berechnet (' + label + ')!');
  } catch(e) { showToast('Fehler: ' + e.message, true); }
  hideSpinner();
}

function renderRanking() {
  if (!ranking) return;

  const d = new Date(ranking.GeneratedAt);
  const dateStr = d.toLocaleDateString('de-DE') + ' ' + d.toLocaleTimeString('de-DE');
  document.getElementById('last-calc-time').textContent      = 'Letzte Berechnung: ' + dateStr;
  document.getElementById('user-last-calc-time').textContent = 'Auswertung vom: ' + dateStr;

  const fs = ranking.FinanceSummary;
  const isFinalized = ranking.IsMainPotFinalized ?? false;

  document.getElementById('stat-grid').innerHTML = `
    <div class="stat-card"><div class="value">${ranking.Leaderboard.length}</div><div class="label">Tipper gesamt</div></div>
    <div class="stat-card"><div class="value">${fs.TotalPot.toLocaleString('de-DE')} &euro;</div><div class="label">Gesamttopf</div></div>
    <div class="stat-card"><div class="value">${fs.DistributedAmount.toLocaleString('de-DE')} &euro;</div><div class="label">Ausgesch\u00fcttet</div></div>
    <div class="stat-card"><div class="value">${fs.RemainingAmount.toLocaleString('de-DE')} &euro;</div><div class="label">Noch offen</div></div>
    <div class="stat-card"><div class="value">${ranking.GroupClusterResults.length}</div><div class="label">Cluster abgerechnet</div></div>
    <div class="stat-card"><div class="value">${ranking.BingoPotResults.length}</div><div class="label">Bingo-T\u00f6pfe</div></div>
  `;

  const medalFor = i => i === 0 ? '<span class="rank-medal">&#127947;</span>' :
                         i === 1 ? '<span class="rank-medal">&#129352;</span>' :
                         i === 2 ? '<span class="rank-medal">&#129353;</span>' : '';

  function potBadge(label) {
    if (label.startsWith('Gruppencluster')) {
      return `<span class="badge badge-green">${esc(label)}</span>`;
    } else if (label.startsWith('Bingo')) {
      return `<span class="badge badge-purple">${esc(label)}</span>`;
    } else {
      const suffix = isFinalized ? '' : ' <em style="font-size:.7rem;">(Zwischenstand)</em>';
      return `<span class="badge badge-orange">${esc(label)}${suffix}</span>`;
    }
  }

  let interimNotice = '';
  if (!isFinalized) {
    interimNotice = `<div class="interim-notice">
      &#9888; <strong>Zwischenstand</strong> \u2013 Die Preisgelder der Gesamtwertung
      (Haupttopf) werden erst nach Turnierende endg\u00fcltig ausgezahlt.
      Gruppencluster- und Bingo-Gewinne sind bereits fest.
    </div>`;
  }

  let lb = interimNotice + '<table><thead><tr><th>Platz</th><th>Name</th><th>Gesamt</th><th>Klassik</th><th>K.O.</th><th>Bingo</th><th>Bingo-Felder</th><th>Bingo-Linien</th><th>Preisgeld</th><th>Gewonnene T\u00f6pfe</th></tr></thead><tbody>';
  ranking.Leaderboard.forEach((e, i) => {
    const isMe     = mode === 'user' && currentUser && e.UserId === currentUser.UserId;
    const rowClass = isMe ? ' class="my-row"' : '';
    const meLabel  = isMe ? ' <span class="badge badge-blue">Ich</span>' : '';
    const pots = e.WonPots.length
      ? e.WonPots.map(p => potBadge(p)).join(' ')
      : '<span style="color:var(--muted)">\u2013</span>';
    lb += `<tr${rowClass}>
      <td><strong>${e.Rank}</strong> ${medalFor(i)}</td>
      <td><strong>${esc(e.Name)}</strong>${meLabel} <small style="color:var(--muted)">${esc(e.UserId)}</small></td>
      <td><strong>${e.TotalPoints} Pkt</strong></td>
      <td>${e.ClassicPoints} Pkt</td>
      <td>${e.KnockoutPoints} Pkt</td>
      <td>${e.BingoPoints} Pkt</td>
      <td>${e.FulfilledBingoCells} / 25</td>
      <td>${e.CompletedBingoLines} / 12</td>
      <td><strong style="color:var(--success)">${e.TotalFinancialWinnings.toLocaleString('de-DE')} &euro;</strong></td>
      <td style="font-size:.78rem;">${pots}</td>
    </tr>`;
  });
  lb += '</tbody></table>';
  document.getElementById('leaderboard-wrap').innerHTML = lb;

  // Gruppen-Cluster
  let cl;
  if (!ranking.GroupClusterResults.length) {
    cl = '<div class="empty-state">Noch keine Gruppen vollst\u00e4ndig abgeschlossen.</div>';
  } else {
    cl = '<table><thead><tr><th>Gruppen-Cluster</th><th>Gewinner</th><th>Punkte</th><th>Gewinn</th><th>Geteilt?</th><th>Status</th></tr></thead><tbody>';
    ranking.GroupClusterResults.forEach(r => {
      cl += `<tr>
        <td><strong>${esc(r.ClusterLabel)}</strong></td>
        <td>${esc(r.WinnerName)}</td>
        <td>${r.WinnerClusterPoints} Pkt</td>
        <td><strong style="color:var(--success)">${r.Prize.toLocaleString('de-DE')} &euro;</strong></td>
        <td>${r.IsShared ? '<span class="badge badge-blue">Ja \u2013 '+esc(r.CoWinners.join(', '))+'</span>' : 'Nein'}</td>
        <td><span class="badge badge-green">&#10003; Endg\u00fcltig</span></td>
      </tr>`;
    });
    cl += '</tbody></table>';
  }
  document.getElementById('cluster-wrap').innerHTML = cl;

  // Bingo-Topf-Übersicht
  const overview = ranking.BingoPotOverview || [];
  const awarded  = overview.filter(e => e.IsAwarded);
  const open     = overview.filter(e => !e.IsAwarded);
  let bp = '';
  if (awarded.length) {
    bp += `<div class="section-title" style="margin-bottom:.6rem;">&#10003; Bereits vergeben</div>
      <table><thead><tr><th>Topf</th><th>Gewinner</th><th>Preisgeld</th></tr></thead><tbody>`;
    awarded.forEach(e => {
      bp += `<tr>
        <td><span class="badge badge-green">${esc(e.PotLabel)}</span></td>
        <td>${esc(e.WinnerName)} <small style="color:var(--muted)">${esc(e.WinnerUserId)}</small></td>
        <td><strong style="color:var(--success)">${e.Prize.toLocaleString('de-DE')} &euro;</strong></td>
      </tr>`;
    });
    bp += '</tbody></table>';
  }
  if (open.length) {
    bp += `<div class="section-title" style="margin-top:1.2rem;margin-bottom:.6rem;">&#10008; Noch offen</div>
      <table><thead><tr><th>Topf</th><th>Preisgeld</th><th>Status</th></tr></thead><tbody>`;
    open.forEach(e => {
      bp += `<tr>
        <td>${esc(e.PotLabel)}</td>
        <td><strong style="color:var(--accent)">${e.Prize.toLocaleString('de-DE')} &euro;</strong></td>
        <td><span class="badge badge-orange">Ausstehend</span></td>
      </tr>`;
    });
    bp += '</tbody></table>';
  }
  if (!bp) bp = '<div class="empty-state">Bitte Auswertung starten.</div>';
  document.getElementById('bingo-pots-wrap').innerHTML = bp;

  // Finanzübersicht
  let fin = `<div class="section-title">Finanz\u00fcbersicht</div>
    <table><thead><tr><th>Posten</th><th>Betrag</th></tr></thead><tbody>
    <tr><td>Gesamttopf</td><td><strong>${fs.TotalPot.toLocaleString('de-DE')} &euro;</strong></td></tr>
    <tr><td>Bereits ausgesch\u00fcttet</td><td style="color:var(--success)"><strong>${fs.DistributedAmount.toLocaleString('de-DE')} &euro;</strong></td></tr>
    <tr><td>Noch offen (inkl. Bingo &amp; Haupttopf)</td><td style="color:var(--accent)"><strong>${fs.RemainingAmount.toLocaleString('de-DE')} &euro;</strong></td></tr>
    </tbody></table>`;
  if (fs.UnclaimedPots?.length) {
    fin += `<div class="section-title" style="margin-top:1.2rem;">Noch nicht ausgesch\u00fcttet</div>
      <table><thead><tr><th>Topf / Grund</th></tr></thead><tbody>`;
    fs.UnclaimedPots.forEach(p => {
      fin += `<tr><td style="color:var(--muted)">${esc(p)}</td></tr>`;
    });
    fin += '</tbody></table>';
  }
  document.getElementById('finance-wrap').innerHTML = fin;
}

// ═══════════════════════════════════════════════════════════════════════════
//  Utils
// ═══════════════════════════════════════════════════════════════════════════
async function fetchJSON(url) {
  const r = await fetch(url);
  if (!r.ok) throw new Error('HTTP ' + r.status);
  return r.json();
}
function esc(s) {
  return String(s||'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}
function flagImg(code) {
  if (!code) return '';
  return '<img class="flag" src="https://flagcdn.com/w20/' + code.toLowerCase() + '.webp" alt="' + code + '" onerror="this.style.display=\'none\'">';
}
function showToast(msg, isErr=false) {
  const t = document.getElementById('toast');
  t.textContent = msg;
  t.className = 'show' + (isErr ? ' error' : '');
  setTimeout(() => { t.className = ''; }, 3000);
}
function showSpinner() { document.getElementById('spinner').classList.add('show'); }
function hideSpinner() { document.getElementById('spinner').classList.remove('show'); }

init();
