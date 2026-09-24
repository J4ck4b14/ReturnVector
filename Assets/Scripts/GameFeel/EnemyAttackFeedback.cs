using ReturnVector.Enemies;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    public enum EnemyAttackStage
    {
        None = 0,
        Windup = 1,
        Active = 2,
        Recovery = 3
    }

    public enum EnemyAttackStyle
    {
        Melee = 0,
        Shield = 1,
        Ranged = 2,
        BossSlam = 3,
        BossCharge = 4,
        BossShockwave = 5
    }

    public interface IEnemyAttackSource
    {
        EnemyAttackStage AttackStage { get; }
        EnemyAttackStyle AttackStyle { get; }
        float AttackProgress { get; }
        Vector3 AttackDirection { get; }
    }

    /// <summary>
    /// Procedural attack pose and telegraph shared by the prototype enemies.
    /// The gameplay root stays untouched so CharacterController dimensions remain stable.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyAttackFeedback : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform radialMarker;
        [SerializeField] private Transform lineMarker;
        [SerializeField, Min(0f)] private float settleSharpness = 18f;
        [SerializeField, Min(0f)] private float pulseSpeed = 9f;

        private IEnemyAttackSource source;
        private Vector3 basePosition;
        private Vector3 baseScale;
        private Quaternion baseRotation;
        private Vector3 radialBaseScale;
        private Vector3 lineBaseScale;

        public void Configure(
            Transform newVisualRoot,
            IEnemyAttackSource newSource,
            Transform newRadialMarker = null,
            Transform newLineMarker = null)
        {
            visualRoot = newVisualRoot;
            source = newSource;
            radialMarker = newRadialMarker;
            lineMarker = newLineMarker;

            if (visualRoot != null)
            {
                basePosition = visualRoot.localPosition;
                baseScale = visualRoot.localScale;
                baseRotation = visualRoot.localRotation;
            }

            radialBaseScale =
                radialMarker != null
                    ? radialMarker.localScale
                    : Vector3.one;

            lineBaseScale =
                lineMarker != null
                    ? lineMarker.localScale
                    : Vector3.one;

            SetMarkerActive(radialMarker, false);
            SetMarkerActive(lineMarker, false);
        }

        private void OnDisable()
        {
            SetMarkerActive(
                radialMarker,
                false);

            SetMarkerActive(
                lineMarker,
                false);

            if (visualRoot != null)
            {
                visualRoot.localPosition =
                    basePosition;

                visualRoot.localScale =
                    baseScale;

                visualRoot.localRotation =
                    baseRotation;
            }
        }

        private void Update()
        {
            if (visualRoot == null || source == null)
            {
                return;
            }

            EnemyAttackStage stage = source.AttackStage;
            float progress = Mathf.Clamp01(source.AttackProgress);

            Vector3 targetPosition = basePosition;
            Vector3 targetScale = baseScale;
            Quaternion targetRotation = baseRotation;

            bool radial = false;
            bool line = false;

            switch (source.AttackStyle)
            {
                case EnemyAttackStyle.Melee:
                    ApplyMeleePose(stage, progress, ref targetPosition, ref targetScale);
                    radial = stage == EnemyAttackStage.Windup;
                    break;

                case EnemyAttackStyle.Shield:
                    ApplyShieldPose(stage, progress, ref targetPosition, ref targetScale, ref targetRotation);
                    radial = stage == EnemyAttackStage.Windup;
                    break;

                case EnemyAttackStyle.Ranged:
                    ApplyRangedPose(stage, progress, ref targetPosition, ref targetScale);
                    line = stage == EnemyAttackStage.Windup;
                    break;

                case EnemyAttackStyle.BossSlam:
                    ApplyBossSlamPose(stage, progress, ref targetPosition, ref targetScale);
                    radial = stage == EnemyAttackStage.Windup;
                    break;

                case EnemyAttackStyle.BossCharge:
                    ApplyBossChargePose(stage, progress, ref targetPosition, ref targetScale, ref targetRotation);
                    line = stage == EnemyAttackStage.Windup;
                    break;

                case EnemyAttackStyle.BossShockwave:
                    ApplyBossShockwavePose(stage, progress, ref targetPosition, ref targetScale);
                    radial = stage == EnemyAttackStage.Windup;
                    break;
            }

            float blend =
                1f - Mathf.Exp(-settleSharpness * Time.deltaTime);

            visualRoot.localPosition =
                Vector3.Lerp(visualRoot.localPosition, targetPosition, blend);
            visualRoot.localScale =
                Vector3.Lerp(visualRoot.localScale, targetScale, blend);
            visualRoot.localRotation =
                Quaternion.Slerp(visualRoot.localRotation, targetRotation, blend);

            float radialScale =
                source.AttackStyle == EnemyAttackStyle.BossShockwave
                    ? 1.95f
                    : 1f;

            UpdateMarker(radialMarker, radial, progress, false, radialScale);
            UpdateMarker(lineMarker, line, progress, true, 1f);
        }

        private void ApplyMeleePose(
            EnemyAttackStage stage,
            float progress,
            ref Vector3 position,
            ref Vector3 scale)
        {
            if (stage == EnemyAttackStage.Windup)
            {
                position.z -= 0.16f * Smooth(progress);
                scale += new Vector3(0.08f, -0.12f, 0.08f) * Smooth(progress);
            }
            else if (stage == EnemyAttackStage.Recovery)
            {
                float kick = 1f - Smooth(progress);
                position.z += 0.38f * kick;
                scale += new Vector3(-0.04f, 0.07f, 0.12f) * kick;
            }
        }

        private void ApplyShieldPose(
            EnemyAttackStage stage,
            float progress,
            ref Vector3 position,
            ref Vector3 scale,
            ref Quaternion rotation)
        {
            if (stage == EnemyAttackStage.Windup)
            {
                float p = Smooth(progress);
                position.z -= 0.12f * p;
                rotation *= Quaternion.Euler(-9f * p, 0f, 0f);
                scale += new Vector3(0.05f, -0.05f, 0.03f) * p;
            }
            else if (stage == EnemyAttackStage.Recovery)
            {
                float kick = 1f - Smooth(progress);
                position.z += 0.3f * kick;
                rotation *= Quaternion.Euler(7f * kick, 0f, 0f);
            }
        }

        private void ApplyRangedPose(
            EnemyAttackStage stage,
            float progress,
            ref Vector3 position,
            ref Vector3 scale)
        {
            if (stage == EnemyAttackStage.Windup)
            {
                float p = Smooth(progress);
                scale += new Vector3(0.04f, 0.12f, 0.04f) * p;
                position.y += 0.06f * p;
            }
            else if (stage == EnemyAttackStage.Recovery)
            {
                float recoil = 1f - Smooth(progress);
                position.z -= 0.26f * recoil;
                scale += new Vector3(0.08f, -0.09f, 0.08f) * recoil;
            }
        }

        private void ApplyBossSlamPose(
            EnemyAttackStage stage,
            float progress,
            ref Vector3 position,
            ref Vector3 scale)
        {
            if (stage == EnemyAttackStage.Windup)
            {
                float p = Smooth(progress);
                position.y += 0.22f * p;
                scale += new Vector3(0.12f, 0.16f, 0.12f) * p;
            }
            else if (stage == EnemyAttackStage.Recovery)
            {
                float impact = 1f - Smooth(progress);
                position.y -= 0.18f * impact;
                scale += new Vector3(0.18f, -0.24f, 0.18f) * impact;
            }
        }

        private void ApplyBossChargePose(
            EnemyAttackStage stage,
            float progress,
            ref Vector3 position,
            ref Vector3 scale,
            ref Quaternion rotation)
        {
            if (stage == EnemyAttackStage.Windup)
            {
                float p = Smooth(progress);
                position.z -= 0.2f * p;
                rotation *= Quaternion.Euler(13f * p, 0f, 0f);
                scale += new Vector3(0.1f, -0.08f, 0.16f) * p;
            }
            else if (stage == EnemyAttackStage.Active)
            {
                scale += new Vector3(-0.04f, -0.04f, 0.22f);
            }
            else if (stage == EnemyAttackStage.Recovery)
            {
                float p = 1f - Smooth(progress);
                position.z += 0.18f * p;
                rotation *= Quaternion.Euler(-8f * p, 0f, 0f);
            }
        }

        private void ApplyBossShockwavePose(
            EnemyAttackStage stage,
            float progress,
            ref Vector3 position,
            ref Vector3 scale)
        {
            if (stage == EnemyAttackStage.Windup)
            {
                float p = Smooth(progress);
                position.y -= 0.12f * p;
                scale += new Vector3(0.2f, -0.18f, 0.2f) * p;
            }
            else if (stage == EnemyAttackStage.Recovery)
            {
                float burst = 1f - Smooth(progress);
                scale += new Vector3(0.32f, 0.08f, 0.32f) * burst;
                position.y += 0.08f * burst;
            }
        }

        private void UpdateMarker(
            Transform marker,
            bool active,
            float progress,
            bool line,
            float sizeMultiplier)
        {
            if (marker == null)
            {
                return;
            }

            marker.gameObject.SetActive(active);
            if (!active)
            {
                return;
            }

            float pulse =
                1f +
                Mathf.Sin(Time.time * pulseSpeed) * 0.08f;

            Vector3 scale =
                line
                    ? lineBaseScale
                    : radialBaseScale;

            if (line)
            {
                scale.x *= pulse;
            }
            else
            {
                float reveal =
                    Mathf.Lerp(0.72f, 1f, Smooth(progress));
                scale.x *= pulse * reveal * sizeMultiplier;
                scale.z *= pulse * reveal * sizeMultiplier;
            }

            marker.localScale = scale;
        }

        private static void SetMarkerActive(Transform marker, bool active)
        {
            if (marker != null)
            {
                marker.gameObject.SetActive(active);
            }
        }

        private static float Smooth(float value)
        {
            return value * value * (3f - 2f * value);
        }
    }
}
