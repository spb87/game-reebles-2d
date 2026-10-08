# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-153: Q1 — Quest JSON schema + QuestLibrary + errand_berries.json

- **Feature**: REEB-146 (M2: One quest end-to-end — vertical slice)
- **Role**: Developer
- **Files allowed**: `game/Assets/Scripts/Quests/*` (new asmdef `Reebles2D.Quests`, autoReferenced per existing pattern), `game/Assets/Data/Quests/errand_berries.json` (+meta), `game/Assets/Tests/EditMode/QuestSchemaTests.cs` (+ update EditMode asmdef refs)
- **Input**: Quest schema (DEC-6 — data-driven, JsonUtility-serializable, flat): `id`, `giverNpcId`, `title`, `objectiveText`, dialogue arrays `offerLines`/`activeLines`/`completeLines`, `fetchItemId`, `fetchTargetId`, `rewardHearts`. `QuestLibrary`: loads all TextAssets under `Assets/Data/Quests/` (or a `[SerializeField] TextAsset[]` manifest — pick the approach that works in WebGL; Resources folder if streaming assets is awkward — note: plain `Assets/Data/` files are NOT included in builds unless in Resources or referenced — use `Assets/Resources/Quests/` if needed for runtime loading; schema tests just need file parsing). `errand_berries.json`: id `errand_berries`, giver `marla_baker`, item `berries`, target `berry_bush`, reward 1 heart, cozy lines. EditMode tests: every quest JSON parses via JsonUtility; required fields non-empty; rewardHearts > 0.
- **Exit criteria**: EditMode suite green incl. new schema tests; errand_berries.json committed + parseable; asmdef wired.
- **Max new lines**: ~250
- **Status**: in progress
- **Git**: `feat(REEB-153): add quest JSON schema + first errand`
