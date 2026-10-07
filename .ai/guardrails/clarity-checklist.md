# Clarity checklist

This checklist is referenced by all agent roles. It defines the five signals that indicate an agent is drifting from its task. Any agent that detects one of these signals must **stop working immediately** and report.

---

## Signal 1: Scope creep

**The question**: Am I touching files not listed in my task's "Files allowed" field?

**What counts as a violation**:

- Opening, reading, or modifying a file not listed in the task.
- Creating a new file that wasn't specified in the task.
- Installing a dependency not mentioned in the task.
- Modifying a configuration file (e.g., `package.json`, `tsconfig.json`) when it wasn't listed.

**What does NOT count**:

- Reading `.ai/` files that your role instructs you to read (e.g., `context.md`, `stack.md`).
- Running test commands that happen to read other files (the test runner reads them, not you).

**When detected**:

1. Stop modifying files immediately.
2. Document which file you were about to touch and why.
3. Add a note to your task entry: `SCOPE VIOLATION DETECTED: needed to modify {file} because {reason}`.
4. Tell the user the task needs to be re-scoped by the Architect.

---

## Signal 2: Role violation

**The question**: Am I making a decision that the Architect should have made?

**What counts as a violation**:

- Choosing between two valid implementation approaches without guidance.
- Deciding on a data structure, API shape, or naming convention that isn't established in `conventions.md` or the task description.
- Creating an abstraction (a new class, module, or pattern) that doesn't exist yet.
- Deciding how to handle an edge case that the task didn't mention.

**What does NOT count**:

- Choosing variable names within an existing pattern.
- Selecting standard library methods (e.g., `.map()` vs `.forEach()`).
- Formatting decisions within established project conventions.

**When detected**:

1. Stop at the decision point.
2. Document the fork: "I need to decide between A and B. A would mean {consequence}. B would mean {consequence}."
3. Add this to your task entry as a `DECISION NEEDED` block.
4. Tell the user this needs Architect input.

---

## Signal 3: Goal drift

**The question**: Can I still state in one sentence what "done" looks like for this task?

**How to test**: Without looking at the task definition, try to say in one sentence what you are building and how you will know it's finished. Then compare against the actual exit criteria.

**Signs of goal drift**:

- Your mental model of "done" has expanded beyond the exit criteria.
- You are thinking about how this connects to the next task.
- You are solving problems that the exit criteria don't require you to solve.
- You started with a clear picture and it has become fuzzy.

**When detected**:

1. Stop working.
2. Re-read your task definition from `active.md`.
3. If re-reading restores clarity, note the drift and continue carefully.
4. If re-reading does NOT restore clarity, your context is polluted. Write a status update and end the session.

---

## Signal 4: Complexity spiral

**The question**: Have I spent more than 2 iterations on something that should be simple?

**What counts as an iteration**:

- Writing code, testing it, finding it doesn't work, and rewriting.
- Each cycle of "try → fail → rethink" is one iteration.

**Signs of a complexity spiral**:

- You are debugging something that should have worked on the first try.
- You are working around a limitation you didn't expect.
- The implementation is growing significantly beyond the estimated line count.
- You are importing or depending on things not in the task description.
- You are writing helper functions that feel like they should already exist.

**When detected**:

1. Stop iterating.
2. Document what you expected vs. what happened.
3. Consider: is the task correctly scoped? Is there a missing dependency? Is the approach wrong?
4. Report it as: `COMPLEXITY SPIRAL: expected {X}, hit {Y} after {N} iterations. Possible causes: {list}`.
5. The Architect may need to re-scope or split the task.

---

## Signal 5: Context pollution

**The question**: Is my working memory getting cluttered with information unrelated to this task?

**Signs of context pollution**:

- You have read files outside your task scope (even just to "understand the codebase").
- You are holding information about other tasks, features, or bugs in your working context.
- The chat history contains long debugging threads, tangential discussions, or abandoned approaches.
- You find yourself scrolling back through the conversation to remember what you were doing.
- The conversation has gone beyond ~40 back-and-forth exchanges.

**When detected**:

1. Do not start new work.
2. Finish the minimum viable version of whatever you are currently doing — or, if you can't, write a partial completion report.
3. Document your progress clearly enough that a fresh session can pick up.
4. Tell the user: "Context is getting heavy. I recommend starting a fresh session for the remaining work."

---

## Summary decision table

| Signal | Action | Who resolves it |
|--------|--------|----------------|
| Scope creep | Stop, report file, request re-scope | Architect |
| Role violation | Stop at decision point, document fork | Architect |
| Goal drift | Re-read task; if unclear, end session | User (new session) |
| Complexity spiral | Stop iterating, report expected vs actual | Architect |
| Context pollution | Finish current step, end session | User (new session) |

---

## For the user

If an agent reports a checkpoint failure, here's what to do:

1. **Scope creep or Role violation**: Open an Architect session. Share the agent's report. The Architect will re-scope the task or make the needed decision.
2. **Goal drift or Context pollution**: Start a fresh Developer session. The task definition in `active.md` plus the agent's status update should be enough context.
3. **Complexity spiral**: Open an Architect session. The task may need to be split, or the approach may need to change.

Checkpoint failures are not errors — they are the system working as designed. They prevent wasted tokens and broken code.
