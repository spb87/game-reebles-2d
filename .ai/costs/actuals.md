# Cost actuals

Post-build cost tracking. Updated by the Architect after each completed task.

---

### TASK-1: Repo hygiene + secrets

- **Model used**: swe-2-medium (developer subagent)
- **Estimated tokens**: ~40k in / ~15k out
- **Actual tokens**: not tracked
- **Session count**: 1 subagent dispatch
- **Checkpoint failures**: 0

### TASK-2: Art pipeline port

- **Model used**: swe-2-medium
- **Estimated tokens**: ~40k in / ~15k out
- **Actual tokens**: not tracked
- **Session count**: 1
- **Checkpoint failures**: 0

### TASK-3: Repo tools + README

- **Model used**: swe-2-medium
- **Estimated tokens**: ~40k in / ~15k out
- **Actual tokens**: not tracked
- **Session count**: 1
- **Checkpoint failures**: 0

### TASK-4: Unity project creation + packages

- **Model used**: swe-2-medium
- **Estimated tokens**: ~40k in / ~15k out
- **Actual tokens**: not tracked (2 Unity batch runs, several minutes each on 9P fs)
- **Session count**: 1
- **Checkpoint failures**: 0

### TASK-5: URP 2D pipeline bootstrap

- **Model used**: swe-2-medium
- **Estimated tokens**: ~40k in / ~15k out
- **Actual tokens**: not tracked
- **Session count**: 1
- **Checkpoint failures**: 0

### TASK-6: Test assemblies + sanity test

- **Model used**: swe-2-medium
- **Estimated tokens**: ~40k in / ~15k out
- **Actual tokens**: not tracked (2 extra Unity runs from the `-quit` discovery)
- **Session count**: 1
- **Checkpoint failures**: 0

### REEB-158: Living-painting web demo

- **Model used**: swe-2 (orchestrator, in-session per REEB-157 constraint)
- **Estimated**: ~100-150k orchestrator tokens + 2-6 hero-tier RouteLLM calls
- **Actual**: within estimate — 5 image gens (1 nano_banana_pro scene, 4 recraft), ~4 headless-Chrome screenshot iterations, single session
- **Session count**: 1
- **Notes**: image-gen salvage work (band-crop, luminance→alpha, flood-fill) was the main unbudgeted effort; cheaper than re-rolls
