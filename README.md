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

# seed demo data — the API auto-migrates the SQLite database on first run
dotnet run --project src/SportclubApp.Api -- seed
```

The seeder is idempotent — re-running it on an already-populated database is a no-op.

## Test users

All seed users use the password **`Test1234!`**.

| Email | Role | Subscription | Demonstrates |
|---|---|---|---|
| `alice@sportclub.test` | Member | TwicePerWeek | 2x/week limit; cancel her Yoga Tuesday 09:00 reservation to trigger the slot-opened notification flow for Charlie |
| `bob@sportclub.test` | Member | Yearly (40 days remaining) | 6-week subscription-expiry local notification fires immediately on login |
| `charlie@sportclub.test` | Member | Unlimited | Head of waitlist for the demo class — receives the slot-opened notification when Alice cancels |
| `test@test.com` | Member | TwicePerWeek | Quick-login convenience account; second on the demo waitlist |
| `diana@sportclub.test` | Instructor | — | Instructor view: her teaching schedule and class participants |

Each non-instructor account also has ~5 attendance rows across the past 6 weeks (History tab non-empty), 2 active future reservations (My classes non-empty), and one already-read SlotOpened notification (Notifications tab non-empty).

## Demo flows

### Slot-opened waitlist notification

The seeder creates one Yoga session capped at 2 — the next Tuesday 09:00 — pre-reserved by Alice and Bob, with Charlie at position 1 and Test at position 2 on the waitlist. To trigger the slot-opened flow:

1. Log in as **alice@sportclub.test**, open *My classes*, cancel the Yoga Tuesday 09:00 reservation. (Cancel requires ≥ 1 hour before start; the demo class is always at least 12 hours out.)
2. Log out, log in as **charlie@sportclub.test**.
3. The Notifications tab title shows `(1)`. Tap the new entry — it deep-links to the class detail and clears the badge. Charlie also has a fresh active reservation for that class (he was promoted off the waitlist).

### Subscription-expiry local notification

Bob's yearly subscription ends 40 days from seed time, inside the 6-week threshold. Logging in as **bob@sportclub.test** schedules an OS-level local notification immediately via `Plugin.LocalNotification` — visible in the Android emulator's notification drawer (or iOS notification center) once the device clock crosses the scheduled time.

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
- **Design tokens**: centralised in `src/SportclubApp.Maui/Resources/Styles/`. `Colors.xaml` defines the semantic palette (`Brand`, `Accent`, `Surface`, `Border`, `TextPrimary/Secondary`, `Success/Warning/Danger/Info`) with paired `*Light` and `*Dark` values. `Styles.xaml` wires `AppThemeBinding` between them and exposes implicit styles (`ContentPage`, `Label`, `Entry`, `Button`, `Border`) plus named styles (`PrimaryButton`, `AccentButton`, `SecondaryButton`, `DangerButton`, `LinkButton`, `Card`, `ErrorBanner`, `WarningBanner`, `InfoBanner`, `PageTitle`, `SectionTitle`, `Caption`, `FieldLabel`, `ErrorText`, `SuccessText`, `Divider`) and spacing/radius constants (`SpacingXS/S/M/L/XL`, `RadiusS/M/L`, `PagePadding`, `CardPadding`, `ListItemPadding`). Pages reference these via `Style="{StaticResource ...}"` — no inline colors or font sizes.

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
