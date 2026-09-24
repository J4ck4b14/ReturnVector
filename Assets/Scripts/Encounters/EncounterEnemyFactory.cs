using ReturnVector.Enemies;
using ReturnVector.GameFeel;
using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Builds the prototype enemy archetypes from shared runtime dependencies.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterEnemyFactory : MonoBehaviour
    {
        [Header("Runtime references")]
        [SerializeField] private Transform player;
        [SerializeField] private PlayerTacticalStateSource tacticalState;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private WeaponInterferenceController interference;

        [Header("Tuning")]
        [SerializeField] private RusherEnemyTuning rusherTuning;
        [SerializeField] private ControllerEnemyTuning controllerTuning;
        [SerializeField] private ReturnWardenTuning returnWardenTuning;
        [SerializeField] private RVGameFeelProfile gameFeelProfile;
        [SerializeField] private bool showDevelopmentOverlays = true;

        [Header("Development materials")]
        [SerializeField] private Material rusherMaterial;
        [SerializeField] private Material shieldMaterial;
        [SerializeField] private Material controllerMaterial;
        [SerializeField] private Material telegraphMaterial;
        [SerializeField] private Material returnWardenMaterial;
        [SerializeField] private Material returnWardenTelegraphMaterial;

        public void Configure(
            Transform newPlayer,
            PlayerTacticalStateSource newTacticalState,
            WeaponController newWeapon,
            WeaponInterferenceController newInterference,
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
            interference = newInterference;
            rusherTuning = newRusherTuning;
            controllerTuning = newControllerTuning;
            rusherMaterial = newRusherMaterial;
            shieldMaterial = newShieldMaterial;
            controllerMaterial = newControllerMaterial;
            telegraphMaterial = newTelegraphMaterial;
        }

        public void ConfigureGameFeel(
            RVGameFeelProfile newProfile)
        {
            gameFeelProfile = newProfile;
        }

        public void ConfigureDevelopmentOverlays(bool visible)
        {
            showDevelopmentOverlays = visible;
        }

        public void ConfigureReturnWarden(
            ReturnWardenTuning newTuning,
            Material newBossMaterial,
            Material newBossTelegraphMaterial)
        {
            returnWardenTuning = newTuning;
            returnWardenMaterial = newBossMaterial;
            returnWardenTelegraphMaterial =
                newBossTelegraphMaterial;
        }

        // Encounter data stays prefab-agnostic; the prototype archetypes are assembled here.
        public EnemyHealth Spawn(
            EncounterSpawnEntry entry,
            Vector3 worldPosition,
            Transform parent)
        {
            switch (entry.Archetype)
            {
                case EnemyArchetype.Rusher:
                    return SpawnRusher(
                        worldPosition,
                        parent,
                        entry.HealthMultiplier);

                case EnemyArchetype.Shielded:
                    return SpawnShielded(
                        worldPosition,
                        parent,
                        entry.HealthMultiplier);

                case EnemyArchetype.Controller:
                    return SpawnController(
                        worldPosition,
                        parent,
                        entry.HealthMultiplier);

                case EnemyArchetype.ReturnWarden:
                    return SpawnReturnWarden(
                        worldPosition,
                        parent,
                        entry.HealthMultiplier);

                default:
                    return null;
            }
        }

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
                    rusherMaterial);

            CharacterController controller =
                root.GetComponent<CharacterController>();

            EnemyMotor motor =
                root.AddComponent<EnemyMotor>();
            motor.Configure(controller, 900f);

            EnemyHealth health =
                root.AddComponent<EnemyHealth>();
            health.Configure(
                3f * Mathf.Max(0.1f, healthMultiplier));

            RusherEnemyAI ai =
                root.AddComponent<RusherEnemyAI>();
            ai.Configure(
                motor,
                health,
                rusherTuning,
                player,
                tacticalState);

            Transform marker =
                CreateTelegraphMarker(
                    root.transform,
                    1.2f);

            EnemyTelegraphVisual telegraph =
                root.AddComponent<EnemyTelegraphVisual>();
            telegraph.Configure(marker, ai, null);

            GameObject nose =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);
            nose.name = "Rusher_Nose";
            nose.transform.SetParent(root.transform);
            nose.transform.localPosition =
                new Vector3(0f, 0f, 0.62f);
            nose.transform.localScale =
                new Vector3(0.48f, 0.34f, 0.55f);
            SetMaterial(nose, rusherMaterial);
            RemoveCollider(nose);

            AttachFeedback(
                root,
                health);

            return health;
        }

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
                    shieldMaterial);

            CharacterController controller =
                root.GetComponent<CharacterController>();

            EnemyMotor motor =
                root.AddComponent<EnemyMotor>();
            motor.Configure(controller, 680f);

            ShieldedEnemyHealth health =
                root.AddComponent<ShieldedEnemyHealth>();

            health.ConfigureShield(
                5f * Mathf.Max(0.1f, healthMultiplier),
                0.25f,
                1.6f,
                30f);

            ShieldedEnemyAI ai =
                root.AddComponent<ShieldedEnemyAI>();

            ai.Configure(
                motor,
                health,
                player,
                tacticalState,
                2.15f,
                2.2f);

            GameObject shield =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);
            shield.name = "Shield_Front";
            shield.transform.SetParent(root.transform);
            shield.transform.localPosition =
                new Vector3(0f, 0f, 0.78f);
            shield.transform.localScale =
                new Vector3(1.45f, 1.35f, 0.18f);
            SetMaterial(shield, shieldMaterial);

            AttachFeedback(
                root,
                health);

            return health;
        }

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
                    controllerMaterial);

            CharacterController controller =
                root.GetComponent<CharacterController>();

            EnemyMotor motor =
                root.AddComponent<EnemyMotor>();
            motor.Configure(controller, 540f);

            EnemyHealth health =
                root.AddComponent<EnemyHealth>();
            health.Configure(
                3f * Mathf.Max(0.1f, healthMultiplier));

            ControllerEnemyAI ai =
                root.AddComponent<ControllerEnemyAI>();

            ai.Configure(
                motor,
                health,
                controllerTuning,
                player,
                tacticalState,
                weapon,
                interference);

            Transform marker =
                CreateTelegraphMarker(
                    root.transform,
                    1.45f);

            EnemyTelegraphVisual telegraph =
                root.AddComponent<EnemyTelegraphVisual>();
            telegraph.Configure(marker, null, ai);

            GameObject core =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere);
            core.name = "Controller_Core";
            core.transform.SetParent(root.transform);
            core.transform.localPosition =
                new Vector3(0f, 0.72f, 0f);
            core.transform.localScale =
                Vector3.one * 0.48f;
            SetMaterial(core, controllerMaterial);
            RemoveCollider(core);

            AttachFeedback(
                root,
                health);

            return health;
        }


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
                    returnWardenMaterial);

            root.transform.localScale =
                new Vector3(
                    1.45f,
                    1.45f,
                    1.45f);

            CharacterController controller =
                root.GetComponent<CharacterController>();

            controller.height = 2.2f;
            controller.radius = 0.68f;

            EnemyMotor motor =
                root.AddComponent<EnemyMotor>();
            motor.Configure(
                controller,
                520f);

            WeaponRecallConstraint constraint =
                weapon != null
                    ? weapon.GetComponent<
                        WeaponRecallConstraint>()
                    : null;

            ReturnWardenHealth health =
                root.AddComponent<
                    ReturnWardenHealth>();

            ReturnWardenTuning bossTuning =
                returnWardenTuning;

            health.ConfigureBoss(
                bossTuning,
                constraint);

            if (bossTuning != null &&
                healthMultiplier != 1f)
            {
                health.Configure(
                    bossTuning.MaxHealth *
                    Mathf.Max(
                        0.1f,
                        healthMultiplier));
            }

            PlayerHealth playerHealth =
                player != null
                    ? player.GetComponentInParent<
                        PlayerHealth>()
                    : null;

            ReturnWardenAI ai =
                root.AddComponent<
                    ReturnWardenAI>();

            ai.Configure(
                motor,
                health,
                bossTuning,
                player,
                playerHealth,
                constraint);

            Transform marker =
                CreateBossTelegraphMarker(
                    root.transform,
                    2.35f);

            ReturnWardenTelegraphVisual telegraph =
                root.AddComponent<
                    ReturnWardenTelegraphVisual>();

            telegraph.Configure(
                ai,
                marker);

            ReturnWardenDebugOverlay overlay =
                root.AddComponent<
                    ReturnWardenDebugOverlay>();

            overlay.Configure(
                health,
                ai,
                constraint);

            overlay.enabled =
                showDevelopmentOverlays;

            GameObject shoulderLeft =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);

            shoulderLeft.name =
                "Warden_Shoulder_L";

            shoulderLeft.transform.SetParent(
                root.transform);

            shoulderLeft.transform.localPosition =
                new Vector3(
                    -0.72f,
                    0.35f,
                    0f);

            shoulderLeft.transform.localScale =
                new Vector3(
                    0.5f,
                    0.45f,
                    0.9f);

            SetMaterial(
                shoulderLeft,
                returnWardenMaterial);

            RemoveCollider(
                shoulderLeft);

            GameObject shoulderRight =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);

            shoulderRight.name =
                "Warden_Shoulder_R";

            shoulderRight.transform.SetParent(
                root.transform);

            shoulderRight.transform.localPosition =
                new Vector3(
                    0.72f,
                    0.35f,
                    0f);

            shoulderRight.transform.localScale =
                new Vector3(
                    0.5f,
                    0.45f,
                    0.9f);

            SetMaterial(
                shoulderRight,
                returnWardenMaterial);

            RemoveCollider(
                shoulderRight);

            AttachFeedback(
                root,
                health);

            return health;
        }

        private Transform CreateBossTelegraphMarker(
            Transform parent,
            float diameter)
        {
            GameObject marker =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder);

            marker.name =
                "Warden_SlamTelegraph";

            marker.transform.SetParent(
                parent);

            marker.transform.localPosition =
                new Vector3(
                    0f,
                    -0.94f,
                    0f);

            marker.transform.localScale =
                new Vector3(
                    diameter,
                    0.025f,
                    diameter);

            SetMaterial(
                marker,
                returnWardenTelegraphMaterial != null
                    ? returnWardenTelegraphMaterial
                    : telegraphMaterial);

            RemoveCollider(marker);
            marker.SetActive(false);

            return marker.transform;
        }


        // Feedback components subscribe to enemy events and remain presentation-only.
        private void AttachFeedback(
            GameObject root,
            EnemyHealth health)
        {
            if (root == null ||
                health == null)
            {
                return;
            }

            RVRendererFlash flash =
                root.GetComponent<RVRendererFlash>() ??
                root.AddComponent<RVRendererFlash>();

            flash.Configure(
                root.GetComponentsInChildren<
                    Renderer>(true));

            EnemyHitFeedback feedback =
                root.GetComponent<EnemyHitFeedback>() ??
                root.AddComponent<EnemyHitFeedback>();

            feedback.Configure(
                health,
                flash,
                gameFeelProfile);

            EnemyDeathFeedback deathFeedback =
                root.GetComponent<EnemyDeathFeedback>() ??
                root.AddComponent<EnemyDeathFeedback>();

            Renderer rootRenderer =
                root.GetComponent<Renderer>();

            deathFeedback.Configure(
                health,
                root.GetComponentsInChildren<Renderer>(true),
                rootRenderer != null
                    ? rootRenderer.sharedMaterial
                    : null);
        }

        // All archetypes share the same root conventions so movement, health and feedback line up.
        private GameObject CreateEnemyRoot(
            string name,
            Vector3 position,
            Transform parent,
            Material material)
        {
            GameObject root =
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule);

            root.name = name;
            root.transform.SetParent(parent);
            root.transform.position = position;
            root.transform.localScale =
                new Vector3(0.9f, 0.9f, 0.9f);

            RemoveCollider(root);

            CharacterController controller =
                root.AddComponent<CharacterController>();
            controller.center = Vector3.zero;
            controller.height = 2f;
            controller.radius = 0.48f;
            controller.stepOffset = 0.15f;
            controller.skinWidth = 0.04f;

            SetMaterial(root, material);
            return root;
        }

        private Transform CreateTelegraphMarker(
            Transform parent,
            float diameter)
        {
            GameObject marker =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder);

            marker.name = "Telegraph";
            marker.transform.SetParent(parent);
            marker.transform.localPosition =
                new Vector3(0f, -0.92f, 0f);
            marker.transform.localScale =
                new Vector3(
                    diameter,
                    0.025f,
                    diameter);

            SetMaterial(marker, telegraphMaterial);
            RemoveCollider(marker);
            marker.SetActive(false);

            return marker.transform;
        }

        private static void SetMaterial(
            GameObject gameObject,
            Material material)
        {
            if (material == null)
            {
                return;
            }

            Renderer renderer =
                gameObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void RemoveCollider(
            GameObject gameObject)
        {
            Collider collider =
                gameObject.GetComponent<Collider>();

            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }
        }
    }
}
