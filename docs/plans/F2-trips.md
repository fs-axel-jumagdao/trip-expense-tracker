# F2 — Trips: implementation plan

_Status: Approved · Last updated: 2026-11-25 · Spec: docs/specs/features/F2-trips.md (Approved 2026-11-24) · Standards: docs/rules/, CLAUDE.md_

## Validation record
| Check | Result |
|---|---|
| V1 Spec file exists | Pass |
| V2 Spec status is Approved | Pass (status found: Approved) |
| V3 Open questions section has no unchecked items | Pass (none) |
| V4 Every acceptance criterion is complete and appears in the test plan | Pass (F2.AC1 to F2.AC12) |
| V5 No placeholders left in the spec | Pass (the delete dialog body uses `{name}`, a runtime value, not a placeholder) |

## Context
Users create named trips, see their own trips newest first, and delete a trip after confirming. Deleting removes everything in it. Trips are the container for F3 to F6, so this feature also defines the cascade those features rely on (F2.R6). Depends on F1 for the session and ownership guard (F1.R10, F1.R11). Build step 2.

## Standards check
| Source | Rule | How this plan complies, or the conflict |
|---|---|---|
| docs/rules/project-rules.md | Unit tests only | Complies. The ownership filter in the list query and the cascade delete are code review, as the rules allow. |
| docs/rules/project-rules.md | Test names carry the AC ID | Complies. `F2_AC1_...` method names and `// F2.AC1` comments; `it("F2.AC1 ...")` in Vitest. |
| docs/rules/project-rules.md | Multi-record writes in one transaction | Complies. Delete removes the trip and all its people, expenses, shares and payments in one transaction (F2.R6). |
| docs/rules/project-rules.md | Times stored in UTC | Complies. `CreatedAt` is UTC. |
| docs/rules/project-rules.md | Logger only | Complies. |
| CLAUDE.md | Vertical slice, feature folders | Complies. `server/Features/Trips/CreateTrip`, `ListTrips`, `DeleteTrip`. |
| CLAUDE.md | MediatR, FluentValidation, EF Core directly | Complies. |
| CLAUDE.md | Audit fields on major entities | Complies with the 2026-11-24 decision. `Trips` gets CreatedAt, CreatedBy, UpdatedAt, UpdatedBy. The spec only names the owner and creation time, so `UpdatedAt` and `UpdatedBy` are added in the plan. A trip is never edited, so both stay null. |
| CLAUDE.md | Strict TypeScript; TanStack Query; React Hook Form with Zod | Complies. |
| CLAUDE.md | 80% coverage on business logic, handlers, validators | Planned. |

## Approach
- Trip name rules live in one validator, shared by create and rename (rename is not in MVP, so only create uses it).
- The list query filters by the owner in the database, and the owner check is part of the query itself (F2.R4, F2.R7).
- Delete runs in one transaction. The cascade removes rows that F3 to F6 own, through a single `TripDeletion` service that F3 to F6 register their deletes with. This keeps the cascade in one place.
- Create returns the new trip, and the frontend opens the trip page straight away (F2.R5).

## Data model, endpoints and technical choices
Data model:
- `Trips`: Id, Name (1 to 100 characters, trimmed), OwnerUserId, CreatedAt, CreatedBy, UpdatedAt (null), UpdatedBy (null). Only these fields; no others (F2.AC13).

Endpoints: `GET /api/trips`, `POST /api/trips`, `GET /api/trips/{tripId}`, `DELETE /api/trips/{tripId}`.

| Choice | Status | Source |
|---|---|---|
| EF Core, MediatR, FluentValidation | Decided | CLAUDE.md |
| Delete in one transaction | Decided | Project rules (multi-record writes) |
| `TripDeletion` service for the cascade | Proposed | Keeps the cascade in one place |

## Files
| Path | Create / change | Purpose |
|---|---|---|
| server/Features/Trips/CreateTrip/ | Create | Command, validator (name rules), handler, endpoint, response, tests |
| server/Features/Trips/ListTrips/ | Create | Query, handler (owner filter, newest first), response, tests |
| server/Features/Trips/DeleteTrip/ | Create | Command, handler (transaction, cascade), endpoint, tests |
| server/Features/Trips/TripDeletion/ | Create | Service that deletes a trip's child data in one transaction |
| server/Common/Auth/ | Reuse (from F1) | Session guard and ownership helper |
| server/Persistence/ | Change | Trips configuration and migration |
| client/src/features/trips/ | Create | TripList, NewTripForm, DeleteTripDialog, api, schemas (Zod), hooks |
| client/src/tests/trips/ | Create | Vitest and React Testing Library tests |

## Tasks

| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| F2.T1 | Trip entity configuration and migration | server/Persistence/Entities/Trip.cs, server/Persistence/Configurations/TripConfiguration.cs, server/Persistence/AppDbContext.cs, server/Persistence/Migrations/<timestamp>_AddTrips.cs | F2.AC12 (storage) | Migration applies to an empty database. Needs approval (migration). |
| F2.T2 | Trip name validator | server/Features/Trips/CreateTrip/CreateTripValidator.cs, tests/Application.Tests/Trips/CreateTripValidatorTests.cs | F2.AC2, F2.AC3 | `dotnet test --filter "FullyQualifiedName~F2_AC2|F2_AC3"` passes. |
| F2.T3 | Create trip use case | server/Features/Trips/CreateTrip/CreateTripCommand.cs, server/Features/Trips/CreateTrip/CreateTripHandler.cs, server/Features/Trips/CreateTrip/CreateTripEndpoint.cs, server/Features/Trips/CreateTrip/CreateTripResponse.cs, tests/Application.Tests/Trips/CreateTripHandlerTests.cs | F2.AC1, F2.AC4, F2.AC13 | Create tests pass with a mocked repository. |
| F2.T4 | List trips use case | server/Features/Trips/ListTrips/ListTripsQuery.cs, server/Features/Trips/ListTrips/ListTripsHandler.cs, server/Features/Trips/ListTrips/ListTripsEndpoint.cs, server/Features/Trips/ListTrips/ListTripsResponse.cs, tests/Application.Tests/Trips/ListTripsHandlerTests.cs | F2.AC5, F2.AC6, F2.AC11 | List tests pass (owner filter, order, empty). |
| F2.T5 | Delete trip use case | server/Features/Trips/DeleteTrip/DeleteTripCommand.cs, server/Features/Trips/DeleteTrip/DeleteTripHandler.cs, server/Features/Trips/DeleteTrip/DeleteTripEndpoint.cs, tests/Application.Tests/Trips/DeleteTripHandlerTests.cs | F2.AC7, F2.AC12 | Delete tests pass with mocked services. |
| F2.T6 | Cascade service | server/Features/Trips/TripDeletion/TripDeletionService.cs, server/Features/Trips/TripDeletion/ITripDeletionService.cs, tests/Application.Tests/Trips/TripDeletionServiceTests.cs | F2.AC7 | Cascade calls are tested; the database cascade is in code review. |
| F2.T7 | Ownership and session checks on trip requests | server/Features/Trips/TripAccessFilter.cs, tests/Application.Tests/Trips/TripAccessFilterTests.cs | F2.AC9, F2.AC10 | Guard tests pass: non-owner gets not-found, no session gets 401. |
| F2.T8 | Trip list, new trip form and delete dialog | client/src/features/trips/components/TripList.tsx, client/src/features/trips/components/NewTripForm.tsx, client/src/features/trips/components/DeleteTripDialog.tsx, client/src/features/trips/schemas/tripSchemas.ts, client/src/tests/trips/TripScreens.test.tsx | F2.AC1, F2.AC2, F2.AC3, F2.AC8, F2.AC11, F2.AC13 | Component tests pass. |

## Test plan
| AC | Unit test | Or code review (reason) |
|---|---|---|
| F2.AC1 | F2.AC1 create returns the new trip owned by the requester (handler, mocked repo) | |
| F2.AC2 | F2.AC2 rejects a name of only spaces (validator) | |
| F2.AC3 | F2.AC3 rejects 101 characters and accepts 100 (validator) | |
| F2.AC4 | F2.AC4 saves two trips with the same name (handler, mocked repo) | |
| F2.AC5 | F2.AC5 list returns only the requester's trips (handler, mocked repo) | Database: the owner filter is in the query itself |
| F2.AC6 | F2.AC6 list is newest first (handler, mocked repo) | |
| F2.AC7 | F2.AC7 confirmed delete calls the cascade for the trip (handler, mocked services) | Database: the cascade removes people, expenses, shares and payments and nothing else |
| F2.AC8 | | Full UI: cancel keeps the trip (covered by F2.T7 component test) |
| F2.AC9 | F2.AC9 another user's trip returns not-found and changes nothing (ownership helper, mocked owner) | Full app returns not-found end to end |
| F2.AC10 | F2.AC10 no session gives 401 and does not call the handler (guard, mocked session) | |
| F2.AC11 | F2.AC11 no trips gives an empty list (handler, mocked repo) | |
| F2.AC12 | F2.AC12 a deleted trip returns not-found (handler, mocked repo) | Database: the trip row is gone for every user |
| F2.AC13 | F2.AC13 the saved trip record has only name, owner and creation time (handler, mocked repo) | Form asks only for a name (component test) |

Frontend component tests (Vitest and React Testing Library): F2.AC1 (opens the trip page after create), F2.AC2 and F2.AC3 (messages), F2.AC8 (cancel keeps the trip), F2.AC11 (empty text).

## Verification
1. `dotnet test` and `dotnet test --filter Category=Unit` pass.
2. `npm run test -- --run`, `npm run lint` and `npm run build` pass.
3. Code review, against a real database: list filter, cascade delete, not-found for another user's trip, and a trip deleted in one browser is not-found in another.

## Needs a person's approval
- **Database migrations:** the Trips table and its migration (F2.T1).
- **Deleting data:** the delete cascade removes user data. It is required by F2.R6, but the cascade code path needs a review.

## Risks and conflicts
- **Dependency: the cascade depends on F3 to F6.** Until those tables exist, the cascade only removes trips and rows that exist. Each later feature must register its child-row delete with `TripDeletion` (F3 to F6 plans). Owner: the F3 to F6 plans.
- **Deviation: audit fields.** `UpdatedAt` and `UpdatedBy` are added to `Trips` although the spec does not name them (2026-11-24 decision). They stay null.
- **Risk: frontend and server name rules must match.** The Zod schema and the server validator both encode F2.R1. A shared test fixture keeps them aligned.

## Open items
None.

## Decisions log
- 2026-11-24: Audit fields on Trips → All four, `UpdatedAt` and `UpdatedBy` left null (user decision on audit fields).
- 2026-11-25: Entity class file (Trip.cs) and AppDbContext.cs added to F2.T1, which the plan had not named. Plan returned to Draft for re-approval.
- 2026-11-25: Re-approved by the user.
- 2026-11-24: Cascade → one `TripDeletion` service, so each feature registers its child deletes in one place.
- 2026-11-24: Plan approved by the user, including its migrations.
