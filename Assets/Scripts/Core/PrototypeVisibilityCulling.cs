using System;
using UnityEngine;

// Script summary: Groups encounter-room renderers behind an explicit camera-frustum gate. Physics and gameplay objects remain active while off-screen rendering is suppressed.

namespace ReturnVector.Core
{
    /// <summary>
    /// Groups encounter-room renderers behind an explicit camera-frustum gate.
    /// Physics and gameplay objects remain active while off-screen rendering is suppressed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeVisibilityCulling : MonoBehaviour
    {
        [Serializable]
        private sealed class RenderGroup
        {
            // Run variables
            public Transform Root;
            [NonSerialized] public Renderer[] Renderers = Array.Empty<Renderer>();
            [NonSerialized] public int LastChildCount = -1;
        }

        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform[] roomRoots = Array.Empty<Transform>();
        [SerializeField, Min(0f)] private float boundsPadding = 2.5f;
        [SerializeField, Min(1)] private int updateEveryFrames = 3;
        [SerializeField, Min(1)] private int refreshRenderersEveryFrames = 30;

        private RenderGroup[] groups = Array.Empty<RenderGroup>();
        private Plane[] planes = new Plane[6];
        private int frame;

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            BuildGroups();
        }

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            SetAllVisible();
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            SetAllVisible();
        }

        /// <summary>
        /// Updates presentation after regular frame logic has completed.
        /// </summary>
        private void LateUpdate()
        {
            if (targetCamera == null || groups.Length == 0)
            {
                return;
            }

            frame++;
            if (frame % Mathf.Max(1, updateEveryFrames) != 0)
            {
                return;
            }

            GeometryUtility.CalculateFrustumPlanes(targetCamera, planes);

            bool refresh =
                frame % Mathf.Max(1, refreshRenderersEveryFrames) == 0;

            for (int i = 0; i < groups.Length; i++)
            {
                RenderGroup group = groups[i];
                if (group?.Root == null)
                {
                    continue;
                }

                if (refresh ||
                    group.Renderers == null ||
                    group.Root.childCount != group.LastChildCount)
                {
                    Refresh(group);
                }

                if (!TryGetBounds(group.Renderers, out Bounds bounds))
                {
                    continue;
                }

                bounds.Expand(boundsPadding * 2f);
                bool visible = GeometryUtility.TestPlanesAABB(planes, bounds);
                SetGroupVisible(group, visible);
            }
        }

        /// <summary>
        /// Builds the groups.
        /// </summary>
        private void BuildGroups()
        {
            groups = new RenderGroup[roomRoots?.Length ?? 0];

            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] =
                    new RenderGroup
                    {
                        Root = roomRoots[i]
                    };

                Refresh(groups[i]);
            }
        }

        /// <summary>
        /// Refreshes the component from its current runtime sources.
        /// </summary>
        private static void Refresh(RenderGroup group)
        {
            if (group?.Root == null)
            {
                return;
            }

            group.Renderers =
                group.Root.GetComponentsInChildren<Renderer>(true);
            group.LastChildCount = group.Root.childCount;
        }

        /// <summary>
        /// Attempts to calculate renderer bounds for the supplied visibility group.
        /// </summary>
        private static bool TryGetBounds(Renderer[] renderers, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;

            if (renderers == null)
            {
                return false;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        /// <summary>
        /// Sets the group visible.
        /// </summary>
        private static void SetGroupVisible(RenderGroup group, bool visible)
        {
            if (group?.Renderers == null)
            {
                return;
            }

            for (int i = 0; i < group.Renderers.Length; i++)
            {
                Renderer renderer = group.Renderers[i];
                if (renderer != null)
                {
                    renderer.forceRenderingOff = !visible;
                }
            }
        }

        /// <summary>
        /// Sets all visible.
        /// </summary>
        private void SetAllVisible()
        {
            for (int i = 0; i < groups.Length; i++)
            {
                SetGroupVisible(groups[i], true);
            }
        }
    }
}
