// ═══════════════════════════════════════════════════════════════════════════
//  State
// ═══════════════════════════════════════════════════════════════════════════
let tourney  = null;
let users    = [];
let editUser = null;
let ranking  = null;

// ═══════════════════════════════════════════════════════════════════════════
//  Bootstrap
// ═══════════════════════════════════════════════════════════════════════════
async function init() {
  showSpinner();
  try {
    [tourney, users] = await Promise.all([fetchJSON('/api/tournament'), fetchJSON('/api/users')]);
    renderUserList();
    tryLoadRanking();
  } catch(e) { showToast('Fehler beim Laden: ' + e.message, true); }
  hideSpinner();
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
//  User List
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
//  User Form
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

  document.querySelectorAll('.tab-btn').forEach((b,i) => b.classList.toggle('active', i===0));
  document.querySelectorAll('.tab-panel').forEach((p,i) => { if(p.closest('#view-form')) p.classList.toggle('active', i===0); });

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

// Group match bets
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

// KO rounds
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
  el.textContent = '(' + cnt + '/' + maxCount + ')';
  el.className = 'round-count ' + (cnt === maxCount ? 'ok' : cnt > maxCount ? 'over' : '');
}

// Bingo card
function renderBingoCard(userData) {
  const container = document.getElementById('bingo-grid-container');
  const cells = userData.BetData?.BingoCard?.Cells || [];
  const cellMap = {};
  cells.forEach(c => { cellMap[c.Position] = c; });
  const eventMap = {};
  tourney.BingoEventCatalog.forEach(e => { eventMap[e.EventId] = e; });
  let html = '';
  for (let pos = 0; pos < 25; pos++) {
    const cell = cellMap[pos] || { Position: pos, EventId: '', IsFulfilled: false };
    if (pos === 12) {
      html += `<div class="bingo-cell free"><span class="pos-badge">12</span>
        <div style="font-size:1.5rem;">&#11088;</div>
        <div style="font-weight:700;color:var(--accent);">FREE</div>
        <div style="font-size:.7rem;color:var(--muted);">Immer erf\u00fcllt</div></div>`;
    } else {
      const currentId = cell.EventId || '';
      const desc = eventMap[currentId]?.Description || '';
      html += `<div class="bingo-cell">
        <span class="pos-badge">${pos}</span>
        <select class="bingo-select" data-pos="${pos}" onchange="onBingoChange(this)">
          <option value="">-- w\u00e4hlen --</option>
          ${tourney.BingoEventCatalog.filter(e=>e.EventId!=='FREE_SPACE').map(e =>
            `<option value="${e.EventId}" ${e.EventId===currentId?'selected':''}>${esc(e.Description)}</option>`
          ).join('')}
        </select>
        <div class="event-desc" id="bdesc-${pos}">${esc(desc)}</div>
      </div>`;
    }
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
  document.querySelectorAll('.bingo-select').forEach(s => { if (s.value) used[s.value] = (used[s.value] || 0) + 1; });
  document.querySelectorAll('.bingo-select').forEach(sel => {
    const myVal = sel.value;
    Array.from(sel.options).forEach(opt => {
      if (!opt.value) return;
      opt.disabled = used[opt.value] > 1 || (used[opt.value] === 1 && opt.value !== myVal);
    });
  });
}

// Tab switching (inner)
function switchTab(btn, panelId) {
  const parent = btn.closest('.container, #view-form, #page-ranking') || document;
  parent.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
  parent.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
  btn.classList.add('active');
  document.getElementById(panelId).classList.add('active');
}

// Save
function collectFormData() {
  const base = JSON.parse(document.getElementById('view-form').dataset.userJson || '{}');
  base.UserId = document.getElementById('f-id').value.trim();
  base.Name   = document.getElementById('f-name').value.trim();
  if (!base.Name) { showToast('Name ist erforderlich!', true); return null; }

  const bets = [];
  document.querySelectorAll('.match-bet').forEach(row => {
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
  for (let pos = 0; pos < 25; pos++) {
    if (pos === 12) {
      bingoCells.push({ Position: 12, EventId: 'FREE_SPACE', IsFulfilled: true, FulfilledAt: '2026-06-11T00:00:00Z' });
    } else {
      const sel = document.querySelector('.bingo-select[data-pos="' + pos + '"]');
      bingoCells.push({ Position: pos, EventId: sel ? sel.value : '', IsFulfilled: false, FulfilledAt: null });
    }
  }
  const evtIds = bingoCells.filter(c => c.EventId && c.EventId !== 'FREE_SPACE').map(c => c.EventId);
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
//  Auswertung / Ranking
// ═══════════════════════════════════════════════════════════════════════════
async function recalculate() {
  showSpinner();
  try {
    const resp = await fetch('/api/ranking/recalculate', { method: 'POST' });
    if (!resp.ok) throw new Error(await resp.text());
    ranking = await resp.json();
    renderRanking();
    showToast('Auswertung berechnet!');
  } catch(e) { showToast('Fehler: ' + e.message, true); }
  hideSpinner();
}

function renderRanking() {
  if (!ranking) return;

  const d = new Date(ranking.GeneratedAt);
  document.getElementById('last-calc-time').textContent =
    'Letzte Berechnung: ' + d.toLocaleDateString('de-DE') + ' ' + d.toLocaleTimeString('de-DE');

  const fs = ranking.FinanceSummary;

  // Stats overview
  document.getElementById('stat-grid').innerHTML = `
    <div class="stat-card"><div class="value">${ranking.Leaderboard.length}</div><div class="label">Tipper gesamt</div></div>
    <div class="stat-card"><div class="value">${fs.TotalPot.toLocaleString('de-DE')} &euro;</div><div class="label">Gesamttopf</div></div>
    <div class="stat-card"><div class="value">${fs.DistributedAmount.toLocaleString('de-DE')} &euro;</div><div class="label">Ausgesch&uuml;ttet</div></div>
    <div class="stat-card"><div class="value">${fs.RemainingAmount.toLocaleString('de-DE')} &euro;</div><div class="label">Verbleibend</div></div>
    <div class="stat-card"><div class="value">${ranking.GroupClusterResults.length}</div><div class="label">Cluster abgerechnet</div></div>
    <div class="stat-card"><div class="value">${ranking.BingoPotResults.length}</div><div class="label">Bingo-T&ouml;pfe</div></div>
  `;

  // Leaderboard
  const medalFor = i => i === 0 ? '<span class="rank-medal">&#127947;</span>' :
                         i === 1 ? '<span class="rank-medal">&#129352;</span>' :
                         i === 2 ? '<span class="rank-medal">&#129353;</span>' : '';
  let lb = '<table><thead><tr><th>Platz</th><th>Name</th><th>Gesamt</th><th>Klassik</th><th>K.O.</th><th>Bingo</th><th>Bingo-Felder</th><th>Bingo-Linien</th><th>Preisgeld</th><th>Gewonnene T&ouml;pfe</th></tr></thead><tbody>';
  ranking.Leaderboard.forEach((e, i) => {
    const pots = e.WonPots.length ? e.WonPots.map(p => '<span class="badge badge-green">'+esc(p)+'</span>').join(' ') : '<span style="color:var(--muted)">–</span>';
    lb += `<tr>
      <td><strong>${e.Rank}</strong> ${medalFor(i)}</td>
      <td><strong>${esc(e.Name)}</strong> <small style="color:var(--muted)">${esc(e.UserId)}</small></td>
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

  // Group cluster results
  let cl = '<table><thead><tr><th>Gruppen-Cluster</th><th>Gewinner</th><th>Punkte</th><th>Gewinn</th><th>Geteilt?</th></tr></thead><tbody>';
  if (!ranking.GroupClusterResults.length) {
    cl = '<div class="empty-state">Noch keine Gruppen vollst&auml;ndig abgeschlossen.</div>';
  } else {
    ranking.GroupClusterResults.forEach(r => {
      cl += `<tr>
        <td><strong>${esc(r.ClusterLabel)}</strong></td>
        <td>${esc(r.WinnerName)}</td>
        <td>${r.WinnerClusterPoints} Pkt</td>
        <td><strong style="color:var(--success)">${r.Prize.toLocaleString('de-DE')} &euro;</strong></td>
        <td>${r.IsShared ? '<span class="badge badge-blue">Ja – '+esc(r.CoWinners.join(', '))+'</span>' : 'Nein'}</td>
      </tr>`;
    });
    cl += '</tbody></table>';
  }
  document.getElementById('cluster-wrap').innerHTML = cl;

  // Bingo pots
  let bp = '<table class="pot-table"><thead><tr><th>Topf</th><th>Gewinner</th><th>Preisgeld</th></tr></thead><tbody>';
  if (!ranking.BingoPotResults.length) {
    bp = '<div class="empty-state">Noch kein Bingo-Topf ausgesch&uuml;ttet.</div>';
  } else {
    ranking.BingoPotResults.forEach(r => {
      bp += `<tr>
        <td>${esc(r.PotLabel)}</td>
        <td>${esc(r.WinnerName)} <small style="color:var(--muted)">${esc(r.WinnerUserId)}</small></td>
        <td>${r.Prize.toLocaleString('de-DE')} &euro;</td>
      </tr>`;
    });
    bp += '</tbody></table>';
  }
  document.getElementById('bingo-pots-wrap').innerHTML = bp;

  // Finance summary
  let fin = `<div class="section-title">Finanz&uuml;bersicht</div>
    <table><thead><tr><th>Posten</th><th>Betrag</th></tr></thead><tbody>
    <tr><td>Gesamttopf</td><td><strong>${fs.TotalPot.toLocaleString('de-DE')} &euro;</strong></td></tr>
    <tr><td>Ausgesch&uuml;ttet</td><td style="color:var(--success)"><strong>${fs.DistributedAmount.toLocaleString('de-DE')} &euro;</strong></td></tr>
    <tr><td>Verbleibend</td><td style="color:var(--accent)"><strong>${fs.RemainingAmount.toLocaleString('de-DE')} &euro;</strong></td></tr>
    </tbody></table>`;

  if (fs.UnclaimedPots?.length) {
    fin += `<div class="section-title" style="margin-top:1.2rem;">Noch nicht ausgesch&uuml;ttet</div><ul style="padding-left:1.2rem;line-height:2;">`;
    fs.UnclaimedPots.forEach(p => { fin += `<li style="color:var(--muted)">${esc(p)}</li>`; });
    fin += '</ul>';
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
