# F3 — People

_Status: Draft · Last updated: 2026-11-24 · Source: product.md Roles, Key terms, Assumptions, Business rules R2 and R7, MVP scope row F3_

## Summary
A user adds the people on one of their trips and removes them again. People are names only. They have no accounts and cannot log in. A person who is part of any expense or payment cannot be removed, so the balances can never lose someone who owes or is owed money.

## User stories
- As a user, I want to add the people on my trip by name, so that I can split costs between them.
- As a user, I want to remove someone I added by mistake, so that the trip list stays accurate.
- As a user, I want to be told why I cannot remove someone, so that I know what to delete first.

## Scope
**In:**
- Add a person to a trip by name.
- Remove a person from a trip, when no expense or payment uses them.
- Show the people on a trip in the order they were added.
- Show a clear message when a removal is blocked, naming the expense or payment that blocks it.

**Out:**
- Renaming a person. Product.md Assumptions says there is no editing; a mistake is fixed by deleting and adding again.
- Accounts or logins for people. Product.md Roles: people have no accounts.
- Editing expenses or payments to change who is involved (covered by F4 and F6, which have no editing either).
- Sharing a trip with other users (product.md, After the core).

## User flow & screens
1. **Add a person.** On the trip page, the user types a name into the "Add person" field and submits. The name appears at the end of the people list. The field is cleared.
2. **Remove a person (allowed).** The user selects Remove next to a person who has no expense or payment. The person is removed from the list.
3. **Remove a person (blocked).** The user selects Remove next to a person who is in an expense or payment. The request is refused and a message names the blocking expense or payment. The person stays in the list.
4. **Empty list.** A trip with no people shows the empty state (see States & errors).

Screen names: Trip page (people section). No mock screens exist yet in `docs/design`; layout is left to the plan.

## Requirements
- **F3.R1** A person has a name. The name is trimmed of surrounding spaces before it is checked or saved.
- **F3.R2** A name is 1 to 50 characters after trimming.
- **F3.R3** A name is unique within its trip. The comparison ignores letter case, so "Sam" and "sam" on the same trip are duplicates. The same name may be used on different trips. (product.md Key terms "Person"; case rule decided by the user.)
- **F3.R4** A trip may have zero or more people. There is no minimum number of people.
- **F3.R5** People are listed in the order they were added to the trip. This is the same order used for the leftover-cent rule (product.md Business rules R3).
- **F3.R6** A person can be removed only when no expense lists them as payer or participant, and no payment lists them as payer or payee (product.md Business rules R7).
- **F3.R7** A refused removal shows which expenses and payments block it, each identified by its description and amount (expenses) or by its payer, payee and amount (payments).
- **F3.R8** People cannot be renamed. A mistake is fixed by removing the person and adding them again (product.md Assumptions "No editing").
- **F3.R9** Adding and removing people require a logged-in user who owns the trip (F1.R10, F1.R11). Other users get a 404.

## Data & contract

**Person record:** a name (1 to 50 characters, stored trimmed), the trip it belongs to, and the time it was added, which sets the list order (F3.R5). Names are unique within a trip, ignoring letter case. A person is never edited, so there is no update time or update author. Deleting a trip removes its people (product.md Business rules R8).

**Removal check:** a removal is refused when any expense (as payer or participant) or any payment (as payer or payee) on the trip uses the person. Payments arrive in F6; until then the check covers expenses only.

**Operations:** add a person; remove a person. A refused removal lists the expenses and payments that block it (F3.R7). Error codes are in States & errors.

## States & errors
- **Empty list:** "No people yet" is not agreed. Copy **open**.
- **Validation errors** (shown next to the field):

| Code | Field | Rule | When checked | Copy |
|---|---|---|---|---|
| `name_required` | name | Empty after trimming | Add | open |
| `name_too_long` | name | More than 50 characters after trimming (F3.R2) | Add | open |
| `name_taken` | name | Same name on this trip ignoring case (F3.R3) | Add | open |

- **Blocked removal:** the message names each blocking expense or payment (F3.R7). The wording of the message is **open**.
- **Server error:** copy **open**.
- **Not logged in or not the owner:** handled by F1 (F1.R10, F1.R11); no new copy.

## Accessibility
- The "Add person" field has a visible label. The Remove control has an accessible name that includes the person's name (for example "Remove Sam").
- Validation errors are linked to their field, so a screen reader reads each error with its field, and focus moves to the field on error.
- A blocked removal is announced to screen readers and the blocking items are readable text, not colour alone.
- Everything works with the keyboard, and Enter submits the add form.
- Focus returns to the Add person field after a successful add.
- No animation is needed, so reduced motion has no effect.

## Edge cases
- Name with only spaces: rejected as `name_required`.
- Name of exactly 50 characters: accepted. 51: rejected.
- "  Sam  " added when "sam" exists: rejected as `name_taken`.
- "Sam" on trip A and "Sam" on trip B: both accepted.
- Person removed, then the same name added again: accepted. The new person is added at the end of the list (F3.R5).
- Removing a person who only appears in a payment after F6 exists: blocked, as in F3.R6.
- Two people on the same trip being added at the same moment with the same name: only one succeeds, because the name-uniqueness rule for the trip rejects the second (`name_taken`).
- Deleting a trip removes its people (product.md Business rules R8).

## Acceptance criteria
- **F3.AC1** Given a trip with Sam and Alex, when the user adds "Jo", then Jo appears last in the list.
- **F3.AC2** Given the Add person field, when the user submits a name of only spaces, then the request is rejected with `name_required` and no person is added.
- **F3.AC3** Given a name of 51 characters, when the user adds it, then it is rejected with `name_too_long`. A name of 50 characters is accepted.
- **F3.AC4** Given a trip with "Sam", when the user adds "  sam  ", then it is rejected with `name_taken`. Given another trip with no "Sam", when the user adds "Sam" there, then it is accepted.
- **F3.AC5** Given a person who is not in any expense or payment, when the user removes them, then they are removed.
- **F3.AC6** Given a person who is the payer of an expense, when the user removes them, then the request is refused with a 409 that lists that expense, and the person stays.
- **F3.AC7** Given a person who is only a participant in an expense, when the user removes them, then the request is refused with a 409 that lists that expense.
- **F3.AC8** Given a person who is the payer or payee of a payment (F6), when the user removes them, then the request is refused with a 409 that lists that payment.
- **F3.AC9** Given people added in the order Sam, Alex, Jo, when the list is shown, then it shows Sam, Alex, Jo in that order.
- **F3.AC10** Given user A and a trip owned by user B, when user A adds or removes a person on it, then the response is 404 and nothing changes.
- **F3.AC11** Given a new trip with no people, when the user views it, then the trip is valid and the empty state is shown.
- **F3.AC12** Given a person on a trip, when the user looks for a way to change their name, then no edit control is shown and no request can change the name.

## Dependencies
- **Depends on:** F1 (login and ownership checks, F1.R10, F1.R11), F2 (trips exist to hold people).
- **Needed by:** F4 (expenses need people as payer and participants), F5 (balances per person), F6 (payments are between people).
- **Build order:** step 3. Removal checks for payments are finished in F6.
- **External services:** none.

## Test plan
Unit tests only (project rules). Each test is mapped to its AC IDs.

- **Unit: name validator** (trim, length 1–50, empty, whitespace). AC2, AC3.
- **Unit: name normaliser** (trim and lowercase; "  Sam " and "sam" normalise the same). AC4 (part).
- **Unit: add-person handler with a mocked repository.** AC1 (new person added at the end), AC4 (duplicate returns `name_taken` when the repository reports a match), AC11 (zero people is valid).
- **Unit: remove-person handler with mocked blockers.** AC5 (no blockers, removed), AC6 (payer blocker, 409 listing the expense), AC7 (participant blocker, 409 listing the expense), AC8 (payment blocker, 409 listing the payment; payment part uses the F6 check when it exists).
- **Unit: people ordering.** AC9 (list is sorted by creation order).
- **Unit: ownership guard for people requests with a mocked trip owner.** AC10 (non-owner gets 404, nothing changed).

Code review only (need a real database or the full running app):
- **AC4 (duplicate is also rejected under a race):** name uniqueness must hold in stored data.
- **Race case in Edge cases:** two simultaneous adds; only a check against stored data can prove this.
- **Cascade on trip delete:** people removed with the trip.
- **AC10 end to end:** full app returns 404 for another user's trip.
- **AC12 (no rename):** no edit control or request exists; code review of the screen and the request list.

## Open questions
- [ ] Exact wording for: `name_required`, `name_too_long`, `name_taken`, the blocked-removal message, the empty-list state, and the server error.

## Decisions log
- 2026-11-19: Name rules? → Trim spaces, unique within the trip ignoring case, max 50 characters.
- 2026-11-19: Minimum number of people on a trip? → None. Zero or more is allowed.
- 2026-11-19: People list order? → Order they were added.
- 2026-11-19: What does the user see when a removal is blocked? → A message that names the expense or payment blocking it.
- 2026-11-24: Grader feedback: F3.R8 had no acceptance criterion → Added F3.AC12.
- 2026-11-24: Grader feedback: spec named tables, endpoints and constraints → Replaced them with product terms; the plan decides the technical design.
