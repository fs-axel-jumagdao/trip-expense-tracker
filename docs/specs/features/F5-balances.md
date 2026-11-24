# F5 — Balances

_Status: Draft · Last updated: 2026-11-24 · Source: product.md Goals, Key terms (Balance), Business rules R4 and R5, worked examples 1 and 3, MVP scope row F5_

## Summary
The trip shows each person's balance: how much they are owed, or how much they owe, after all expenses and payments. Balances are worked out from the saved expenses, so they are always current and always add up to exactly zero. This is the number the settle-up step (F6) works from.

## User stories
- As a user, I want to see each person's balance on the trip, so that I know who is owed money and who owes it.
- As a user, I want each balance worded as "is owed" or "owes", so that I can read it without doing any sums.
- As a user, I want a balance of zero to say so clearly, so that I know a person is settled.

## Scope
**In:**
- Show each person's balance on the trip, worded as "is owed", "owes" or "Settled".
- Show every person on the trip, including people with no expenses.
- Order the list by the order people were added (F3.R5).
- Check on every calculation that the balances add up to zero (product.md Business rules R5). If they do not, log a warning.

**Out:**
- Showing each person's total paid and total share (not in F5; the user chose balance only).
- Payments in the balance. Payments are added in F6 (product.md Business rules R4). Until F6 exists, balance = paid − share.
- Balances in another currency, or per-expense balances (product.md Assumptions: one currency).
- Showing the mismatch warning to the user. It is written to the log only.

## User flow & screens
1. **Open the trip.** The balances section shows one row per person on the trip, in the order they were added.
2. **Read a row.** Each row shows the person's name, the label ("is owed", "owes" or "Settled") and the amount to two decimal places.
3. **Add or delete an expense.** The balances update the next time the section is shown, with no extra step by the user (F4.R8).
4. **Empty trip.** A trip with no people shows the empty state (see States & errors).

Screen names: Trip page (balances section). No mock screens exist yet in `docs/design`; layout is left to the plan.

## Requirements
- **F5.R1** A person's balance = total paid as payer − total of their shares in expenses (product.md Business rules R4). After F6 exists, it also includes payments they made and minus payments they received (F6).
- **F5.R2** A positive balance is shown as "is owed" followed by the amount. A negative balance is shown as "owes" followed by the amount as a positive number. The amount is never shown with a minus sign.
- **F5.R3** A balance of exactly zero is shown as "Settled" followed by 0.00.
- **F5.R4** Every person on the trip has a row, including people with no expenses. Their balance is 0.00 and shows as "Settled".
- **F5.R5** Rows are listed in the order people were added to the trip (F3.R5).
- **F5.R6** Every amount is shown with exactly two decimal places (for example "40.00", never "40").
- **F5.R7** Balances are worked out from the saved expenses and shares each time they are requested. No balance is stored separately.
- **F5.R8** On every calculation, the balances are checked to add up to exactly zero (product.md Business rules R5). If they do not, the app writes a warning to the log and still returns the balances.
- **F5.R9** Viewing balances requires a logged-in user who owns the trip (F1.R10, F1.R11). Other users get a 404.

## Data & contract

**No new stored data.** Balances are worked out from the expenses and their shares (F4), and from payments after F6.

**Operations:** view balances. The result has one row per person, in add order (F5.R5). Each row has the person's name, a status (owed, owes or settled), and an amount with two decimal places that is always positive. The status carries the direction, so the amount never has a minus sign. Errors: not logged in, or trip not the user's (F1.R10, F1.R11).

**Log entry on mismatch (F5.R8):** a warning is written to the log with the trip and the total that should have been zero. It contains no names, descriptions or per-person amounts.

## States & errors
- **Loading:** the balances section shows a loading indicator until the list arrives. Copy: open (no user wording given; treated as an implementation detail for the plan).
- **Empty trip (no people):** copy **open**.
- **Labels:** "is owed", "owes" and "Settled" (set by the user's choice; the wording is fixed).
- **Server error:** copy **open**.
- **Not logged in or not the owner:** handled by F1; no new copy.
- **Mismatch:** not shown to the user (see F5.R8). The user sees the balances as calculated.

## Accessibility
- Each row is a list item that reads as one sentence to a screen reader, for example "Sam is owed 50.00".
- The label is text, not colour alone. "is owed", "owes" and "Settled" are each distinct words.
- The list has a heading, and the balances section is reachable by keyboard.
- Text and label colours meet WCAG AA contrast.
- No animation is needed, so reduced motion has no effect.

## Edge cases
- Worked example 1 (Sam pays 90.00, Alex pays 30.00, all three share both): Sam is owed 50.00, Alex owes 10.00, Jo owes 40.00. The three add up to zero.
- A person who paid nothing and shared nothing: Settled 0.00.
- A person who paid exactly their share: Settled 0.00.
- A trip where every balance is zero (every expense paid back evenly): every row shows Settled.
- Deleting the only expense: every balance returns to Settled 0.00.
- A balance of 0.01 is shown as "is owed 0.01" or "owes 0.01" (not rounded away).
- A balance is never shown as "owes -10.00": the sign comes from the label (F5.R2).

## Acceptance criteria
- **F5.AC1** Given worked example 1, when the balances are shown, then Sam is owed 50.00, Alex owes 10.00 and Jo owes 40.00.
- **F5.AC2** Given a positive balance of 50.00, when it is shown, then the row reads "is owed 50.00".
- **F5.AC3** Given a negative balance of -10.00, when it is shown, then the row reads "owes 10.00" with no minus sign.
- **F5.AC4** Given a balance of exactly zero, when it is shown, then the row reads "Settled 0.00".
- **F5.AC5** Given a person with no expenses or payments, when the balances are shown, then they have a row reading "Settled 0.00".
- **F5.AC6** Given people added in the order Sam, Alex, Jo, when the balances are shown, then the rows are in that order.
- **F5.AC7** Given any set of expenses on a trip, when the balances are calculated, then they add up to exactly zero.
- **F5.AC8** Given balances that do not add up to zero, when they are calculated, then a warning is written to the log, the balances are still returned, and the log entry contains no person names or amounts.
- **F5.AC9** Given an expense is deleted, when the balances are next shown, then they reflect the deletion.
- **F5.AC10** Given user A and a trip owned by user B, when user A requests the balances, then the response is 404 and no balance is returned.
- **F5.AC11** Given any balance, when it is shown, then the amount has exactly two decimal places (for example 40 is shown as 40.00).

## Dependencies
- **Depends on:** F1 (login and ownership checks), F2 (trips), F3 (people), F4 (expenses and shares).
- **Needed by:** F6 (settle up uses balances; F6 also adds payments to the balance calculation, F5.R1).
- **Build order:** step 5.
- **External services:** none.

## Test plan
Unit tests only (project rules). Each test is mapped to its AC IDs.

- **Unit: balance calculator** (pure function over paid and shares). AC1 (worked example 1), AC5 (no expenses gives 0.00), AC7 (sum is zero over several sample trips), AC9 (recalculated after a deletion, using the expense list without the deleted expense).
- **Unit: balance label formatter** (positive, negative, zero; two decimal places; no minus sign). AC2, AC3, AC4, AC11.
- **Unit: ordering** (rows follow add order). AC6.
- **Unit: mismatch check with a mocked logger.** AC8 (warning logged, balances still returned, no names or amounts in the log message).
- **Unit: balances request handler with a mocked trip owner.** AC10 (non-owner gets 404).
- **Unit (frontend): Balances component test.** AC2, AC3, AC4, AC5 as visible text and in order (AC6).

Code review only (need a real database or the full running app):
- **AC9 end to end:** the balances change after a real expense is deleted.
- **AC7 with real data:** a query over the saved expenses and shares for a trip sums to zero.
- **AC10 end to end:** full app returns 404 for another user's trip.

## Open questions
- [ ] Exact wording for: the empty trip state, the server error, and the loading indicator (if any text is shown).

## Decisions log
- 2026-11-19: How is a zero balance shown? → "Settled" with 0.00.
- 2026-11-19: Balances list order? → Order people were added to the trip.
- 2026-11-19: Show paid and share totals too? → No. Balance only.
- 2026-11-19: What if balances do not add up to zero? → Show the balances anyway and log a warning.
- 2026-11-24: Grader feedback: spec named tables, endpoints and libraries → Replaced them with product terms; the plan decides the technical design.
- 2026-11-24: Show the balance mismatch warning to the user as well as the log? → Log only (F5.AC8).
