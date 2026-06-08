// ─── TRACK METADATA STORE ────────────────────────────────────
const trackMeta = {
    1: {
        title: 'Xtal', bpm: 125, key: 'F# minor', duration: '4:54',
        sampleRate: '44100 Hz', bitDepth: '24-bit', channels: 'stereo',
        fileSize: '52.4 MB', codec: 'FLAC', recorded: '1985-03-11',
        freq: ['60Hz', '240Hz', '1.2kHz', '4.8kHz', '12kHz'],
        coords: [50.2660, -5.0527], tags: ['ambient', 'IDM', 'rave'],
        label: 'APOLLO / R&S', cat: 'AMB LP 3922',
        waveform: [0.2, 0.5, 0.8, 0.6, 0.4, 0.9, 0.3, 0.7],
        peak: -0.3, rms: -18.4, dynamic: 14.1, tempo: 'moderate'
    },
    2: {
        title: 'Ageispolis', bpm: 115, key: 'D minor', duration: '5:21',
        sampleRate: '44100 Hz', bitDepth: '16-bit', channels: 'stereo',
        fileSize: '58.7 MB', codec: 'FLAC', recorded: '1987-08-22',
        freq: ['80Hz', '320Hz', '1.6kHz', '6.4kHz', '16kHz'],
        coords: [50.2660, -5.0527], tags: ['acid', 'electronic', 'rave'],
        label: 'APOLLO / R&S', cat: 'AMB LP 3923',
        waveform: [0.3, 0.7, 0.5, 0.9, 0.2, 0.6, 0.8, 0.4],
        peak: -0.1, rms: -16.2, dynamic: 16.1, tempo: 'mid'
    },
    3: {
        title: 'Windowlicker', bpm: 134, key: 'A# minor', duration: '6:00',
        sampleRate: '48000 Hz', bitDepth: '24-bit', channels: 'stereo',
        fileSize: '67.1 MB', codec: 'WAV', recorded: '1999-01-30',
        freq: ['55Hz', '220Hz', '880Hz', '3.5kHz', '14kHz'],
        coords: [51.5074, -0.1278], tags: ['drill', 'IDM', 'breakbeat'],
        label: 'WARP RECORDS', cat: 'WAP 105',
        waveform: [0.9, 0.4, 0.7, 0.3, 0.8, 0.5, 0.6, 0.2],
        peak: 0.0, rms: -12.8, dynamic: 12.8, tempo: 'fast'
    },
    4: {
        title: 'Flim', bpm: 170, key: 'E major', duration: '3:01',
        sampleRate: '44100 Hz', bitDepth: '24-bit', channels: 'stereo',
        fileSize: '33.2 MB', codec: 'FLAC', recorded: '1997-05-09',
        freq: ['40Hz', '160Hz', '640Hz', '2.5kHz', '10kHz'],
        coords: [51.5074, -0.1278], tags: ['ambient', 'jazz', 'complex'],
        label: 'WARP RECORDS', cat: 'WAP 94',
        waveform: [0.4, 0.8, 0.3, 0.7, 0.5, 0.9, 0.2, 0.6],
        peak: -0.5, rms: -19.1, dynamic: 18.6, tempo: 'fast'
    },
    5: {
        title: 'Come to Daddy', bpm: 155, key: 'C minor', duration: '5:26',
        sampleRate: '44100 Hz', bitDepth: '16-bit', channels: 'stereo',
        fileSize: '59.8 MB', codec: 'FLAC', recorded: '1997-09-22',
        freq: ['50Hz', '200Hz', '800Hz', '3.2kHz', '12.8kHz'],
        coords: [51.5074, -0.1278], tags: ['acid', 'noise', 'industrial'],
        label: 'WARP RECORDS', cat: 'WAP 94',
        waveform: [0.7, 0.3, 0.9, 0.4, 0.6, 0.2, 0.8, 0.5],
        peak: 0.0, rms: -9.3, dynamic: 9.3, tempo: 'aggressive'
    },
    6: {
        title: 'Bucephalus Bouncing Ball', bpm: 200, key: 'G minor', duration: '5:15',
        sampleRate: '44100 Hz', bitDepth: '24-bit', channels: 'stereo',
        fileSize: '57.8 MB', codec: 'FLAC', recorded: '1997-08-01',
        freq: ['70Hz', '280Hz', '1.1kHz', '4.4kHz', '17kHz'],
        coords: [51.5074, -0.1278], tags: ['drill', 'percussion', 'polyrhythm'],
        label: 'WARP RECORDS', cat: 'WAP 94',
        waveform: [0.6, 0.9, 0.2, 0.8, 0.3, 0.7, 0.4, 0.9],
        peak: -0.2, rms: -11.5, dynamic: 11.3, tempo: 'extreme'
    }
};

// ─── METADATA BACKGROUND ─────────────────────────────────────
const metaBg = (function () {
    const container = document.getElementById('metaBg');
    if (!container) return { update: () => {} };

    const debugEl = document.createElement('div');
    debugEl.className = 'meta-debug-text';
    container.appendChild(debugEl);

    const brandEl = document.createElement('div');
    brandEl.className = 'meta-brand';
    brandEl.textContent = 'EPOKA TWINS';
    container.appendChild(brandEl);

    let mouseX = 0, mouseY = 0;
    document.addEventListener('mousemove', e => { mouseX = e.clientX; mouseY = e.clientY; });

    // static values — computed once
    const ua    = navigator.userAgent.toLowerCase();
    const plat  = (navigator.platform || '').toLowerCase();
    const lang  = navigator.language || 'unknown';
    const tz    = Intl.DateTimeFormat().resolvedOptions().timeZone || 'unknown';
    const url   = window.location.href;
    const dpr   = (window.devicePixelRatio || 1).toFixed(1);
    const depth = screen.colorDepth || 24;
    const load  = Math.round(performance.now());

    function render() {
        const ww  = window.innerWidth;
        const wh  = window.innerHeight;
        const now = new Date().toTimeString().slice(0, 8);

        debugEl.textContent =
            `${ua} ${plat} screen:${screen.width}x${screen.height} ` +
            `window:${ww}x${wh} cursor:${mouseX}px,${mouseY}px ` +
            `lang:${lang} tz:${tz} dpr:${dpr} depth:${depth}bit ` +
            `${url} load:${load}ms t:${now}`;

        requestAnimationFrame(render);
    }

    render();

    return { update: () => {} };
})();

// ─── CLOCK ───────────────────────────────────────────────────
function updateClock() {
    const el = document.getElementById('system-time');
    if (!el) return;
    const now = new Date();
    el.textContent = now.toTimeString().slice(0, 8);
}
setInterval(updateClock, 1000);
updateClock();

// ─── CANVAS WAVEFORM ─────────────────────────────────────────
(function initCanvas() {
    const canvas = document.getElementById('waveCanvas');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    let w, h, frame = 0;

    function resize() {
        w = canvas.width  = canvas.offsetWidth;
        h = canvas.height = canvas.offsetHeight;
    }

    function draw() {
        ctx.clearRect(0, 0, w, h);
        const lines = 6;
        for (let l = 0; l < lines; l++) {
            ctx.beginPath();
            ctx.strokeStyle = l % 2 === 0
                ? `rgba(0,255,65,${0.08 + l * 0.04})`
                : `rgba(0,255,255,${0.04 + l * 0.02})`;
            ctx.lineWidth = 1;
            const yBase = h * (0.2 + l * 0.12);
            const amp   = 20 + l * 12;
            const freq  = 0.008 + l * 0.003;
            const speed = 0.015 + l * 0.005;

            for (let x = 0; x <= w; x += 2) {
                const y = yBase + Math.sin(x * freq + frame * speed + l) * amp
                        + Math.sin(x * freq * 2.3 - frame * speed * 0.7) * (amp * 0.4);
                x === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
            }
            ctx.stroke();
        }
        frame++;
        requestAnimationFrame(draw);
    }

    window.addEventListener('resize', resize);
    resize();
    draw();
})();

// ─── PLAYER ──────────────────────────────────────────────────
const playerState = {
    playing: false,
    currentId: null,
    currentTitle: 'NO TRACK LOADED',
    currentAlbum: '_ _ _',
    progress: 0,
    volume: 0.8,
    timer: null,
    duration: 0,
    elapsed: 0,
    trackOrder: []
};

function getAllCards() {
    return Array.from(document.querySelectorAll('.track-card'));
}

function parseDuration(str) {
    const [m, s] = str.split(':').map(Number);
    return m * 60 + s;
}

function formatTime(sec) {
    const m = Math.floor(sec / 60);
    const s = Math.floor(sec % 60);
    return `${m}:${s.toString().padStart(2, '0')}`;
}

function loadTrack(card) {
    const id    = card.dataset.id;
    const title = card.dataset.title;
    const album = card.dataset.album;
    const dur   = card.dataset.duration;

    getAllCards().forEach(c => c.classList.remove('active', 'playing'));
    card.classList.add('active', 'playing');

    playerState.currentId    = id;
    playerState.currentTitle = title;
    playerState.currentAlbum = album;
    playerState.elapsed      = 0;
    playerState.duration     = parseDuration(dur);
    playerState.playing      = true;

    document.getElementById('playerTrackName').textContent   = title;
    document.getElementById('playerTrackArtist').textContent = album;
    document.getElementById('totalTime').textContent         = dur;
    document.getElementById('playPauseBtn').textContent      = '⏸';

    metaBg.update(trackMeta[id] || null);
    startTimer();
}

function startTimer() {
    clearInterval(playerState.timer);
    playerState.timer = setInterval(() => {
        if (!playerState.playing) return;
        playerState.elapsed = Math.min(playerState.elapsed + 1, playerState.duration);
        const pct = playerState.duration > 0
            ? (playerState.elapsed / playerState.duration) * 100
            : 0;
        document.getElementById('progressFill').style.width = pct + '%';
        document.getElementById('currentTime').textContent  = formatTime(playerState.elapsed);
        if (playerState.elapsed >= playerState.duration) nextTrack();
    }, 1000);
}

function togglePlay() {
    if (!playerState.currentId) return;
    playerState.playing = !playerState.playing;
    const btn = document.getElementById('playPauseBtn');
    btn.textContent = playerState.playing ? '⏸' : '▶';

    const active = document.querySelector('.track-card.active');
    if (active) {
        if (playerState.playing) active.classList.add('playing');
        else active.classList.remove('playing');
    }
}

function nextTrack() {
    const cards = getAllCards().filter(c => !c.classList.contains('hidden'));
    const idx   = cards.findIndex(c => c.dataset.id === String(playerState.currentId));
    const next  = cards[(idx + 1) % cards.length];
    if (next) loadTrack(next);
}

function prevTrack() {
    const cards = getAllCards().filter(c => !c.classList.contains('hidden'));
    const idx   = cards.findIndex(c => c.dataset.id === String(playerState.currentId));
    const prev  = cards[(idx - 1 + cards.length) % cards.length];
    if (prev) loadTrack(prev);
}

function seekTo(e) {
    const wrap = document.getElementById('progressWrap');
    const rect = wrap.getBoundingClientRect();
    const pct  = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
    playerState.elapsed = Math.floor(pct * playerState.duration);
    document.getElementById('progressFill').style.width = (pct * 100) + '%';
    document.getElementById('currentTime').textContent  = formatTime(playerState.elapsed);
}

function setVolume(v) {
    playerState.volume = parseFloat(v);
}

function playAll() {
    const first = getAllCards().find(c => !c.classList.contains('hidden'));
    if (first) loadTrack(first);
}

function shuffleTracks() {
    const cards = getAllCards().filter(c => !c.classList.contains('hidden'));
    if (!cards.length) return;
    const pick = cards[Math.floor(Math.random() * cards.length)];
    loadTrack(pick);
}

// ─── FILTER ───────────────────────────────────────────────────
document.querySelectorAll('.filter-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        const filter = btn.dataset.filter;
        getAllCards().forEach(card => {
            const match = filter === 'all' || card.dataset.genre === filter;
            card.classList.toggle('hidden', !match);
        });
    });
});

// ─── PLAY BUTTON ON CARD ──────────────────────────────────────
function handlePlay(e, btn) {
    e.stopPropagation();
    const card = btn.closest('.track-card');
    loadTrack(card);
}

// ─── COMMENTS ────────────────────────────────────────────────
let activeCommentTrackId = null;

const sampleComments = {
    1: [
        { user: 'USER_4412', time: '2026-05-22 03:17', text: 'this one hits different at 3am. classic energy.' },
        { user: 'RDTN_77',   time: '2026-05-23 18:44', text: 'the acid line at 2:30 is unreal. studied this for years.' }
    ],
    2: [
        { user: 'NX_88',     time: '2026-05-20 11:02', text: 'ageispolis forever. nothing sounds like this.' }
    ],
    3: [
        { user: 'GLITCH_X',  time: '2026-05-24 22:15', text: 'windowlicker changed my brain chemistry permanently.' },
        { user: 'USER_0001', time: '2026-05-25 09:30', text: 'the video still haunts me lol' }
    ]
};

function handleComments(e, trackId, trackTitle) {
    e.stopPropagation();
    activeCommentTrackId = trackId;
    document.getElementById('commentsTitle').textContent = `// COMMENTS :: ${trackTitle.toUpperCase()}`;
    renderComments(trackId);
    document.getElementById('commentsPanel').classList.add('open');
}

function renderComments(trackId) {
    const list = document.getElementById('commentsList');
    const comments = sampleComments[trackId] || [];
    list.innerHTML = comments.length
        ? comments.map(c => `
            <div class="comment">
                <div class="comment-meta">
                    <span class="comment-user">${escHtml(c.user)}</span>
                    <span class="comment-time">${escHtml(c.time)}</span>
                </div>
                <p class="comment-text">${escHtml(c.text)}</p>
            </div>`).join('')
        : '<p style="color:var(--text-dim);font-size:0.7rem">// NO TRANSMISSIONS YET</p>';
}

function closeComments() {
    document.getElementById('commentsPanel').classList.remove('open');
}

function postComment() {
    const userEl = document.getElementById('commentUser');
    const textEl = document.getElementById('commentText');
    const user   = userEl.value.trim() || 'ANON_' + Math.floor(Math.random() * 9999);
    const text   = textEl.value.trim();
    if (!text) return;

    const id = activeCommentTrackId;
    if (!sampleComments[id]) sampleComments[id] = [];
    sampleComments[id].push({ user, time: new Date().toISOString().slice(0,16).replace('T',' '), text });
    renderComments(id);

    userEl.value = '';
    textEl.value = '';

    const list = document.getElementById('commentsList');
    list.scrollTop = list.scrollHeight;
}

// ─── SHARE ───────────────────────────────────────────────────
let shareUrl = '';

function handleShare(e, trackId) {
    e.stopPropagation();
    shareUrl = `${window.location.origin}/track/${trackId}`;
    document.getElementById('shareUrl').textContent = shareUrl;
    document.getElementById('shareCopied').classList.remove('visible');
    document.getElementById('shareModal').classList.add('open');
    document.getElementById('shareOverlay').classList.add('open');
}

function closeShare() {
    document.getElementById('shareModal').classList.remove('open');
    document.getElementById('shareOverlay').classList.remove('open');
}

function copyShareLink() {
    navigator.clipboard.writeText(shareUrl).then(() => {
        document.getElementById('shareCopied').classList.add('visible');
    });
}

// ─── UTIL ─────────────────────────────────────────────────────
function escHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}