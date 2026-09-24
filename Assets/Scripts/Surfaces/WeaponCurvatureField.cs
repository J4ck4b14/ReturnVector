using ReturnVector.Combat;
using UnityEngine;

namespace ReturnVector.Surfaces
{
    /// <summary>
    /// Authored trigger volume that continuously steers weapon travel direction.
    /// Its behavior is deterministic: same entry direction, position and timestep produce the same turn.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponCurvatureField : MonoBehaviour
    {
        [SerializeField] private WeaponCurvatureMode mode =
            WeaponCurvatureMode.GuideDirection;
        [SerializeField] private Vector3 localGuideDirection = Vector3.forward;
        [SerializeField, Min(0f)] private float steeringDegreesPerSecond = 540f;
        [SerializeField] private bool affectsOutbound = true;
        [SerializeField] private bool affectsRecall = true;

        public WeaponCurvatureMode Mode => mode;
        public float SteeringDegreesPerSecond => steeringDegreesPerSecond;

        public void Configure(
            WeaponCurvatureMode newMode,
            Vector3 newLocalGuideDirection,
            float newSteeringDegreesPerSecond,
            bool outbound = true,
            bool recall = true)
        {
            mode = newMode;
            localGuideDirection =
                newLocalGuideDirection.sqrMagnitude > 0.000001f
                    ? newLocalGuideDirection.normalized
                    : Vector3.forward;
            steeringDegreesPerSecond =
                Mathf.Max(0f, newSteeringDegreesPerSecond);
            affectsOutbound = outbound;
            affectsRecall = recall;
        }

        public bool AppliesTo(AttackPhase phase)
        {
            return phase == AttackPhase.Outbound
                ? affectsOutbound
                : phase == AttackPhase.Recall && affectsRecall;
        }

        public Vector3 DesiredDirection(Vector3 worldPosition)
        {
            if (mode == WeaponCurvatureMode.GuideDirection)
            {
                Vector3 guide =
                    transform.TransformDirection(localGuideDirection);
                guide.y = 0f;

                return guide.sqrMagnitude >= 0.000001f
                    ? guide.normalized
                    : transform.forward;
            }

            Vector3 radial = worldPosition - transform.position;
            radial.y = 0f;

            if (radial.sqrMagnitude < 0.000001f)
            {
                radial = transform.right;
                radial.y = 0f;
            }

            radial.Normalize();

            Vector3 tangent =
                Vector3.Cross(Vector3.up, radial);

            if (mode == WeaponCurvatureMode.OrbitCounterClockwise)
            {
                tangent = -tangent;
            }

            tangent.y = 0f;
            return tangent.normalized;
        }

        public Vector3 Apply(
            Vector3 worldPosition,
            Vector3 currentDirection,
            AttackPhase phase,
            float deltaTime)
        {
            if (!AppliesTo(phase))
            {
                return currentDirection;
            }

            return WeaponSurfaceMath.SteerPlanar(
                currentDirection,
                DesiredDirection(worldPosition),
                steeringDegreesPerSecond,
                deltaTime);
        }
    }
}
