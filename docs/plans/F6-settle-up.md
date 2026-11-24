# F6 — Settle up: implementation plan

_Status: Draft · Last updated: 2026-11-24 · Spec: docs/specs/features/F6-settle-up.md (Approved 2026-11-24) · Standards: docs/rules/, CLAUDE.md_

## Validation record
| Check | Result |
|---|---|
| V1 Spec file exists | Pass |
| V2 Spec status is Approved | Pass (status found: Approved) |
| V3 Open questions section has no unchecked items | Pass (none) |
| V4 Every acceptance criterion is complete and appears in the test plan | Pass (F6.AC1 to F6.AC15) |
| V5 No placeholders left in the spec | Pass |

## Context
The app suggests the fewest payments that bring every balance to zero, the user marks each one paid, and the balances update. Paid payments form a list, newest first, and can be deleted after confirmation. A suggestion that changed before it is marked paid is refused (F6.AC15). Depends on F1 to F5. Build step 6, the last feature.

## Standards check
| Source | Rule | How this plan complies, or the conflict |
|---|---|---|
| docs/rules/project-rules.md | Money as whole cents | Complies. Payment amounts are `long` cents; the API sends decimal strings. |
| docs/rules/project-rules.md | Multi-record writes in one transaction | Complies. Marking a payment as paid and deleting a payment each run in one transaction. |
| docs/rules/project-rules.md | Times in UTC | Complies. `MarkedAt` is UTC (F6.R4). |
| docs/rules/project-rules.md | Unit tests only | Complies. Algorithm, validation and the stale check are unit tests; the cascade, foreign keys and real list order are code review. |
| docs/rules/project-rules.md | Logger only | Complies. |
| CLAUDE.md | Vertical slice | Complies: `server/Features/Settlements/` with `MarkPaid`, `DeletePayment`, `GetSettlements`, and a `SettlementAlgorithm` shared folder. |
| CLAUDE.md | Audit fields | Complies with the 2026-11-24 decision. `Payments` gets CreatedAt, CreatedBy, UpdatedAt, UpdatedBy. `MarkedAt` is the business time the spec names. `UpdatedAt` and `UpdatedBy` stay null, since payments cannot be edited (F6.R9). |
| CLAUDE.md | Strict TypeScript; TanStack Query | Complies. |
| CLAUDE.md | 80% coverage | Planned. |

## Approach
- **Algorithm** is a pure function: sort people by balance, pair the largest debtor with the largest creditor, pay the smaller of the two, and repeat until every balance is zero (F6.R1). Ties go to the person added first (F6.R2). Each step's amount is in cents (F6.R3).
- **Mark as paid** does not take an amount from the client. The server recomputes the current suggestion for that payer and payee, and only records it if it still matches. If it no longer matches, the request is refused and nothing is written (F6.R4, F6.AC15).
- **Balances (F5)** receive the payment list as an input, so a recorded payment shifts the balances (F6.R6). This is the payment terms F5 left empty.
- **Removal rule (F3)** gets its payment implementation here, so people in a payment cannot be removed (F6.R10, F3.AC8).

## Data model, endpoints and technical choices
Data model:
- `Payments`: Id, TripId, PayerPersonId, PayeePersonId (different from payer), AmountCents (greater than 0), MarkedAt (UTC), CreatedAt, CreatedBy, UpdatedAt (null), UpdatedBy (null).

Endpoints: `GET /api/trips/{tripId}/settlements`, `POST /api/trips/{tripId}/payments`, `DELETE /api/trips/{tripId}/payments/{paymentId}`.

| Choice | Status | Source |
|---|---|---|
| Server recomputes the amount from the current suggestion | Decided | Spec F6.R4 |
| Mark and delete each in one transaction | Decided | Project rules (multi-record writes) |
| Suggestions recomputed on each read | Decided | Spec F6.R7, F6.AC16 |

## Files
| Path | Create / change | Purpose |
|---|---|---|
| server/Features/Settlements/Shared/SettlementAlgorithm.cs | Create | Pure suggestion algorithm (F6.R1 to F6.R3) |
| server/Features/Settlements/MarkPaid/ | Create | Command, validator (payer not equal to payee), handler (recompute, match, transaction), endpoint, tests |
| server/Features/Settlements/DeletePayment/ | Create | Command, handler (transaction), endpoint, tests |
| server/Features/Settlements/GetSettlements/ | Create | Query, handler (suggestions and Paid list newest first), response, tests |
| server/Features/Balances/Shared/BalanceCalculator.cs | Change (from F5) | Accept the payment list (F6.R6) |
| server/Features/People/RemovalRules/ | Change (from F3) | Payment implementation of the removal check |
| server/Features/Trips/TripDeletion/ | Change (from F2) | Register the payment deletes |
| server/Persistence/ | Change | Payments configuration, migration |
| client/src/features/settlements/ | Create | SuggestedPayments, PaidList, confirmation dialogs, api, schemas, hooks |
| client/src/tests/settlements/ | Create | Vitest and React Testing Library tests |

## Tasks

| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| F6.T1 | Payments table and migration | server/Persistence, migration | F6.AC3 (storage of time and amount) | Migration applies to an empty database. Needs approval (migration). |
| F6.T2 | Settlement algorithm | server/Features/Settlements/Shared/ | F6.AC1, F6.AC2, F6.AC5, F6.AC6, F6.AC7 | Algorithm tests pass, including worked examples 1 and 3. |
| F6.T3 | Mark as paid use case | server/Features/Settlements/MarkPaid/ | F6.AC3, F6.AC8, F6.AC15 | Handler tests pass with mocked repositories and a mocked transaction. |
| F6.T4 | Delete payment use case | server/Features/Settlements/DeletePayment/ | F6.AC4 | Handler tests pass with mocked repositories. |
| F6.T5 | Balances include payments | server/Features/Balances/Shared/ | F6.AC12 | Calculator test passes with a payment. |
| F6.T6 | Read model: suggestions and Paid list | server/Features/Settlements/GetSettlements/ | F6.AC9, F6.AC14, F6.AC16 | Handler tests pass (order and row content). |
| F6.T7 | Payment removal check | server/Features/People/RemovalRules/ | F6.AC10 | Test passes with a mocked payment lookup. |
| F6.T8 | Ownership checks on settlement requests | server/Features/Settlements/ | F6.AC11 | Guard test passes. |
| F6.T9 | Settle-up screen and confirmations | client/src/features/settlements/ | F6.AC1, F6.AC2, F6.AC4 (cancel), F6.AC6, F6.AC9 (rows), F6.AC14 | Component tests pass. |

## Test plan
| AC | Unit test | Or code review (reason) |
|---|---|---|
| F6.AC1 | F6.AC1 worked example 1 gives Jo pays Sam 40.00, then Alex pays Sam 10.00 (algorithm) | |
| F6.AC2 | F6.AC2 after Jo's payment only Alex pays Sam 10.00 remains (algorithm with payments) | |
| F6.AC3 | F6.AC3 marking records the full suggested amount with the time (MarkPaid handler, mocked repos) | |
| F6.AC4 | F6.AC4 confirmed delete removes the payment and its suggestion returns (DeletePayment handler) | |
| F6.AC5 | F6.AC5 equal balances pay the first-added person first (algorithm) | |
| F6.AC6 | F6.AC6 all paid gives no suggestions and zero balances (algorithm) | |
| F6.AC7 | F6.AC7 applying all suggestions leaves every balance at zero, for sampled inputs (algorithm) | |
| F6.AC8 | F6.AC8 payer equal to payee is rejected with `payer_equals_payee` (validator) | |
| F6.AC9 | F6.AC9 Paid list row has payer, payee, amount and time (GetSettlements handler) | Full app: real stored times |
| F6.AC10 | F6.AC10 removing a payer or payee is refused (removal rule, mocked payment lookup) | Full app: real removal refused |
| F6.AC11 | F6.AC11 non-owner gets not-found and nothing changes (ownership guard, mocked owner) | Full app end to end |
| F6.AC12 | F6.AC12 a recorded payment raises the payer's balance and lowers the payee's (calculator) | |
| F6.AC13 | | No edit control or request exists (screen and request list reviewed) |
| F6.AC14 | F6.AC14 newest payment is first (GetSettlements handler) | |
| F6.AC15 | F6.AC15 a suggestion that changed is refused and nothing is written (MarkPaid handler, mocked repo) | Database: the transaction rolls back on conflict |
| F6.AC16 | F6.AC16 suggestions are recomputed from the balances after each change (read model, mocked balances) | Full app: suggestions after a real payment or delete |

Frontend component tests: F6.AC1 and F6.AC2 (text), F6.AC4 (cancel keeps the payment), F6.AC6 (all settled text), F6.AC9 and F6.AC14 (rows and order).

## Verification
1. `dotnet test` and `npm run test -- --run` pass; `npm run lint` and `npm run build` pass.
2. Code review, against a real database: the mark-as-paid transaction, the Paid list order, cascade on trip delete, and that balances match the settlement after a payment.

## Needs a person's approval
- **Database migrations:** the Payments table (F6.T1).

## Risks and conflicts
- **Dependency: the stale-suggestion check.** F6.AC15 relies on the server recomputing the suggestion inside the transaction. Two requests in parallel must not both succeed; this is checked in code review with a real database.
- **Deviation: audit fields.** `UpdatedAt` and `UpdatedBy` on Payments stay null, since payments cannot be edited (F6.R9).

## Open items
None.

## Decisions log
- 2026-11-24: Stale suggestion → refuse and show a message; nothing recorded (user decision, F6.AC15).
- 2026-11-24: Paid list order → newest first (user decision, F6.AC14).
- 2026-11-24: Audit fields → all four on Payments.
