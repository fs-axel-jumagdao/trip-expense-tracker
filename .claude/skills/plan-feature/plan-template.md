# F<N> — <Feature name>: implementation plan

_Status: Draft · Last updated: <YYYY-MM-DD> · Spec: docs/specs/features/F<N>-<slug>.md (Approved <YYYY-MM-DD>) · Standards: docs/rules/, .claude/CLAUDE.md_

## Validation record
Checks run before this plan was written. Any failure stops the plan, so every row here should say Pass.

| Check | Result |
|---|---|
| V1 Spec file exists | Pass / Fail |
| V2 Spec status is Approved | Pass / Fail (status found: …) |
| V3 Open questions section has no unchecked items | Pass / Fail (items: …) |
| V4 Every acceptance criterion is complete (Given, when, then) and appears in the test plan | Pass / Fail (missing: …) |
| V5 No placeholders (`…`, `TBD`) left in the spec | Pass / Fail (found in: …) |

## Context
Why this feature is being built and what the spec asks for, in 2–4 sentences. Name the dependencies (F-IDs) and the build-order step from the spec.

## Standards check
Each applicable rule from `docs/rules/` and `.claude/CLAUDE.md`, with how this plan meets it. A conflict is written as a conflict, never silently worked around.

| Source | Rule | How this plan complies, or the conflict |
|---|---|---|
| docs/rules/project-rules.md | Unit tests only; no integration or end-to-end tests | … |
| docs/rules/project-rules.md | Test names carry the full AC ID (for example F3.AC4) | … |
| docs/rules/project-rules.md | Money stored as whole cents; no floating-point | … |
| docs/rules/project-rules.md | Multi-record writes in one transaction | … |
| docs/rules/project-rules.md | Times stored in UTC | … |
| docs/rules/project-rules.md | Passwords hashed; no secrets; no plain-text logging | … |
| docs/rules/project-rules.md | Logger only; no print or console statements | … |
| .claude/CLAUDE.md | Vertical slice layout, feature-based folders | … |
| .claude/CLAUDE.md | MediatR, FluentValidation, EF Core directly (no generic repository) | … |
| .claude/CLAUDE.md | Audit fields on major entities (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) | … |
| .claude/CLAUDE.md | Strict TypeScript; no `any`; TanStack Query and React Hook Form with Zod | … |
| .claude/CLAUDE.md | Minimum 80% coverage for business logic, handlers and validators | … |

Add or remove rows for rules that do not apply, and say why in the row.

## Approach
The recommended approach only, in short paragraphs or a short list. No alternatives unless one was rejected for a reason that matters to the reader.

## Files
Critical files to create or change, and existing code to reuse (with paths). For a pattern repeated across many files, describe it once and list a few representative paths.

| Path | Create / change | Purpose |
|---|---|---|
| … | … | … |

## Tasks
Work is split into tasks with IDs `F<N>.T1`, `F<N>.T2`, and so on. Each task is small enough to finish and test on its own. Tasks follow the spec's build order and dependency list. Every AC appears in at least one task.

| Task | Title | Files | ACs covered | Done when |
|---|---|---|---|---|
| F<N>.T1 | … | … | F<N>.AC1, … | … |

### F<N>.T1 — <title>
- **Goal:** …
- **Files:** …
- **Acceptance criteria covered:** F<N>.AC1, …
- **Tests written (names include the AC ID):** …
- **Done when:** the named tests pass and the code builds. Use a command or an observable result, not "it works".

(Repeat for each task.)

## Test plan
Unit tests only, as required by `docs/rules/project-rules.md`. Every AC is mapped to a task, and to either named unit tests or "code review".

| AC | Unit test (name includes the AC ID) | Or code review (reason) |
|---|---|---|
| F<N>.AC1 | … | |
| F<N>.AC2 | | Database constraint: … |

## Verification
How a reviewer confirms the work is done, using only the unit test run and the code-review checklist: the commands to run, the expected results, and the code-review items with the AC each one proves.

## Needs a person's approval
Changes the rules say a person must approve. Write "None" if there are none.
- Authentication or sessions
- Secrets or environment files
- Database migrations
- CI or deployment config
- Deleting or disabling tests

## Risks and conflicts
Anything that could block or change the build, including conflicts with the standards above. Each one has an owner or a decision needed.

## Open items
Questions this plan could not settle. Each one is also a blocker if it changes a task. Write "None" if there are none.
- [ ] …

## Decisions log
- <YYYY-MM-DD>: <question> → <answer>
