using ReturnVector.Surfaces;
using UnityEditor;
using UnityEngine;

namespace ReturnVector.Editor
{
    /// <summary>
    /// Draws surface orientation and curvature helpers in the Scene view.
    /// </summary>
    public static class WeaponSurfaceGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawSurface(WeaponSurface surface, GizmoType gizmoType)
        {
            if (surface == null || surface.Profile == null)
            {
                return;
            }

            Vector3 labelPosition = surface.transform.position + Vector3.up * 1.25f;
            Handles.Label(labelPosition, $"RV: {surface.Profile.Kind}");
        }

        [DrawGizmo(GizmoType.Selected)]
        private static void DrawCurvatureField(WeaponCurvatureField field, GizmoType gizmoType)
        {
            if (field == null)
            {
                return;
            }

            Vector3 origin = field.transform.position;
            Vector3 direction = field.DesiredDirection(origin + field.transform.forward * 0.25f);

            Handles.ArrowHandleCap(
                0,
                origin,
                Quaternion.LookRotation(direction, Vector3.up),
                1.5f,
                EventType.Repaint);

            Handles.Label(
                origin + Vector3.up * 1.55f,
                $"Curve: {field.Mode} | {field.SteeringDegreesPerSecond:0}°/s");
        }
    }
}
