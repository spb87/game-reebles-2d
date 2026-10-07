# Cost estimates

Pre-build cost projections created by the Architect before feature work begins.

---

### Feature: M1 — Repo & Toolchain Scaffold (TASK-1 … TASK-6)

- **Estimated tasks**: 6
- **Avg tokens per task (input)**: ~40k (task def + contracts + target files; Unity-log tailing adds noise)
- **Avg tokens per task (output)**: ~15k (boilerplate-heavy, mostly file writes)
- **Model tier**: Developer (`swe-2-medium` subagent, pinned via `.devin/agents/developer.md`)
- **Completion-processing overhead**: ~8k orchestrator tokens per task (review + memory updates)
- **Total estimated cost**: roughly 2-4 Devin ACU-equivalents for the feature; Unity batch runs dominate wall-clock (each `-createProject`/`-quit` cycle is 1-3 min on `/mnt/c` 9P I/O)
- **Time estimate**: 1-2 hours wall clock (Unity import cycles are the bottleneck, not tokens)
- **Worth it?**: yes — this is the foundation everything else compiles against

### Feature: M1 — Greybox Scene + Player Movement (TASK-7 … TASK-10)

- **Estimated tasks**: 4
- **Avg tokens per task (input)**: ~50k
- **Avg tokens per task (output)**: ~20k (the inputactions JSON + scene builder are the chunky writes)
- **Model tier**: Developer (`swe-2-medium`)
- **Completion-processing overhead**: ~8k orchestrator per task
- **Total estimated cost**: similar to F1; TASK-8/9 carry iterate-until-green risk (PlayMode physics tests)
- **Time estimate**: 1-2 hours wall clock
- **Worth it?**: yes — proves movement + the code-generated-scene convention before art spends a cent

### Feature: M1 — Web Smoke Build + Public Deploy (TASK-11 … TASK-12)

- **Estimated tasks**: 2
- **Avg tokens per task (input)**: ~45k
- **Avg tokens per task (output)**: ~15k
- **Model tier**: Developer (`swe-2-medium`)
- **Completion-processing overhead**: ~8k orchestrator per task
- **Total estimated cost**: small; wall-clock dominated by the WebGL build (5-15 min) and Pages propagation
- **Time estimate**: ~1 hour wall clock including deploy propagation
- **Worth it?**: yes — retires the "live URL" pillar risk in week one per scope

### Epic total (M1)

- ~12 tasks, order-of-magnitude 0.6-1M developer tokens + ~100k orchestrator tokens
- Wall clock: ~half a day if batch runs behave; longer if Unity import/9P stalls appear
