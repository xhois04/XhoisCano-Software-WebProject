# AphexXhois / EPOKA TWINS

Retro-cyberpunk music web app — ASP.NET MVC frontend + ASP.NET Web API backend with PostgreSQL.

---

## Project Structure

```
XhoisCano-Software-WebProject/
├── Keep-on-Rolling/          ← Frontend (ASP.NET MVC, .NET 10)
└── backend/
    └── AphexXhois.Api/       ← Backend (ASP.NET Web API, .NET 10)
```

---

## Backend Setup

### 1. Enter the backend folder

```bash
cd backend/AphexXhois.Api
```

### 2. Configure PostgreSQL connection string

Edit `appsettings.json` and replace the placeholders:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=aphex_xhois;Username=postgres;Password=YOUR_PASSWORD"
}
```

### 3. Set a JWT secret

In `appsettings.json` — must be at least 32 characters:

```json
"Jwt": {
  "Secret": "your-super-secret-key-at-least-32-characters-long"
}
```

### 4. Add Spotify credentials

Register at https://developer.spotify.com/dashboard and create an app.
Copy your Client ID and Client Secret into `appsettings.json`:

```json
"Spotify": {
  "ClientId": "your_client_id",
  "ClientSecret": "your_client_secret"
}
```

### 5. Create the database and run migrations

Make sure PostgreSQL is running, then:

```bash
dotnet ef database update
```

This creates the `aphex_xhois` database and all tables automatically.

### 6. Start the backend

```bash
dotnet run
```

The API will start at `http://localhost:5102` (check the console for the actual port).

---

## Frontend Setup

### 1. Update the API base URL if needed

Open `Keep-on-Rolling/wwwroot/js/api.js` and update:

```js
const API_BASE = 'http://localhost:5102';
```

### 2. Start the frontend

```bash
cd Keep-on-Rolling
dotnet run
```

The MVC app starts at `http://localhost:5000` or `https://localhost:7000`.

---

## Environment Variables (alternative to appsettings.json)

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Secret` | JWT signing secret (32+ chars) |
| `Spotify__ClientId` | Spotify app client ID |
| `Spotify__ClientSecret` | Spotify app client secret |

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register |
| POST | `/api/auth/login` | Public | Login |
| GET | `/api/users/me` | User | Get own profile |
| GET | `/api/users/search?q=` | User | Search users |
| GET | `/api/friends` | User | List friends |
| POST | `/api/friends/{id}` | User | Add friend |
| DELETE | `/api/friends/{id}` | User | Remove friend |
| GET | `/api/friends/activity` | User | Friend rating activity |
| POST | `/api/ratings` | User | Rate a track |
| PUT | `/api/ratings/{id}` | User | Update rating |
| GET | `/api/ratings/my` | User | My ratings |
| GET | `/api/ratings/friends` | User | Friends' ratings |
| GET | `/api/spotify/search?q=` | User | Search Spotify |
| GET | `/api/comments?trackId=` | Public | Get track comments |
| POST | `/api/comments` | User | Post comment |
| DELETE | `/api/comments/{id}` | User/Admin | Delete comment |
| GET | `/api/admin/users` | Admin | List all users |
| POST | `/api/admin/users/{id}/ban` | Admin | Ban user |
| POST | `/api/admin/users/{id}/unban` | Admin | Unban user |
| DELETE | `/api/admin/reviews/{id}` | Admin | Remove rating/comment |

---

## How to create an Admin user

After registering normally, update the role directly in PostgreSQL:

```sql
UPDATE "Users" SET "Role" = 'ADMIN' WHERE "Username" = 'your_username';
```

---

## CORS

Frontend origins allowed by default: `http://localhost:5000`, `https://localhost:7000`.
Update `Cors:AllowedOrigins` in `appsettings.json` if your frontend runs on a different port.
