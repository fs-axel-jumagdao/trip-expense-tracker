# F5 — Balances: implementation plan

_Status: Draft · Last updated: 2026-11-24 · Spec: docs/specs/features/F5-balances.md (Approved 2026-11-24) · Standards: docs/rules/, CLAUDE.md_

## Validation record
| Check | Result |
|---|---|
| V1 Spec file exists | Pass |
| V2 Spec status is Approved | Pass (status found: Approved) |
| V3 Open questions section has no unchecked items | Pass (none) |
| V4 Every acceptance criterion is complete and appears in the test plan | Pass (F5.AC1 to F5.AC11) |
| V5 No placeholders left in the spec | Pass, with one agreed exception: the loading label "Loading balances…" is finished copy (decided 2026-11-24) |

## Context
The trip shows each person's balance: owed, owes or settled, in add order, always summing to zero. Balances are worked out from saved expenses and shares, and from payments once F6 exists (F5.R1). Nothing is stored separately (F5.R7). Depends on F1 to F4. Build step 5.

## Standards check
| Source | Rule | How this plan complies, or the conflict |
|---|---|---|
| docs/rules/project-rules.md | Money as whole cents | Complies. Balances are `long` cents. Only the API turns them into two-decimal strings. |
| docs/rules/project-rules.md | Unit tests only | Complies. Calculation, labels, order and the mismatch check are unit tests; the end-to-end checks are code review. |
| docs/rules/project-rules.md | Logger only; no personal data in logs | Complies. The mismatch log holds the trip ID and the total only (F5.AC8). |
| docs/rules/project-rules.md | Times in UTC | Not applicable (no times in balances). |
| CLAUDE.md | Vertical slice | Complies: `server/Features/Balances/GetBalances/`, and the calculator in `server/Features/Balances/Shared/`. |
| CLAUDE.md | Audit fields on major entities | Not applicable: balances are computed and stored nowhere (F5.R7). |
| CLAUDE.md | Strict TypeScript; TanStack Query | Complies. The balances component reads through TanStack Query. |
| CLAUDE.md | 80% coverage | Planned. |

## Approach
- **One pure calculator** does the arithmetic: for each person, paid minus shares, plus payments made minus payments received (the payment terms are empty until F6). It takes a list of people, expenses and shares, and returns cents per person.
- **Labels and formatting** are a second pure function: sign to "owed", "owes" or "settled", amount always positive and always two decimals.
- **Mismatch check** runs on every calculation. If the total is not zero, it logs a warning with the trip ID and the total, and still returns the balances (F5.R8).
- **No table.** Balances are computed on request (F5.R7).

## Data model, endpoints and technical choices
Data model: none stored. Balances are computed per request from `Expenses`, `ExpenseShares` and, after F6, `Payments` (F5.AC12).

Endpoint: `GET /api/trips/{tripId}/balances`.

| Choice | Status | Source |
|---|---|---|
| Pure calculator, no balance table | Proposed | Spec F5.R7 |
| Mismatch warning through the project logger, with IDs and totals only | Decided | Project rules (logging) |

## Files
| Path | Create / change | Purpose |
|---|---|---|
| server/Features/Balances/Shared/BalanceCalculator.cs | Create | Pure calculator (F5.R1) |
| server/Features/Balances/Shared/BalanceFormatter.cs | Create | Labels and two-decimal output (F5.R2, F5.R3, F5.R6) |
| server/Features/Balances/GetBalances/ | Create | Query, handler (order, mismatch check, ownership), endpoint, response, tests |
| client/src/features/balances/ | Create | BalancesList component, api, hooks |
| client/src/tests/balances/ | Create | Vitest and React Testing Library tests |

## Tasks

| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| F5.T1 | Balance calculator | server/Features/Balances/Shared/ | F5.AC1, F5.AC5, F5.AC7, F5.AC9 | Calculator tests pass, including worked example 1. |
| F5.T2 | Label and two-decimal formatter | server/Features/Balances/Shared/ | F5.AC2, F5.AC3, F5.AC4, F5.AC11 | Formatter tests pass. |
| F5.T3 | Row order | server/Features/Balances/GetBalances/ | F5.AC6 | Order test passes. |
| F5.T4 | Mismatch check and warning | server/Features/Balances/GetBalances/ | F5.AC8 | Test passes with a mocked logger, checking no names or amounts are logged. |
| F5.T5 | Get balances request and ownership | server/Features/Balances/GetBalances/ | F5.AC10, F5.AC12 | Handler and guard tests pass. |
| F5.T6 | Balances component | client/src/features/balances/ | F5.AC2 to F5.AC6 (as text and order) | Component tests pass. |

## Test plan
| AC | Unit test | Or code review (reason) |
|---|---|---|
| F5.AC1 | F5.AC1 worked example 1 gives Sam 50.00 owed, Alex 10.00 owes, Jo 40.00 owes (calculator) | |
| F5.AC2 | F5.AC2 positive balance reads "is owed 50.00" (formatter) | |
| F5.AC3 | F5.AC3 negative balance reads "owes 10.00" with no minus sign (formatter) | |
| F5.AC4 | F5.AC4 zero balance reads "Settled 0.00" (formatter) | |
| F5.AC5 | F5.AC5 person with no expenses has a 0.00 row (calculator) | |
| F5.AC6 | F5.AC6 rows follow add order (GetBalances handler) | |
| F5.AC7 | F5.AC7 sampled data sums to zero (calculator) | Real data query sums to zero (database) |
| F5.AC8 | F5.AC8 mismatch logs a warning, returns balances, and logs no names or amounts (handler, mocked logger) | |
| F5.AC9 | F5.AC9 balances change after deleting an expense (calculator, recalculated) | Full app: balances after a real delete |
| F5.AC10 | F5.AC10 non-owner gets not-found (ownership guard, mocked owner) | Full app end to end |
| F5.AC11 | F5.AC11 every amount has two decimals (formatter) | |
| F5.AC12 | | No balance table or column exists; balances are computed per request (schema reviewed) |

Frontend component tests: F5.AC2 to F5.AC5 as visible text, F5.AC6 in order.

## Verification
1. `dotnet test` and `npm run test -- --run` pass.
2. Code review: real data sums to zero, a real deletion updates the balances, and the warning log contains only the trip ID and total.

## Needs a person's approval
None. No migrations, secrets or auth changes; balances are computed, not stored.

## Risks and conflicts
- **Dependency on F6.** The payment terms in the calculator are empty until F6 adds payments (F5.R1). The calculator takes the payment list as an input so F6 only has to pass it in.
- **Risk: performance on long trips.** Balances are computed on every request (F5.R7). Trips are small in the MVP, so this is accepted; a recompute cache would need a new spec.

## Open items
None.

## Decisions log
- 2026-11-24: Balances are computed, not stored (F5.R7, from the spec).
- 2026-11-24: Mismatch → log only (user decision).
