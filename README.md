# RETURN VECTOR

RETURN VECTOR is a top-down combat prototype built around a single persistent thrown weapon.

The weapon damages on the way out and on recall. Throwing it creates a temporary commitment: the player loses their primary offense, enemies continue moving, and the return path becomes a second attack that can be shaped through positioning and the environment.

This project was made in under a week and it still contains some bugs.

## Open the project

- Unity: **6000.0.56f1**
- Main scene: `Assets/Scenes/PrototypeScene.unity`

`PrototypeScene` contains the complete combat sequence, all enemy archetypes, the surface interactions, and the Return Warden encounter.

## Controls

| Action | Keyboard / Mouse | Gamepad |
| --- | --- | --- |
| Move | WASD | Left Stick |
| Aim | Mouse | Right Stick |
| Throw | Left Mouse Button | Right Trigger |
| Recall | Right Mouse Button | Left Trigger |
| Dodge | Space | South Button |
| Restart after victory / death | R | Start |

## Core loop

**Throw → reposition → exploit the line → recall → catch**

The projectile is transform-driven and uses controlled casts rather than Rigidbody propulsion. Surface responses are authored and deterministic so the first interaction is predictable.

Enemies use visible anticipation and recovery windows. The ranged archetype commits to a shot line aimed at the player, while the Return Warden escalates through three phases: an open arena, a compressed solid-walled phase, and a final full-health reinforcement phase.

## Project structure

```text
ReturnVector
├─ Assets
│  ├─ Editor
│  ├─ Input
│  ├─ Materials
│  ├─ Scenes
│  │  └─ PrototypeScene.unity
│  ├─ ScriptableObjects
│  │  ├─ Encounters
│  │  ├─ Surfaces
│  │  └─ Tuning
│  ├─ Scripts
│  ├─ Settings
│  └─ Tests
├─ Packages
└─ ProjectSettings
```

### Naming

- Materials: `M_Name`
- Gameplay ScriptableObjects: `SO_Name`
- Scenes and scripts use descriptive names based on their role.
- Unity render-pipeline/settings assets keep their conventional Unity names.

## Tests

Edit Mode tests live in `Assets/Tests/EditMode`.

They cover deterministic travel math, state machines, surface responses, enemy hit rules, encounter timing, movement helpers, collision placement, and recall constraints.

## Notes

The project keeps a single playable scene. Earlier construction scenes are not part of the final project.
