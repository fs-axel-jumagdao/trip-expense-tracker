# F1 — Registration and login: implementation plan

_Status: Approved · Last updated: 2026-11-25 · Spec: docs/specs/features/F1-registration-and-login.md (Approved 2026-11-24) · Standards: docs/rules/, CLAUDE.md_

## Validation record
| Check | Result |
|---|---|
| V1 Spec file exists | Pass |
| V2 Spec status is Approved | Pass (status found: Approved) |
| V3 Open questions section has no unchecked items | Pass (none) |
| V4 Every acceptance criterion is complete (Given, when, then) and appears in the test plan | Pass (F1.AC1 to F1.AC12) |
| V5 No placeholders (`…`, `TBD`) left in the spec | Pass, with one agreed exception: the loading labels "Creating account…" and "Logging in…" are finished copy (decided 2026-11-24) |

## Context
Registration, login and logout, with hashed passwords, a 7-day session that logout ends on every device, and the login and ownership checks that every later trip request uses. This is build step 1: F2 to F6 depend on the guard built here (F1.R10, F1.R11). The spec is Approved with its wording filled in.

## Standards check
| Source | Rule | How this plan complies, or the conflict |
|---|---|---|
| docs/rules/project-rules.md | Unit tests only; no integration or end-to-end tests | Complies. Database uniqueness, the logout cascade in storage and the full redirect flow are listed for code review, as the rules allow. |
| docs/rules/project-rules.md | Test names carry the full AC ID | Complies. C# tests use `F1_AC2_...` as the method name and a `// F1.AC2` comment. Vitest tests use `it("F1.AC2 ...")`. |
| docs/rules/project-rules.md | Money as whole cents | Not applicable to F1 (no money). |
| docs/rules/project-rules.md | Multi-record writes in one transaction | Applies to logout only if it touches more than one row. Logout deletes all of one user's sessions in one statement, inside a transaction. |
| docs/rules/project-rules.md | Times stored in UTC | Complies. Session expiry and `created_at` are UTC `DateTimeOffset`. |
| docs/rules/project-rules.md | Passwords hashed; no secrets; no plain text in logs | Complies. Passwords use ASP.NET Core's `PasswordHasher<T>`. The log writer is never passed a password, token or email in a field that could be logged in full. |
| docs/rules/project-rules.md | Logger only; no print or console | Complies. `ILogger<T>` with structured messages only. |
| CLAUDE.md | Vertical slice layout, feature-based folders | Complies. Auth code lives in `server/Features/Auth/`, with one folder per use case. |
| CLAUDE.md | MediatR, FluentValidation, EF Core directly | Complies. Each use case is a MediatR request with a FluentValidation validator. No generic repository. |
| CLAUDE.md | Audit fields on major entities (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) | Complies with the decision of 2026-11-24. `Users` gets all four. `CreatedBy` on a user is the user's own ID, set to the same value at creation. |
| CLAUDE.md | Strict TypeScript; no `any`; TanStack Query and React Hook Form with Zod | Complies. The auth forms use React Hook Form with Zod schemas. |
| CLAUDE.md | Minimum 80% coverage for business logic, handlers and validators | Planned. Coverage is checked with `npm run test:coverage` and `dotnet test --collect:"XPlat Code Coverage"`. |
| CLAUDE.md | Session secrets from environment variables | Conflict, recorded in Risks: CLAUDE.md lists JWT settings, but F1.R9 needs logout on every device, which a stateless JWT cannot do without a revocation store. The plan uses a server-side session table instead. This needs approval. |

## Approach
- **Sessions are server-side.** A `Sessions` row holds a random token hash, the user, and the expiry. The cookie carries the token. Logout deletes every session for that user (F1.R9). Expiry is checked against UTC time (F1.R8). This is the only way to meet F1.R9 without a JWT revocation list.
- **Passwords** use `PasswordHasher<T>`. Emails are trimmed and lowercased before any lookup or insert (F1.R3, F1.R5).
- **One guard** (`RequireSessionFilter` plus an ownership helper in `server/Common/Auth/`) is the only way trip requests check the session and the owner (F1.R10, F1.R11). F2 to F6 reuse it.
- **Frontend** keeps the destination URL in the redirect after login (F1.AC10), and shows the error messages from the spec.

## Data model, endpoints and technical choices
Data model:
- `Users`: Id, Email (normalised, unique), PasswordHash, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy.
- `Sessions`: Id, UserId, TokenHash, ExpiresAt, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy.

Endpoints: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`. Every trip request goes through the session guard (F1.T6).

| Choice | Status | Source |
|---|---|---|
| Server-side session table, token stored as a hash | Decided by the user, 2026-11-24 | Departs from CLAUDE.md's JWT settings; CLAUDE.md now has a note |
| `PasswordHasher<T>` from ASP.NET Core | Proposed | Built into .NET; no extra dependency |
| EF Core, MediatR, FluentValidation | Decided | CLAUDE.md |
| React Hook Form, Zod, Axios, TanStack Query | Decided | CLAUDE.md |

## Files
| Path | Create / change | Purpose |
|---|---|---|
| server/Features/Auth/Register/ | Create | RegisterCommand, RegisterValidator, RegisterHandler, RegisterEndpoint, RegisterResponse, RegisterTests |
| server/Features/Auth/Login/ | Create | Login command, validator, handler, endpoint, response, tests |
| server/Features/Auth/Logout/ | Create | Logout command, handler, endpoint, tests |
| server/Common/Auth/ | Create | Session guard, ownership helper, session expiry rule |
| server/Persistence/ | Create | AppDbContext, `IEntityTypeConfiguration` for Users and Sessions, migration |
| server/Infrastructure/ | Create | Password hasher and clock wrappers (so tests can inject time) |
| tests/Application.Tests/Auth/ | Create | xUnit tests for auth handlers, validators and the guard |
| client/src/features/auth/ | Create | components (RegisterForm, LoginForm), schemas (Zod), api (Axios client), hooks |
| client/src/routes/ | Change | Redirect logged-out visitors to Log in and back again (F1.AC10) |
| client/src/tests/auth/ | Create | Vitest and React Testing Library tests |

Existing code to reuse: none. The repo has no application code yet, so these paths follow the CLAUDE.md layout.

## Tasks

| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| F1.T1 | Users and Sessions entity configuration | server/Persistence/Entities/User.cs, server/Persistence/Entities/Session.cs, server/Persistence/Configurations/UserConfiguration.cs, server/Persistence/Configurations/SessionConfiguration.cs, server/Persistence/AppDbContext.cs | F1.AC4, F1.AC5, F1.AC7 (storage) | Both configurations compile and `dotnet build` passes. |
| F1.T2 | Migration | server/Persistence/Migrations/<timestamp>_AddUsersAndSessions.cs | F1.AC4 (storage) | Migration applies to an empty database. Needs approval (migration). |
| F1.T3 | Email normaliser and password rule | server/Features/Auth/Register/EmailNormaliser.cs, server/Features/Auth/Register/RegisterValidator.cs, tests/Application.Tests/Auth/EmailNormaliserTests.cs, tests/Application.Tests/Auth/RegisterValidatorTests.cs | F1.AC2, F1.AC3, F1.AC12 | `dotnet test --filter "FullyQualifiedName~F1_AC2|F1_AC3|F1_AC12"` passes. |
| F1.T4 | Password hashing | server/Infrastructure/PasswordHasherService.cs, server/Infrastructure/IPasswordHasherService.cs, tests/Application.Tests/Auth/PasswordHasherTests.cs | F1.AC5 | Hash tests pass. |
| F1.T5 | Register use case | server/Features/Auth/Register/RegisterCommand.cs, server/Features/Auth/Register/RegisterHandler.cs, server/Features/Auth/Register/RegisterEndpoint.cs, server/Features/Auth/Register/RegisterResponse.cs, tests/Application.Tests/Auth/RegisterHandlerTests.cs | F1.AC1, F1.AC4, F1.AC5 (response) | Register tests pass; duplicate returns the duplicate error. |
| F1.T6 | Login use case | server/Features/Auth/Login/LoginCommand.cs, server/Features/Auth/Login/LoginHandler.cs, server/Features/Auth/Login/LoginEndpoint.cs, tests/Application.Tests/Auth/LoginHandlerTests.cs | F1.AC6, F1.AC12 | Login tests pass. |
| F1.T7 | Logout use case | server/Features/Auth/Logout/LogoutCommand.cs, server/Features/Auth/Logout/LogoutHandler.cs, server/Features/Auth/Logout/LogoutEndpoint.cs, tests/Application.Tests/Auth/LogoutHandlerTests.cs | F1.AC7 | Logout tests pass. |
| F1.T8 | Session expiry rule | server/Common/Auth/SessionExpiryRule.cs, server/Common/Auth/IClock.cs, tests/Application.Tests/Auth/SessionExpiryRuleTests.cs | F1.AC8 | Expiry tests pass with an injected clock. |
| F1.T9 | Session guard | server/Common/Auth/SessionGuardFilter.cs, tests/Application.Tests/Auth/SessionGuardFilterTests.cs | F1.AC9 | Guard tests pass. Needs approval (authentication). |
| F1.T10 | Ownership helper for trip requests | server/Common/Auth/TripOwnershipHelper.cs, tests/Application.Tests/Auth/TripOwnershipHelperTests.cs | F1.AC11 | Ownership test returns not-found for another user's trip. |
| F1.T11 | Register and log-in forms | client/src/features/auth/components/RegisterForm.tsx, client/src/features/auth/components/LoginForm.tsx, client/src/features/auth/schemas/authSchemas.ts, client/src/features/auth/api/authApi.ts, client/src/tests/auth/AuthForms.test.tsx | F1.AC1, F1.AC6 (messages) | `npm run test -- --run` passes for the form tests. |
| F1.T12 | Redirect for logged-out visitors | client/src/routes/RequireAuth.tsx, client/src/routes/AppRoutes.tsx, client/src/tests/auth/RequireAuth.test.tsx | F1.AC9 (401 leads to log in), F1.AC10 | Component tests pass; full redirect in code review. |

Done-when for the whole feature: `npm run test -- --run`, `dotnet test`, `npm run lint` and `npm run build` all pass, and the code-review items below are checked.

## Test plan
Unit tests only. Test names include the AC ID.

| AC | Unit test | Or code review (reason) |
|---|---|---|
| F1.AC1 | F1.AC1 handler returns a logged-in user for valid input (RegisterHandler, mocked repository) | |
| F1.AC2 | F1.AC2 rejects a 7-character password (validator) | |
| F1.AC3 | F1.AC3 stores `sam@example.com` for `  Sam@Example.com ` (normaliser) | |
| F1.AC4 | F1.AC4 returns duplicate-email error when the repository finds a match | Database constraint: unique index on normalised email |
| F1.AC5 | F1.AC5 hash differs from the password, verifies correct and rejects wrong, and is absent from the response (hasher, handler) | Logs and stored data checked by reading the code |
| F1.AC6 | F1.AC6 correct credentials create a session; wrong password and unknown email return the same error (login handler, mocked) | |
| F1.AC7 | F1.AC7 logout removes every session for the user (logout handler, mocked store) | Database: the logout delete runs in storage across devices |
| F1.AC8 | F1.AC8 session is invalid at and after 7 days, valid just before (expiry rule, injected clock) | |
| F1.AC9 | F1.AC9 no session gives 401 and does not call the handler (guard, mocked session) | |
| F1.AC10 | | Full-app redirect and return to the requested page |
| F1.AC11 | F1.AC11 another user's trip and a missing trip both return not-found (ownership helper, mocked owner) | |
| F1.AC12 | F1.AC12 login with `  SAM@example.com ` matches the stored email (login handler, normaliser) | |

## Verification
1. `dotnet build` and `dotnet test` pass. Run `dotnet test --filter Category=Unit` for the unit set.
2. `npm run lint`, `npm run test -- --run` and `npm run build` pass.
3. `npm run test:coverage` and `dotnet test --collect:"XPlat Code Coverage"` show at least 80% on auth business logic and handlers.
4. Code review, in a real app with a database, checks: AC4 (unique index), AC5 (no plain text in storage or logs), AC7 (logout deletes every session row for the user), AC10 (redirect and return), and the migration against an empty database.

## Needs a person's approval
- **Authentication or sessions:** the session design in Approach (F1.T5, F1.T6). This is a deliberate departure from CLAUDE.md's JWT settings.
- **Secrets or environment files:** any session key or cookie-signing key is set through environment variables. Nothing is committed.
- **Database migrations:** the Users and Sessions migration (F1.T2).

## Risks and conflicts
- **Conflict: session design vs CLAUDE.md. Resolved 2026-11-24.** CLAUDE.md lists `Jwt__Key`, `Jwt__Issuer` and `Jwt__Audience`. A JWT cannot be revoked on logout (F1.R9) without storing revoked tokens, which makes it stateful anyway. The user chose server-side sessions. CLAUDE.md now has a note saying the JWT settings are not used for login.
- **Deviation: audit fields.** Decided 2026-11-24 to follow CLAUDE.md, so `Users` and `Sessions` have all four fields.
- **Risk: no application code exists yet.** File paths are proposed from CLAUDE.md. They may need adjusting once the project is scaffolded.
- **Risk: the 401 response on expired sessions.** The frontend must handle 401 by redirecting to Log in. The guard test covers the server side; the redirect is checked in code review.

## Open items
None. Copy and rules are settled in the spec.

## Decisions log
- 2026-11-24: Session design → Server-side sessions with logout across all devices, which departs from CLAUDE.md's JWT settings. Decided by the user.
- 2026-11-24: Audit fields → All four on every entity (user decision).
- 2026-11-24: Loading labels with "…" → Finished copy, not placeholders (user decision).
- 2026-11-24: Plan approved by the user, including its migrations.
- 2026-11-25: Entity class files (User.cs, Session.cs) added to F1.T1, which the plan had not named. Plan returned to Draft for re-approval.
- 2026-11-25: Re-approved by the user.
- 2026-11-25: Session foreign key to Users with cascade delete (user decision). Migration AddUsersAndSessions generated for review (F1.T2); not yet confirmed.
- 2026-11-25: Audit fields CreatedBy and UpdatedBy are text (nvarchar(36)) holding the creating user's Id, not a Guid column (user decision). Migration regenerated accordingly; the earlier Guid version was never approved and was removed from the repo.
- 2026-11-25: Audit field length changed to 320 (nvarchar(320)), the same as Email. Migration AddUsersAndSessions (20261125080242) approved by the user, which completes F1.T2.
