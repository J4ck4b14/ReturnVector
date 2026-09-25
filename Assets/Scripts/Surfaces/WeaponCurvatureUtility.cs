using ReturnVector.Combat;
using UnityEngine;

// Script summary: Finds active curvature fields and applies their steering in a stable order.

namespace ReturnVector.Surfaces
{
    /// <summary>
    /// Finds active curvature fields and applies their steering in a stable order.
    /// </summary>
    public static class WeaponCurvatureUtility
    {
        /// <summary>
        /// Applies the at position.
        /// </summary>
        public static Vector3 ApplyAtPosition(
            Vector3 position,
            Vector3 currentDirection,
            AttackPhase phase,
            float deltaTime,
            Collider[] overlapBuffer,
            out int fieldsApplied)
        {
            fieldsApplied = 0;

            if (overlapBuffer == null || overlapBuffer.Length == 0)
            {
                return currentDirection;
            }

            int count = Physics.OverlapSphereNonAlloc(
                position,
                0.025f,
                overlapBuffer,
                ~0,
                QueryTriggerInteraction.Collide);

            Vector3 result = currentDirection;

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null || !collider.isTrigger)
                {
                    continue;
                }

                WeaponCurvatureField field =
                    collider.GetComponentInParent<WeaponCurvatureField>();

                if (field == null || !field.AppliesTo(phase))
                {
                    continue;
                }

                result = field.Apply(
                    position,
                    result,
                    phase,
                    deltaTime);
                fieldsApplied++;
            }

            return result;
        }
    }
}
