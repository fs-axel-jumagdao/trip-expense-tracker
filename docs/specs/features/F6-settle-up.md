# F6 — Settle up

_Status: Draft · Last updated: 2026-11-24 · Source: product.md Key terms (Suggested payment, Payment), Business rules R4 and R6, worked examples 1 and 3, Goals, MVP scope row F6_

## Summary
The app proposes the fewest-step set of payments that would bring every balance to zero, and the user records each one as paid once the money has moved outside the app. Recording a payment changes the balances, so the next suggestions are worked out from the new balances. The user can delete a payment if it was recorded by mistake. The step is done when every suggested payment has been marked as paid and every balance is zero.

## User stories
- As a user, I want to see who should pay whom and how much, so that the trip can be settled in as few payments as possible.
- As a user, I want to mark a suggested payment as paid once the money has moved, so that the balances reflect it.
- As a user, I want to see what has already been paid, so that I know where the trip stands.
- As a user, I want to undo a payment I marked by mistake, after confirming, so that the debt returns.

## Scope
**In:**
- Suggested payments, worked out from the current balances (product.md Business rules R6).
- Mark a suggested payment as paid. It becomes a Payment for its full amount.
- A "Paid" list of payments, each with its payer, payee, amount and the time it was marked, newest first.
- Delete a payment after confirmation.
- Recalculate the suggestions and balances after any change.

**Out:**
- Partial payments or a different amount from the suggestion (product.md Key terms: a Payment is a suggested payment marked as paid, so it always equals the full suggested amount).
- Editing a payment (product.md Assumptions: no editing).
- Payments made through the app (product.md Assumptions: "Money moves outside the app").
- Reminders or notifications about payments (not in product.md; out of MVP).

## User flow & screens
1. **See suggestions.** The settle-up section lists the suggested payments in the order the algorithm produces them, for example "Jo pays Sam 40.00".
2. **Mark as paid.** The user selects "Mark as paid" next to a suggestion. A confirmation step asks them to confirm. On confirm, the payment is recorded and moves to the Paid list. The suggestions and balances update.
3. **Paid list.** The Paid list sits under the suggestions. Each row shows payer, payee, amount, and when it was marked as paid. The newest payment is first.
4. **Delete a payment.** The user selects Delete on a row in the Paid list. A confirmation step asks them to confirm. On confirm the payment is removed, its debt returns to the suggestions, and balances update. On cancel nothing changes.
5. **All settled.** When there are no suggestions left, the section shows the all-settled state (see States & errors).

Screen names: Trip page (settle-up section: Suggested payments, Paid list). No mock screens exist yet in `docs/design`; layout is left to the plan.

## Requirements
- **F6.R1** Suggested payments are worked out from the current balances, using the rule in product.md Business rules R6: repeatedly, the person owed the most is paid by the person who owes the most, for the smaller of their two amounts, until every balance is zero.
- **F6.R2** When two people have the same balance, the person added to the trip first is chosen first (F3.R5 order).
- **F6.R3** Each suggested payment has a payer, a payee and an amount greater than zero. The amount is the smaller of the payer's debt and the payee's credit at that step.
- **F6.R4** Marking a suggested payment as paid records a Payment with the same payer, payee and full amount. It also records the date and time it was marked.
- **F6.R5** A Payment's payer and payee must be different people on the same trip.
- **F6.R6** Payments are included in balances: a payer's balance increases and the payee's decreases by the payment amount (product.md Business rules R4; F5.R1).
- **F6.R7** Suggestions are recalculated from the balances after every change, including a payment being marked, a payment being deleted, and an expense being added or deleted. Recording a payment therefore removes or reduces its suggestion (worked example 3).
- **F6.R8** Deleting a Payment removes it. Its amount returns to the balances, and the suggestions are recalculated.
- **F6.R9** A Payment cannot be edited.
- **F6.R10** A person who is the payer or payee of a Payment cannot be removed from the trip (F3.R6, product.md Business rules R7).
- **F6.R11** Viewing, marking and deleting payments require a logged-in user who owns the trip (F1.R10, F1.R11). Other users get a 404.
- **F6.R12** The Paid list shows the newest payment first.

## Data & contract

**Payment record:** the payer, the payee, the amount in whole cents (always greater than zero), the time it was marked as paid (F6.R4), and the user who marked it. A payment's payer and payee are different people on the same trip (F6.R5). Payments cannot be edited (F6.R9).

**Operations:**
- View the suggested payments and the Paid list.
- Mark a suggested payment as paid. The user never types the amount; the app takes it from the current suggestion for that payer and payee, so the payment always matches what the user saw (F6.R4).
- Delete a payment.

Refused requests: a payer and payee that are the same person (`payer_equals_payee`); a suggestion that changed or no longer exists (`no_matching_suggestion`), in which case nothing is recorded; a trip or person that is not the user's (404). Amounts are shown as text with two decimal places, for example "40.00".

## States & errors
- **No suggestions (all settled):** "Everyone is settled up. No payments are left to make." Shown when every balance is zero.
- **Empty Paid list:** "No payments recorded yet."
- **Mark as paid confirmation:** title "Mark this payment as paid?"; body "{payer} pays {payee} {amount}. This updates everyone's balances."; buttons "Mark as paid" and "Cancel".
- **Delete payment confirmation:** title "Delete this payment?"; body "{payer} will owe {payee} {amount} again. This updates everyone's balances."; buttons "Delete payment" and "Cancel".
- **Suggestion no longer matches (`no_matching_suggestion`):** "These payments have changed since you opened this page. Review the updated suggestions and try again."
- **`payer_equals_payee`:** "A person can't pay themselves." Shown only if the request is sent without going through the form.
- **Server error:** "Something went wrong. Please try again."
- **Not logged in or not the owner:** handled by F1; no new copy.

## Accessibility
- Each suggestion reads as one sentence to a screen reader, for example "Jo pays Sam 40.00".
- Each "Mark as paid" and "Delete" control has an accessible name that includes the payment details (for example "Mark Jo's payment of 40.00 to Sam as paid").
- Both confirmation steps are modal dialogs: focus moves in, Escape cancels, focus returns to the control that opened the dialog.
- After a payment is marked, focus moves to the Paid list heading so the screen reader announces the change.
- Everything works with the keyboard.
- The Paid list is a labelled list. Text and colour meet WCAG AA contrast.
- No animation is needed, so reduced motion has no effect.

## Edge cases
- **Worked example 1:** balances are Sam +50.00, Alex −10.00, Jo −40.00. Suggestions: Jo pays Sam 40.00, then Alex pays Sam 10.00.
- **Worked example 3:** after Jo's 40.00 payment is marked as paid, the only suggestion left is Alex pays Sam 10.00.
- **All suggestions marked paid:** every balance is zero and no suggestions remain (the done-when rule).
- **Ties:** two people owe 10.00 each, and one person is owed 20.00. The person added first pays first (F6.R2).
- **Delete a payment:** its debt returns to the suggestions. For example, deleting the Jo-to-Sam payment brings back "Jo pays Sam 40.00".
- **Add an expense after payments exist:** the balances change, and the suggestions are recalculated from the new balances. A payment already recorded stays in the Paid list.
- **Delete an expense after a payment:** the balances change, and the suggestions are recalculated. The Paid list is not changed.
- **Trip with one person or no expenses:** all balances are zero, so there are no suggestions and the all-settled state is shown.
- **Stale suggestion:** two browsers open; one marks a suggestion as paid; the other marks the same one. The second request is refused with `no_matching_suggestion`, and nothing is recorded (F6.AC15).

## Acceptance criteria
- **F6.AC1** Given worked example 1, when suggestions are worked out, then they are "Jo pays Sam 40.00" followed by "Alex pays Sam 10.00".
- **F6.AC2** Given worked example 3 (Jo's 40.00 payment marked as paid), when the suggestions are worked out, then the only suggestion is "Alex pays Sam 10.00".
- **F6.AC3** Given the suggestion "Jo pays Sam 40.00", when the user confirms "Mark as paid", then a payment of 40.00 from Jo to Sam is recorded with the time it was marked, and it appears in the Paid list.
- **F6.AC4** Given a payment in the Paid list, when the user selects Delete and confirms, then the payment is removed and its suggestion returns. When they cancel, then it stays.
- **F6.AC5** Given two people who each owe 10.00 and one person owed 20.00, with the first-added person listed first, when suggestions are worked out, then the first-added person's payment is suggested first.
- **F6.AC6** Given a trip where every suggestion has been marked as paid, when the settle-up section is shown, then there are no suggestions, every balance is zero, and the all-settled state is shown.
- **F6.AC7** Given any set of balances that add up to zero, when suggestions are worked out and all of them are applied, then every balance is zero.
- **F6.AC8** Given a payment request where the payer and payee are the same person, when it is submitted, then it is rejected with `payer_equals_payee` and nothing is saved.
- **F6.AC9** Given a Paid list, when it is shown, then each row shows the payer, payee, amount and the time it was marked.
- **F6.AC10** Given a person who is a payer or payee of a Payment, when the user tries to remove them from the trip, then the removal is refused (see F3.AC8).
- **F6.AC11** Given user A and a trip owned by user B, when user A views, marks or deletes a payment on it, then the response is 404 and nothing changes.
- **F6.AC12** Given a recorded payment, when the balances are calculated, then the payer's balance has gone up and the payee's has gone down by the payment amount (F5.R1, F6.R6).
- **F6.AC13** Given a payment in the Paid list, when the user looks for a way to change it, then no edit control is shown and no request can change its payer, payee or amount.
- **F6.AC14** Given three payments recorded at different times, when the Paid list is shown, then the newest payment is first.
- **F6.AC15** Given a suggestion that changed after the user loaded the page, when the user marks it as paid, then no payment is recorded and the user is shown the updated suggestions.

## Dependencies
- **Depends on:** F1 (login and ownership), F2 (trips), F3 (people; removal rule), F4 (expenses), F5 (balances).
- **Needed by:** nothing in the MVP. F6 is the last feature in the build order.
- **Build order:** step 6.
- **External services:** none.

## Test plan
Unit tests only (project rules). Each test is mapped to its AC IDs.

- **Unit: settlement algorithm** (pure function over balances). AC1 (worked example 1), AC2 (worked example 3, using the balances after the payment), AC5 (tie-breaking by add order), AC6 (no suggestions when balances are zero), AC7 (sum of all suggestions brings every balance to zero, over several sample balance sets).
- **Unit: create-payment handler with mocked repositories.** AC3 (records full amount, payer, payee and time), AC8 (same payer and payee rejected), and AC15 (a changed suggestion is refused and nothing is saved).
- **Unit: delete-payment handler.** AC4 (confirmed delete removes the payment; cancel is handled in the UI test).
- **Unit: balance calculation including payments** (the F5 calculator with payments added). AC12.
- **Unit: ownership guard for settlement requests with a mocked trip owner.** AC11.
- **Unit: removal rule including payments** (the F3 removal check). AC10 (link to F3.AC8).
- **Unit (frontend): Settle-up component test.** AC1 and AC2 as visible text, AC4 cancel keeps the payment, AC6 all-settled state, AC9 Paid list row content, AC14 newest first.

Code review only (need a real database or the full running app):
- **AC9 end to end:** Paid list reflects real stored payments and their times.
- **Cascade on trip delete:** payments removed with the trip.
- **Person check:** payer and payee must be people on this trip in stored data.
- **AC11 end to end:** full app returns 404 for another user's trip.
- **AC10 end to end:** a real removal attempt is refused in the database-backed app.
- **AC13 (no edit):** no edit control or request exists; code review of the screen and the request list.

## Open questions
None.

## Decisions log
- 2026-11-19: Tie-breaking for suggested payments? → Person added to the trip first pays first.
- 2026-11-19: Confirm before deleting a payment? → Yes.
- 2026-11-19: Where do paid payments appear? → A "Paid" list under the suggestions, with the delete option.
- 2026-11-19: Confirm before marking a payment as paid? → Yes.
- 2026-11-19: Record when a payment was marked as paid? → Yes, store the date and time.
- 2026-11-24: Grader feedback: F6.R9 had no acceptance criterion → Added F6.AC13.
- 2026-11-24: A suggestion that changed before it was marked paid? → Refuse it and show a message; nothing is recorded (F6.AC15).
- 2026-11-24: Paid list order? → Newest first (F6.R12, F6.AC14).
- 2026-11-24: Error and confirmation wording → Standard plain wording proposed by Claude at the user's request. The user to review before build.
- 2026-11-24: Grader feedback: spec named tables, endpoints and libraries → Replaced them with product terms; the plan decides the technical design.
