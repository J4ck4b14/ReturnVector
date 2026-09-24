using System;
using UnityEngine;

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

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            BuildGroups();
        }

        private void OnEnable()
        {
            SetAllVisible();
        }

        private void OnDisable()
        {
            SetAllVisible();
        }

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

        private void SetAllVisible()
        {
            for (int i = 0; i < groups.Length; i++)
            {
                SetGroupVisible(groups[i], true);
            }
        }
    }
}
