# ClinicSite

A clinic appointment system: patients book online without registering, doctors run their day from a
private portal, and administrators manage the clinic's doctors and schedule.

Built with **ASP.NET Core 8**, **EF Core 8** (SQL Server LocalDB) and three **React + TypeScript +
Vite** front-ends, in a Clean Architecture layout.

---

## Contents

- [What it does](#what-it-does)
- [Architecture](#architecture)
- [Domain model](#domain-model)
- [Getting started](#getting-started)
- [Configuration](#configuration)
- [Running](#running)
- [Development seed data](#development-seed-data)
- [Tests](#tests)
- [Security](#security)

---

## What it does

### For patients — public site (`ClinicSite.Client`)

Booking takes four steps and no account: **specialty → doctor → time slot → contact form**. The
chosen slot is held by a short-lived reservation while the form is filled in, so two people cannot
take the same time.

The booking is **not final on submit**. It has to be confirmed by e-mail:

1. The slot is reserved, the booking is created as `PendingConfirmation`, and a confirmation e-mail
   goes out. The patient has **30 minutes**.
2. The link opens `/booking/confirm?token=…` in the React app, which sends a **POST** to the API —
   never a GET, so mail scanners and link previewers cannot confirm a booking by accident.
3. On success the booking becomes `Confirmed` and a **second e-mail** arrives with the appointment
   details and a separate **cancellation** link, valid until the appointment starts.
4. Unconfirmed bookings are swept to `Expired` by a background service and their slot is freed. The
   confirm endpoint enforces the same deadline itself, so a late click cannot slip through.

Cancellation is idempotent — clicking the link twice is not an error.

### For doctors — doctor portal (`ClinicSite.DoctorClient`)

Doctors never receive a password by e-mail. An administrator sends an **invitation**; the doctor
follows a single-use link and sets their own password. After that they sign in and see only their
own data — the doctor id comes from the JWT, never from the request.

- **Dashboard** — today's appointments, how many remain, when the next patient arrives, free windows.
- **Schedule** — day and week views, slot by slot.
- **Bookings list** — filter by date range, visit status, or patient name/e-mail; paged.
- **Visit status** — `Scheduled → CheckedIn → InProgress → Completed`, plus `NoShow` and `Cancelled`.
- **Private note** — free-form note per booking, never shown to the patient.
- **Patient history** — every visit of that patient across the whole clinic, newest first, with the
  doctor, specialty and outcome of each. See [Patient history](#patient-history) below.
- **Message patient** — free-form e-mail, always sent to the address on the booking.
- **Slot blocking** — close a single free slot, or the same time on every future day (a recurring
  block, e.g. a standing lunch break). Booked and past slots cannot be blocked.

### For administrators — admin portal (`ClinicSite.PortalClient`)

Manage specialties and doctors (including profile photos shown on the public site), invite doctors
to the portal and track account state (`No account` / `Invited` / `Active`), and review bookings and
the clinic schedule.

### Patient history

There is deliberately **no `Patient` entity** — the clinic takes bookings from anyone without
registration, so a patient is identified by the **e-mail on the booking**, compared trimmed and
case-insensitively. `patient@example.com`, `PATIENT@EXAMPLE.COM` and `" patient@example.com "` are
one person.

Access is gated by the booking the history is opened from: it must belong to the calling doctor,
otherwise the API answers **404** — the same response as a booking that does not exist, so it never
reveals that one exists under another doctor. Past that gate the history is **clinic-wide**: a
cardiologist sees that the patient also saw a dermatologist last month, which is the point of the
feature. The booking the history was opened from is part of the result and marked *This visit* in the
UI.

---

## Architecture

```
src/
  ClinicSite.Domain          Entities and enums. No dependencies.
  ClinicSite.Application     Use cases: services, DTOs, interfaces, domain exceptions.
                             Talks to the database only through IApplicationDbContext.
  ClinicSite.Infrastructure  EF Core DbContext, migrations, SMTP e-mail, JWT, seeding.
  ClinicSite.Api             Controllers, auth, rate limiting, exception middleware.

  ClinicSite.Client          Public patient site      → http://localhost:5173
  ClinicSite.PortalClient    Admin portal             → http://localhost:5174
  ClinicSite.DoctorClient    Doctor portal            → http://localhost:5175

test/
  ClinicSite.Tests           xUnit suite (SQLite in-memory)
```

Dependencies point inwards: `Api → Infrastructure → Application → Domain`.

Domain exceptions (`NotFoundException`, `ValidationException`, `ConflictException`, `GoneException`,
`EmailDeliveryException`) are thrown by services and translated into HTTP status codes in one place,
`ExceptionHandlingMiddleware`, so controllers stay free of try/catch.

---

## Domain model

Four entities: **Specialty**, **Doctor**, **AppointmentSlot**, **Booking**.

A slot belongs to one doctor and one point in time. It may accumulate several bookings over its
lifetime (one expired, then a fresh one), but at most one is active at a time.

Three status axes, kept separate on purpose:

| Enum | Meaning | Values |
|---|---|---|
| `SlotStatus` | The slot's own availability | `Free`, `Reserved`, `Booked`, `Blocked` |
| `BookingStatus` | The e-mail confirmation lifecycle | `PendingConfirmation`, `Confirmed`, `Cancelled`, `Expired`, `Completed`, `NoShow` |
| `AppointmentStatus` | The visit outcome, managed by the doctor | `Scheduled`, `CheckedIn`, `InProgress`, `Completed`, `NoShow`, `Cancelled` |

All times are stored and compared in **UTC**; the clients format to local time for display.

---

## Getting started

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (`(localdb)\mssqllocaldb`)
- Node.js 18+
- `dotnet-ef` — `dotnet tool install --global dotnet-ef`

### Database

The API applies migrations automatically on startup. To do it by hand:

```bash
dotnet ef database update \
  --project src/ClinicSite.Infrastructure \
  --startup-project src/ClinicSite.Api
```

---

## Configuration

`appsettings.json` holds only non-secret settings. Secrets go in **User Secrets** (`UserSecretsId`
is already set in `ClinicSite.Api.csproj`, so no `init` is needed):

```bash
cd src/ClinicSite.Api

# Gmail SMTP — see below
dotnet user-secrets set "Email:SenderEmail"  "you@gmail.com"
dotnet user-secrets set "Email:SmtpPassword" "your16charapppassword"

# JWT signing key for the doctor portal — any random string of 32+ characters
dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-secret-at-least-32-chars"
```

Environment variables work too, with a double underscore: `Email__SmtpPassword`.

### E-mail (Gmail SMTP)

Mail is sent over SMTP through a normal Gmail account using MailKit. Because Google signs the
message for `gmail.com`, it passes the DKIM/DMARC checks that Gmail, Yahoo and Microsoft now
require — so **real delivery works without owning a domain**. Free Gmail allows roughly 500 messages
a day, which is plenty for development.

One-time setup in your Google account:

1. Enable **2-Step Verification** — <https://myaccount.google.com/security>
2. Create an **App password** — <https://myaccount.google.com/apppasswords> — and copy the
   16-character value. This is *not* your Google password; it is used only by this app.

### Settings that matter

| Setting | Default | Notes |
|---|---|---|
| `Email:ClientBaseUrl` | `http://localhost:5173` | The only source of truth for links in patient e-mails |
| `Email:DoctorClientBaseUrl` | `http://localhost:5175` | Where doctor-invite links point |
| `Email:ConfirmationLifetimeMinutes` | `30` | How long a booking may stay unconfirmed |
| `Email:ResendCooldownMinutes` | `1` | Throttle on resending the confirmation e-mail |
| `Jwt:ExpiryMinutes` | `480` | Doctor session length |

If `Email:SenderEmail` / `Email:SmtpPassword` are missing, booking creation fails with a clear
**503** and a logged reason — the app password is never returned to the client or written to a log.
If `Jwt:Key` is missing the rest of the API still runs; only doctor login fails, with a clear error.

---

## Running

```bash
# API — https://localhost:7100
dotnet run --project src/ClinicSite.Api

# Public patient site  → http://localhost:5173
cd src/ClinicSite.Client       && npm install && npm run dev

# Admin portal         → http://localhost:5174
cd src/ClinicSite.PortalClient && npm install && npm run dev

# Doctor portal        → http://localhost:5175
cd src/ClinicSite.DoctorClient && npm install && npm run dev
```

Set `VITE_API_URL` if the API is not on `https://localhost:7100`. Swagger is at `/swagger`.

### Trying the booking flow end to end

1. Book an appointment on the patient site — the page asks you to check your mail.
2. Open the e-mail in a **real inbox** and click the confirmation button.
3. `/booking/confirm?token=…` opens and reports success.
4. A second e-mail arrives with the details and a cancellation link.
5. Or wait 30 minutes without confirming — the background sweep frees the slot.

---

## Development seed data

`DbSeeder` creates specialties, doctors and two weeks of free slots on first run.

`DevSeeder` additionally creates fixtures for manual testing. It runs **only when the environment is
Development** and is never called in Production. It is idempotent — re-running the app does not
duplicate anything.

It seeds one patient, `history.patient@clinicsite.local`, with three visits across different dates,
doctors and outcomes (one `Completed`, one `NoShow`, one upcoming `Scheduled`), so the patient
history panel has something real to show. It also activates a demo doctor login:

```
demo.doctor@clinicsite.local / DevPassw0rd!
```

The demo login is only bound to a doctor who has no account yet, so it never overwrites a real
account or an invitation in flight. To see the history: sign in to the doctor portal, open the
appointment on the dashboard, and look at **Patient history**.

---

## Tests

```bash
dotnet test
```

68 xUnit tests. Everything that needs a database uses **SQLite in-memory**, one fresh connection per
test — the tests share no state, do not depend on execution order, and never touch LocalDB. E-mail
goes to a fake `IEmailService`, so no message is ever sent. The whole suite runs in about a second.

| Suite | Covers |
|---|---|
| `BookingServiceTests` | Creation, expiry stamping, hash-only token storage, confirm/cancel including wrong and expired tokens, idempotency, slot freeing, the expiration sweep, compensation when the first e-mail fails |
| `DoctorBookingServiceTests` | The doctor's list, filters, visit status, notes, schedule, dashboard, patient messaging, and rejection of another doctor's booking |
| `DoctorPatientHistoryTests` | E-mail normalization, newest-first ordering, exclusion of other patients, cross-doctor visits, and 404 for a missing or someone else's booking |
| `DoctorSlotServiceTests` | Single and recurring slot blocking, and its limits |
| `DoctorAccountServiceTests` | Invite → set password → login, plus expired and invalid tokens |
| `DevSeederTests` | That the Development fixtures are idempotent and leave existing accounts alone |
| `BookingTokensTests`, `PasswordHasherTests` | Token generation and PBKDF2 hashing |
| `AppointmentSlotServiceTests`, `EmailTemplateTests` | Past-slot rules and HTML encoding of patient data in e-mails |

Not covered: the HTTP layer (routing, JWT, status codes) has no integration tests yet, and there are
no front-end tests.

Run one suite with `dotnet test --filter "FullyQualifiedName~DoctorPatientHistoryTests"`.

---

## Security

- **Tokens** — confirmation, cancellation and doctor-invite tokens are 256-bit CSPRNG values,
  delivered Base64Url in links. Only their **SHA-256 hashes** are stored. Confirmation and
  cancellation tokens are always different, so one can never be used in place of the other.
- **Passwords** — PBKDF2-HMAC-SHA256, 210 000 iterations, per-password salt, constant-time
  verification. A plaintext password is never stored or e-mailed.
- **Authorization** — the doctor id always comes from the `doctorId` JWT claim, never from the URL
  or body, so a doctor cannot reach another doctor's data by editing a request.
- **No data in links** — booking ids, e-mail addresses and personal data never appear in e-mail links.
- **Logging** — tokens, the SMTP password and e-mail bodies are never logged; addresses are masked.
- **POST-only** state changes — confirmation and cancellation cannot be triggered by a GET, and the
  React pages strip the token from the address bar.
- **Concurrency** — a SQL Server `rowversion` guards concurrent confirm / cancel / expire.
- **Rate limiting** — per IP: booking creation 5/min, confirm & cancel 20/min, doctor login 10/min.
- **Output encoding** — patient-supplied data is HTML-encoded in e-mail templates.

---

## Licence

See `LICENSE.txt`.
