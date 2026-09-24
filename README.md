# RETURN VECTOR

RETURN VECTOR is a top-down combat prototype built around a single persistent thrown weapon.

The weapon damages on the way out and on recall. Throwing it creates a temporary commitment: the player loses their primary offense, enemies continue moving, and the return path becomes a second attack that can be shaped through positioning and the environment.

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

## Core loop

**Throw → reposition → exploit the line → recall → catch**

The projectile is transform-driven and uses controlled casts rather than Rigidbody propulsion. Surface responses are authored and deterministic so the first interaction is predictable.

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

They cover the deterministic travel math, state machines, surface responses, enemy hit rules, encounter timing, movement helpers, and recall constraints.

## Notes

The project intentionally keeps a single playable scene. Earlier construction scenes are not part of the final project.
