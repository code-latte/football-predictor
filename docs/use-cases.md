# Use Cases

## Actors

| Actor | Description |
|---|---|
| **User** | Registered end-user who interacts via the web app or mobile app |
| **Admin** | Backoffice operator who manages platform data and configuration |
| **Updater** | Automated background process that ingests data from external football APIs |

---

## User Use Cases

### UC-U01 — Register

**Actor:** User  
**Trigger:** User visits the platform for the first time and chooses to create an account.

**Flow:**
1. User submits email, password, and chosen nickname.
2. Auth service validates uniqueness and creates the identity record.
3. `identity.user.registered.v1` is emitted.
4. User Profile service creates the profile (nickname, default avatar).
5. Leagues service initialises the user's league membership state.
6. User receives a confirmation and is redirected to the home screen.

**Postconditions:** Identity and profile records exist. User can log in.

---

### UC-U02 — Login / Logout

**Actor:** User  
**Trigger:** User provides credentials on the login screen, or taps "Log out".

**Flow (login):**
1. User submits email and password.
2. Auth service validates credentials and issues a JWT plus a refresh token.
3. `identity.deviceToken.added.v1` is emitted when a new device is registered.
4. User is redirected to the home screen.

**Flow (logout):**
1. User taps "Log out".
2. Client discards the JWT; the refresh token is revoked server-side.

---

### UC-U03 — Manage Profile

**Actor:** User  
**Trigger:** User opens the profile settings screen.

**Flow:**
1. User updates nickname, avatar, or notification preferences.
2. User Profile service persists the changes.
3. `profile.updated.v1` is emitted.
4. Leagues service updates the display name in league standings.

**Postconditions:** Changes are reflected across the platform.

---

### UC-U04 — Browse Competitions and Fixtures

**Actor:** User  
**Trigger:** User opens the home screen or navigates to the fixtures section.

**Flow:**
1. User selects a competition (e.g., Premier League, La Liga).
2. Fixtures service returns the list of upcoming and past matches.
3. User can filter by matchday or team.

**Preconditions:** At least one competition and its fixtures have been loaded by the Updater.

---

### UC-U05 — Submit a Prediction

**Actor:** User  
**Trigger:** User selects an upcoming match and enters a score prediction.

**Preconditions:**
- The match status is `Upcoming` (not yet kicked off).
- The user is authenticated.

**Flow:**
1. User selects a match and enters home score / away score.
2. Predictions service validates that the match has not kicked off.
3. Prediction is persisted and `prediction.submitted.v1` is emitted.

**Postconditions:** Prediction is stored and visible in the user's prediction history.

---

### UC-U06 — Replace a Prediction

**Actor:** User  
**Trigger:** User changes their mind before a match kicks off.

**Preconditions:**
- A prediction already exists for the match.
- The match has not kicked off.

**Flow:**
1. User opens their existing prediction and edits the score.
2. Predictions service replaces the previous entry.
3. `prediction.replaced.v1` is emitted.

---

### UC-U07 — View Prediction History

**Actor:** User  
**Trigger:** User navigates to the "My Predictions" section.

**Flow:**
1. Predictions service returns all predictions for the user, grouped by matchday.
2. Each prediction shows the submitted score, actual result (if available), and points earned.

---

### UC-U08 — View Match Results

**Actor:** User  
**Trigger:** User opens the results section.

**Flow:**
1. Fixtures service returns completed matches with final scores.
2. User can drill into a match to see individual prediction outcomes.

---

### UC-U09 — View Personal Score and Ranking

**Actor:** User  
**Trigger:** User opens their dashboard or ranking page.

**Flow:**
1. Stats service returns the user's total points and global rank.
2. Scoring Engine data feeds into the display in near-real-time.

---

### UC-U10 — View Global Leaderboard

**Actor:** User  
**Trigger:** User opens the leaderboard section.

**Flow:**
1. Stats service returns the ranked list of all users by total points.
2. User can filter by competition or time period.
3. `leaderboard.updated.v1` is emitted whenever rankings change.

---

### UC-U11 — Create a Private League

**Actor:** User  
**Trigger:** User taps "Create League" and provides a name and optional description.

**Flow:**
1. Leagues service creates the league and sets the creator as the admin.
2. `league.created.v1` is emitted.
3. An invite code is returned for the creator to share.

---

### UC-U12 — Join a Private League

**Actor:** User  
**Trigger:** User enters an invite code.

**Flow:**
1. Leagues service validates the code.
2. User is added to the league.
3. `league.joined.v1` is emitted.
4. League standings are updated.

---

### UC-U13 — Leave a Private League

**Actor:** User  
**Trigger:** User taps "Leave League".

**Flow:**
1. Leagues service removes the membership.
2. `league.left.v1` is emitted.
3. League table is recalculated.

---

### UC-U14 — View League Standings

**Actor:** User  
**Trigger:** User opens a league they belong to.

**Flow:**
1. Leagues service returns the ranked table of members with their points.
2. `league.tableUpdated.v1` is emitted after each scoring update.

---

### UC-U15 — Receive Notifications

**Actor:** User  
**Trigger:** Platform events that affect the user (kickoff approaching, score updated, points awarded).

**Flow:**
1. Notifications service consumes `match.kickoff.v1`, `prediction.locked.v1`, or `scoring.userScoreUpdated.v1`.
2. Service dispatches a push notification to the user's registered device and/or an email.
3. `notification.sent.v1` is emitted.

**Preconditions:** User has not disabled the relevant notification type in their preferences.

---

## Admin Use Cases

### UC-A01 — Create / Update a Competition

**Actor:** Admin  
**Trigger:** Admin opens the Competitions section in the backoffice.

**Flow:**
1. Admin fills in the competition name, country, logo, and season dates.
2. Catalog service persists the competition.
3. `competition.created.v1` or `competition.updated.v1` is emitted.
4. Fixtures service is notified and can now accept matches for this competition.

---

### UC-A02 — Create / Update a Team

**Actor:** Admin  
**Trigger:** Admin opens the Teams section in the backoffice or the Updater triggers an upsert.

**Flow:**
1. Admin fills in the team name, short code, crest, and home city.
2. Catalog service persists or updates the team.
3. `team.upserted.v1` is emitted.
4. Fixtures service updates any affected match records.

---

### UC-A03 — Configure Scoring Rules

**Actor:** Admin  
**Trigger:** Admin opens the Scoring Rules section in the backoffice.

**Flow:**
1. Admin sets the point values for exact-score prediction, correct-result prediction, and any bonus rules.
2. Configuration is persisted and applied by the Scoring Engine from the next scored match onward.

**Postconditions:** All future match scoring uses the updated ruleset.

---

### UC-A04 — Moderate a User

**Actor:** Admin  
**Trigger:** Admin reviews a reported account in the backoffice.

**Flow:**
1. Admin selects a user and chooses "Disable Account".
2. Auth service marks the identity as disabled.
3. `identity.user.disabled.v1` is emitted.
4. All active sessions for that user are invalidated.

**Postconditions:** Disabled user cannot log in or submit predictions.

---

### UC-A05 — Monitor System Health

**Actor:** Admin  
**Trigger:** Admin opens the Grafana dashboard or checks the observability panel.

**Flow:**
1. Each microservice exposes `/metrics` collected by Prometheus.
2. Grafana displays dashboards for throughput, error rates, latency, and queue depth.
3. Serilog structured logs are available with correlation IDs for cross-service tracing.
4. Admin receives alerts when thresholds are breached.

---

### UC-A06 — Manual Fixture Override

**Actor:** Admin  
**Trigger:** External API data is incorrect or missing; admin needs to correct a match record.

**Flow:**
1. Admin opens the match record in the backoffice.
2. Admin edits kickoff time, teams, or score.
3. Fixtures service persists the change and emits the relevant events (`match.upserted.v1`, `match.scoreChanged.v1`, or `match.finalized.v1`).
4. Downstream services (Scoring Engine, Predictions) react to the updated events.

---

## Updater Process Use Cases

> The Updater is a background service (or scheduled job) that reads data from one or more external football data APIs (e.g., API-Football, football-data.org) and keeps the platform's internal state in sync.

---

### UC-UP01 — Fetch and Sync Competitions

**Actor:** Updater  
**Trigger:** Scheduled run (e.g., start of season) or manual trigger by Admin.

**Flow:**
1. Updater calls the external API for the list of active competitions.
2. For each competition, Updater calls the Catalog service to upsert the record.
3. Catalog service emits `competition.created.v1` or `competition.updated.v1`.

**Postconditions:** All known competitions are present in the Catalog.

---

### UC-UP02 — Fetch and Sync Teams

**Actor:** Updater  
**Trigger:** Scheduled run (e.g., start of season, transfer window close) or on-demand.

**Flow:**
1. Updater calls the external API for teams within each tracked competition.
2. For each team, Updater calls the Catalog service to upsert the record.
3. Catalog service emits `team.upserted.v1`.

**Postconditions:** All known teams are present in the Catalog with up-to-date metadata.

---

### UC-UP03 — Fetch and Sync Upcoming Fixtures

**Actor:** Updater  
**Trigger:** Scheduled run at the start of each matchday (e.g., every Monday morning).

**Flow:**
1. Updater calls the external API for fixtures in the upcoming matchday window.
2. For each fixture, Updater calls the Fixtures service to upsert the match record.
3. Fixtures service emits `match.upserted.v1`.
4. Predictions service receives the event and opens the prediction window for the match.

**Postconditions:** Users can submit predictions for the new fixtures.

---

### UC-UP04 — Update Live Match Score

**Actor:** Updater  
**Trigger:** Polling loop during active match windows (e.g., every 60 seconds on matchdays).

**Preconditions:** At least one match is currently in progress.

**Flow:**
1. Updater polls the external API for current scores of in-progress matches.
2. If the score has changed since the last poll, Updater calls the Fixtures service.
3. Fixtures service persists the updated score and emits `match.scoreChanged.v1`.
4. Scoring Engine consumes the event and recalculates affected user scores in real time.
5. Stats service updates the leaderboard.

**Postconditions:** Users see live score updates; leaderboard reflects provisional standings.

---

### UC-UP05 — Finalize a Match Result

**Actor:** Updater  
**Trigger:** External API reports a match as finished.

**Flow:**
1. Updater detects that a match status has changed to `Finished`.
2. Updater calls the Fixtures service with the final score.
3. Fixtures service emits `match.finalized.v1`.
4. Scoring Engine consumes the event, runs the final scoring calculation, and emits `scoring.matchScored.v1` and `scoring.userScoreUpdated.v1`.
5. Leagues service updates league tables and emits `league.tableUpdated.v1`.
6. Stats service updates the global leaderboard and emits `leaderboard.updated.v1`.
7. Notifications service emits push/email notifications to users with predictions on that match.

**Postconditions:** All scores are final and immutable for the match. Users see their earned points.

---

### UC-UP06 — Lock Predictions at Kickoff

**Actor:** Updater  
**Trigger:** External API reports a match status change to `In Progress` (kickoff).

**Flow:**
1. Updater detects kickoff for a match.
2. Updater calls the Fixtures service to emit `match.kickoff.v1`.
3. Predictions service consumes the event and marks all existing predictions as locked; no further submissions are accepted.
4. Notifications service sends kickoff reminders to users who have not yet predicted.

**Postconditions:** Prediction window is closed. Locked predictions await final scoring.

---

## Event Summary

The table below maps each use case to the integration events it produces or consumes across the microservices.

| Use Case | Events Emitted | Events Consumed |
|---|---|---|
| UC-U01 Register | `identity.user.registered.v1` | — |
| UC-U02 Login | `identity.deviceToken.added.v1` | — |
| UC-U03 Update Profile | `profile.updated.v1` | — |
| UC-U05 Submit Prediction | `prediction.submitted.v1` | `match.upserted.v1` |
| UC-U06 Replace Prediction | `prediction.replaced.v1` | `match.kickoff.v1` |
| UC-U11 Create League | `league.created.v1` | — |
| UC-U12 Join League | `league.joined.v1` | `identity.user.registered.v1` |
| UC-U13 Leave League | `league.left.v1` | — |
| UC-U15 Notifications | `notification.sent.v1` | `match.kickoff.v1`, `prediction.locked.v1`, `scoring.userScoreUpdated.v1` |
| UC-A01 Create Competition | `competition.created.v1` / `competition.updated.v1` | — |
| UC-A02 Create Team | `team.upserted.v1` | — |
| UC-A04 Disable User | `identity.user.disabled.v1` | — |
| UC-A06 Manual Fixture Override | `match.upserted.v1` / `match.scoreChanged.v1` / `match.finalized.v1` | — |
| UC-UP03 Sync Fixtures | `match.upserted.v1` | `competition.created.v1`, `team.upserted.v1` |
| UC-UP04 Live Score Update | `match.scoreChanged.v1` | — |
| UC-UP05 Finalize Result | `match.finalized.v1` → `scoring.matchScored.v1` → `scoring.userScoreUpdated.v1` → `league.tableUpdated.v1` → `leaderboard.updated.v1` | — |
| UC-UP06 Lock at Kickoff | `match.kickoff.v1` → `prediction.locked.v1` | — |
