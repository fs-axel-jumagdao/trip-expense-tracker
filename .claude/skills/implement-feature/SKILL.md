---
name: implement-feature
description: Implement exactly one plan task (F<N>.T<k>) by handing it to the developer agent, after checking that the feature's spec and plan are Approved and that earlier tasks are done. Use when the user runs /implement-feature F<N>.T<k>. Never edits the spec or the plan, and never commits or pushes.
argument-hint: F<N>.T<k> (e.g. F1.T3)
model: haiku
---

# Implement one task

Run one task from an approved plan. The work is done by the `developer` agent (`.claude/agents/developer.md`). This command only checks the gates, gives the agent its one task, and shows the report.

This command needs the Agent tool, because it spawns the `developer` agent.

Target task: **$ARGUMENTS**

## 1. Parse the task

- Expect `F<N>.T<k>` (accept `f1.t3` and `F1 T3`, and normalise them). Find the plan with `docs/plans/F<N>-*.md`.
- No argument, or a bad format: list the tasks in the plans (`F<N>.T<k>` with title and status) and ask which one. Don't guess.
- If the plan for `F<N>` does not exist, say so and list the plans that exist.

## 2. Gate on approval

Both must say `_Status: Approved` on their status line:
- The plan: `docs/plans/F<N>-*.md`.
- The spec: `docs/specs/features/F<N>-*.md`.

If either is not Approved, stop and reply:
- which file, and the status found;
- "Resolve it with `/plan-feature F<N>`" (for the plan) or "`/spec-feature F<N>`" (for the spec).

Never edit either file to pass this check.

## 3. Check earlier tasks

Every task earlier in the same plan (lower `k`) must be finished. An earlier task counts as finished when both checks below pass:

- **Files exist.** Every file the earlier task lists in the plan exists. A migration file matches by its name, ignoring the timestamp. Check with Glob.
- **Unit tests named.** For each AC the earlier task maps to a unit test in the plan's Test plan, a test whose name contains that AC ID (`F<N>.AC<k>`) exists under `tests/` or in the client test folder. Check with Grep on the test names. ACs the plan lists for code review are exempt.

A migration task counts as finished only when the user has confirmed in this run or an earlier one that they reviewed and approved the migration. The skill never treats a migration as finished on its own.

If an earlier task is not finished, stop and name it. Do not skip ahead. The one exception is a task the user marks as "run it anyway", and only if the user says so in this run.

## 4. Extract only this task

From the plan, take:
- the task's row in the **Tasks** table: title, files, ACs covered, done-when check;
- the AC lines for each AC the task covers, from the spec's **Acceptance criteria** section.

Do not pass the rest of the plan or the spec to the agent. The spec and plan paths go in the message, so the agent can read more if it needs to.

## 5. Spawn the developer agent

Use the Agent tool with `subagent_type: "developer"` and `model: "haiku"`. Set the model explicitly on every spawn.

The task message contains only:
- the task ID and title;
- the files the task names;
- the AC lines it covers (full text);
- the done-when check, as written in the plan;
- the spec path `docs/specs/features/F<N>-<slug>.md` and the plan path `docs/plans/F<N>-<slug>.md`, for more detail if needed.

Say in the message: "Implement this one task only. Read CLAUDE.md and docs/rules/ first, as your first step."

## 6. Show the report

Show the agent's report exactly as it returns, starting with its `Status:` line. Then:
- **Done:** say which task is next (the next `F<N>.T<k>` in the plan), and that the user can run `/implement-feature` on it.
- **Partial:** list what is left and what the user must decide. Do not start the next task.
- **Blocked:** state the reason in the report, and what the user needs to provide. Stop.

## 7. Retries

If the user asks for a retry, spawn the agent again with `model: "haiku"` set explicitly, using the same task message. Never switch to another model.

## 8. Files and commits

Do not commit or push. Never run `git commit` or `git push`. After the report, list the files the agent changed (from its "What changed" list) so the user can review and commit them.
