# F2 — Trips

_Status: Approved · Last updated: 2026-11-24 · Source: product.md Overview, Goals, Roles, Key terms (Trip), Business rules R8, MVP scope row F2_

## Summary
A user creates a named trip, sees the list of their own trips, and deletes a trip they no longer need. A trip belongs to exactly one user, and nobody else can see it. Trips are the container for people, expenses and payments, so deleting a trip removes everything in it, after the user confirms.

## User stories
- As a user, I want to create a trip with a name, so that I have a place to track its spending.
- As a user, I want to see my trips in one list, so that I can find the one I need.
- As a user, I want to delete a trip I no longer need, after confirming, so that its spending stops cluttering my list.

## Scope
**In:**
- Create a trip with a name.
- List the logged-in user's trips, newest first.
- Open a trip to add people (the people feature is F3).
- Delete a trip after confirmation. Deleting removes its people, expenses and payments.

**Out:**
- Trip dates, destination, currency or any other field. Product.md Key terms and F2 name only a name, and product.md Assumptions say there is one currency.
- Editing a trip's name. Product.md Assumptions: no editing; a mistake is fixed by deleting and creating again.
- Sharing a trip with other users (product.md, After the core).
- Archiving or restoring a trip (not in product.md; out of MVP).

## User flow & screens
1. **Trip list.** The logged-in user opens the trip list. It shows their trips, newest first. If they have none, the empty state is shown.
2. **Create a trip.** The user selects New trip, types a name and submits. On success the new trip's page opens so the user can add people (F3).
3. **Open a trip.** The user selects a trip in the list and its page opens.
4. **Delete a trip.** The user selects Delete on a trip. A confirmation dialog asks them to confirm. On confirm the trip and everything in it is removed, and the user returns to the list. On cancel nothing changes.

Screen names: Trip list, New trip form, Trip page (the page F3–F6 add their sections to), Delete trip confirmation. No mock screens exist yet in `docs/design`; layout is left to the plan.

## Requirements
- **F2.R1** A trip has a name. The name is trimmed of surrounding spaces before it is checked or saved, and it must be 1 to 100 characters after trimming.
- **F2.R2** Two trips may have the same name, including for the same user. Trips are told apart by their position in the list, not by name.
- **F2.R3** A trip belongs to exactly one user, the one who created it (product.md Roles: ownership rule).
- **F2.R4** The trip list shows only the logged-in user's trips, newest first.
- **F2.R5** After a trip is created, the user goes straight to that trip's page to add people.
- **F2.R6** A trip can be deleted only after the user confirms. Deleting removes the trip and everything in it: its people, expenses and payments (product.md Business rules R8).
- **F2.R7** Listing, creating, opening and deleting trips require a logged-in user (F1.R10). A trip that belongs to another user is treated as not found (F1.R11).
- **F2.R8** A trip has only a name and an owner. No other trip fields exist in the MVP (see Scope).

## Data & contract

**Trip record:** a name (1 to 100 characters, stored trimmed), the owning user, and the time it was created. Trip names are not unique (F2.R2).

**Operations:**
- Create a trip from a name. Returns the new trip.
- List the logged-in user's trips, newest first.
- Open one of the user's trips.
- Delete one of the user's trips. Removes all of its people, expenses and payments (F2.R6).

Each error has a code and a message (see States & errors). A trip that is not the user's is treated as not found.

## States & errors
- **Empty trip list:** "You don't have any trips yet. Create one to start tracking spending."
- **Validation errors** (shown next to the name field):

| Code | Field | Rule | When checked | Copy |
|---|---|---|---|---|
| `trip_name_required` | name | Empty after trimming (F2.R1) | Create | "Enter a trip name." |
| `trip_name_too_long` | name | More than 100 characters after trimming (F2.R1) | Create | "Trip names can be up to 100 characters." |

- **Delete confirmation:** title "Delete this trip?"; body "{name} and everything in it will be removed: its people, expenses and payments. This can't be undone."; buttons "Delete trip" and "Cancel" (F2.R6).
- **Server error:** "Something went wrong. Please try again."
- **Trip not found** (another user's trip, or a trip already deleted): "This trip couldn't be found. It may have been deleted."
- **Not logged in:** handled by F1; no new copy.

## Accessibility
- The name field has a visible label, and its validation errors are linked to it so a screen reader reads them with the field.
- The trip list is a labelled list. Each row's link or button has an accessible name that includes the trip name (for example "Open Lisbon 2026").
- The Delete control's accessible name includes the trip name.
- The delete confirmation is a modal dialog: focus moves into it, Escape cancels, focus returns to the Delete control on cancel.
- After a trip is deleted, focus moves to the trip list heading.
- Everything works with the keyboard.
- Focus outlines stay visible. Text and error colours meet WCAG AA contrast.
- No animation is needed, so reduced motion has no effect.

## Edge cases
- Name of only spaces: rejected as `trip_name_required`.
- Name of exactly 100 characters: accepted. 101: rejected as `trip_name_too_long`.
- Two trips named "Lisbon 2026" for the same user: both are kept and both appear in the list.
- The same trip name used by two different users: each user sees only their own trip.
- The user deletes their last trip: the list shows the empty state.
- A trip is deleted in one browser while another browser has its page open: the next action there returns "not found" (F2.R7).
- Deleting a trip with people, expenses and payments removes all of them, and the people, expenses and payments of other trips are not affected.

## Acceptance criteria
- **F2.AC1** Given a logged-in user, when they create a trip named "Lisbon 2026", then the trip is saved, belongs to them, and its page opens so they can add people.
- **F2.AC2** Given the New trip form, when the user submits a name of only spaces, then the request is rejected with `trip_name_required` and no trip is saved.
- **F2.AC3** Given a name of 101 characters, when the user creates the trip, then it is rejected with `trip_name_too_long`. A name of 100 characters is accepted.
- **F2.AC4** Given the user has a trip named "Lisbon 2026", when they create another trip with the same name, then both trips are saved and both appear in the list.
- **F2.AC5** Given two users each with trips, when a user opens the trip list, then it shows only their own trips.
- **F2.AC6** Given trips created in the order A, B, C, when the list is shown, then it shows C, B, A.
- **F2.AC7** Given a trip with people, expenses and payments, when the user selects Delete and confirms, then the trip, its people, expenses and payments are removed and the trip no longer appears in the list.
- **F2.AC8** Given a trip, when the user selects Delete and then cancels, then the trip and everything in it stays.
- **F2.AC9** Given user A and a trip owned by user B, when user A opens or deletes that trip, then the response is "not found" and nothing changes.
- **F2.AC10** Given a logged-out visitor, when they request the trip list, create a trip or delete a trip, then the request is refused with 401 and no trip data is returned.
- **F2.AC11** Given a user with no trips, when they open the trip list, then the empty state is shown.
- **F2.AC12** Given a trip deleted by its owner, when any user requests it, then it is treated as not found.

## Dependencies
- **Depends on:** F1 (login and ownership checks, F1.R10 and F1.R11).
- **Needed by:** F3 (people belong to a trip), F4 (expenses belong to a trip), F5 and F6 (balances and payments belong to a trip). Deleting a trip also has to clear data from those features (F2.R6).
- **Build order:** step 2.
- **External services:** none.

## Test plan
Unit tests only (project rules). Each test is mapped to its AC IDs.

- **Unit: trip name validator** (trim, 1–100 characters, empty, whitespace). AC2, AC3.
- **Unit: create-trip handler with a mocked repository.** AC1 (saved with the owner, returns the new trip so the page can open), AC4 (duplicate names are both accepted).
- **Unit: list-trips handler with mocked repository.** AC5 (only the requester's trips are returned), AC6 (newest first), AC11 (no trips gives an empty list).
- **Unit: delete-trip handler with mocked repositories.** AC7 (confirmed delete removes the trip and calls the removal for its people, expenses and payments), AC8 (cancel is handled in the UI test; the handler is not called on cancel), AC12 (a deleted trip returns not found).
- **Unit: ownership guard for trip requests with a mocked owner.** AC9 (another user's trip returns not found and nothing changes), AC10 (no session gives 401 without calling the handler).
- **Unit (frontend): Trip list and New trip form component tests.** AC1 (opens the trip page after create), AC2 and AC3 as visible messages, AC11 (empty state text), AC8 (cancel keeps the trip in the list).

Code review only (need a real database or the full running app):
- **AC5 in the query:** the trip list query filters by the owner in the database, and the ownership check is in the query itself.
- **AC7 cascade:** deleting a trip removes its people, expenses and payments in the database, and does not touch other trips.
- **AC9 end to end:** full app returns "not found" for another user's trip.
- **AC12 end to end:** a deleted trip cannot be opened by any user.

## Open questions
None.

## Decisions log
- 2026-11-24: Trip name rules? → Trim spaces, required, max 100 characters.
- 2026-11-24: Can two trips have the same name? → Yes, allowed.
- 2026-11-24: Trip list order? → Newest first.
- 2026-11-24: Where does the user go after creating a trip? → Straight to the new trip's page to add people.
- 2026-11-24: Error and confirmation wording → Standard plain wording proposed by Claude at the user's request. The user to review before build.
- 2026-11-24: Approval → Approved by the user.
