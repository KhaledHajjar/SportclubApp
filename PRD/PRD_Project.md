# PRD — Project (Implementation)

**Sportclub App — .NET MAUI + ASP.NET Web API**

| | |
|---|---|
| **Module** | LU3 — Agile realisatie .NET informatiesysteem |
| **Deliverable** | Working Proof of Concept: ASP.NET Web API + .NET MAUI app |
| **Out of scope** | Blazor admin app, real payment integration, real RFID hardware |
| **Stack** | C# 14, .NET 10, ASP.NET Core Web API, .NET MAUI, EF Core, JWT auth |
| **Platforms** | iOS + Android (both must be demonstrable) |
| **Sensors** | Camera/Gallery (profile photo) |
| **Cross-cutting** | JWT/OAuth authentication, push notifications |

---

## 1. Goal

Build a Proof of Concept that a sportclub member can use on iOS and Android to manage their subscription, browse the class schedule, reserve and cancel classes, and join a waiting list. The PoC includes its own ASP.NET Web API backed by a real database. The MAUI app and the API are the two artefacts under assessment; the Blazor admin app from the original case is replaced by direct-database seeding for the PoC.

## 2. In scope

- Member authentication via JWT (login + token refresh).
- View own profile; upload/replace profile photo from camera or gallery.
- View own subscription status (active subscription, end date, remaining weekly visits if 2x/week plan).
- Browse class schedule for the next 7 days.
- View class detail with current participants count and instructor.
- Reserve a class (respects 2x/week limit; respects 1-week-ahead window).
- Cancel a reservation up to 1 hour before start.
- Join and leave the waiting list when a class is full.
- Receive a push notification when a slot becomes available on a class the user is waiting for.
- Receive a push notification 6 weeks before a yearly subscription expires.
- View own attendance history for the past year.
- API endpoints exposing all of the above, secured with JWT and role-based authorization.

## 3. Out of scope (explicit)

- Blazor admin web app and any employee-only feature.
- Real payment integration (subscription purchase is mocked or seeded).
- Real RFID hardware and on-arrival check-in (out of scope for the PoC).
- Spinning seat picker (mention in design as future extension; not implemented).
- Multi-tenant or multi-club support.
- Real email delivery (logged to console / captured in test fake).

## 4. User stories

All stories follow: *As a `<role>`, I want `<goal>`, so that `<reason>`.* Each has acceptance criteria in Given/When/Then in the design document. MoSCoW priority is shown below; only Must-have stories are required for the PoC pass; Should/Could stories are stretch.

| ID | Story | Role | Priority |
|---|---|---|---|
| US-01 | Register and log in to the app with email and password. | Member | Must |
| US-02 | Stay logged in across app restarts via stored refresh token. | Member | Must |
| US-03 | View and update my profile, including a profile photo from camera or gallery. | Member | Must |
| US-04 | View my current subscription, its type, and its end date. | Member | Must |
| US-05 | Browse the class schedule for the next 7 days. | Member | Must |
| US-06 | Reserve a class that has free spots and is within the allowed window. | Member | Must |
| US-07 | Cancel a reservation up to one hour before class start. | Member | Must |
| US-08 | Join the waiting list when a class is full and leave it whenever I want. | Member | Must |
| US-09 | Get a push notification when a spot opens on a class I am waiting for. | Member | Must |
| US-10 | Receive a notification 6 weeks before my yearly subscription expires. | Member | Should |
| US-11 | View my class history for the past year. | Member | Should |
| US-12 | As an instructor, see who is reserved for my class. | Instructor | Could |

Detailed Given/When/Then acceptance criteria for every story live in the design document, not here. The design document is the source of truth for AC; this PRD lists what must be built.

## 5. Functional requirements

### 5.1 ASP.NET Web API

- Built on ASP.NET Core (.NET 10) with controllers or minimal APIs (consistent across the project).
- Persistence via EF Core with a real provider (SQL Server, PostgreSQL, or SQLite — pick one and stay with it).
- Authentication: ASP.NET Core Identity issuing JWT access tokens (short-lived, e.g. 15 min) and refresh tokens (longer, rotated on use).
- Authorization: role-based, at minimum roles "Member" and "Instructor".
- OpenAPI/Swagger enabled in Development; spec is what the design document references.
- Versioned routes: `/api/v1/...`
- Errors return RFC 7807 ProblemDetails.
- Validation via DataAnnotations or FluentValidation; 400 responses include field-level errors.
- Database is seeded with workouts, locations, instructors, classes, and at least 2 test members so the API is demoable on a clean clone.
- Push: a server-side endpoint that triggers Firebase Cloud Messaging (Android) and APNs (iOS) — directly or via Azure Notification Hubs. Either is acceptable; choice is justified in the design.

### 5.2 .NET MAUI app

- Built on .NET 10 MAUI with C# 14.
- MVVM throughout: Views in XAML, ViewModels with `INotifyPropertyChanged` (or CommunityToolkit.Mvvm source generators), bound via `{Binding}`.
- Dependency injection via the built-in `MauiAppBuilder`; no service locators or static singletons for services.
- HTTP layer through a typed `HttpClient`; API responses deserialized into DTOs distinct from ViewModels.
- JWT and refresh tokens stored in MAUI `SecureStorage`. Never in `Preferences`/`SharedPreferences`/`UserDefaults` plain.
- Push notifications received and surfaced via OS notification channels; tapping a notification deep-links into the relevant class detail.
- Camera + gallery via Essentials/`MediaPicker`; runtime permissions requested with rationale.
- App runs on iOS and Android. Debug build on at least one physical or emulated device per platform is reproducible.

## 6. Non-functional requirements

### 6.1 Code quality

- All source code, identifiers, comments, commit messages, and UI strings in English.
- Consistent style: `.editorconfig` committed; warnings treated as errors in Release.
- Nullable reference types enabled.
- Async APIs used end-to-end; no `.Result` or `.Wait()` on Tasks in production paths.
- OO principles applied with intent: encapsulation, single responsibility, programming to interfaces. The design document explains where and why.
- Design patterns used and named: at minimum Repository (or equivalent), DI, MVVM, and one more (e.g. Strategy, Observer, Mediator). Each is justified.

### 6.2 Security

- HTTPS enforced for all API traffic; HTTP redirected.
- Passwords hashed (Identity default: PBKDF2 with per-user salt).
- JWT signing key from configuration (User Secrets in dev, env vars in CI), never committed.
- Refresh tokens are single-use and rotated; revocation is supported on logout.
- Authorization checked on every API endpoint that touches user-owned data; an unauthenticated request returns 401, an authenticated-but-forbidden one returns 403.

### 6.3 Reliability and performance

- API endpoints respond under 500 ms for happy-path requests on local dev hardware (target, not a hard SLA).
- App handles offline gracefully: failed network calls show a user-readable error and a retry option, do not crash.
- Push delivery has a fallback: if the device token is missing, the in-app notification list still updates on next foreground.

### 6.4 Architecture

- API: layered or Clean Architecture; the choice is named and justified in the design document.
- MAUI app: MVVM with a Services layer between ViewModels and the HTTP client.
- No direct API calls from XAML code-behind.
- Dependency direction is enforced: domain has no reference to infrastructure or presentation projects.

### 6.5 Documentation

- `README.md` at the repo root with setup, run, and test instructions.
- Separate "Run on iOS" and "Run on Android" subsections — exact commands, prerequisites, environment variables.
- Database seeding command documented (e.g. `dotnet run --project Api -- seed`).
- Test users with credentials listed in the README (these are obviously fake / dev-only).

## 7. Tests

LU3 requires at least 3 meaningful unit tests and at least 3 meaningful UI/device-running tests. "Meaningful" rules out trivial getter/setter or framework-validation tests. Each test must be defendable: "this test exists because if it failed, X user-visible behaviour would break."

### 7.1 Unit tests (minimum 3)

- Test framework: xUnit (or NUnit/MSTest — pick one and stay).
- Mocking via NSubstitute or Moq.
- Suggested coverage: reservation business rule (refuses when class full and 2x/week limit reached), waiting-list promotion logic, JWT refresh flow, ViewModel command behaviour (e.g. `ReserveCommand` disables itself while in flight).
- Tests run on every build.

### 7.2 UI / device-running tests (minimum 3)

- Approach: Appium with .NET, Maestro, or .NET MAUI's UITest — pick one and document the choice.
- Suggested scenarios: login happy path, reserve a class end-to-end, cancel a reservation, profile photo upload (mocked picker), receiving a push notification deep-links to the right screen.
- At least one UI test runs on each target platform (iOS and Android); the rest can run on either.
- UI tests have a deterministic setup: seeded DB, fresh login, screenshots on failure.

### 7.3 API integration tests (recommended)

- `WebApplicationFactory<TProgram>` for in-memory hosting.
- Test database is in-memory or a disposable container (Testcontainers).
- At least the reservation flow and the JWT auth flow are covered end-to-end.

### 7.4 Test report

- Test results exported (TRX or JUnit XML) and summarised in the design document.
- For each test: what it covers, expected outcome, last-run result.

## 8. Deliverables

1. Source code repository (Git) containing API project, MAUI project, test projects, and `/docs`.
2. Design document (separate `.docx`/`.pdf` — see PRD-Design).
3. README with installation and usage instructions.
4. Definition of Done checklist (filled in, see chapter 9) — included in the repo or in the design document.
5. Demo video covering: app purpose, user stories realised, design and implementation choices, parts the student is proud of.
6. Test report (exported test results).

## 9. Definition of Done

A user story is Done when ALL of the following are true:

1. Code is written, compiles without warnings in Release, and is merged to main.
2. Acceptance criteria from the design document pass.
3. Unit tests covering the new behaviour are written and pass.
4. If the story touches UI: at least one UI test or manual test path is documented.
5. API changes (if any) are reflected in the OpenAPI spec.
6. Code reviewed by self against checklist (no commented-out code, no TODOs without ticket reference, no `console.log`/`Debug.WriteLine` left in production paths).
7. Story is demoable on both iOS and Android.
8. Documentation (README or design document) updated where needed.

## 10. Risks and mitigations

| Risk | Why it matters | Mitigation |
|---|---|---|
| iOS build setup on Windows | Mac is required for full iOS builds and signing. | Use a Mac-in-cloud or pair-build session early; alternatively use the iOS simulator on a borrowed Mac for the demo. |
| Push notifications setup time | FCM and APNs each need certificates and project setup; this can blow up early in the project. | Spike early: send one test notification end-to-end before building any business logic on top. |
| UI test framework instability | MAUI UI testing tooling is younger than the rest; tests can be flaky. | Pick the framework early, write one smoke test, accept that scope on UI tests is exactly 3 if the tooling fights back. |
| Scope creep from the original case | The Bress case has spinning seat picker, instructor app, payment, etc. — all out of scope here. | Section 3 of this PRD is the answer; check against it before adding any feature. |
| Auth refresh edge cases | Refresh token logic is the most common source of subtle bugs. | Cover with unit tests early; manually test app on token expiry regularly. |
