const API_BASE = 'http://127.0.0.1:5102';

const Auth = {
    getToken: () => localStorage.getItem('ax_token'),
    getUser:  () => JSON.parse(localStorage.getItem('ax_user') || 'null'),
    save(token, user) {
        localStorage.setItem('ax_token', token);
        localStorage.setItem('ax_user', JSON.stringify(user));
    },
    clear() {
        localStorage.removeItem('ax_token');
        localStorage.removeItem('ax_user');
    },
    isLoggedIn: () => !!localStorage.getItem('ax_token'),
    isAdmin: () => {
        const u = JSON.parse(localStorage.getItem('ax_user') || 'null');
        return u?.role === 'ADMIN';
    }
};

async function apiFetch(path, options = {}) {
    const token = Auth.getToken();
    const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
    if (token) headers['Authorization'] = `Bearer ${token}`;

    let res;
    try {
        res = await fetch(`${API_BASE}${path}`, { ...options, headers });
    } catch (networkErr) {
        throw new Error('Cannot reach server. Is the backend running?');
    }

    if (res.status === 401) {
        Auth.clear();
        updateNavAuth();
        throw new Error('Not authenticated.');
    }

    const text = await res.text();
    let data = null;
    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        if (!res.ok) throw new Error(`Server error ${res.status}. Is the API running on ${API_BASE}?`);
        throw new Error('Unexpected response from server.');
    }

    if (!res.ok) throw Object.assign(new Error(data?.message || `Error ${res.status}`), { status: res.status, data });
    return data;
}

const ApiAuth = {
    async register(username, email, password) {
        SpotifyAuth.clear();
        const data = await apiFetch('/api/auth/register', {
            method: 'POST',
            body: JSON.stringify({ username, email, password })
        });
        Auth.save(data.token, { id: data.userId, username: data.username, role: data.role });
        return data;
    },
    async login(username, password) {
        SpotifyAuth.clear();
        const data = await apiFetch('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
        Auth.save(data.token, { id: data.userId, username: data.username, role: data.role });
        return data;
    },
    logout() {
        Auth.clear();
        SpotifyAuth.clear();
        window.location.reload();
    }
};

const ApiSpotify = {
    async search(q, limit = 20) {
        const spToken = await SpotifyAuth.getValidToken();
        if (!spToken) throw new Error('Connect Spotify first to search.');

        const url = `https://api.spotify.com/v1/search?q=${encodeURIComponent(q)}&type=track`;
        const res = await fetch(url, { headers: { 'Authorization': `Bearer ${spToken}` } });
        if (!res.ok) {
            const err = await res.json().catch(() => null);
            console.error('Spotify search response:', res.status, err);
            throw Object.assign(new Error(`Spotify error ${res.status}`), { data: err });
        }
        return res.json();
    }
};

const ApiRatings = {
    create:     (req) => apiFetch('/api/ratings', { method: 'POST', body: JSON.stringify(req) }),
    update:     (id, req) => apiFetch(`/api/ratings/${id}`, { method: 'PUT', body: JSON.stringify(req) }),
    getMy:      () => apiFetch('/api/ratings/my'),
    getFriends: () => apiFetch('/api/ratings/friends'),
    getByUser:  (userId) => apiFetch(`/api/ratings/user/${userId}`),
    getTop:     () => apiFetch('/api/ratings/top')
};

const ApiFriends = {
    list:    () => apiFetch('/api/friends'),
    add:     (id) => apiFetch(`/api/friends/${id}`, { method: 'POST' }),
    remove:  (id) => apiFetch(`/api/friends/${id}`, { method: 'DELETE' }),
    activity:() => apiFetch('/api/friends/activity')
};

const ApiUsers = {
    me:             () => apiFetch('/api/users/me'),
    search:         (q) => apiFetch(`/api/users/search?q=${encodeURIComponent(q)}`),
    updateMe:       (req) => apiFetch('/api/users/me', { method: 'PUT', body: JSON.stringify(req) }),
    changePassword: (req) => apiFetch('/api/users/me/password', { method: 'PUT', body: JSON.stringify(req) })
};

const ApiComments = {
    get:    (trackId) => apiFetch(`/api/comments?trackId=${encodeURIComponent(trackId)}`),
    post:   (trackId, content) => apiFetch('/api/comments', { method: 'POST', body: JSON.stringify({ trackId, content }) }),
    delete: (id) => apiFetch(`/api/comments/${id}`, { method: 'DELETE' })
};

const ApiAdmin = {
    users:        () => apiFetch('/api/admin/users'),
    ban:          (id) => apiFetch(`/api/admin/users/${id}/ban`, { method: 'POST' }),
    unban:        (id) => apiFetch(`/api/admin/users/${id}/unban`, { method: 'POST' }),
    promote:      (id) => apiFetch(`/api/admin/users/${id}/promote`, { method: 'POST' }),
    demote:       (id) => apiFetch(`/api/admin/users/${id}/demote`, { method: 'POST' }),
    deleteUser:   (id) => apiFetch(`/api/admin/users/${id}`, { method: 'DELETE' }),
    allRatings:    () => apiFetch('/api/admin/ratings'),
    deleteReview:  (id) => apiFetch(`/api/admin/reviews/${id}`, { method: 'DELETE' }),
    allComments:   () => apiFetch('/api/admin/comments'),
    deleteComment: (id) => apiFetch(`/api/admin/comments/${id}`, { method: 'DELETE' })
};

const ApiAuthExtra = {
    forgotPassword: (email) => apiFetch('/api/auth/forgot-password', { method: 'POST', body: JSON.stringify({ email }) }),
    resetPassword:  (token, newPassword) => apiFetch('/api/auth/reset-password', { method: 'POST', body: JSON.stringify({ token, newPassword }) })
};

const SpotifyAuth = {
    save(token, refresh, expiresIn) {
        localStorage.setItem('sp_token', token);
        localStorage.setItem('sp_refresh', refresh);
        localStorage.setItem('sp_expiry', Date.now() + expiresIn * 1000);
    },
    getToken:   () => localStorage.getItem('sp_token'),
    getRefresh: () => localStorage.getItem('sp_refresh'),
    isConnected:() => !!localStorage.getItem('sp_token'),
    isExpired:  () => Date.now() > parseInt(localStorage.getItem('sp_expiry') || '0'),
    clear() {
        localStorage.removeItem('sp_token');
        localStorage.removeItem('sp_refresh');
        localStorage.removeItem('sp_expiry');
    },
    async getValidToken() {
        if (!SpotifyAuth.isConnected()) return null;
        if (SpotifyAuth.isExpired()) {
            const refresh = SpotifyAuth.getRefresh();
            if (!refresh) { SpotifyAuth.clear(); return null; }
            try {
                const res = await fetch('https://accounts.spotify.com/api/token', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: new URLSearchParams({
                        client_id:     SPOTIFY_CLIENT_ID,
                        grant_type:    'refresh_token',
                        refresh_token: refresh
                    })
                });
                const data = await res.json();
                if (data.access_token) {
                    SpotifyAuth.save(data.access_token, data.refresh_token || refresh, data.expires_in || 3600);
                } else {
                    SpotifyAuth.clear(); return null;
                }
            } catch { SpotifyAuth.clear(); return null; }
        }
        return SpotifyAuth.getToken();
    }
};

const SPOTIFY_CLIENT_ID = 'f6f1f356ecfc4e5881449cf256e6dc81';
const SPOTIFY_REDIRECT  = 'http://127.0.0.1:5222/';
const SPOTIFY_SCOPES    = 'streaming user-read-email user-read-private user-read-playback-state user-modify-playback-state';

function generateRandomString(length) {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
    let result = '';
    const values = crypto.getRandomValues(new Uint8Array(length));
    values.forEach(v => result += chars[v % chars.length]);
    return result;
}

async function generateCodeChallenge(verifier) {
    const data = new TextEncoder().encode(verifier);
    const digest = await crypto.subtle.digest('SHA-256', data);
    return btoa(String.fromCharCode(...new Uint8Array(digest)))
        .replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

async function connectSpotify() {
    if (!Auth.isLoggedIn()) { openAuthModal('login'); return; }
    const verifier  = generateRandomString(64);
    const challenge = await generateCodeChallenge(verifier);
    sessionStorage.setItem('sp_verifier', verifier);
    const params = new URLSearchParams({
        client_id:             SPOTIFY_CLIENT_ID,
        response_type:         'code',
        redirect_uri:          SPOTIFY_REDIRECT,
        scope:                 SPOTIFY_SCOPES,
        code_challenge_method: 'S256',
        code_challenge:        challenge
    });
    window.location.href = 'https://accounts.spotify.com/authorize?' + params.toString();
}

(async function handleSpotifyCallback() {
    const params = new URLSearchParams(window.location.search);
    const code   = params.get('code');
    if (!code) return;

    const verifier = sessionStorage.getItem('sp_verifier');
    if (!verifier) return;

    history.replaceState(null, '', window.location.pathname);
    sessionStorage.removeItem('sp_verifier');

    try {
        const res = await fetch('https://accounts.spotify.com/api/token', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({
                client_id:     SPOTIFY_CLIENT_ID,
                grant_type:    'authorization_code',
                code:          code,
                redirect_uri:  SPOTIFY_REDIRECT,
                code_verifier: verifier
            })
        });
        const data = await res.json();
        if (data.access_token) {
            SpotifyAuth.save(data.access_token, data.refresh_token || '', data.expires_in || 3600);
            updateNavAuth();
            window.dispatchEvent(new CustomEvent('spotify:connected', { detail: { token: data.access_token } }));
        }
    } catch (err) {
        console.error('Spotify token exchange failed:', err);
    }
})();

function updateNavAuth() {
    const loggedIn = Auth.isLoggedIn();
    const user = Auth.getUser();

    const loginBtn       = document.getElementById('nav-login-btn');
    const registerBtn    = document.getElementById('nav-register-btn');
    const logoutBtn      = document.getElementById('nav-logout-btn');
    const userLabel      = document.getElementById('nav-user-label');
    const adminLink      = document.getElementById('nav-admin-link');
    const profileLink    = document.getElementById('nav-profile-link');
    const spotifyBtn     = document.getElementById('nav-spotify-btn');
    const spotifyStatus  = document.getElementById('nav-spotify-status');
    const forgotPwBtn    = document.getElementById('forgotPwBtn');

    if (loginBtn)      loginBtn.style.display      = loggedIn ? 'none' : 'inline';
    if (registerBtn)   registerBtn.style.display   = loggedIn ? 'none' : 'inline';
    if (logoutBtn)     logoutBtn.style.display      = loggedIn ? 'inline' : 'none';
    if (userLabel)     userLabel.textContent        = loggedIn ? user?.username?.toUpperCase() : '';
    if (adminLink)     adminLink.style.display      = Auth.isAdmin() ? 'inline' : 'none';
    if (profileLink)   profileLink.style.display    = loggedIn ? 'inline' : 'none';
    if (spotifyBtn)    spotifyBtn.style.display     = loggedIn && !SpotifyAuth.isConnected() ? 'inline' : 'none';
    if (spotifyStatus) spotifyStatus.style.display  = SpotifyAuth.isConnected() ? 'inline' : 'none';
    if (forgotPwBtn)   forgotPwBtn.style.display    = document.getElementById('authMode')?.value === 'login' ? 'inline' : 'none';
}

document.addEventListener('DOMContentLoaded', updateNavAuth);

function openForgotPassword() {
    closeAuthModal();
    document.getElementById('forgotStep1').style.display = '';
    document.getElementById('forgotStep2').style.display = 'none';
    document.getElementById('forgotErr').textContent = '';
    document.getElementById('forgotEmail').value = '';
    document.getElementById('forgotModal').classList.add('open');
    document.getElementById('forgotOverlay').classList.add('open');
}

function closeForgotPassword() {
    document.getElementById('forgotModal')?.classList.remove('open');
    document.getElementById('forgotOverlay')?.classList.remove('open');
}

async function submitForgot() {
    const email = document.getElementById('forgotEmail').value.trim();
    const errEl = document.getElementById('forgotErr');
    if (!email) { errEl.textContent = '// EMAIL REQUIRED'; return; }
    try {
        const res = await ApiAuthExtra.forgotPassword(email);
        if (res.resetToken) {
            document.getElementById('resetToken').value = res.resetToken;
        }
        document.getElementById('forgotStep1').style.display = 'none';
        document.getElementById('forgotStep2').style.display = '';
        document.getElementById('resetErr').textContent = '';
        document.getElementById('resetOk').textContent = '';
    } catch (err) {
        errEl.textContent = err.message;
    }
}

async function submitReset() {
    const token  = document.getElementById('resetToken').value.trim();
    const pw     = document.getElementById('resetNewPw').value;
    const errEl  = document.getElementById('resetErr');
    const okEl   = document.getElementById('resetOk');
    errEl.textContent = '';
    okEl.textContent  = '';
    if (!token || !pw) { errEl.textContent = '// TOKEN AND PASSWORD REQUIRED'; return; }
    try {
        await ApiAuthExtra.resetPassword(token, pw);
        okEl.textContent = '// PASSWORD RESET — you can now log in';
        setTimeout(closeForgotPassword, 2000);
    } catch (err) {
        errEl.textContent = err.message;
    }
}

function togglePasswordVisibility() {
    const input = document.getElementById('authPassword');
    const btn   = document.getElementById('togglePwBtn');
    if (!input) return;
    const isHidden = input.type === 'password';
    input.type = isHidden ? 'text' : 'password';
    btn.textContent = isHidden ? '[ HIDE ]' : '[ SHOW ]';
}

function openAuthModal(mode) {
    const modal = document.getElementById('authModal');
    const overlay = document.getElementById('authOverlay');
    if (!modal) return;
    document.getElementById('authMode').value = mode;
    document.getElementById('authTitle').textContent = mode === 'login' ? '// LOGIN' : '// REGISTER';
    document.getElementById('authEmailRow').style.display = mode === 'register' ? '' : 'none';
    document.getElementById('authError').textContent = '';
    const pw = document.getElementById('authPassword');
    if (pw) pw.type = 'password';
    const toggleBtn = document.getElementById('togglePwBtn');
    if (toggleBtn) toggleBtn.textContent = '[ SHOW ]';
    modal.classList.add('open');
    overlay.classList.add('open');
}

function closeAuthModal() {
    document.getElementById('authModal')?.classList.remove('open');
    document.getElementById('authOverlay')?.classList.remove('open');
}

async function submitAuth() {
    const mode     = document.getElementById('authMode').value;
    const username = document.getElementById('authUsername').value.trim();
    const password = document.getElementById('authPassword').value;
    const email    = document.getElementById('authEmail')?.value.trim();
    const errEl    = document.getElementById('authError');

    try {
        if (mode === 'login') {
            await ApiAuth.login(username, password);
        } else {
            await ApiAuth.register(username, email, password);
        }
        closeAuthModal();
        updateNavAuth();
        // reload to refresh any page data
        window.location.reload();
    } catch (err) {
        errEl.textContent = err.message || 'Error. Try again.';
    }
}