# SportclubApp

ASP.NET Core Web API + .NET MAUI app where members can manage their subscription, browse classes, reserve and cancel, join a waiting list, and see in-app notifications.

## Stack

- .NET 10, C# 14
- ASP.NET Core Web API + EF Core (SQLite)
- ASP.NET Core Identity, JWT bearer auth, refresh tokens
- .NET MAUI (Android + iOS), CommunityToolkit.Mvvm, CommunityToolkit.Maui
- FluentValidation, Scalar (OpenAPI UI), Plugin.LocalNotification

## Repo layout

```
src/
├── SportclubApp.Api      ASP.NET Core Web API
├── SportclubApp.Admin    Blazor Web App (Interactive Server) — read-only admin panel
├── SportclubApp.Maui     .NET MAUI client (Android + iOS)
└── SportclubApp.Shared   DTOs, enums, error-type constants
tests/
├── SportclubApp.Api.Tests
└── SportclubApp.Maui.Tests
PRD/                       Product requirements + implementation plan
```

## Prerequisites

- .NET SDK 10.0.x
- MAUI workload: `dotnet workload install maui`
- Android: Android SDK + an emulator or physical device (USB debugging enabled)
- iOS: a Mac with Xcode (Windows can build but not deploy iOS)
- Visual Studio 2022 17.13+ or VS Code with the C# Dev Kit

## First-time setup

```powershell
git clone <this-repo>
cd SportclubApp

# restore the local dotnet-ef tool
dotnet tool restore

# JWT signing key is read from User Secrets (never committed)
cd src\SportclubApp.Api
dotnet user-secrets set "Jwt:SigningKey" "$([Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48)))"
cd ..\..

# seed demo data — the API auto-migrates the SQLite database on first run
dotnet run --project src/SportclubApp.Api -- seed
```

The seeder is idempotent — re-running it on an already-populated database is a no-op.

## Test users

All seed users use the password **`Test1234!`**.

| Email | Role | Plan | Demonstrates |
|---|---|---|---|
| `alice@sportclub.test` | Member | Standard Monthly (10 days remaining) | Standard cancellation lockout (1 h); cancel her Yoga Tuesday 09:00 reservation to trigger the slot-opened notification flow for Charlie |
| `bob@sportclub.test` | Member | Standard Yearly (40 days remaining) | Yearly billing → the 6-week subscription-expiry local notification fires on login |
| `charlie@sportclub.test` | Member | Premium Monthly (15 days remaining) | Premium cancellation lockout (15 min); head of waitlist for the demo class — receives the slot-opened notification when Alice cancels |
| `test@test.com` | Member | Standard Monthly (20 days remaining) | Quick-login convenience account; second on the demo waitlist |
| `diana@sportclub.test` | Instructor | — | Instructor view: her teaching schedule and class participants |
| `admin@sportclub.test` | Admin | — | Blazor admin panel — Dashboard / Members / Plans / Class sessions / Reservations |

Each non-instructor account also has ~5 attendance rows across the past 6 weeks (History tab non-empty), 2 active future reservations (My classes non-empty), and one already-read SlotOpened notification (Notifications tab non-empty).

## Demo flows

### Slot-opened waitlist notification

The seeder creates one Yoga session capped at 2 — the next Tuesday 09:00 — pre-reserved by Alice and Bob, with Charlie at position 1 and Test at position 2 on the waitlist. To trigger the slot-opened flow:

1. Log in as **alice@sportclub.test**, open *My classes*, cancel the Yoga Tuesday 09:00 reservation. (Alice is on the Standard tier, so cancellation requires ≥ 1 hour before start; the demo class is always at least 12 hours out.)
2. Log out, log in as **charlie@sportclub.test**.
3. The Notifications tab title shows `(1)`. Tap the new entry — it deep-links to the class detail and clears the badge. Charlie also has a fresh active reservation for that class (he was promoted off the waitlist).

### Subscription-expiry local notification

The expiry-warning lead time depends on the plan's billing period (Yearly: 6 weeks, Monthly: 1 week). Bob's Standard Yearly subscription ends 40 days from seed time — inside the 6-week threshold — so logging in as **bob@sportclub.test** schedules an OS-level local notification immediately via `Plugin.LocalNotification`. It's visible in the Android emulator's notification drawer (or iOS notification center) once the device clock crosses the scheduled time.

### Tier-based cancellation lockout

Alice (Standard) can cancel up to 1 hour before class start; Charlie (Premium) can cancel up to 15 minutes before. The lockout is chosen at runtime by `IPlanCancellationPolicy` (Strategy pattern: `StandardPlanPolicy` vs `PremiumPlanPolicy`). To demo, reserve a class that starts in ~30 minutes for both a Standard and a Premium member (e.g. via Scalar `POST /api/v1/classes/{classId}/reservations`) and attempt to cancel — the Standard cancel returns 409 `cancel-too-late`; the Premium cancel succeeds.

## Run the API

```powershell
dotnet run --project src/SportclubApp.Api
```

- Scalar UI: `https://localhost:<port>/scalar/v1`
- OpenAPI spec: `https://localhost:<port>/openapi/v1.json`
- Use the Bearer button in Scalar after `POST /api/v1/auth/login` to authorize subsequent calls.

## Run the admin panel (Blazor)

The Blazor Web App is a small read-only dashboard for staff. It runs as a separate process and talks to the API over HTTPS — same auth flow as the MAUI client (JWT bearer with silent refresh). Start the API first, then in another terminal:

```powershell
dotnet run --project src/SportclubApp.Admin
```

Open the URL printed in the console (defaults to `https://localhost:7xxx` — exact port from `launchSettings.json`). Sign in with **`admin@sportclub.test`** / **`Test1234!`**. Non-admin accounts (alice/bob/charlie/test/diana) are refused at the login screen — only the `Admin` role can enter.

What you can see: a dashboard with member/subscription/class/reservation counts, a searchable member directory, the plan catalog with active-subscription counts per plan, the class schedule for a configurable date range, and the 50 most recent reservations.

The admin API surface is gated by `[Authorize(Roles = AuthRoles.Admin)]` — see `src/SportclubApp.Api/Controllers/AdminController.cs`. The Blazor project itself is intentionally minimal: a per-circuit `AdminAuthState` holds the JWT, and `AdminApi` (the typed `HttpClient`) attaches the Bearer + refreshes on 401 inline — *not* via a `DelegatingHandler`, because `IHttpClientFactory` resolves handlers from its own DI scope rather than the Blazor circuit's, so a scoped `AdminAuthState` in a delegating handler would always be empty. Each page is a Razor component that calls `IAdminApi`.

## Run the MAUI app — Android

The MAUI app expects the API at `https://10.0.2.2:5001` from the Android emulator (default `localhost` resolves to the emulator itself, not the host). The HTTPS dev cert isn't trusted by Android by default — the app bypasses cert validation in DEBUG builds via an `HttpClientHandler` override.

```powershell
# from the repo root
dotnet build src/SportclubApp.Maui -t:Run -f net10.0-android
```

For a physical Android device, change `AppConstants.ApiBaseUrl` to your host's LAN IP (e.g. `https://192.168.1.50:5001`).

## Run the MAUI app — iOS

iOS builds and deployment require a Mac with Xcode installed.

```bash
# on the Mac, from the repo root
dotnet build src/SportclubApp.Maui -t:Run -f net10.0-ios
```

On the iOS simulator the API base URL `https://localhost:5001` resolves to the host correctly — no LAN-IP swap needed.

## Architecture

- **API**: layered — `Controllers/`, `Services/`, `Services/Policies/`, `Validators/`, `Common/Events/`, `Data/`, `Entities/`, `Extensions/`. `AppDbContext` is the Repository + Unit-of-Work (no separate `IRepository<T>` wrappers).
- **MAUI**: MVVM throughout. Pages and view models are constructor-injected via DI; no service-locator or static singletons except `UserContext.Current` and `NotificationContext.Current` which are observable singletons used as XAML binding sources.
- **Auth**: JWT access tokens (15 min) + rotated single-use refresh tokens. The MAUI `AuthDelegatingHandler` attaches the bearer header, refreshes silently on 401, and retries the original request once with the new token.
- **Photos**: profile photos stored under `wwwroot/uploads/{memberId}.{jpg|png}` and served by `UseStaticFiles`.
- **Design tokens**: centralised in `src/SportclubApp.Maui/Resources/Styles/`. `Colors.xaml` defines the semantic palette (`Brand`, `Accent`, `Surface`, `Border`, `TextPrimary/Secondary`, `Success/Warning/Danger/Info`) with paired `*Light` and `*Dark` values. `Styles.xaml` wires `AppThemeBinding` between them and exposes implicit styles (`ContentPage`, `Label`, `Entry`, `Button`, `Border`) plus named styles (`PrimaryButton`, `AccentButton`, `SecondaryButton`, `DangerButton`, `LinkButton`, `Card`, `ErrorBanner`, `WarningBanner`, `InfoBanner`, `PageTitle`, `SectionTitle`, `Caption`, `FieldLabel`, `ErrorText`, `SuccessText`, `Divider`) and spacing/radius constants (`SpacingXS/S/M/L/XL`, `RadiusS/M/L`, `PagePadding`, `CardPadding`, `ListItemPadding`). Pages reference these via `Style="{StaticResource ...}"` — no inline colors or font sizes.

### Patterns used (for the design document)

| Pattern | Where |
|---|---|
| Dependency Injection | API + MAUI throughout |
| MVVM | every MAUI page / view model |
| Strategy | `IPlanCancellationPolicy` (`StandardPlanPolicy`, `PremiumPlanPolicy`) — chooses the cancellation lockout per plan tier |
| Observer / Mediator | `IDomainEventDispatcher` + `SlotOpenedEvent` + handlers |
| Repository (via EF) | `AppDbContext` + `DbSet<T>` |

## Out of scope

- Real payment integration (subscriptions are seeded)
- RFID hardware and on-arrival check-in
- Push notifications via FCM/APNs (replaced by in-app notifications + a local OS notification for subscription expiry)
- Real email delivery
- Multi-tenant / multi-club support

## Limitations and future work

The following are known limitations of the PoC. They're tolerated for the scope of this build and should be carried over to the design document's architecture section when it's written.

### Reservation capacity race condition

`ReservationService.ReserveAsync` does check-then-act on capacity and the weekly-visit limit without a surrounding transaction. Duplicate active reservations are caught at the DB layer by a unique filtered index on `(MemberId, ClassSessionId)` where `Status = Active`, so the second insert fails and is translated to an `AlreadyReserved` ProblemDetails. Capacity overrun under concurrent writes is still possible because the count read is not atomic with the insert. Production fix: add a `RowVersion` token on `ClassSession` so capacity checks become an atomic compare-and-swap.

### Waitlist promotion bypasses reservation rules

`WaitingListPromotionService.TryPromoteHeadAsync` writes a `Reservation` directly without re-running the active-subscription check or the 7-day booking window. This is **intentional**: by joining the waitlist a member explicitly opted in to that class, regardless of any state change in the interim (subscription lapsed, booking window now stricter, etc.). Anyone tempted to "fix" this should treat it as a product decision, not a bug.

## Definition of Done — per user story

For each story to count as done:

1. Code compiles without warnings in Release.
2. Acceptance criteria from the design document pass.
3. Story is reachable in the running app on at least one platform.
4. API changes are reflected in the OpenAPI spec.
5. No commented-out code, no `Debug.WriteLine` left in production paths.
6. README is up-to-date if a setup step changed.
