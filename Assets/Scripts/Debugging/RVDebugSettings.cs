using UnityEngine;

// Script summary: Centralizes optional diagnostic drawing and overlay settings.

namespace ReturnVector.Debugging
{
    [CreateAssetMenu(
        fileName = "SO_DebugSettings",
        menuName = "RETURN VECTOR/Debug Settings")]
    /// <summary>
    /// Centralizes optional diagnostic drawing and overlay settings.
    /// </summary>
    public sealed class RVDebugSettings : ScriptableObject
    {
        // Logging variables
        [Header("Logging")]
        public bool LogStateTransitions = true;
        public bool LogRejectedStateTransitions = true;

        // Scene Diagnostics variables
        [Header("Scene Diagnostics")]
        public bool DrawWeaponOwnerLine = true;
        public bool DrawCollisionNormals = true;
        public bool DrawCastSweeps = true;
        public bool DrawPlannedReturnRoute = true;
        public bool DrawSurfaceResponses = true;
        public bool DrawCurvatureInfluence = true;
        public bool DrawActualWeaponTrace = true;

        // Trace Recording variables
        [Header("Trace Recording")]
        public bool RecordActualWeaponTrace = true;
        public bool ClearTraceOnNewThrow = true;
        [Min(0.01f)] public float TraceSampleSpacing = 0.12f;
        [Min(16)] public int MaxTraceSamples = 256;

        // Event History variables
        [Header("Event History")]
        [Min(8)] public int EventHistoryCapacity = 96;
        [Range(1, 12)] public int OverlayRecentEventCount = 5;

        // Overlay variables
        [Header("Overlay")]
        public bool ShowRuntimeOverlay = true;
        public bool ShowRecentEventsInOverlay = true;
        public bool ShowSessionCounters = true;

        // Debug Colours variables
        [Header("Debug Colours")]
        public Color OwnerLineColor = Color.cyan;
        public Color CollisionNormalColor = Color.yellow;
        public Color OutboundPathColor = new Color(1f, 0.45f, 0.1f);
        public Color ReturnPathColor = new Color(0.2f, 0.9f, 1f);
        public Color ReflectionColor = new Color(1f, 0.2f, 0.9f);
        public Color CurvatureColor = new Color(0.5f, 1f, 0.35f);
    }
}
