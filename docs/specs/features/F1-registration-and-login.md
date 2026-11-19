# F1 — Registration and login

_Status: Approved · Last updated: 2026-11-19 · Source: product.md Roles, Assumptions, Goals, MVP scope row F1_

## Summary
A visitor creates an account with an email and password, then logs in and out. Every trip belongs to one account, and every trip request checks that the logged-in user owns it. This is the first build step, because nothing else can be safely built without it. Passwords are stored only as hashes.

## User stories
- As a visitor, I want to register with an email and password, so that I can start tracking trips.
- As a user, I want to log in and out, so that only I can use my account on my device.
- As a user, I want my trips to be visible only to me, so that my spending stays private.

## Scope
**In:**
- Registration with email, password and confirm password.
- Login and logout.
- A session that lasts 7 days after login.
- Hashed password storage.
- The login check and the ownership check that every trip endpoint uses (the checks are built here; the trip endpoints themselves come in F2–F6).

**Out:**
- Email verification and password reset (product.md Assumptions, "No email verification or password reset").
- Sharing trips with other users (product.md After the core, "After the core").
- Account deletion and profile editing (not in product.md; out of MVP).
- Login through a third party (not in product.md; out of scope).

## User flow & screens
1. **Register.** Visitor opens the Register screen, enters email, password and confirm password, and submits. On success they are logged in and sent to the trip list (F2). On failure they stay on the form with the errors shown.
2. **Log in.** Visitor opens the Log in screen, enters email and password, and submits. On success they go to the page they asked for, or to the trip list if they had no destination. On failure they stay on the form with the generic error.
3. **Log out.** User selects Log out. Their session ends on every device they are logged in on, and they go to the Log in screen.
4. **Logged-out visitor opens a trip page.** They are sent to the Log in screen. After a successful login they return to the page they asked for.
5. **Session expires.** On the next request the user is treated as logged out, as in step 4.

Screen names: Register, Log in. No mock screens exist yet in `docs/design`; the layout is left to the plan.

## Requirements
- **F1.R1** Registration takes an email, a password and a confirm-password value, and creates one account per email.
- **F1.R2** A password must be at least 8 characters. There are no other composition rules.
- **F1.R3** Before saving, the email is trimmed of surrounding spaces and lowercased. It must have a basic valid format (one `@`, with a dot in the domain part). It must be unique across accounts.
- **F1.R4** The password is stored only as a salted hash. The plain-text password is never stored, returned, or logged. (product.md Roles: "hashed passwords".)
- **F1.R5** Login takes an email and password. The email is normalised the same way as in F1.R3 before the lookup.
- **F1.R6** A failed login (unknown email or wrong password) shows the same message in both cases: "Email or password is incorrect".
- **F1.R7** A successful registration logs the new user in and sends them to the trip list.
- **F1.R8** A successful login starts a session that expires 7 days after login. The expiry is fixed; it is not extended by activity.
- **F1.R9** Logout ends every session of that user, on all devices. Requests made with any of those sessions are then treated as logged out.
- **F1.R10** Every trip endpoint requires a valid session. A request without a valid session gets a 401 response and no trip data.
- **F1.R11** Every trip endpoint checks that the trip belongs to the logged-in user. If it does not, the response is 404, the same as for a trip that does not exist. (product.md Roles, ownership rule; Goals: "No user can see or change another user's trips".)

## Data & contract

**New table `users`:**

| Column | Type | Rules |
|---|---|---|
| id | primary key | generated |
| email | text | required; stored trimmed and lowercased; unique |
| password_hash | text | required; never the plain password |
| created_at | timestamp | required |

**Sessions:** a session record (or signed token) is created at login and expires 7 days later. Its storage choice (database table or signed token) is left to the plan. It must be invalidated at logout. Each session is linked to its user, so logout can end all of that user's sessions at once.

**API (proposed; the plan confirms the paths):**

| Method and path | Request | Success | Errors |
|---|---|---|---|
| POST /api/auth/register | `{email, password, confirmPassword}` | 201, user `{id, email}`, session set | 400 with field errors (see States & errors); 409 if email already exists |
| POST /api/auth/login | `{email, password}` | 200, user `{id, email}`, session set | 401 `{error: "Email or password is incorrect"}` |
| POST /api/auth/logout | none | 204, session ended | none |
| All trip endpoints (F2–F6) | session required | per endpoint | 401 when no valid session; 404 when the trip is not the user's |

Error shape for all of the above: `{error: string, fields?: {[field]: string}}`. The final copy for each error is open (see Open questions).

## States & errors
- **Loading:** the submit button is disabled while a request is in flight, and it shows a loading label. Copy for the label: to be set in the plan (no user wording given; treated as implementation detail).
- **Empty:** not applicable; the forms have no list state.
- **Validation errors** (each shown next to its field and listed in a summary at the top of the form). Checked on the server; the form may also check them early, but the server result is final.

| Code | Field | Rule | When checked | Copy |
|---|---|---|---|---|
| `email_required` | email | Empty after trimming | Register, login | open |
| `email_invalid` | email | Not one `@` with a dot in the domain part | Register | open |
| `email_taken` | email | Normalised email already exists (F1.R3) | Register | open |
| `password_required` | password | Empty | Register, login | open |
| `password_too_short` | password | Fewer than 8 characters (F1.R2) | Register | open |
| `confirm_required` | confirm password | Empty | Register | open |
| `confirm_mismatch` | confirm password | Does not exactly match the password | Register | open |
| `credentials_invalid` | email and password | Unknown email or wrong password (F1.R6) | Login | "Email or password is incorrect" (set) |

Field-level checks run in the order listed. The server returns the first failing code for each field, so one field shows one message at a time.
- **Login failure:** "Email or password is incorrect" (the user chose this wording). Shown above the form. Password field is cleared.
- **Server error:** copy **open**. Shown when the request fails for a reason other than the cases above.
- **Logged-out API request:** 401 with `{error: "Not logged in"}`. Copy **open** (API-only; no screen shows it if the frontend redirects first).

## Accessibility
- Every field has a visible label. Placeholder text is not used as a label.
- Errors are linked to their field with `aria-describedby`, and the error summary is announced to screen readers.
- After a failed submit, focus moves to the error summary.
- The forms can be used entirely by keyboard, and Enter submits.
- Focus outlines stay visible. Text and error colours meet WCAG AA contrast.
- No animation is needed, so reduced motion has no effect on this feature.

## Edge cases
- Email with capitals or spaces (`  Sam@Example.com `) is stored and matched as `sam@example.com`.
- Registering `SAM@example.com` when `sam@example.com` exists is rejected as a duplicate.
- A password of exactly 8 characters is accepted. A password of 7 characters is rejected.
- A password containing spaces is accepted; spaces count toward the 8 characters.
- Logging in right after registration works the same as any other login.
- Session expires while the user is on a page: the next request returns 401 and the user is sent to the Log in screen (F1.R10, flow step 5).
- Two devices logged in at once: logging out on one ends the session on both (F1.R9).
- Visitor opens a trip page while logged out, then logs in as a different user: they land on the page they asked for, and the ownership check (F1.R11) applies to that page.

## Acceptance criteria
- **F1.AC1** Given a visitor on the Register screen, when they submit a valid email, a password of 8+ characters and a matching confirm password, then an account is created, they are logged in, and they are sent to the trip list.
- **F1.AC2** Given a password of 7 characters, when the visitor registers, then the request is rejected with the password error and no account is created.
- **F1.AC3** Given the email `  Sam@Example.com `, when the visitor registers, then the stored email is `sam@example.com`.
- **F1.AC4** Given an account exists for `sam@example.com`, when someone registers `SAM@example.com`, then the request is rejected with the duplicate-email error.
- **F1.AC5** Given a registered user, when the user table is read, then the password appears only as a hash that is not equal to the password, and it is never returned in any response or log line.
- **F1.AC6** Given a registered user, when they log in with the correct email and password, then a session is created. Given a wrong password or an unknown email, then both return "Email or password is incorrect".
- **F1.AC7** Given a user logged in on two devices, when they log out on one, then both sessions end and a trip request made from either device returns 401.
- **F1.AC8** Given a session created at time T, when a request is made at T + 7 days + 1 second, then the request is treated as logged out and returns 401.
- **F1.AC9** Given no session, when any trip endpoint is called, then the response is 401 and contains no trip data.
- **F1.AC10** Given a logged-out visitor, when they open a trip page, then they are sent to the Log in screen, and after a successful login they return to that page.
- **F1.AC11** Given user A is logged in and trip T belongs to user B, when user A requests trip T, then the response is 404, the same response as for a trip ID that does not exist.

## Dependencies
- **Depends on:** none. This is build step 1.
- **Needed by:** F2–F6 (every trip endpoint uses the login and ownership checks in F1.R10 and F1.R11).
- **External services:** none. Password hashing uses a library chosen in the plan.

## Test plan
Unit tests only (project rules). Each test is mapped to its AC IDs.

- **Unit: password length validator.** AC2 (7 chars rejected, 8 chars accepted, spaces counted).
- **Unit: email normaliser and format check.** AC3 (trim and lowercase), part of AC4 (`SAM@example.com` and `sam@example.com` normalise to the same value), invalid format rejected.
- **Unit: registration service with a mocked user repository.** AC1 (valid input returns a logged-in user), AC4 (duplicate returns the duplicate error when the repository reports a match).
- **Unit: password hashing.** AC5 (hash differs from the password; hash verifies the correct password and rejects a wrong one; the hash is not returned in the service's result).
- **Unit: login service with a mocked repository.** AC6 (correct credentials create a session; wrong password and unknown email return the same error).
- **Unit: session expiry check with an injected clock.** AC8 (just before 7 days valid; at and after 7 days, invalid).
- **Unit: auth guard with a mocked session.** AC9 (no session gives 401 without calling the trip handler).
- **Unit: ownership check with mocked repository.** AC11 (another user's trip returns 404; a missing trip returns 404).

Code review only (need a real database or the full running app):
- **AC4 (duplicate, case-insensitive):** the unique constraint on `email` must hold in the database.
- **AC5 (no plain text in storage or logs):** check the table and logger calls.
- **AC7 (logout ends all sessions in storage):** every session row for the user must be removed or invalidated.
- **AC10 (redirect and return-to-page):** full-app behaviour.

## Open questions
- [ ] Exact wording for: email missing or invalid format; password under 8 characters; confirm password does not match; email already registered; server error; logged-out API message ("Not logged in" is a placeholder).

## Decisions log
- 2026-11-19: Password rules? → Minimum 8 characters, no other rules.
- 2026-11-19: How long should a login last? → Until logout, with a fixed expiry.
- 2026-11-19: Response when a user requests another user's trip? → 404 Not Found.
- 2026-11-19: What happens after successful registration? → Logged in and sent to the trip list.
- 2026-11-19: Fixed session expiry length? → 7 days.
- 2026-11-19: Email handling? → Trim spaces, store lowercase, must be unique, basic format check.
- 2026-11-19: Failed login message? → Generic "Email or password is incorrect".
- 2026-11-19: Logged-out visitor opening a trip page? → Redirect to login, then back to the page after login.
- 2026-11-19: Confirm-password field on registration? → Yes.
- 2026-11-19: Who writes registration error copy? → The user will supply exact wording (left open; not filled in).
- 2026-11-19: Should logout end sessions on all devices or only the current one? → All devices (F1.R9, F1.AC7).
- 2026-11-19: Apply all error validations? → Yes. Every validation listed under States & errors applies. The wording is still open.
- 2026-11-19: Apply all error validations → Added a validation table (codes, rules, when checked). Copy for every code except `credentials_invalid` is still open.
- 2026-11-19: Approval → Approved by the user (status set to Approved). Copy for the open error messages is still pending, so it is listed under Open questions.
