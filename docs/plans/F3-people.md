# F3 — People: implementation plan

_Status: Draft · Last updated: 2026-11-24 · Spec: docs/specs/features/F3-people.md (Approved 2026-11-24) · Standards: docs/rules/, CLAUDE.md_

## Validation record
| Check | Result |
|---|---|
| V1 Spec file exists | Pass |
| V2 Spec status is Approved | Pass (status found: Approved) |
| V3 Open questions section has no unchecked items | Pass (none) |
| V4 Every acceptance criterion is complete and appears in the test plan | Pass (F3.AC1 to F3.AC12) |
| V5 No placeholders left in the spec | Pass (the blocked-removal message uses `{Name}` and `{list}`, runtime values) |

## Context
Users add people to a trip by name and remove them. A person who appears in any expense or payment cannot be removed (F3.R6). People have no accounts. This feature depends on F1 (session and ownership) and F2 (trips). F4 to F6 depend on people. Build step 3. The removal check covers payments, which arrive in F6, so the plan gives F6 a hook to plug into.

## Standards check
| Source | Rule | How this plan complies, or the conflict |
|---|---|---|
| docs/rules/project-rules.md | Unit tests only | Complies. The unique name index and the cascade on trip delete are code review. |
| docs/rules/project-rules.md | Test names carry the AC ID | Complies, same convention as F1 and F2. |
| docs/rules/project-rules.md | Multi-record writes in one transaction | Not applicable: adding or removing a person writes one row. |
| docs/rules/project-rules.md | Times in UTC | Complies. |
| docs/rules/project-rules.md | Logger only | Complies. |
| CLAUDE.md | Vertical slice | Complies: `server/Features/People/AddPerson`, `RemovePerson`, `ListPeople`. |
| CLAUDE.md | MediatR, FluentValidation, EF Core | Complies. |
| CLAUDE.md | Audit fields on major entities | Complies with the 2026-11-24 decision. `People` gets CreatedAt, CreatedBy, UpdatedAt, UpdatedBy. The spec says people are never edited, so UpdatedAt and UpdatedBy stay null. The spec's "no update time" line is the deviation, recorded here. |
| CLAUDE.md | Strict TypeScript and Zod, React Hook Form | Complies. |
| CLAUDE.md | 80% coverage | Planned. |

## Approach
- A name is normalised once (trim, lowercase) for the uniqueness check, and stored trimmed for display (F3.R1, F3.R3).
- Order is the insertion order, from `CreatedAt` plus the row ID as a tie-breaker, so people added in the same instant keep a stable order.
- The removal check asks two questions through interfaces: expenses (F4) and payments (F6). F3 ships with an expenses check and a payments check that returns "none", and F6 replaces it. This keeps F3 buildable before F4 and F6 exist, but the expense check cannot be finished until F4 exists. See Risks.
- The uniqueness rule is enforced by a unique index on (TripId, NormalizedName) (code review) and by the handler (unit tested).

## Files
| Path | Create / change | Purpose |
|---|---|---|
| server/Features/People/AddPerson/ | Create | Command, validator (name rules), handler, endpoint, response, tests |
| server/Features/People/RemovePerson/ | Create | Command, handler (blocker check), endpoint, tests |
| server/Features/People/ListPeople/ | Create | Query, handler (order), tests |
| server/Features/People/RemovalRules/ | Create | `IPersonReferenceCheck` interface; expense check (F4) and payment check (F6) implementations |
| server/Persistence/ | Change | People configuration, migration, unique index |
| client/src/features/people/ | Create | PeopleSection, AddPersonForm, RemovePersonButton, BlockedRemovalDialog, api, schemas, hooks |
| client/src/tests/people/ | Create | Vitest and React Testing Library tests |

## Tasks

| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| F3.T1 | People table, unique index and migration | server/Persistence, migration | F3.AC4 (storage) | Migration applies to an empty database. Needs approval (migration). |
| F3.T2 | Name rules: trim, length 1 to 50, normalise | server/Features/People/AddPerson/ | F3.AC2, F3.AC3, F3.AC4 (part) | Validator tests pass. |
| F3.T3 | Add person use case | server/Features/People/AddPerson/ | F3.AC1, F3.AC4, F3.AC11 | Handler tests pass with a mocked repository. |
| F3.T4 | Removal rules and remove person use case | server/Features/People/RemovePerson/, RemovalRules/ | F3.AC5, F3.AC6, F3.AC7, F3.AC8 | Remove tests pass with mocked blockers. The payment blocker is a stub until F6 (see Risks). |
| F3.T5 | People list order | server/Features/People/ListPeople/ | F3.AC9 | Ordering test passes. |
| F3.T6 | Ownership checks on people requests | server/Features/People/ | F3.AC10 | Guard test passes: non-owner gets not-found. |
| F3.T7 | People section and blocked-removal dialog | client/src/features/people/ | F3.AC1, F3.AC2, F3.AC3, F3.AC11 (empty text), F3.AC12 (no rename control, code review) | Component tests pass. |

## Test plan
| AC | Unit test | Or code review (reason) |
|---|---|---|
| F3.AC1 | F3.AC1 added person is last in the list (handler, mocked repo) | |
| F3.AC2 | F3.AC2 rejects a name of only spaces (validator) | |
| F3.AC3 | F3.AC3 rejects 51 characters and accepts 50 (validator) | |
| F3.AC4 | F3.AC4 rejects `  sam  ` when `Sam` exists on the trip (handler, mocked repo) | Database: the unique index on (TripId, NormalizedName) rejects a race |
| F3.AC5 | F3.AC5 person with no expense or payment is removed (handler, mocked blockers) | |
| F3.AC6 | F3.AC6 payer blocker returns 409 listing the expense (handler, mocked blocker) | |
| F3.AC7 | F3.AC7 participant blocker returns 409 listing the expense (handler, mocked blocker) | |
| F3.AC8 | F3.AC8 payment blocker returns 409 listing the payment (handler, mocked blocker) | |
| F3.AC9 | F3.AC9 list is in creation order (ListPeople handler) | |
| F3.AC10 | F3.AC10 non-owner gets not-found and nothing changes (ownership guard, mocked owner) | Full app end to end |
| F3.AC11 | F3.AC11 zero people is a valid trip (handler, mocked repo) | |
| F3.AC12 | | No rename control or request exists (screen and request list reviewed) |

Frontend component tests: F3.AC1 (new person appears last), F3.AC2 and F3.AC3 (messages), F3.AC11 (empty text).

## Verification
1. `dotnet test` and `npm run test -- --run` pass; `npm run lint` and `npm run build` pass.
2. Code review, against a real database: unique index (AC4), cascade on trip delete, the removal check against real expenses and payments once F4 and F6 exist.

## Needs a person's approval
- **Database migrations:** the People table and unique index (F3.T1).

## Risks and conflicts
- **Dependency on F4 and F6.** The expense check needs F4's expense tables, and the payment check needs F6's payments table. F3 can be built first with both checks as interfaces. The full removal rule (F3.AC7, F3.AC8) is only proven after F4 and F6 are built. Owner: F4 and F6 plans.
- **Deviation: audit fields.** `UpdatedAt` and `UpdatedBy` are kept for CLAUDE.md compliance but stay null, since people are never edited (spec and 2026-11-24 decision).
- **Risk: a name that is normalised differently on screen and on the server.** The Zod and server validators share one set of test cases.

## Open items
None.

## Decisions log
- 2026-11-24: Audit fields → All four on People, `UpdatedAt` and `UpdatedBy` null.
- 2026-11-24: Removal check → an interface with an expense implementation (F4) and a payment implementation (F6), with F3 testing the rule through the interface.
