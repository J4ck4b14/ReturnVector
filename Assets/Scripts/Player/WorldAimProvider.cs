using ReturnVector.Input;
using UnityEngine;

// Script summary: Converts pointer or gamepad aim into a flat world-space direction. Mouse aim is projected onto a horizontal plane passing through the supplied origin.

namespace ReturnVector.Player
{
    /// <summary>
    /// Converts pointer or gamepad aim into a flat world-space direction.
    /// Mouse aim is projected onto a horizontal plane passing through the supplied origin.
    /// </summary>
    public sealed class WorldAimProvider : MonoBehaviour
    {
        // Player variables
        [SerializeField] private RVInputReader input;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float directionalAimDistance = 20f;

        private Vector3 lastValidDirection = Vector3.forward;

        public Vector3 LastValidDirection => lastValidDirection;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(RVInputReader newInput, Camera newCamera)
        {
            input = newInput;
            worldCamera = newCamera;
        }

        /// <summary>
        /// Attempts to read a valid world-space aim direction and reports whether it succeeded.
        /// </summary>
        public bool TryGetAim(Vector3 origin, out Vector3 worldPoint, out Vector3 direction)
        {
            worldPoint = origin + lastValidDirection * directionalAimDistance;
            direction = lastValidDirection;

            if (input == null)
            {
                return false;
            }

            if (input.AimKind == AimInputKind.ScreenPosition)
            {
                Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;
                if (cameraToUse == null)
                {
                    return false;
                }

                Ray ray = cameraToUse.ScreenPointToRay(input.Aim);
                Plane plane = new Plane(Vector3.up, origin);

                if (!plane.Raycast(ray, out float enter))
                {
                    return false;
                }

                worldPoint = ray.GetPoint(enter);
                Vector3 flat = worldPoint - origin;
                flat.y = 0f;

                if (flat.sqrMagnitude < 0.0001f)
                {
                    return false;
                }

                direction = flat.normalized;
                lastValidDirection = direction;
                return true;
            }

            Vector2 stick = input.Aim;
            if (stick.sqrMagnitude < 0.04f)
            {
                direction = lastValidDirection;
                worldPoint = origin + direction * directionalAimDistance;
                return true;
            }

            direction = new Vector3(stick.x, 0f, stick.y).normalized;
            lastValidDirection = direction;
            worldPoint = origin + direction * directionalAimDistance;
            return true;
        }
    }
}
