# Feature: Greybox Scene + Player Movement

**Epic**: `reebles-2d-mvp.md` · Milestone: M1 · Tracking: local (`TASK-7` … `TASK-10`)

## Goal

A generated greybox `Village.unity` scene exists where the Reeble walks/runs with keyboard and touch controls, a Cinemachine 2D camera follows within map bounds, and placeholder colliders block movement.

## Acceptance criteria

1. `ReeblesInput.inputactions` defines Move (WASD/arrows + gamepad stick + on-screen stick), Run (shift + on-screen or auto), Interact (E + tap button)
2. `PlayerMovement` moves a kinematic `Rigidbody2D` via `MovePosition` in `FixedUpdate`; walk/run speeds are `[SerializeField]` (DEC-4)
3. `VillageSceneBuilder.Build` (single entry point, `-executeMethod`) regenerates `Village.unity`: ground area, 5 building footprints with colliders, fountain at plaza center, perimeter fence colliders (visible boundary), Player prefab instance, Cinemachine 2D follow camera
4. PlayMode tests green: player translates under simulated input; player is blocked by a collider; on-screen controls exist and feed the action map
5. EditMode tests green: movement math (run multiplier, diagonal normalization) has unit coverage
6. Zero compile errors/warnings in the editor log

## Known constraints

- Scenes/prefabs are **code-generated**, never hand-edited (conventions.md "Scene generation") — greybox sprites use `Sprite` created from `Texture2D` at runtime/editor-time (plain colored rects are fine — art is M3)
- All tunables `[SerializeField]` — inspector-tunable rule (scope handoff notes)
- Input System active already (TASK-4); do NOT reference legacy `UnityEngine.Input`
- C# 9, block-scoped namespaces, `Reebles2D.*` naming (conventions.md)
- Design guidance: `.ai/design/brief.md` — greybox palette only, no art expectations this milestone

## Out of scope

- NPCs, quests, dialogue, HUD beyond the mobile-controls canvas
- Generated art (M3), sprite animation beyond a placeholder
- Web build + hosting (F3)

## Tasks

- TASK-7: Input actions asset + `MovementMath` + `PlayerMovement`
- TASK-8: `VillageSceneBuilder` — greybox scene, player prefab, colliders, Cinemachine camera
- TASK-9: EditMode + PlayMode movement/collision tests
- TASK-10: Mobile on-screen controls (joystick + interact button) folded into the scene builder

## Status

- [x] Spec complete
- [x] Tasks decomposed
- [x] Cost estimated
- [ ] All tasks done
- [ ] Feature verified
