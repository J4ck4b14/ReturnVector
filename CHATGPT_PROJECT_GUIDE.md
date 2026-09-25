# RETURN VECTOR

This is intentionally a **quick orientation document**, not a replacement for reading the code. The source is heavily commented already; use this file to find the right place to start.

## Project in 30 seconds

* **Engine:** Unity `6000.0.56f1` / Unity 6.
* **Main scene:** `Assets/Scenes/PrototypeScene.unity`.
* **Core loop:** throw the persistent baton → move while disarmed → shape the return line → recall → catch.
* **Weapon simulation:** transform-driven, deterministic swept casts; no Rigidbody-driven projectile motion.
* **Enemies:** Rusher, Shielded, Controller and the multi-phase Return Warden.
* **Navigation:** grid-based A\* is used when direct pursuit is blocked.
* **Difficulty:** Very Easy, Easy, Normal, Hard and Extreme. Extreme keeps Hard-style raw numbers and adds coordinated systemic pressure.
* **Boss progression:** one Warden phase on Very Easy/Easy, two on Normal, three on Hard/Extreme.

## “I want to change X — where do I look?”

|Goal|Start here|
|-|-|
|Change difficulty-wide values|`Assets/Scripts/Core/GameDifficulty.cs`|
|Make the Warden faster / harder|`GameDifficulty.cs`, `ReturnWardenTuning.cs`, `SO\_ReturnWardenTuning.asset`|
|Change Extreme-only enemy behaviour|`Assets/Scripts/Enemies/ExtremeTactics.cs` plus the individual enemy AI scripts|
|Change Warden attacks/phases|`ReturnWardenAI.cs`, `ReturnWardenHealth.cs`, `ReturnWardenTuning.cs`|
|Change Warden arena contraction/minions|`Assets/Scripts/Encounters/ReturnWardenArenaController.cs`|
|Change menus, HUD, score or win/lose flow|`Assets/Scripts/Core/PrototypeFlow.cs`|
|Change enemy pathfinding|`AStarPathfinder.cs` and `EnemyMotor.cs`|
|Change encounter composition|`SO\_Encounter\*.asset`, `EncounterController.cs`, `EncounterEnemyFactory.cs`|
|Change player movement/dodge|`PlayerMov.cs` and `SO\_PlayerMovementTuning.asset`|
|Change throw behaviour|`OutboundWeaponMotor.cs` and `SO\_ThrowTuning.asset`|
|Change recall behaviour|`RecallWeaponMotor.cs` and `SO\_RecallTuning.asset`|
|Change wall/surface behaviour|`Assets/Scripts/Surfaces/\*` and `SO\_Surface\*.asset`|
|Change hit-stop/camera/game feel|`Assets/Scripts/GameFeel/\*` and `SO\_GameFeel.asset`|
|Investigate baton/wall collision|`WeaponCollisionUtility.cs`, `OutboundWeaponMotor.cs`, `RecallWeaponMotor.cs`|

## Folder map

* **`Assets/Editor`** — Editor-only Scene view helpers. None of these scripts ship as gameplay logic.
* **`Assets/Input`** — The Input System action asset used by the player input boundary.
* **`Assets/Materials`** — Runtime/editor visual materials for enemies, surfaces, telegraphs, gates and weapon previews.
* **`Assets/Scenes`** — The playable scene. The project intentionally keeps a single final scene.
* **`Assets/ScriptableObjects/Encounters`** — Authored encounter definitions: phase order, spawn composition and pacing.
* **`Assets/ScriptableObjects/Surfaces`** — Authored weapon/surface behaviours such as reflect, penetrate, curve and absorb.
* **`Assets/ScriptableObjects/Tuning`** — Editable gameplay and presentation numbers for player, weapon, enemies, Warden and game feel.
* **`Assets/Scripts/Combat`** — Shared combat payloads/contracts used by weapons, player and enemies.
* **`Assets/Scripts/Core`** — Run difficulty, menus/HUD, visibility culling and small shared state infrastructure.
* **`Assets/Scripts/Debugging`** — Optional development visualization helpers.
* **`Assets/Scripts/Encounters`** — Encounter lifecycle, spawning, gates, sequence progression, Warden arena logic and camera follow.
* **`Assets/Scripts/Enemies`** — Enemy AI, health, movement, A\* routing, Extreme tactics and Warden logic.
* **`Assets/Scripts/GameFeel`** — Camera, hit-stop, flashes, procedural posing, attack/death feedback and Warden presentation.
* **`Assets/Scripts/Input`** — Input System boundary and aim-source typing.
* **`Assets/Scripts/Player`** — Movement, health, throw/recall commands, combat state and world aiming.
* **`Assets/Scripts/Surfaces`** — Weapon-surface response definitions, resolution and curvature fields.
* **`Assets/Scripts/Weapon`** — Persistent baton lifecycle, outbound/return simulation, collision, pinning and weapon visuals.
* **`Assets/Settings`** — URP/render pipeline and Unity rendering profiles. Mostly engine configuration, not gameplay.
* **`Assets/Shaders`** — Custom Shader Graph assets.
* **`Assets/Tests/EditMode`** — Edit Mode regression tests for deterministic math, navigation, difficulty, boss phases and weapon rules.
* **`Packages`** — Unity package manifest and lockfile.
* **`ProjectSettings`** — Unity project-wide settings such as physics, graphics, quality, tags, input defaults and build scene list.

## Important non-script assets

* **`Assets/Input/ReturnVector.inputactions`** — Input System action map for movement, aim, throw, recall, dodge and related controls.
* **`Assets/Scenes/PrototypeScene.unity`** — The single playable scene containing the complete run and all encounter spaces.
* **`Assets/Shaders/SG\_WardenSecondPhase.shadergraph`** — Custom Shader Graph used by the Warden transformation material.
* **`Assets/Scripts/ReturnVector.Runtime.asmdef`** — Assembly Definition that compiles runtime scripts as `ReturnVector.Runtime` and references the Input System.
* **`Assets/Editor/ReturnVector.Editor.asmdef`** — Editor-only assembly containing Scene view gizmo drawers.
* **`Assets/Tests/EditMode/ReturnVector.Tests.EditMode.asmdef`** — Edit Mode test assembly referencing the runtime assembly and Unity Test Framework.

## Materials

|Material|Purpose|
|-|-|
|`M\_Controller.mat`|Controller ranged-enemy body material.|
|`M\_EncounterGate.mat`|Encounter gate visual material.|
|`M\_EncounterGeometry.mat`|General encounter-room geometry material.|
|`M\_EnemyTelegraph.mat`|Shared enemy attack telegraph material.|
|`M\_RecallPath.mat`|Recall-path / return-line visualization material.|
|`M\_ReturnWarden.mat`|Base Return Warden material used before the transformation.|
|`M\_ReturnWardenTelegraph.mat`|Warden-specific attack/phase telegraph material.|
|`M\_Rusher.mat`|Rusher enemy body material.|
|`M\_Shield.mat`|Shielded enemy body/shield material.|
|`M\_SurfaceAbsorb.mat`|Visual identity for absorbing weapon surfaces.|
|`M\_SurfaceCurve.mat`|Visual identity for curving weapon surfaces.|
|`M\_SurfacePenetrate.mat`|Visual identity for penetrable weapon surfaces.|
|`M\_SurfaceReflect.mat`|Visual identity for reflective weapon surfaces.|
|`M\_ThrowPreview.mat`|Outbound throw-preview material.|
|`M\_WardenSecondPhase.mat`|Phase-two/three Warden material driven by the custom Shader Graph.|

## ScriptableObjects

### Encounters

* **`SO\_EncounterCombinedPressure.asset`** — Encounter definition for the combined-pressure room.
* **`SO\_EncounterReturnLine.asset`** — Encounter definition built around learning/exploiting the return line.
* **`SO\_EncounterReturnWarden.asset`** — Final Return Warden encounter definition.
* **`SO\_EncounterShieldGeometry.asset`** — Encounter definition centered on shield geometry and return angles.
* **`SO\_EncounterSurfaceRouting.asset`** — Encounter definition focused on authored surface routing.

### Surfaces

* **`SO\_SurfaceAbsorbing.asset`** — Surface profile that absorbs/stops weapon momentum according to its authored response.
* **`SO\_SurfaceCurving.asset`** — Surface profile used with curvature behaviour.
* **`SO\_SurfacePenetrable.asset`** — Surface profile that permits the weapon to pass through.
* **`SO\_SurfaceReflective.asset`** — Surface profile that reflects the weapon.

### Tuning / settings

* **`SO\_ControllerTuning.asset`** — Controller movement, spacing, fire cadence and projectile tuning.
* **`SO\_DebugSettings.asset`** — Optional debug colors/toggles for visual diagnostics.
* **`SO\_GameFeel.asset`** — Shared hit-stop, camera impulse, zoom and impact-feedback tuning.
* **`SO\_PlayerMovementTuning.asset`** — Armed/weaponless movement, turning and dodge tuning.
* **`SO\_RecallTuning.asset`** — Recall travel, steering, damage, catch and spin tuning.
* **`SO\_ReturnWardenTuning.asset`** — Warden health, movement, attacks, phase multipliers, shockwave and transition tuning.
* **`SO\_RusherTuning.asset`** — Rusher pursuit, aggression and strike tuning.
* **`SO\_ThrowTuning.asset`** — Outbound baton speed, collision and damage tuning.

## Scripts

### `Assets/Editor`

|Script|Quick responsibility|
|-|-|
|`EncounterGizmoDrawer.cs`|Draws encounter spawn positions and labels in the Scene view.|
|`WeaponSurfaceGizmoDrawer.cs`|Draws surface orientation and curvature helpers in the Scene view.|

### `Assets/Scripts/Combat`

|Script|Quick responsibility|
|-|-|
|`AttackPhase.cs`|Identifies which leg of the weapon cycle produced a hit.|
|`DamageInfo.cs`|Data passed to a combat target when a hit is resolved. Keeping the phase explicit lets enemies react differently to outbound and recall hits.|
|`IDamageable.cs`|Minimal damage contract shared by player, enemies and debug targets.|
|`IWeaponHitReceiver.cs`|Optional richer combat contract for targets whose armor or facing changes how the weapon itself should respond.|
|`WeaponHitResult.cs`|Describes how a target changes weapon travel when resolving a hit.|

### `Assets/Scripts/Core`

|Script|Quick responsibility|
|-|-|
|`GameDifficulty.cs`|Shared run difficulty values. Hard preserves the authored combat values; Extreme keeps those values and adds coordinated systemic pressure.|
|`PrototypeFlow.cs`|Owns the menu, run HUD, fail state, victory summary and scene-level flow.|
|`PrototypeVisibilityCulling.cs`|Groups encounter-room renderers behind an explicit camera-frustum gate. Physics and gameplay objects remain active while off-screen rendering is suppressed.|

### `Assets/Scripts/Core/StateMachine`

|Script|Quick responsibility|
|-|-|
|`StateMachine.cs`|Small explicit state container used by gameplay-specific owners.|

### `Assets/Scripts/Debugging`

|Script|Quick responsibility|
|-|-|
|`RVDebugDraw.cs`|Small wrapper for development-only world debug drawing.|
|`RVDebugSettings.cs`|Centralizes optional diagnostic drawing and overlay settings.|

### `Assets/Scripts/Encounters`

|Script|Quick responsibility|
|-|-|
|`EncounterController.cs`|Runs one authored encounter from activation through its final phase.|
|`EncounterDebugOverlay.cs`|Displays live encounter state while testing the prototype.|
|`EncounterDefinition.cs`|Authored phase data for a combat encounter.|
|`EncounterEnemyFactory.cs`|Builds the prototype enemy archetypes from shared runtime dependencies.|
|`EncounterGate.cs`|Controls the physical gate used to contain or release an encounter.|
|`EncounterPhaseDefinition.cs`|Defines the spawn list and timing constraints for one encounter phase.|
|`EncounterSequenceDirector.cs`|Tracks progress across the ordered encounter chain.|
|`EncounterSpawnEntry.cs`|Serialized spawn instruction used by an encounter phase.|
|`EncounterState.cs`|Lifecycle states for an encounter.|
|`EncounterTimelineMath.cs`|Pure timing helpers used by encounter progression.|
|`EncounterTrigger.cs`|Starts an encounter when the player crosses its trigger.|
|`ReturnWardenArenaController.cs`|Owns the Warden arena layout changes and the phase-three reinforcement waves.|
|`TopDownCameraFollow.cs`|Smooth top-down follow camera with additive combat feedback offsets.|

### `Assets/Scripts/Enemies`

|Script|Quick responsibility|
|-|-|
|`AStarPathfinder.cs`|Small grid A\* solver used by enemy movement when a straight route is blocked.|
|`ControllerEnemyAI.cs`|Holds distance, commits a visible shot line, then fires at the player. Fire cadence increases while the player is weaponless.|
|`ControllerEnemyTuning.cs`|Movement and ranged-attack values for the Controller.|
|`EnemyArchetype.cs`|Prototype enemy families understood by the encounter factory.|
|`EnemyHealth.cs`|Base enemy health and weapon-hit handling shared by standard archetypes.|
|`EnemyLifeState.cs`|Basic alive/dead state for enemy actors.|
|`EnemyMotor.cs`|Planar enemy locomotion with A\* routing around blocking geometry.|
|`EnemyProjectile.cs`|Fixed-line ranged projectile with swept collision against the player and environment.|
|`ExtremeTactics.cs`|Shared Extreme-mode reads for the temporary return corridor created by a throw.|
|`ReturnWardenAI.cs`|Return Warden behaviour. Distance and phase select its telegraphed attack vocabulary.|
|`ReturnWardenHealth.cs`|Return Warden hit rules, phase thresholds and recall relationship.|
|`ReturnWardenState.cs`|High-level behaviour states and attack vocabulary for the Return Warden.|
|`ReturnWardenTuning.cs`|Movement, attack and phase values for the Return Warden.|
|`RusherEnemyAI.cs`|Closes distance faster while the player is weaponless and resolves a telegraphed short-range strike.|
|`RusherEnemyState.cs`|Behaviour states used by the Rusher.|
|`RusherEnemyTuning.cs`|Movement and attack values for the Rusher.|
|`ShieldedEnemyAI.cs`|Keeps its shield toward the player and uses a readable shove when the player stays close.|
|`ShieldedEnemyHealth.cs`|Configures the Shielded enemy health rules and shield facing source.|

### `Assets/Scripts/GameFeel`

|Script|Quick responsibility|
|-|-|
|`EnemyAttackFeedback.cs`|Procedural attack pose and telegraph shared by the prototype enemies. The gameplay root stays untouched so CharacterController dimensions remain stable.|
|`EnemyDeathFeedback.cs`|Plays the enemy fall and leaves a flat material-matched splatter on the floor.|
|`EnemyHitFeedback.cs`|Applies phase-sensitive hit feedback to an enemy renderer.|
|`PlayerDamageFeedback.cs`|Routes player damage into short renderer feedback.|
|`PlayerProceduralPose.cs`|Adds lightweight pose offsets for movement, throw, dodge and catch beats.|
|`RVCameraFeedback.cs`|Accumulates short camera impulses and framing changes from combat events.|
|`RVCombatFeedbackDirector.cs`|Coordinates hit-stop, camera response and visual feedback from gameplay events.|
|`RVGameFeelMath.cs`|Pure easing and damping helpers used by presentation systems.|
|`RVGameFeelProfile.cs`|Shared tuning profile for hit-stop, camera and impact feedback.|
|`RVHitStopController.cs`|Owns brief global time-scale punches used on high-value impacts.|
|`RVRendererFlash.cs`|Short MaterialPropertyBlock flash used for impact feedback.|
|`ReturnWardenPhaseFeedback.cs`|Drives Warden transformations, shockwaves and the final death beat.|
|`WeaponFeedbackVisual.cs`|Applies presentation-only compression, flash and catch tension to the weapon visual.|

### `Assets/Scripts/Input`

|Script|Quick responsibility|
|-|-|
|`AimInputKind.cs`|Distinguishes pointer aim from directional-stick aim.|
|`RVInputReader.cs`|Thin input boundary for gameplay code. It keeps concrete Input System assets out of combat and weapon classes.|

### `Assets/Scripts/Player`

|Script|Quick responsibility|
|-|-|
|`PlayerCombatController.cs`|Tracks whether the player currently owns their primary offensive tool. Later enemy aggression and animation can key off this state.|
|`PlayerCombatMode.cs`|Tracks whether the player currently has access to the weapon.|
|`PlayerHealth.cs`|Tracks persistent player health and publishes damage/death events.|
|`PlayerMov.cs`|Top-down locomotion and evasive dodge movement. Weapon ownership selects the movement and dodge tuning used by the same input.|
|`PlayerMovementMath.cs`|Pure movement and dodge calculations used by PlayerMov.|
|`PlayerMovementState.cs`|Movement states exposed to gameplay and animation.|
|`PlayerMovementTuning.cs`|Movement, turn and dodge values for armed and weaponless states.|
|`PlayerRecallController.cs`|Converts recall input into the explicit Returning state and hands motion to RecallWeaponMotor. Recall may begin while the weapon is outbound, parked or embedded.|
|`PlayerTacticalSnapshot.cs`|Read-only combat and movement state exposed to AI and encounter systems.|
|`PlayerTacticalStateSource.cs`|Consolidates weapon ownership and movement state into a stable read model. Provides a stable read model for enemy AI and encounter logic.|
|`PlayerThrowController.cs`|Owns the player's outbound throw command and anticipation window. Aim remains live during anticipation; the direction is committed on release.|
|`WorldAimProvider.cs`|Converts pointer or gamepad aim into a flat world-space direction. Mouse aim is projected onto a horizontal plane passing through the supplied origin.|

### `Assets/Scripts/Surfaces`

|Script|Quick responsibility|
|-|-|
|`WeaponCurvatureField.cs`|Authored trigger volume that continuously steers weapon travel direction. Its behavior is deterministic: same entry direction, position and timestep produce the same turn.|
|`WeaponCurvatureMode.cs`|Available direction models for a curvature field.|
|`WeaponCurvatureUtility.cs`|Finds active curvature fields and applies their steering in a stable order.|
|`WeaponSurface.cs`|Binds a collider to an authored weapon-surface profile.|
|`WeaponSurfaceInteractionInfo.cs`|Snapshot of one resolved surface interaction for feedback and diagnostics.|
|`WeaponSurfaceKind.cs`|Surface behaviours understood by weapon travel.|
|`WeaponSurfaceMath.cs`|Pure reflection and steering helpers for authored surface responses.|
|`WeaponSurfaceProfile.cs`|Authored response data for outbound and recall interaction with a surface.|
|`WeaponSurfaceResolver.cs`|Resolves the effective surface response for a collider and attack phase.|
|`WeaponSurfaceResponse.cs`|Resolved response values consumed by the weapon motors.|

### `Assets/Scripts/Weapon`

|Script|Quick responsibility|
|-|-|
|`FirstCollisionPreview.cs`|Shows the straight outbound segment up to the first relevant interaction.|
|`OutboundTravelMath.cs`|Pure distance and speed helpers for outbound weapon simulation.|
|`OutboundWeaponMotor.cs`|Controlled outbound projectile simulation. Travel uses fixed-step swept casts so collision response and surface routing stay predictable.|
|`RecallTravelMath.cs`|Pure steering, acceleration and catch helpers for recall travel.|
|`RecallWeaponMotor.cs`|Controlled return simulation. The weapon continuously steers toward the live catch point, then authored surface rules may redirect, pass or stop that return.|
|`WeaponCollisionUtility.cs`|Shared collision filtering and component lookup for both weapon travel phases.|
|`WeaponController.cs`|Runtime owner of the weapon lifecycle. Motion systems are separate and may only act while the corresponding explicit state is active.|
|`WeaponImpactInfo.cs`|Common impact payload published by outbound and recall motors.|
|`WeaponPinVisual.cs`|Shows the Warden pin around the resting weapon and its release progress.|
|`WeaponRecallConstraint.cs`|Temporary recall lock that releases after sufficient player displacement or its fail-safe window.|
|`WeaponRecallConstraintMath.cs`|Pure release test for distance-based recall constraints.|
|`WeaponRecallTuning.cs`|Simulation, steering, damage and catch values for recall travel.|
|`WeaponSpinVisual.cs`|Cosmetic spin applied to the weapon visual child during travel.|
|`WeaponState.cs`|Explicit lifecycle states for the persistent weapon.|
|`WeaponStateMachine.cs`|Owns the legal transitions across the weapon lifecycle.|
|`WeaponThrowTuning.cs`|Simulation, collision and damage values for outbound travel.|

### `Assets/Tests/EditMode`

|Script|Quick responsibility|
|-|-|
|`AStarPathfinderTests.cs`|Regression coverage for `AStarPathfinder`.|
|`EncounterTimelineMathTests.cs`|Regression coverage for `EncounterTimelineMath`.|
|`GameDifficultyTests.cs`|Regression coverage for `GameDifficulty`.|
|`OutboundTravelMathTests.cs`|Regression coverage for `OutboundTravelMath`.|
|`PlayerMovementMathTests.cs`|Regression coverage for `PlayerMovementMath`.|
|`PlayerTacticalSnapshotTests.cs`|Regression coverage for `PlayerTacticalSnapshot`.|
|`RVGameFeelMathTests.cs`|Regression coverage for `RVGameFeelMath`.|
|`RecallTravelMathTests.cs`|Regression coverage for `RecallTravelMath`.|
|`ReturnWardenHealthTests.cs`|Regression coverage for `ReturnWardenHealth`.|
|`ShieldedEnemyHealthTests.cs`|Regression coverage for `ShieldedEnemyHealth`.|
|`WeaponCollisionUtilityTests.cs`|Regression coverage for `WeaponCollisionUtility`.|
|`WeaponHitResultTests.cs`|Regression coverage for `WeaponHitResult`.|
|`WeaponRecallConstraintMathTests.cs`|Regression coverage for `WeaponRecallConstraintMath`.|
|`WeaponStateMachineTests.cs`|Regression coverage for `WeaponStateMachine`.|
|`WeaponSurfaceMathTests.cs`|Regression coverage for `WeaponSurfaceMath`.|

## Render/settings assets

These are conventional Unity/URP assets rather than game-rule assets:

* `Assets/Settings/DefaultVolumeProfile.asset`
* `Assets/Settings/Mobile\_RPAsset.asset`
* `Assets/Settings/Mobile\_Renderer.asset`
* `Assets/Settings/PC\_RPAsset.asset`
* `Assets/Settings/PC\_Renderer.asset`
* `Assets/Settings/SampleSceneProfile.asset`
* `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`

The `ProjectSettings` folder contains Unity-wide configuration such as physics, graphics, tags/layers, quality, build settings and project version. In normal maintenance, change those through the Unity editor unless there is a specific reason to edit YAML directly.

## Packages worth knowing about

* **Input System** `1.14.2` — player controls.
* **Universal Render Pipeline** `17.0.4` — rendering.
* **Unity Test Framework** `1.5.1` — Edit Mode regression tests.
* **AI Navigation** `2.0.8` is installed, although RETURN VECTOR’s active enemy routing is handled by its own lightweight grid A\* implementation.
* Rider / Visual Studio integrations and standard Unity modules are also present.

## Tests

All current automated coverage lives in `Assets/Tests/EditMode`. The suite focuses on pure or deterministic pieces that are valuable to protect during tuning: movement math, weapon travel, collision placement, surface math, recall constraints, difficulty rules, A\*, shield behaviour and Warden phase transitions.

## Practical maintenance notes

1. **Tune data before rewriting systems.** A large amount of combat feel lives in `SO\_\*` assets and `GameDifficulty.cs`.
2. **Keep Hard as the authored baseline.** Extreme is mainly differentiated by systemic coordination rather than simple stat inflation.
3. **Preserve the weapon lifecycle.** `Held → Outbound/Resting → Returning → Held` is central to several systems.
4. **Respect authored surfaces.** Not every wall is meant to behave like ordinary blocking geometry.
5. **Run Edit Mode tests after touching deterministic math or phase rules.** Those are the easiest changes to accidentally break in subtle ways.
6. **The Warden is spread across concerns on purpose:** AI chooses/executes attacks, Health owns phase thresholds, Tuning owns numbers, PhaseFeedback owns presentation, and ArenaController owns space/reinforcements.

