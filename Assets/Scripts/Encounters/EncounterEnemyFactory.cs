using ReturnVector.Core;
using ReturnVector.Enemies;
using ReturnVector.GameFeel;
using ReturnVector.Player;
using ReturnVector.Surfaces;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Builds the prototype enemy archetypes from shared runtime dependencies.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Builds the prototype enemy archetypes from shared runtime dependencies.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterEnemyFactory : MonoBehaviour
    {
        // Runtime reference variables
        [Header("Runtime references")]
        [SerializeField] private Transform player;
        [SerializeField] private PlayerTacticalStateSource tacticalState;
        [SerializeField] private WeaponController weapon;

        // Tuning variables
        [Header("Tuning")]
        [SerializeField] private RusherEnemyTuning rusherTuning;
        [SerializeField] private ControllerEnemyTuning controllerTuning;
        [SerializeField] private ReturnWardenTuning returnWardenTuning;
        [SerializeField] private RVGameFeelProfile gameFeelProfile;

        // Material variables
        [Header("Materials")]
        [SerializeField] private Material rusherMaterial;
        [SerializeField] private Material shieldMaterial;
        [SerializeField] private Material controllerMaterial;
        [SerializeField] private Material telegraphMaterial;
        [SerializeField] private Material returnWardenMaterial;
        [SerializeField] private Material returnWardenPhaseTwoMaterial;
        [SerializeField] private Material returnWardenTelegraphMaterial;

        // Warden arena variables
        [Header("Warden arena")]
        [SerializeField] private WeaponSurfaceProfile reflectiveSurface;
        [SerializeField] private WeaponSurfaceProfile penetrableSurface;
        [SerializeField] private WeaponSurfaceProfile absorbingSurface;
        [SerializeField] private WeaponSurfaceProfile curvingSurface;
        [SerializeField] private Material reflectiveSurfaceMaterial;
        [SerializeField] private Material penetrableSurfaceMaterial;
        [SerializeField] private Material absorbingSurfaceMaterial;
        [SerializeField] private Material curvingSurfaceMaterial;
        [SerializeField] private Material solidArenaMaterial;

        public Transform Player => player;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            Transform newPlayer,
            PlayerTacticalStateSource newTacticalState,
            WeaponController newWeapon,
            RusherEnemyTuning newRusherTuning,
            ControllerEnemyTuning newControllerTuning,
            Material newRusherMaterial,
            Material newShieldMaterial,
            Material newControllerMaterial,
            Material newTelegraphMaterial)
        {
            player = newPlayer;
            tacticalState = newTacticalState;
            weapon = newWeapon;
            rusherTuning = newRusherTuning;
            controllerTuning = newControllerTuning;
            rusherMaterial = newRusherMaterial;
            shieldMaterial = newShieldMaterial;
            controllerMaterial = newControllerMaterial;
            telegraphMaterial = newTelegraphMaterial;
        }

        /// <summary>
        /// Assigns the game-feel profile used by enemies created by the factory.
        /// </summary>
        public void ConfigureGameFeel(RVGameFeelProfile newProfile)
        {
            gameFeelProfile = newProfile;
        }

        /// <summary>
        /// Assigns the Warden tuning and presentation materials used by the factory.
        /// </summary>
        public void ConfigureReturnWarden(
            ReturnWardenTuning newTuning,
            Material newBossMaterial,
            Material newBossPhaseTwoMaterial,
            Material newBossTelegraphMaterial)
        {
            returnWardenTuning = newTuning;
            returnWardenMaterial = newBossMaterial;
            returnWardenPhaseTwoMaterial = newBossPhaseTwoMaterial;
            returnWardenTelegraphMaterial = newBossTelegraphMaterial;
        }

        /// <summary>
        /// Creates the pressure warning.
        /// </summary>
        public GameObject CreatePressureWarning(
            Vector3 worldPosition)
        {
            GameObject marker =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder);

            marker.name = "Extreme_Reinforcement_Warning";
            marker.transform.position =
                worldPosition + Vector3.down * 0.91f;
            marker.transform.localScale =
                new Vector3(0.9f, 0.018f, 0.9f);

            SetMaterial(marker, telegraphMaterial);
            RemoveCollider(marker);
            return marker;
        }

        /// <summary>
        /// Spawns the requested enemy archetype at the supplied world position.
        /// </summary>
        public EnemyHealth Spawn(
            EncounterSpawnEntry entry,
            Vector3 worldPosition,
            Transform parent)
        {
            switch (entry.Archetype)
            {
                case EnemyArchetype.Rusher:
                    return SpawnRusher(worldPosition, parent, entry.HealthMultiplier);

                case EnemyArchetype.Shielded:
                    return SpawnShielded(worldPosition, parent, entry.HealthMultiplier);

                case EnemyArchetype.Controller:
                    return SpawnController(worldPosition, parent, entry.HealthMultiplier);

                case EnemyArchetype.ReturnWarden:
                    return SpawnReturnWarden(worldPosition, parent, entry.HealthMultiplier);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Spawns the rusher.
        /// </summary>
        private EnemyHealth SpawnRusher(
            Vector3 position,
            Transform parent,
            float healthMultiplier)
        {
            GameObject root =
                CreateEnemyRoot(
                    "Rusher",
                    position,
                    parent,
                    rusherMaterial,
                    out Transform visual);

            CharacterController controller = root.GetComponent<CharacterController>();
            EnemyMotor motor = root.AddComponent<EnemyMotor>();
            motor.Configure(controller, 900f);

            EnemyHealth health = root.AddComponent<EnemyHealth>();
            health.Configure(
                3f *
                Mathf.Max(0.1f, healthMultiplier) *
                GameDifficulty.Current.EnemyHealthMultiplier);

            RusherEnemyAI ai = root.AddComponent<RusherEnemyAI>();
            ai.Configure(motor, health, rusherTuning, player, tacticalState);

            Transform marker = CreateRadialTelegraph(root.transform, 1.2f, telegraphMaterial);

            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Rusher_Nose";
            nose.transform.SetParent(visual, false);
            nose.transform.localPosition = new Vector3(0f, 0f, 0.68f);
            nose.transform.localScale = new Vector3(0.48f, 0.34f, 0.55f);
            SetMaterial(nose, rusherMaterial);
            RemoveCollider(nose);

            AttachAttackFeedback(root, visual, ai, marker, null);
            AttachHitAndDeathFeedback(root, health, rusherMaterial);
            return health;
        }

        /// <summary>
        /// Spawns the shielded.
        /// </summary>
        private EnemyHealth SpawnShielded(
            Vector3 position,
            Transform parent,
            float healthMultiplier)
        {
            GameObject root =
                CreateEnemyRoot(
                    "Shielded",
                    position,
                    parent,
                    shieldMaterial,
                    out Transform visual);

            CharacterController controller = root.GetComponent<CharacterController>();
            EnemyMotor motor = root.AddComponent<EnemyMotor>();
            motor.Configure(controller, 680f);

            ShieldedEnemyHealth health = root.AddComponent<ShieldedEnemyHealth>();
            health.ConfigureShield(
                5f *
                Mathf.Max(0.1f, healthMultiplier) *
                GameDifficulty.Current.EnemyHealthMultiplier,
                0.25f,
                1.6f,
                30f);

            ShieldedEnemyAI ai = root.AddComponent<ShieldedEnemyAI>();
            ai.Configure(motor, health, player, tacticalState, 2.15f, 2.2f);

            GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shield.name = "Shield_Front";
            shield.transform.SetParent(visual, false);
            shield.transform.localPosition = new Vector3(0f, 0f, 0.86f);
            shield.transform.localScale = new Vector3(1.45f, 1.35f, 0.18f);
            SetMaterial(shield, shieldMaterial);
            RemoveCollider(shield);

            Transform marker = CreateRadialTelegraph(root.transform, 1.45f, telegraphMaterial);

            AttachAttackFeedback(root, visual, ai, marker, null);
            AttachHitAndDeathFeedback(root, health, shieldMaterial);
            return health;
        }

        /// <summary>
        /// Spawns the controller.
        /// </summary>
        private EnemyHealth SpawnController(
            Vector3 position,
            Transform parent,
            float healthMultiplier)
        {
            GameObject root =
                CreateEnemyRoot(
                    "Controller",
                    position,
                    parent,
                    controllerMaterial,
                    out Transform visual);

            CharacterController controller = root.GetComponent<CharacterController>();
            EnemyMotor motor = root.AddComponent<EnemyMotor>();
            motor.Configure(controller, 540f);

            EnemyHealth health = root.AddComponent<EnemyHealth>();
            health.Configure(
                3f *
                Mathf.Max(0.1f, healthMultiplier) *
                GameDifficulty.Current.EnemyHealthMultiplier);

            ControllerEnemyAI ai = root.AddComponent<ControllerEnemyAI>();
            ai.Configure(
                motor,
                health,
                controllerTuning,
                player,
                tacticalState,
                controllerMaterial);

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Controller_Core";
            core.transform.SetParent(visual, false);
            core.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            core.transform.localScale = Vector3.one * 0.48f;
            SetMaterial(core, controllerMaterial);
            RemoveCollider(core);

            Transform lineMarker =
                CreateLineTelegraph(
                    root.transform,
                    6.2f,
                    0.18f,
                    telegraphMaterial,
                    "Shot_Telegraph");

            AttachAttackFeedback(root, visual, ai, null, lineMarker);
            AttachHitAndDeathFeedback(root, health, controllerMaterial);
            return health;
        }

        /// <summary>
        /// Spawns the Return Warden.
        /// </summary>
        private EnemyHealth SpawnReturnWarden(
            Vector3 position,
            Transform parent,
            float healthMultiplier)
        {
            GameObject root =
                CreateEnemyRoot(
                    "Return_Warden",
                    position,
                    parent,
                    returnWardenMaterial,
                    out Transform visual);

            visual.localScale = Vector3.one * 1.35f;

            CharacterController controller = root.GetComponent<CharacterController>();
            controller.height = 2.2f;
            controller.radius = 0.68f;

            EnemyMotor motor = root.AddComponent<EnemyMotor>();
            motor.Configure(controller, 520f);

            WeaponRecallConstraint constraint =
                weapon != null
                    ? weapon.GetComponent<WeaponRecallConstraint>()
                    : null;

            ReturnWardenHealth health = root.AddComponent<ReturnWardenHealth>();
            ReturnWardenTuning bossTuning = returnWardenTuning;
            health.ConfigureBoss(bossTuning, constraint);

            if (bossTuning != null)
            {
                health.Configure(
                    bossTuning.MaxHealth *
                    Mathf.Max(0.1f, healthMultiplier) *
                    GameDifficulty.Current.BossHealthMultiplier);
            }

            PlayerHealth playerHealth =
                player != null
                    ? player.GetComponentInParent<PlayerHealth>()
                    : null;

            ReturnWardenAI ai = root.AddComponent<ReturnWardenAI>();
            ai.Configure(
                motor,
                health,
                bossTuning,
                player,
                playerHealth,
                constraint);

            Material bossTelegraph =
                returnWardenTelegraphMaterial != null
                    ? returnWardenTelegraphMaterial
                    : telegraphMaterial;

            Transform radialMarker =
                CreateRadialTelegraph(
                    root.transform,
                    bossTuning != null ? bossTuning.SlamRange * 2f : 4.3f,
                    bossTelegraph);

            Transform lineMarker =
                CreateLineTelegraph(
                    root.transform,
                    bossTuning != null ? bossTuning.ChargeDistance : 5.4f,
                    0.65f,
                    bossTelegraph,
                    "Charge_Telegraph");

            CreateWardenShoulder(visual, -0.72f);
            CreateWardenShoulder(visual, 0.72f);

            EnemyAttackFeedback attackFeedback =
                AttachAttackFeedback(
                    root,
                    visual,
                    ai,
                    radialMarker,
                    lineMarker);

            RVCameraFeedback cameraFeedback =
                Object.FindFirstObjectByType<RVCameraFeedback>();

            ReturnWardenPhaseFeedback phaseFeedback =
                root.AddComponent<ReturnWardenPhaseFeedback>();

            phaseFeedback.Configure(
                health,
                ai,
                bossTuning,
                visual,
                visual.GetComponentsInChildren<Renderer>(true),
                returnWardenPhaseTwoMaterial,
                bossTelegraph,
                cameraFeedback,
                attackFeedback);

            AttachHitFeedback(root, health);

            Transform arenaRoot =
                parent != null && parent.parent != null
                    ? parent.parent
                    : parent;

            EncounterController encounter =
                arenaRoot != null
                    ? arenaRoot.GetComponent<EncounterController>()
                    : null;

            ReturnWardenArenaController arena =
                root.AddComponent<ReturnWardenArenaController>();

            arena.Configure(
                health,
                this,
                encounter,
                arenaRoot,
                parent,
                player,
                cameraFeedback,
                reflectiveSurface,
                penetrableSurface,
                absorbingSurface,
                curvingSurface,
                reflectiveSurfaceMaterial,
                penetrableSurfaceMaterial,
                absorbingSurfaceMaterial,
                curvingSurfaceMaterial,
                solidArenaMaterial,
                bossTelegraph);

            return health;
        }

        /// <summary>
        /// Creates the warden shoulder.
        /// </summary>
        private void CreateWardenShoulder(Transform visual, float x)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shoulder.name = x < 0f ? "Warden_Shoulder_L" : "Warden_Shoulder_R";
            shoulder.transform.SetParent(visual, false);
            shoulder.transform.localPosition = new Vector3(x, 0.35f, 0f);
            shoulder.transform.localScale = new Vector3(0.5f, 0.45f, 0.9f);
            SetMaterial(shoulder, returnWardenMaterial);
            RemoveCollider(shoulder);
        }

        /// <summary>
        /// Adds procedural attack telegraph feedback to an enemy.
        /// </summary>
        private EnemyAttackFeedback AttachAttackFeedback(
            GameObject root,
            Transform visual,
            IEnemyAttackSource source,
            Transform radialMarker,
            Transform lineMarker)
        {
            EnemyAttackFeedback feedback =
                root.AddComponent<EnemyAttackFeedback>();

            feedback.Configure(
                visual,
                source,
                radialMarker,
                lineMarker);

            return feedback;
        }

        /// <summary>
        /// Adds hit feedback to an enemy without changing its death behaviour.
        /// </summary>
        private void AttachHitFeedback(
            GameObject root,
            EnemyHealth health)
        {
            RVRendererFlash flash =
                root.AddComponent<RVRendererFlash>();

            flash.Configure(
                root.GetComponentsInChildren<Renderer>(true));

            EnemyHitFeedback hitFeedback =
                root.AddComponent<EnemyHitFeedback>();

            hitFeedback.Configure(
                health,
                flash,
                gameFeelProfile);
        }

        /// <summary>
        /// Adds the standard hit and death feedback package to an enemy.
        /// </summary>
        private void AttachHitAndDeathFeedback(
            GameObject root,
            EnemyHealth health,
            Material splatterMaterial)
        {
            RVRendererFlash flash = root.AddComponent<RVRendererFlash>();
            flash.Configure(root.GetComponentsInChildren<Renderer>(true));

            EnemyHitFeedback hitFeedback = root.AddComponent<EnemyHitFeedback>();
            hitFeedback.Configure(health, flash, gameFeelProfile);

            EnemyDeathFeedback deathFeedback = root.AddComponent<EnemyDeathFeedback>();
            deathFeedback.Configure(
                health,
                root.GetComponentsInChildren<Renderer>(true),
                splatterMaterial);
        }

        /// <summary>
        /// Creates the enemy root.
        /// </summary>
        private GameObject CreateEnemyRoot(
            string name,
            Vector3 position,
            Transform parent,
            Material material,
            out Transform visual)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CharacterController controller = root.AddComponent<CharacterController>();
            controller.center = Vector3.zero;
            controller.height = 2f;
            controller.radius = 0.48f;
            controller.stepOffset = 0.15f;
            controller.skinWidth = 0.04f;

            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visualObject.name = "Visual";
            visualObject.transform.SetParent(root.transform, false);
            visualObject.transform.localScale = Vector3.one * 0.9f;
            SetMaterial(visualObject, material);
            RemoveCollider(visualObject);

            visual = visualObject.transform;
            return root;
        }

        /// <summary>
        /// Creates the radial telegraph.
        /// </summary>
        private Transform CreateRadialTelegraph(
            Transform parent,
            float diameter,
            Material material)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Attack_Telegraph";
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = new Vector3(0f, -0.92f, 0f);
            marker.transform.localScale = new Vector3(diameter, 0.025f, diameter);
            SetMaterial(marker, material);
            RemoveCollider(marker);
            marker.SetActive(false);
            return marker.transform;
        }

        /// <summary>
        /// Creates the line telegraph.
        /// </summary>
        private Transform CreateLineTelegraph(
            Transform parent,
            float length,
            float width,
            Material material,
            string objectName)
        {
            float safeLength = Mathf.Max(0.2f, length);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = objectName;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = new Vector3(0f, -0.9f, safeLength * 0.5f);
            marker.transform.localScale = new Vector3(width, 0.035f, safeLength);
            SetMaterial(marker, material);
            RemoveCollider(marker);
            marker.SetActive(false);
            return marker.transform;
        }

        /// <summary>
        /// Sets the material.
        /// </summary>
        private static void SetMaterial(GameObject gameObject, Material material)
        {
            if (material == null)
            {
                return;
            }

            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        /// <summary>
        /// Removes the collider.
        /// </summary>
        private static void RemoveCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }
        }
    }
}
