# F4 — Expenses

_Status: Draft · Last updated: 2026-11-19 · Source: product.md Goals, Roles, Key terms, Assumptions, Business rules R1–R3 and R8, worked examples 1–2, MVP scope row F4_

## Summary
A user records a shared cost: what it was, how much, who paid, and who shares it. Each expense is split equally to the cent, and the split is saved with the expense so balances can always be recalculated exactly. The user can delete an expense after confirming, and cannot edit one. Adding an expense should take four inputs (product.md Goals).

## User stories
- As a user, I want to add an expense with what it was, how much, who paid, and who shares it, so that the trip's spending is recorded.
- As a user, I want everyone on the trip to share an expense by default, so that I only change the list for the exceptions.
- As a user, I want the split to be exact to the cent, so that nobody is charged a cent more or less than their share.
- As a user, I want to delete an expense I added by mistake, after confirming, so that a slip does not stay in the trip's totals.

## Scope
**In:**
- Add an expense: description, amount, payer, participants.
- Default participants to everyone on the trip.
- Split the amount equally to the cent, with leftover cents given out in the order people were added (R3).
- List the trip's expenses, newest first.
- Delete an expense after a confirmation step.

**Out:**
- Editing an expense. Product.md Assumptions: a mistake is fixed by deleting and adding again.
- Uneven splits, custom percentages, or a participant with a different share (product.md After the core, "After the core").
- Currencies or conversion (product.md Assumptions).
- Categories, receipts, or attachments (not in product.md; out of MVP).

## User flow & screens
1. **Open Add expense.** The form opens with the description and amount empty, no payer selected, and every person on the trip selected as a participant.
2. **Fill in the form.** The user enters a description, an amount, chooses a payer, and may untick participants who do not share the cost. The payer may be unticked too, since they do not need to be a participant (R2).
3. **Submit.** The expense is saved and appears at the top of the expense list. The form closes or clears (plan decides), and the trip's balances update (F5).
4. **Delete.** The user selects Delete on an expense. A confirmation step asks them to confirm. On confirm the expense is removed and the balances update. On cancel nothing changes.
5. **Empty list.** A trip with no expenses shows the empty state (see States & errors).

Screen names: Trip page (Add expense form, Expense list). No mock screens exist yet in `docs/design`; layout is left to the plan.

## Requirements
- **F4.R1** An expense amount is greater than zero (product.md Business rules R1). Amounts are stored as whole cents, never floating-point (product.md Assumptions).
- **F4.R2** An amount typed with more than 2 decimal places is rounded to 2 decimal places before it is saved. The rounding method is open (see Open questions).
- **F4.R3** An expense has a description of 1 to 200 characters after trimming. It is required.
- **F4.R4** An expense has exactly one payer, chosen from the people on the same trip (product.md Business rules R2). No payer is pre-selected.
- **F4.R5** An expense has at least one participant, all of them people on the same trip (product.md Business rules R2). When the form opens, every person on the trip is selected.
- **F4.R6** The payer does not have to be a participant (product.md Business rules R2).
- **F4.R7** Each participant's share is the amount divided by the number of participants, rounded down to the cent. Leftover cents go one each to participants in the order they were added to the trip, starting from the first (product.md Business rules R3). The shares always add up to the amount exactly.
- **F4.R8** The payer, the participants and each share are saved with the expense. Balances (F5) are calculated from these saved values.
- **F4.R9** The expense list shows the newest expense first.
- **F4.R10** An expense can be deleted after the user confirms. Deleting it removes its shares too. An expense cannot be edited.
- **F4.R11** Adding, listing and deleting expenses require a logged-in user who owns the trip (F1.R10, F1.R11). Other users get a 404.

## Data & contract

**New table `Expenses`:**

| Column | Type | Rules |
|---|---|---|
| Id | primary key | generated |
| TripId | foreign key to Trips | required; cascades on trip delete |
| Description | text, max 200 | required; stored trimmed |
| AmountCents | integer | required; greater than 0 |
| PayerPersonId | foreign key to People | required |
| CreatedAt | timestamp | required; sets list order (F4.R9) |
| CreatedBy | user reference | required (audit field) |

**New table `ExpenseShares`:**

| Column | Type | Rules |
|---|---|---|
| ExpenseId | foreign key to Expenses | required; cascades on expense delete |
| PersonId | foreign key to People | required; unique together with ExpenseId |
| ShareCents | integer | required; zero or more |

Rule enforced in the handler and tested: the sum of `ShareCents` for an expense equals `AmountCents` exactly (F4.R7). Each participant has one row. The payer has a row only if they are also a participant.

**API (proposed; the plan confirms the paths):**

| Method and path | Request | Success | Errors |
|---|---|---|---|
| POST /api/trips/{tripId}/expenses | `{description, amount, payerPersonId, participantPersonIds: []}` where `amount` is a decimal string such as `"90.00"` | 201, expense with `{id, description, amount, payer, shares: [{person, amount}]}` | 400 with validation code; 404 trip or person not the user's or not on the trip |
| GET /api/trips/{tripId}/expenses | none | 200, list newest first | 404 |
| DELETE /api/trips/{tripId}/expenses/{expenseId} | none | 204 | 404 |

The amount is sent and returned as a decimal string and stored as integer cents, so no floating-point value is used at any step.

Error shape: `{error: string, code: string, fields?: {[field]: string}}`.

## States & errors
- **Empty list:** copy **open**.
- **Validation errors** (shown next to the field, and in a summary at the top of the form):

| Code | Field | Rule | When checked | Copy |
|---|---|---|---|---|
| `description_required` | description | Empty after trimming (F4.R3) | Add | open |
| `description_too_long` | description | More than 200 characters after trimming (F4.R3) | Add | open |
| `amount_required` | amount | Empty | Add | open |
| `amount_not_a_number` | amount | Not a valid decimal number | Add | open |
| `amount_not_positive` | amount | Zero or less (F4.R1) | Add | open |
| `payer_required` | payer | No payer chosen (F4.R4) | Add | open |
| `participants_required` | participants | No participant chosen (F4.R5) | Add | open |
| `person_not_on_trip` | payer or participants | A chosen person is not on this trip (F4.R4, F4.R5) | Add (server) | open |

- **Delete confirmation:** the confirm step asks the user to confirm deleting the expense. Its wording, and the confirm and cancel labels, are **open**.
- **Server error:** copy **open**.
- **Not logged in or not the owner:** handled by F1; no new copy.

## Accessibility
- Every field has a visible label. The participant selection is a set of labelled checkboxes, one per person, and the payer is a labelled select.
- Validation errors are linked to their field with `aria-describedby`, and focus moves to the first invalid field on submit.
- The delete confirmation is a modal dialog: focus moves into it, Escape cancels, focus returns to the Delete button on cancel.
- The Delete button's accessible name includes the expense description (for example "Delete Dinner").
- Everything works with the keyboard. The amount field accepts a decimal keypad on touch devices.
- Focus outlines stay visible. Text and error colours meet WCAG AA.
- No animation is needed, so reduced motion has no effect.

## Edge cases
- Amount `90` is accepted and stored as 9000 cents. Amount `90.5` is stored as 9050 cents.
- Amount `0.00` and `-5` are rejected with `amount_not_positive`.
- Amount `10.005`: rounded to 2 decimals (F4.R2). The rounding method is open.
- Shares for worked example 2: 10.00 among Sam, Alex and Jo, in that add order, gives 3.34, 3.33, 3.33. Sam gets the extra cent because they were added first.
- Shares for worked example 1: 90.00 among Sam, Alex and Jo gives 30.00 each; Sam's 90.00 is paid by Sam, who is a participant.
- Payer not in participants (payer Sam, participants Alex and Jo): accepted (F4.R6).
- Only one participant (the payer alone): accepted, share equal to the whole amount.
- A person removed from the trip after an expense includes them: F3.R6 blocks that removal, so this cannot happen.
- Trip with no people: the form has no payer or participants to choose, so an expense cannot be added (`payer_required`).
- Deleting the last expense: the list shows the empty state (F4 empty list).

## Acceptance criteria
- **F4.AC1** Given Sam, Alex and Jo on a trip, when Sam adds "Dinner" for 90.00 paid by Sam and shared by all three, then the expense is saved with a share of 30.00 for each person.
- **F4.AC2** Given an amount of 0.00 or -5, when the user submits, then the request is rejected with `amount_not_positive` and no expense is saved.
- **F4.AC3** Given a description of only spaces, when the user submits, then it is rejected with `description_required`. Given a description of 201 characters, then it is rejected with `description_too_long`. Given 200 characters, then it is accepted.
- **F4.AC4** Given no payer chosen, when the user submits, then the request is rejected with `payer_required`.
- **F4.AC5** Given no participant chosen, when the user submits, then the request is rejected with `participants_required`.
- **F4.AC6** Given payer Sam and participants Alex and Jo, when the user submits, then the expense is accepted and Sam has no share.
- **F4.AC7** Given 10.00 shared by Sam, Alex and Jo in that add order, when the expense is saved, then the shares are 3.34, 3.33 and 3.33.
- **F4.AC8** Given any expense, when it is saved, then the shares add up to exactly the amount.
- **F4.AC9** Given a payer or participant who is not on this trip, when the user submits, then the request is rejected with `person_not_on_trip`.
- **F4.AC10** Given expenses added in the order A, B, C, when the list is shown, then it shows C, B, A.
- **F4.AC11** Given an expense, when the user selects Delete and then confirms, then the expense is removed. When they cancel, then it stays.
- **F4.AC12** Given user A and a trip owned by user B, when user A adds, lists or deletes an expense on it, then the response is 404 and nothing changes.
- **F4.AC13** Given a trip with Sam, Alex and Jo, when the user opens Add expense, then all three are selected as participants and no payer is selected.

## Dependencies
- **Depends on:** F1 (login and ownership checks), F2 (trips), F3 (people who pay and share).
- **Needed by:** F5 (balances come from expenses and shares), F6 (settle up uses balances).
- **Build order:** step 4.
- **External services:** none.

## Test plan
Unit tests only (project rules). Each test is mapped to its AC IDs.

- **Unit: amount parser and validator.** Decimal string to cents; `0.00` and `-5` rejected; `90` and `90.5` accepted; `amount_not_a_number`. AC2 (part).
- **Unit: split calculator** (pure function). AC1 (30.00 each), AC6 (payer not a participant gets no share), AC7 (3.34, 3.33, 3.33), AC8 (sum always equals amount, with several sample amounts and participant counts).
- **Unit: description validator** (trim, 1–200). AC3.
- **Unit: add-expense handler with mocked repositories.** AC1 (saved with shares), AC4, AC5, AC9 (person not on trip), AC6 (accepted when payer is not a participant).
- **Unit: expense list ordering.** AC10.
- **Unit: delete handler.** AC11 (confirmed delete removes expense and its shares; cancel leaves it, covered by the UI step).
- **Unit: ownership guard for expense endpoints with a mocked trip owner.** AC12.
- **Unit: Add expense form component (Vitest + React Testing Library).** AC13 (all people selected by default, no payer selected), and the delete confirmation step for AC11 (cancel keeps the expense).

Code review only (need a real database or the full running app):
- **Cascade on expense delete:** `ExpenseShares` rows removed with the expense.
- **Cascade on trip delete:** expenses and shares removed with the trip (product.md Business rules R8).
- **Foreign keys:** payer and participant must be people in the database; the server check (`person_not_on_trip`) is the same rule enforced on the server.
- **AC12 end to end:** full app returns 404 for another user's trip.

## Open questions
- [ ] How should an amount with more than 2 decimal places be rounded? Options: round half up (10.005 becomes 10.01) or round down (10.005 becomes 10.00). Needed before F4.R2 can be built.
- [ ] Exact wording for: `description_required`, `description_too_long`, `amount_required`, `amount_not_a_number`, `amount_not_positive`, `payer_required`, `participants_required`, `person_not_on_trip`, the empty list, the delete confirmation (text and buttons), and the server error.
- [ ] After a successful add, does the form close or stay open with the fields cleared? (Affects AC13 and the flow; the plan can decide, but the user should confirm.)

## Decisions log
- 2026-11-19: Description rules? → Required, 1 to 200 characters.
- 2026-11-19: Largest allowed amount? → No maximum (only greater than zero, R1).
- 2026-11-19: Default payer? → None. The user must choose.
- 2026-11-19: Delete confirmation? → Yes, confirm before deleting.
- 2026-11-19: Expense list order? → Newest first.
- 2026-11-19: Amount with more than 2 decimals? → Round to 2 decimals. The rounding method is still open.
