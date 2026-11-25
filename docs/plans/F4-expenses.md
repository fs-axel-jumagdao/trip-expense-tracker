# F4 — Expenses: implementation plan

_Status: Approved · Last updated: 2026-11-24 · Spec: docs/specs/features/F4-expenses.md (Approved 2026-11-24) · Standards: docs/rules/, CLAUDE.md_

## Validation record
| Check | Result |
|---|---|
| V1 Spec file exists | Pass |
| V2 Spec status is Approved | Pass (status found: Approved) |
| V3 Open questions section has no unchecked items | Pass (none) |
| V4 Every acceptance criterion is complete and appears in the test plan | Pass (F4.AC1 to F4.AC15) |
| V5 No placeholders left in the spec | Pass |

## Context
A user records a shared cost with a description, an amount, a payer and the participants who share it. Each expense is split equally to the cent, the shares are saved with the expense, and balances (F5) are worked out from them. Amounts are whole cents, with extra decimals rounded half up (F4.R2). Depends on F1, F2 and F3. Build step 4. Balances and payments depend on the saved shares.

## Standards check
| Source | Rule | How this plan complies, or the conflict |
|---|---|---|
| docs/rules/project-rules.md | Money stored as whole cents; no floating-point | Complies. Stored as `long` cents. The API takes and returns amounts as decimal strings, so no float is used at any step. |
| docs/rules/project-rules.md | Multi-record writes in one transaction | Complies. Creating an expense writes the expense and its share rows in one transaction. Deleting an expense removes its shares in the same transaction. |
| docs/rules/project-rules.md | Unit tests only | Complies. The share rule, amount parsing and the cascade are unit or code review as listed. |
| docs/rules/project-rules.md | Times in UTC | Complies. |
| docs/rules/project-rules.md | Logger only | Complies. |
| CLAUDE.md | Vertical slice | Complies: `server/Features/Expenses/AddExpense`, `ListExpenses`, `DeleteExpense`, and a `SplitCalculator` in `server/Features/Expenses/Shared/`. |
| CLAUDE.md | Money example uses `decimal Amount` | Deviation, recorded here: the project rule requires whole cents, so the command takes a decimal string and the entity stores `long` cents. `decimal` is not floating point, but cents make the rule explicit. |
| CLAUDE.md | Audit fields | Complies with the 2026-11-24 decision: `Expenses` and `ExpenseShares` get all four fields. |
| CLAUDE.md | Strict TypeScript; forms with React Hook Form and Zod | Complies. |
| CLAUDE.md | 80% coverage | Planned. |

## Approach
- **Amount parsing** is one pure function: text to cents, with rounding half up at the third decimal (F4.R2). It rejects non-numbers, zero and negatives.
- **Split** is one pure function: `amount / n` rounded down, with the leftover cents handed out one each in the order people were added (F4.R7). Shares always sum to the amount.
- **Add** validates the description, payer and participants, checks every person is on the trip, computes shares, and writes the expense and shares in one transaction.
- **Balances (F5)** read the saved shares; F4 never recalculates them later (F4.R8).

## Data model, endpoints and technical choices
Data model:
- `Expenses`: Id, TripId, Description (1 to 200 characters, trimmed), AmountCents (greater than 0), PayerPersonId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy.
- `ExpenseShares`: Id, ExpenseId, PersonId (unique with ExpenseId), ShareCents (0 or more), CreatedAt, CreatedBy, UpdatedAt, UpdatedBy.

Endpoints: `GET /api/trips/{tripId}/expenses`, `POST /api/trips/{tripId}/expenses`, `DELETE /api/trips/{tripId}/expenses/{expenseId}`.

| Choice | Status | Source |
|---|---|---|
| Whole cents in storage, decimal strings in the API | Decided | Project rules (money) |
| Typed extra decimals rounded half up | Decided | User decision, F4.AC14 |
| Shares rounded down, leftover cents in add order | Decided | Spec F4.R7 |
| EF Core, MediatR, FluentValidation | Decided | CLAUDE.md |

## Files
| Path | Create / change | Purpose |
|---|---|---|
| server/Features/Expenses/Shared/SplitCalculator.cs | Create | Pure split rule (F4.R7) |
| server/Features/Expenses/Shared/AmountParser.cs | Create | Text to cents with half-up rounding (F4.R1, F4.R2) |
| server/Features/Expenses/AddExpense/ | Create | Command, validator, handler (transaction), endpoint, response, tests |
| server/Features/Expenses/ListExpenses/ | Create | Query, handler (newest first), response, tests |
| server/Features/Expenses/DeleteExpense/ | Create | Command, handler (transaction), endpoint, tests |
| server/Features/Trips/TripDeletion/ | Change (from F2) | Register the expense and share deletes |
| server/Features/People/RemovalRules/ | Change (from F3) | Expense implementation of the removal check |
| server/Persistence/ | Change | Expenses and ExpenseShares configuration, migration |
| client/src/features/expenses/ | Create | AddExpenseForm, ExpenseList, DeleteExpenseDialog, api, schemas, hooks |
| client/src/tests/expenses/ | Create | Vitest and React Testing Library tests |

## Tasks

| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| F4.T1 | Expense and share entity configuration and migration | server/Persistence/Configurations/ExpenseConfiguration.cs, server/Persistence/Configurations/ExpenseShareConfiguration.cs, server/Persistence/Migrations/<timestamp>_AddExpenses.cs | F4.AC8 (storage) | Migration applies to an empty database. Needs approval (migration). |
| F4.T2 | Amount parser with half-up rounding | server/Features/Expenses/Shared/AmountParser.cs, tests/Application.Tests/Expenses/AmountParserTests.cs | F4.AC2, F4.AC14 | `dotnet test --filter "FullyQualifiedName~F4_AC2|F4_AC14"` passes. |
| F4.T3 | Split calculator | server/Features/Expenses/Shared/SplitCalculator.cs, tests/Application.Tests/Expenses/SplitCalculatorTests.cs | F4.AC1, F4.AC7, F4.AC8 | Split tests pass, including sampled amounts. |
| F4.T4 | Description validator | server/Features/Expenses/AddExpense/AddExpenseValidator.cs, tests/Application.Tests/Expenses/AddExpenseValidatorTests.cs | F4.AC3 | Validator tests pass. |
| F4.T5 | Add expense use case | server/Features/Expenses/AddExpense/AddExpenseCommand.cs, server/Features/Expenses/AddExpense/AddExpenseHandler.cs, server/Features/Expenses/AddExpense/AddExpenseEndpoint.cs, server/Features/Expenses/AddExpense/AddExpenseResponse.cs, tests/Application.Tests/Expenses/AddExpenseHandlerTests.cs | F4.AC1, F4.AC4, F4.AC5, F4.AC6, F4.AC9 | Handler tests pass with mocked repositories and a mocked transaction. |
| F4.T6 | Saved shares for balances | server/Features/Expenses/Shared/SavedShareReader.cs, tests/Application.Tests/Expenses/SavedShareReaderTests.cs | F4.AC16 | Reader returns saved shares unchanged; tests pass. |
| F4.T7 | List and delete expense use cases | server/Features/Expenses/ListExpenses/ListExpensesHandler.cs, server/Features/Expenses/DeleteExpense/DeleteExpenseHandler.cs, server/Features/Expenses/DeleteExpense/DeleteExpenseEndpoint.cs, tests/Application.Tests/Expenses/ListAndDeleteExpenseTests.cs | F4.AC10, F4.AC11 | Tests pass with mocked repositories. |
| F4.T8 | Ownership checks on expense requests | server/Features/Expenses/ExpenseAccessFilter.cs, tests/Application.Tests/Expenses/ExpenseAccessFilterTests.cs | F4.AC12 | Guard test passes. |
| F4.T9 | Add expense form, list and delete dialog | client/src/features/expenses/components/AddExpenseForm.tsx, client/src/features/expenses/components/ExpenseList.tsx, client/src/features/expenses/components/DeleteExpenseDialog.tsx, client/src/features/expenses/schemas/expenseSchemas.ts, client/src/tests/expenses/ExpenseScreens.test.tsx | F4.AC11 (cancel), F4.AC13, F4.AC15 | Component tests pass. |

## Test plan
| AC | Unit test | Or code review (reason) |
|---|---|---|
| F4.AC1 | F4.AC1 split of 90.00 among three gives 30.00 each (split calculator, handler) | |
| F4.AC2 | F4.AC2 rejects 0.00 and -5 (amount parser) | |
| F4.AC3 | F4.AC3 rejects a blank description and 201 characters; accepts 200 (validator) | |
| F4.AC4 | F4.AC4 no payer returns `payer_required` (handler, validator) | |
| F4.AC5 | F4.AC5 no participant returns `participants_required` (handler, validator) | |
| F4.AC6 | F4.AC6 payer not in participants is accepted and gets no share (handler, split) | |
| F4.AC7 | F4.AC7 10.00 among Sam, Alex, Jo gives 3.34, 3.33, 3.33 (split calculator) | |
| F4.AC8 | F4.AC8 shares sum to the amount for sampled inputs (split calculator) | Database: share rows sum check in the transaction |
| F4.AC9 | F4.AC9 person not on the trip returns `person_not_on_trip` (handler, mocked trip lookup) | |
| F4.AC10 | F4.AC10 list is newest first (ListExpenses handler) | |
| F4.AC11 | F4.AC11 confirmed delete removes the expense and its shares (DeleteExpense handler) | Cascade in the database; cancel covered by the component test |
| F4.AC12 | F4.AC12 another user's trip returns not-found (ownership guard, mocked owner) | Full app end to end |
| F4.AC13 | | Component test: all people selected, no payer selected |
| F4.AC14 | F4.AC14 10.005 is stored as 1001 cents, i.e. 10.01 (amount parser) | |
| F4.AC15 | | Component test: fields clear and the form stays open |
| F4.AC16 | F4.AC16 balances read the saved shares and do not recompute them (calculator, saved input) | |

Frontend component tests: F4.AC11 (cancel keeps the expense), F4.AC13 (defaults), F4.AC15 (form clears).

## Verification
1. `dotnet test` and `npm run test -- --run` pass; `npm run lint` and `npm run build` pass.
2. Code review, against a real database: transaction for add and delete, cascade on delete, shares sum to the amount in stored data.

## Needs a person's approval
- **Database migrations:** Expenses and ExpenseShares (F4.T1).

## Risks and conflicts
- **Deviation: money example.** CLAUDE.md's example uses `decimal`; this plan stores cents, following the project rule. Recorded above.
- **Dependency: F3 removal check.** The expense implementation of the removal check is built here, which finishes F3.AC7.
- **Risk: rounding vs. the rounding-down rule.** F4.R2 rounds half up for typed amounts; F4.R7 rounds shares down. Both are in the spec and tested separately, so the two rules do not mix.

## Open items
None.

## Decisions log
- 2026-11-24: Amount rounding → half up for typed extra decimals (user decision, F4.AC14).
- 2026-11-24: Form after add → clears and stays open (user decision, F4.AC15).
- 2026-11-24: Audit fields → all four on Expenses and ExpenseShares.
- 2026-11-24: Plan approved by the user, including its migrations.
