using System;
using System.Collections.Generic;
using UnityEngine;

// Script summary: Small grid A* solver used by enemy movement when a straight route is blocked.

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Small grid A* solver used by enemy movement when a straight route is blocked.
    /// </summary>
    public static class AStarPathfinder
    {
        // Enemy variables
        private static readonly Collider[] overlapBuffer = new Collider[32];
        private static readonly RaycastHit[] castBuffer = new RaycastHit[32];

        // Navigation variables
        private static readonly Vector2Int[] neighbourOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        /// <summary>
        /// Attempts to find an A* route between the supplied world positions.
        /// </summary>
        public static bool TryFindPath(
            Vector3 start,
            Vector3 goal,
            float requestedCellSize,
            float agentRadius,
            Transform ignoreRoot,
            List<Vector3> result,
            float padding = 5f,
            int maxNodes = 2304)
        {
            if (result == null)
            {
                return false;
            }

            result.Clear();

            float cellSize = Mathf.Max(0.45f, requestedCellSize);
            padding = Mathf.Max(2f, padding);

            float minX = Mathf.Min(start.x, goal.x) - padding;
            float maxX = Mathf.Max(start.x, goal.x) + padding;
            float minZ = Mathf.Min(start.z, goal.z) - padding;
            float maxZ = Mathf.Max(start.z, goal.z) + padding;

            int width = Mathf.CeilToInt((maxX - minX) / cellSize) + 1;
            int height = Mathf.CeilToInt((maxZ - minZ) / cellSize) + 1;

            int nodeCount = width * height;
            if (nodeCount > maxNodes)
            {
                float scale = Mathf.Sqrt(nodeCount / (float)Mathf.Max(1, maxNodes));
                cellSize *= scale;
                width = Mathf.CeilToInt((maxX - minX) / cellSize) + 1;
                height = Mathf.CeilToInt((maxZ - minZ) / cellSize) + 1;
                nodeCount = width * height;
            }

            if (width < 2 || height < 2 || nodeCount <= 0)
            {
                return false;
            }

            Vector2Int startCell = WorldToCell(start, minX, minZ, cellSize, width, height);
            Vector2Int goalCell = WorldToCell(goal, minX, minZ, cellSize, width, height);

            float[] g = new float[nodeCount];
            int[] parent = new int[nodeCount];
            bool[] closed = new bool[nodeCount];
            bool[] blockedKnown = new bool[nodeCount];
            bool[] blockedValue = new bool[nodeCount];
            List<int> open = new List<int>(128);

            for (int i = 0; i < nodeCount; i++)
            {
                g[i] = float.PositiveInfinity;
                parent[i] = -1;
            }

            int startIndex = ToIndex(startCell.x, startCell.y, width);
            int goalIndex = ToIndex(goalCell.x, goalCell.y, width);
            g[startIndex] = 0f;
            open.Add(startIndex);

            int guard = 0;
            while (open.Count > 0 && guard++ < nodeCount * 2)
            {
                int openSlot = FindBestOpenSlot(open, g, goalCell, width);
                int currentIndex = open[openSlot];
                open.RemoveAt(openSlot);

                if (closed[currentIndex])
                {
                    continue;
                }

                if (currentIndex == goalIndex)
                {
                    ReconstructPath(
                        parent,
                        currentIndex,
                        startIndex,
                        width,
                        minX,
                        minZ,
                        cellSize,
                        start.y,
                        result);

                    if (result.Count > 0)
                    {
                        result[result.Count - 1] = goal;
                    }

                    return result.Count > 0;
                }

                closed[currentIndex] = true;
                Vector2Int current = FromIndex(currentIndex, width);

                for (int n = 0; n < neighbourOffsets.Length; n++)
                {
                    Vector2Int offset = neighbourOffsets[n];
                    Vector2Int next = current + offset;

                    if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height)
                    {
                        continue;
                    }

                    int nextIndex = ToIndex(next.x, next.y, width);
                    if (closed[nextIndex])
                    {
                        continue;
                    }

                    if (nextIndex != goalIndex &&
                        IsBlockedCached(
                            nextIndex,
                            next,
                            blockedKnown,
                            blockedValue,
                            minX,
                            minZ,
                            cellSize,
                            start.y,
                            agentRadius,
                            ignoreRoot))
                    {
                        continue;
                    }

                    bool diagonal = offset.x != 0 && offset.y != 0;
                    if (diagonal)
                    {
                        Vector2Int sideA = new Vector2Int(current.x + offset.x, current.y);
                        Vector2Int sideB = new Vector2Int(current.x, current.y + offset.y);

                        int sideAIndex = ToIndex(sideA.x, sideA.y, width);
                        int sideBIndex = ToIndex(sideB.x, sideB.y, width);

                        if (IsBlockedCached(
                                sideAIndex,
                                sideA,
                                blockedKnown,
                                blockedValue,
                                minX,
                                minZ,
                                cellSize,
                                start.y,
                                agentRadius,
                                ignoreRoot) ||
                            IsBlockedCached(
                                sideBIndex,
                                sideB,
                                blockedKnown,
                                blockedValue,
                                minX,
                                minZ,
                                cellSize,
                                start.y,
                                agentRadius,
                                ignoreRoot))
                        {
                            continue;
                        }
                    }

                    float stepCost = diagonal ? 1.41421356f : 1f;
                    float tentative = g[currentIndex] + stepCost;

                    if (tentative >= g[nextIndex])
                    {
                        continue;
                    }

                    g[nextIndex] = tentative;
                    parent[nextIndex] = currentIndex;

                    if (!open.Contains(nextIndex))
                    {
                        open.Add(nextIndex);
                    }
                }
            }

            result.Clear();
            return false;
        }

        /// <summary>
        /// Checks whether the navigation segment is unobstructed.
        /// </summary>
        public static bool HasClearSegment(
            Vector3 start,
            Vector3 end,
            float agentRadius,
            Transform ignoreRoot)
        {
            Vector3 delta = end - start;
            delta.y = 0f;
            float distance = delta.magnitude;

            if (distance <= 0.001f)
            {
                return true;
            }

            Vector3 origin = start + Vector3.up * 0.15f;
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                Mathf.Max(0.12f, agentRadius),
                delta / distance,
                castBuffer,
                distance,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = castBuffer[i].collider;
                if (IsBlocking(collider, ignoreRoot))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the cached blocked state for a navigation cell.
        /// </summary>
        private static bool IsBlockedCached(
            int index,
            Vector2Int cell,
            bool[] known,
            bool[] values,
            float minX,
            float minZ,
            float cellSize,
            float y,
            float radius,
            Transform ignoreRoot)
        {
            if (known[index])
            {
                return values[index];
            }

            Vector3 world = CellToWorld(cell, minX, minZ, cellSize, y);
            bool blocked = IsBlocked(world, radius, ignoreRoot);
            known[index] = true;
            values[index] = blocked;
            return blocked;
        }

        /// <summary>
        /// Checks whether the supplied navigation cell is blocked.
        /// </summary>
        private static bool IsBlocked(
            Vector3 world,
            float radius,
            Transform ignoreRoot)
        {
            int count = Physics.OverlapSphereNonAlloc(
                world + Vector3.up * 0.15f,
                Mathf.Max(0.12f, radius),
                overlapBuffer,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                if (IsBlocking(overlapBuffer[i], ignoreRoot))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether a collider should block enemy navigation.
        /// </summary>
        private static bool IsBlocking(
            Collider collider,
            Transform ignoreRoot)
        {
            if (collider == null || collider.isTrigger)
            {
                return false;
            }

            if (ignoreRoot != null &&
                collider.transform.IsChildOf(ignoreRoot))
            {
                return false;
            }

            // Characters move independently and should not become permanent grid walls.
            if (collider is CharacterController)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds the lowest-cost open node in the current A* search.
        /// </summary>
        private static int FindBestOpenSlot(
            List<int> open,
            float[] g,
            Vector2Int goal,
            int width)
        {
            int bestSlot = 0;
            float bestF = float.PositiveInfinity;
            float bestH = float.PositiveInfinity;

            for (int i = 0; i < open.Count; i++)
            {
                int index = open[i];
                Vector2Int cell = FromIndex(index, width);
                float h = Octile(cell, goal);
                float f = g[index] + h;

                if (f < bestF ||
                    (Mathf.Approximately(f, bestF) && h < bestH))
                {
                    bestF = f;
                    bestH = h;
                    bestSlot = i;
                }
            }

            return bestSlot;
        }

        /// <summary>
        /// Returns the octile-grid heuristic used by A*.
        /// </summary>
        private static float Octile(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            int diagonal = Mathf.Min(dx, dy);
            int straight = Mathf.Max(dx, dy) - diagonal;
            return diagonal * 1.41421356f + straight;
        }

        /// <summary>
        /// Builds world-space waypoints from the completed A* parent chain.
        /// </summary>
        private static void ReconstructPath(
            int[] parent,
            int current,
            int start,
            int width,
            float minX,
            float minZ,
            float cellSize,
            float y,
            List<Vector3> result)
        {
            result.Clear();
            int guard = 0;

            while (current != start && current >= 0 && guard++ < parent.Length)
            {
                Vector2Int cell = FromIndex(current, width);
                result.Add(CellToWorld(cell, minX, minZ, cellSize, y));
                current = parent[current];
            }

            result.Reverse();
        }

        /// <summary>
        /// Returns the world to cell.
        /// </summary>
        private static Vector2Int WorldToCell(
            Vector3 world,
            float minX,
            float minZ,
            float cellSize,
            int width,
            int height)
        {
            int x = Mathf.Clamp(
                Mathf.RoundToInt((world.x - minX) / cellSize),
                0,
                width - 1);

            int z = Mathf.Clamp(
                Mathf.RoundToInt((world.z - minZ) / cellSize),
                0,
                height - 1);

            return new Vector2Int(x, z);
        }

        /// <summary>
        /// Returns the cell to world.
        /// </summary>
        private static Vector3 CellToWorld(
            Vector2Int cell,
            float minX,
            float minZ,
            float cellSize,
            float y)
        {
            return new Vector3(
                minX + cell.x * cellSize,
                y,
                minZ + cell.y * cellSize);
        }

        /// <summary>
        /// Converts a grid coordinate into the flat navigation-array index.
        /// </summary>
        private static int ToIndex(int x, int y, int width)
        {
            return y * width + x;
        }

        /// <summary>
        /// Converts a flat navigation-array index back into a grid coordinate.
        /// </summary>
        private static Vector2Int FromIndex(int index, int width)
        {
            return new Vector2Int(index % width, index / width);
        }
    }
}
