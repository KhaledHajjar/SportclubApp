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

# create the SQLite database and seed demo data
dotnet ef database update --project src/SportclubApp.Api
dotnet run --project src/SportclubApp.Api -- seed
```

The seeder is idempotent — re-running it on an already-populated database is a no-op.

## Test users

All seed users use the password **`Password123!`**.

| Email | Role | Subscription |
|---|---|---|
| `alice@sportclub.test` | Member | TwicePerWeek (6 months) |
| `bob@sportclub.test` | Member | Yearly |
| `instructor@sportclub.test` | Instructor + Member | — |

## Run the API

```powershell
dotnet run --project src/SportclubApp.Api
```

- Scalar UI: `https://localhost:<port>/scalar/v1`
- OpenAPI spec: `https://localhost:<port>/openapi/v1.json`
- Use the Bearer button in Scalar after `POST /api/v1/auth/login` to authorize subsequent calls.

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

### Patterns used (for the design document)

| Pattern | Where |
|---|---|
| Dependency Injection | API + MAUI throughout |
| MVVM | every MAUI page / view model |
| Strategy | `ISubscriptionLimitPolicy` (`TwicePerWeekPolicy`, `UnlimitedPolicy`) |
| Observer / Mediator | `IDomainEventDispatcher` + `SlotOpenedEvent` + handlers |
| Repository (via EF) | `AppDbContext` + `DbSet<T>` |

## Out of scope

- Real payment integration (subscriptions are seeded)
- RFID hardware and on-arrival check-in
- Push notifications via FCM/APNs (replaced by in-app notifications + a local OS notification for subscription expiry)
- Real email delivery
- Multi-tenant / multi-club support
- Blazor admin web app

## Limitations and future work

The following are known limitations of the PoC. They're tolerated for the scope of this build and should be carried over to the design document's architecture section when it's written.

### Reservation race condition

`ReservationService.ReserveAsync` does check-then-act on capacity, duplicate reservations, and the weekly-visit limit without a surrounding transaction. Two concurrent requests can both pass the same gate and both insert. SQLite's single-writer model masks this in the PoC. Production needs:

- A unique filtered index `(MemberId, ClassSessionId)` where `Status = Active` to make a second active reservation a DB-level violation.
- A `RowVersion` token on `ClassSession` so capacity checks become an atomic compare-and-swap.

### Token-refresh race in the MAUI client

`AuthDelegatingHandler` has no mutex around the refresh path. If two requests hit 401 simultaneously, both call `/auth/refresh`; refresh tokens are single-use, so the second call fails, the handler clears the secure store, and the user is silently signed out mid-session. Production fix: a `SemaphoreSlim(1, 1)` so only one refresh runs and waiters re-read the freshly saved token.

### Waitlist promotion bypasses reservation rules

`WaitingListPromotionService.TryPromoteHeadAsync` writes a `Reservation` directly without consulting `ISubscriptionLimitPolicy`, the active-subscription check, or the 7-day window. This is **intentional**: by joining the waitlist a member explicitly opts in to a visit that may exceed their weekly limit. Anyone tempted to "fix" this should treat it as a product decision, not a bug.

## Definition of Done — per user story

For each story to count as done:

1. Code compiles without warnings in Release.
2. Acceptance criteria from the design document pass.
3. Story is reachable in the running app on at least one platform.
4. API changes are reflected in the OpenAPI spec.
5. No commented-out code, no `Debug.WriteLine` left in production paths.
6. README is up-to-date if a setup step changed.
