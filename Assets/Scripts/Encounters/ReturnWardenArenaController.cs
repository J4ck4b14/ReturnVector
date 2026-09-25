using System.Collections;
using System.Collections.Generic;
using ReturnVector.Core;
using ReturnVector.Enemies;
using ReturnVector.GameFeel;
using ReturnVector.Surfaces;
using UnityEngine;

// Script summary: Owns the Warden arena layout changes and the phase-three reinforcement waves.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Owns the Warden arena layout changes and the phase-three reinforcement waves.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReturnWardenArenaController : MonoBehaviour
    {
        // Warden arena variables
        private const float PhaseOneHalfWidth = 9.6f;
        private const float PhaseOneHalfDepth = 6f;
        private const float PhaseTwoHalfWidth = 6.15f;
        private const float PhaseTwoHalfDepth = 4.15f;
        private const float CombatantInset = 1.05f;
        private const float MinimumBossPlayerSeparation = 3.8f;

        // Runtime reference variables
        [SerializeField] private ReturnWardenHealth health;
        [SerializeField] private EncounterEnemyFactory factory;
        [SerializeField] private EncounterController encounter;
        [SerializeField] private Transform arenaRoot;
        [SerializeField] private Transform enemyParent;
        [SerializeField] private Transform player;
        [SerializeField] private RVCameraFeedback cameraFeedback;

        // Surface variables
        [SerializeField] private WeaponSurfaceProfile reflectiveProfile;
        [SerializeField] private WeaponSurfaceProfile penetrableProfile;
        [SerializeField] private WeaponSurfaceProfile absorbingProfile;
        [SerializeField] private WeaponSurfaceProfile curvingProfile;

        // Material variables
        [SerializeField] private Material reflectiveMaterial;
        [SerializeField] private Material penetrableMaterial;
        [SerializeField] private Material absorbingMaterial;
        [SerializeField] private Material curvingMaterial;
        [SerializeField] private Material solidMaterial;
        [SerializeField] private Material spawnTelegraphMaterial;

        // Arena state variables
        private readonly List<GameObject> phaseOneObjects =
            new List<GameObject>(12);

        private readonly List<GameObject> phaseTwoWalls =
            new List<GameObject>(4);

        private readonly List<EnemyHealth> phaseThreeMinions =
            new List<EnemyHealth>(12);

        // Runtime state variables
        private Vector3 arenaCenter;
        private bool phaseTwoStarted;
        private bool phaseThreeStarted;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            ReturnWardenHealth newHealth,
            EncounterEnemyFactory newFactory,
            EncounterController newEncounter,
            Transform newArenaRoot,
            Transform newEnemyParent,
            Transform newPlayer,
            RVCameraFeedback newCameraFeedback,
            WeaponSurfaceProfile newReflectiveProfile,
            WeaponSurfaceProfile newPenetrableProfile,
            WeaponSurfaceProfile newAbsorbingProfile,
            WeaponSurfaceProfile newCurvingProfile,
            Material newReflectiveMaterial,
            Material newPenetrableMaterial,
            Material newAbsorbingMaterial,
            Material newCurvingMaterial,
            Material newSolidMaterial,
            Material newSpawnTelegraphMaterial)
        {
            Unsubscribe();

            health = newHealth;
            factory = newFactory;
            encounter = newEncounter;
            arenaRoot = newArenaRoot;
            enemyParent = newEnemyParent;
            player = newPlayer;
            cameraFeedback = newCameraFeedback;

            reflectiveProfile = newReflectiveProfile;
            penetrableProfile = newPenetrableProfile;
            absorbingProfile = newAbsorbingProfile;
            curvingProfile = newCurvingProfile;

            reflectiveMaterial = newReflectiveMaterial;
            penetrableMaterial = newPenetrableMaterial;
            absorbingMaterial = newAbsorbingMaterial;
            curvingMaterial = newCurvingMaterial;
            solidMaterial = newSolidMaterial;
            spawnTelegraphMaterial = newSpawnTelegraphMaterial;

            arenaCenter =
                arenaRoot != null
                    ? arenaRoot.position
                    : transform.position;

            BuildPhaseOneLayout();
            Subscribe();
        }

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Subscribes to the runtime events used by this component.
        /// </summary>
        private void Subscribe()
        {
            if (health == null)
            {
                return;
            }

            health.PhaseTwoStarted -= HandlePhaseTwoStarted;
            health.PhaseTwoStarted += HandlePhaseTwoStarted;

            health.PhaseThreeStarted -= HandlePhaseThreeStarted;
            health.PhaseThreeStarted += HandlePhaseThreeStarted;

            health.Died -= HandleBossDeath;
            health.Died += HandleBossDeath;
        }

        /// <summary>
        /// Unsubscribes from the runtime events used by this component.
        /// </summary>
        private void Unsubscribe()
        {
            if (health == null)
            {
                return;
            }

            health.PhaseTwoStarted -= HandlePhaseTwoStarted;
            health.PhaseThreeStarted -= HandlePhaseThreeStarted;
            health.Died -= HandleBossDeath;
        }

        /// <summary>
        /// Builds the phase one layout.
        /// </summary>
        private void BuildPhaseOneLayout()
        {
            if (arenaRoot == null)
            {
                return;
            }

            // The boss room begins with a genuinely open centre. Existing pieces are
            // pushed toward the perimeter so the first phase has room to teach the fight.
            MoveExistingPiece(
                "Boss_ReflectBank_Left",
                new Vector3(-8.25f, 0.75f, 1.55f),
                new Vector3(0.45f, 1.5f, 3.35f));

            MoveExistingPiece(
                "Boss_ReflectBank_Right",
                new Vector3(8.25f, 0.75f, 1.55f),
                new Vector3(0.45f, 1.5f, 3.35f));

            MoveExistingPiece(
                "Boss_Cover_Left",
                new Vector3(-7.85f, 0.8f, -2.85f),
                new Vector3(1.05f, 1.6f, 1.35f));

            MoveExistingPiece(
                "Boss_Cover_Right",
                new Vector3(7.85f, 0.8f, -2.85f),
                new Vector3(1.05f, 1.6f, 1.35f));

            // Phase one is wider than the original boss room and has visible side bounds.
            phaseOneObjects.Add(
                CreateSolidWall(
                    arenaRoot,
                    "Boss_Phase1_Boundary_Left",
                    new Vector3(-PhaseOneHalfWidth, 0.8f, 0f),
                    new Vector3(0.45f, 1.6f, PhaseOneHalfDepth * 2f)));

            phaseOneObjects.Add(
                CreateSolidWall(
                    arenaRoot,
                    "Boss_Phase1_Boundary_Right",
                    new Vector3(PhaseOneHalfWidth, 0.8f, 0f),
                    new Vector3(0.45f, 1.6f, PhaseOneHalfDepth * 2f)));

            // Surface vocabulary stays near the edges instead of cluttering the centre.
            phaseOneObjects.Add(
                CreateSurfaceWall(
                    arenaRoot,
                    "Boss_Phase1_Penetrable",
                    new Vector3(-4.65f, 0.78f, 4.85f),
                    new Vector3(3.2f, 1.55f, 0.42f),
                    Quaternion.identity,
                    penetrableProfile,
                    penetrableMaterial));

            phaseOneObjects.Add(
                CreateSurfaceWall(
                    arenaRoot,
                    "Boss_Phase1_Absorbing",
                    new Vector3(4.65f, 0.78f, 4.85f),
                    new Vector3(3.2f, 1.55f, 0.42f),
                    Quaternion.identity,
                    absorbingProfile,
                    absorbingMaterial));

            GameObject curving =
                CreateSurfaceWall(
                    arenaRoot,
                    "Boss_Phase1_Curving",
                    new Vector3(5.15f, 0.78f, -4.55f),
                    new Vector3(3f, 1.55f, 0.42f),
                    Quaternion.Euler(0f, -10f, 0f),
                    curvingProfile,
                    curvingMaterial);

            phaseOneObjects.Add(curving);
            AddCurvatureField(curving);

            // The original reflective banks supply the fourth authored wall type.
            ExpandEntranceAndExitGates();
            cameraFeedback?.SetArenaFraming(10.6f, true);
        }

        /// <summary>
        /// Moves the boss-room gates outward for the wider first phase.
        /// </summary>
        private void ExpandEntranceAndExitGates()
        {
            if (encounter == null || arenaRoot == null)
            {
                return;
            }

            EncounterGate entrance = encounter.EntranceGate;
            EncounterGate exit = encounter.ExitGate;

            if (entrance != null)
            {
                Vector3 position = entrance.transform.position;
                position.z = arenaCenter.z - PhaseOneHalfDepth;
                entrance.transform.position = position;
            }

            if (exit != null)
            {
                Vector3 position = exit.transform.position;
                position.z = arenaCenter.z + PhaseOneHalfDepth;
                exit.transform.position = position;
            }
        }

        /// <summary>
        /// Responds when Warden phase two begins.
        /// </summary>
        private void HandlePhaseTwoStarted()
        {
            if (phaseTwoStarted)
            {
                return;
            }

            phaseTwoStarted = true;
            StartCoroutine(CloseArenaForPhaseTwo());
        }

        /// <summary>
        /// Responds when Warden phase three begins.
        /// </summary>
        private void HandlePhaseThreeStarted()
        {
            if (phaseThreeStarted)
            {
                return;
            }

            phaseThreeStarted = true;
            StartCoroutine(RunPhaseThreeReinforcements());
        }

        /// <summary>
        /// Animates the boss-room perimeter inward for phase two.
        /// </summary>
        private IEnumerator CloseArenaForPhaseTwo()
        {
            SolidifyAuthoredSurfaces();
            CreatePhaseTwoPerimeter();
            EnsureCombatantsInsidePhaseTwoBounds();

            cameraFeedback?.SetArenaFraming(8.9f, false);
            cameraFeedback?.Impulse(
                0.52f,
                1.35f,
                0.24f);

            const float duration = 1.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t =
                    Mathf.Clamp01(
                        elapsed / duration);

                float eased =
                    1f -
                    Mathf.Pow(1f - t, 3f);

                for (int i = 0; i < phaseTwoWalls.Count; i++)
                {
                    GameObject wall = phaseTwoWalls[i];
                    if (wall == null)
                    {
                        continue;
                    }

                    Vector3 position = wall.transform.localPosition;
                    position.y = Mathf.Lerp(-2.2f, 0.8f, eased);
                    wall.transform.localPosition = position;
                }

                // The perimeter is still non-collidable while it rises. Keep both
                // combatants inside the future bounds so nobody can be sealed out.
                EnsureCombatantsInsidePhaseTwoBounds();
                yield return null;
            }

            EnsureCombatantsInsidePhaseTwoBounds();

            for (int i = 0; i < phaseTwoWalls.Count; i++)
            {
                GameObject wall = phaseTwoWalls[i];
                if (wall == null)
                {
                    continue;
                }

                Collider collider = wall.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }

        /// <summary>
        /// Creates the phase two perimeter.
        /// </summary>
        private void CreatePhaseTwoPerimeter()
        {
            if (arenaRoot == null)
            {
                return;
            }

            phaseTwoWalls.Add(
                CreateRisingSolidWall(
                    "Boss_Phase2_Boundary_Left",
                    new Vector3(-PhaseTwoHalfWidth, -2.2f, 0f),
                    new Vector3(0.55f, 1.6f, PhaseTwoHalfDepth * 2f)));

            phaseTwoWalls.Add(
                CreateRisingSolidWall(
                    "Boss_Phase2_Boundary_Right",
                    new Vector3(PhaseTwoHalfWidth, -2.2f, 0f),
                    new Vector3(0.55f, 1.6f, PhaseTwoHalfDepth * 2f)));

            phaseTwoWalls.Add(
                CreateRisingSolidWall(
                    "Boss_Phase2_Boundary_North",
                    new Vector3(0f, -2.2f, PhaseTwoHalfDepth),
                    new Vector3(PhaseTwoHalfWidth * 2f, 1.6f, 0.55f)));

            phaseTwoWalls.Add(
                CreateRisingSolidWall(
                    "Boss_Phase2_Boundary_South",
                    new Vector3(0f, -2.2f, -PhaseTwoHalfDepth),
                    new Vector3(PhaseTwoHalfWidth * 2f, 1.6f, 0.55f)));
        }

        /// <summary>
        /// Creates the rising solid wall.
        /// </summary>
        private GameObject CreateRisingSolidWall(
            string wallName,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject wall =
                CreateSolidWall(
                    arenaRoot,
                    wallName,
                    localPosition,
                    localScale);

            Collider collider = wall.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            return wall;
        }

        /// <summary>
        /// Keeps the player and Warden inside the phase-two playable bounds before the walls close.
        /// </summary>
        private void EnsureCombatantsInsidePhaseTwoBounds()
        {
            if (arenaRoot == null)
            {
                return;
            }

            float safeHalfWidth =
                PhaseTwoHalfWidth - CombatantInset;

            float safeHalfDepth =
                PhaseTwoHalfDepth - CombatantInset;

            ClampActorInside(
                player,
                safeHalfWidth,
                safeHalfDepth);

            ClampActorInside(
                transform,
                safeHalfWidth,
                safeHalfDepth);

            if (player == null)
            {
                return;
            }

            Vector3 bossFlat = transform.position;
            Vector3 playerFlat = player.position;
            bossFlat.y = 0f;
            playerFlat.y = 0f;

            if (Vector3.Distance(bossFlat, playerFlat) >=
                MinimumBossPlayerSeparation)
            {
                return;
            }

            // If the contraction catches both combatants in the same pocket, move only
            // the Warden to the safest edge. The roar masks this corrective repositioning.
            Vector3 playerLocal =
                arenaRoot.InverseTransformPoint(player.position);

            Vector3[] candidates =
            {
                new Vector3(-safeHalfWidth, 0f, 0f),
                new Vector3(safeHalfWidth, 0f, 0f),
                new Vector3(0f, 0f, -safeHalfDepth),
                new Vector3(0f, 0f, safeHalfDepth)
            };

            Vector3 best = candidates[0];
            float bestDistance = -1f;

            for (int i = 0; i < candidates.Length; i++)
            {
                Vector3 delta = candidates[i] - playerLocal;
                delta.y = 0f;
                float distance = delta.sqrMagnitude;

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = candidates[i];
                }
            }

            Vector3 current = arenaRoot.InverseTransformPoint(transform.position);
            best.y = current.y;

            CharacterController bossController =
                GetComponent<CharacterController>();

            bool restoreBossController =
                bossController != null && bossController.enabled;

            if (restoreBossController)
            {
                bossController.enabled = false;
            }

            transform.position = arenaRoot.TransformPoint(best);

            if (restoreBossController)
            {
                bossController.enabled = true;
            }
        }

        /// <summary>
        /// Clamps the actor inside to its valid range.
        /// </summary>
        private void ClampActorInside(
            Transform actor,
            float halfWidth,
            float halfDepth)
        {
            if (actor == null || arenaRoot == null)
            {
                return;
            }

            Vector3 local =
                arenaRoot.InverseTransformPoint(actor.position);

            Vector3 clamped = local;
            clamped.x = Mathf.Clamp(clamped.x, -halfWidth, halfWidth);
            clamped.z = Mathf.Clamp(clamped.z, -halfDepth, halfDepth);

            if ((clamped - local).sqrMagnitude < 0.0001f)
            {
                return;
            }

            CharacterController controller =
                actor.GetComponent<CharacterController>();

            bool restoreController =
                controller != null && controller.enabled;

            if (restoreController)
            {
                controller.enabled = false;
            }

            actor.position =
                arenaRoot.TransformPoint(clamped);

            if (restoreController)
            {
                controller.enabled = true;
            }
        }

        /// <summary>
        /// Converts the boss-room special surfaces into ordinary solid blockers for phase two.
        /// </summary>
        private void SolidifyAuthoredSurfaces()
        {
            if (arenaRoot == null)
            {
                return;
            }

            WeaponSurface[] surfaces =
                arenaRoot.GetComponentsInChildren<WeaponSurface>(true);

            for (int i = 0; i < surfaces.Length; i++)
            {
                WeaponSurface surface = surfaces[i];
                if (surface == null)
                {
                    continue;
                }

                surface.Configure(null);

                Renderer renderer = surface.GetComponent<Renderer>();
                if (renderer != null && solidMaterial != null)
                {
                    renderer.sharedMaterial = solidMaterial;
                }
            }

            WeaponCurvatureField[] fields =
                arenaRoot.GetComponentsInChildren<WeaponCurvatureField>(true);

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] != null)
                {
                    fields[i].enabled = false;
                }
            }
        }

        /// <summary>
        /// Runs the staggered phase-three reinforcement sequence.
        /// </summary>
        private IEnumerator RunPhaseThreeReinforcements()
        {
            cameraFeedback?.Impulse(
                0.62f,
                1.2f,
                0.2f);

            // The first warning arrives while the Warden is still transforming.
            yield return new WaitForSeconds(1.05f);

            if (GameDifficulty.IsExtreme)
            {
                yield return SpawnWave(
                    new[]
                    {
                        EnemyArchetype.Rusher,
                        EnemyArchetype.Rusher,
                        EnemyArchetype.Controller
                    },
                    0.82f);

                yield return WaitForWaveWindow(1, 3.0f);

                yield return SpawnWave(
                    new[]
                    {
                        EnemyArchetype.Shielded,
                        EnemyArchetype.Controller,
                        EnemyArchetype.Rusher,
                        EnemyArchetype.Rusher
                    },
                    0.72f);

                yield return WaitForWaveWindow(2, 3.6f);

                yield return SpawnWave(
                    new[]
                    {
                        EnemyArchetype.Shielded,
                        EnemyArchetype.Shielded,
                        EnemyArchetype.Controller,
                        EnemyArchetype.Controller
                    },
                    0.62f);

                yield break;
            }

            yield return SpawnWave(
                new[]
                {
                    EnemyArchetype.Rusher,
                    EnemyArchetype.Shielded,
                    EnemyArchetype.Controller
                },
                0.82f);

            yield return WaitForWaveWindow(1, 3.2f);

            yield return SpawnWave(
                new[]
                {
                    EnemyArchetype.Controller,
                    EnemyArchetype.Rusher,
                    EnemyArchetype.Shielded
                },
                0.72f);

            yield return WaitForWaveWindow(2, 4f);

            yield return SpawnWave(
                new[]
                {
                    EnemyArchetype.Rusher,
                    EnemyArchetype.Controller,
                    EnemyArchetype.Shielded,
                    EnemyArchetype.Rusher
                },
                0.62f);
        }

        /// <summary>
        /// Waits for reinforcement pressure to ease or for the wave timeout to expire.
        /// </summary>
        private IEnumerator WaitForWaveWindow(
            int desiredAlive,
            float maximumWait)
        {
            float elapsed = 0f;

            while (elapsed < maximumWait)
            {
                if (health == null || !health.CanReceiveDamage)
                {
                    yield break;
                }

                if (CountLivingPhaseThreeMinions() <= desiredAlive)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Spawns the wave.
        /// </summary>
        private IEnumerator SpawnWave(
            EnemyArchetype[] archetypes,
            float warningDuration)
        {
            if (archetypes == null || archetypes.Length == 0)
            {
                yield break;
            }

            List<Vector3> positions =
                SelectSpawnPositions(archetypes.Length);

            List<GameObject> warnings =
                CreateSpawnWarnings(positions);

            yield return new WaitForSeconds(warningDuration);

            for (int i = 0; i < warnings.Count; i++)
            {
                if (warnings[i] != null)
                {
                    Destroy(warnings[i]);
                }
            }

            for (int i = 0; i < archetypes.Length; i++)
            {
                if (factory == null ||
                    health == null ||
                    !health.CanReceiveDamage)
                {
                    yield break;
                }

                EncounterSpawnEntry entry =
                    new EncounterSpawnEntry(
                        archetypes[i],
                        Vector3.zero,
                        0f,
                        1f,
                        "Warden phase three");

                EnemyHealth minion =
                    factory.Spawn(
                        entry,
                        positions[i],
                        enemyParent);

                if (minion != null)
                {
                    ConfigureFragileMinion(minion);
                    phaseThreeMinions.Add(minion);
                    encounter?.RegisterRuntimeEnemy(minion);
                }

                yield return new WaitForSeconds(0.12f);
            }
        }

        /// <summary>
        /// Chooses spaced reinforcement positions that favour distance from the player.
        /// </summary>
        private List<Vector3> SelectSpawnPositions(int count)
        {
            Vector3[] localSlots =
            {
                new Vector3(-4.8f, 0.95f, -2.75f),
                new Vector3(0f, 0.95f, -3.05f),
                new Vector3(4.8f, 0.95f, -2.75f),
                new Vector3(-4.95f, 0.95f, 0f),
                new Vector3(4.95f, 0.95f, 0f),
                new Vector3(-4.8f, 0.95f, 2.75f),
                new Vector3(0f, 0.95f, 3.05f),
                new Vector3(4.8f, 0.95f, 2.75f)
            };

            List<Vector3> available =
                new List<Vector3>(localSlots.Length);

            for (int i = 0; i < localSlots.Length; i++)
            {
                available.Add(
                    arenaRoot != null
                        ? arenaRoot.TransformPoint(localSlots[i])
                        : arenaCenter + localSlots[i]);
            }

            List<Vector3> chosen =
                new List<Vector3>(count);

            while (chosen.Count < count && available.Count > 0)
            {
                int bestIndex = 0;
                float bestScore = float.NegativeInfinity;

                for (int i = 0; i < available.Count; i++)
                {
                    Vector3 candidate = available[i];
                    float playerDistance =
                        player != null
                            ? FlatDistance(candidate, player.position)
                            : 10f;

                    float bossDistance =
                        FlatDistance(candidate, transform.position);

                    float chosenSpacing = 10f;
                    for (int j = 0; j < chosen.Count; j++)
                    {
                        chosenSpacing =
                            Mathf.Min(
                                chosenSpacing,
                                FlatDistance(candidate, chosen[j]));
                    }

                    // Player distance dominates. Spacing prevents a wave from becoming
                    // a single body-blocking clump before it has even started moving.
                    float score =
                        playerDistance * 1.35f +
                        bossDistance * 0.35f +
                        chosenSpacing * 0.8f;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = i;
                    }
                }

                chosen.Add(available[bestIndex]);
                available.RemoveAt(bestIndex);
            }

            return chosen;
        }

        /// <summary>
        /// Creates the spawn warnings.
        /// </summary>
        private List<GameObject> CreateSpawnWarnings(
            List<Vector3> positions)
        {
            List<GameObject> warnings =
                new List<GameObject>(positions.Count);

            for (int i = 0; i < positions.Count; i++)
            {
                GameObject marker =
                    GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                marker.name = "Phase3_SpawnWarning";
                marker.transform.position =
                    positions[i] + Vector3.down * 0.91f;

                marker.transform.localScale =
                    new Vector3(0.72f, 0.018f, 0.72f);

                Renderer renderer = marker.GetComponent<Renderer>();
                if (renderer != null && spawnTelegraphMaterial != null)
                {
                    renderer.sharedMaterial = spawnTelegraphMaterial;
                }

                Collider collider = marker.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                    Destroy(collider);
                }

                warnings.Add(marker);
            }

            return warnings;
        }

        /// <summary>
        /// Configures a phase-three reinforcement as a one-hit enemy.
        /// </summary>
        private static void ConfigureFragileMinion(EnemyHealth minion)
        {
            if (minion == null)
            {
                return;
            }

            minion.Configure(1f);

            ShieldedEnemyHealth shielded =
                minion as ShieldedEnemyHealth;

            if (shielded != null &&
                !GameDifficulty.IsExtreme)
            {
                shielded.SetShieldActive(false);
            }
        }

        /// <summary>
        /// Counts the phase-three reinforcements that are still alive.
        /// </summary>
        private int CountLivingPhaseThreeMinions()
        {
            int living = 0;

            for (int i = phaseThreeMinions.Count - 1; i >= 0; i--)
            {
                EnemyHealth minion = phaseThreeMinions[i];
                if (minion == null)
                {
                    phaseThreeMinions.RemoveAt(i);
                    continue;
                }

                if (minion.CanReceiveDamage)
                {
                    living++;
                }
            }

            return living;
        }

        /// <summary>
        /// Responds when the Warden dies.
        /// </summary>
        private void HandleBossDeath(
            ReturnVector.Combat.DamageInfo damage)
        {
            for (int i = 0; i < phaseThreeMinions.Count; i++)
            {
                EnemyHealth minion = phaseThreeMinions[i];
                if (minion != null)
                {
                    Destroy(minion.gameObject);
                }
            }

            phaseThreeMinions.Clear();
        }

        /// <summary>
        /// Moves the existing piece.
        /// </summary>
        private void MoveExistingPiece(
            string objectName,
            Vector3 localPosition,
            Vector3 localScale)
        {
            if (arenaRoot == null)
            {
                return;
            }

            Transform piece = arenaRoot.Find(objectName);
            if (piece == null)
            {
                return;
            }

            piece.localPosition = localPosition;
            piece.localScale = localScale;
        }

        /// <summary>
        /// Creates the surface wall.
        /// </summary>
        private GameObject CreateSurfaceWall(
            Transform parent,
            string wallName,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            WeaponSurfaceProfile profile,
            Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = wallName;
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = localPosition;
            wall.transform.localRotation = localRotation;
            wall.transform.localScale = localScale;

            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            WeaponSurface surface = wall.AddComponent<WeaponSurface>();
            surface.Configure(profile);
            return wall;
        }

        /// <summary>
        /// Creates the solid wall.
        /// </summary>
        private GameObject CreateSolidWall(
            Transform parent,
            string wallName,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = wallName;
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = localPosition;
            wall.transform.localScale = localScale;

            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null && solidMaterial != null)
            {
                renderer.sharedMaterial = solidMaterial;
            }

            return wall;
        }

        /// <summary>
        /// Adds the cuRVature field.
        /// </summary>
        private static void AddCurvatureField(GameObject wall)
        {
            GameObject field = new GameObject("Curvature_Field");
            field.transform.SetParent(wall.transform, false);
            field.transform.localPosition = Vector3.zero;

            BoxCollider trigger = field.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.4f, 1.6f, 4.2f);

            WeaponCurvatureField curvature =
                field.AddComponent<WeaponCurvatureField>();

            curvature.Configure(
                WeaponCurvatureMode.OrbitClockwise,
                Vector3.forward,
                430f);
        }

        /// <summary>
        /// Returns the flat distance.
        /// </summary>
        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
