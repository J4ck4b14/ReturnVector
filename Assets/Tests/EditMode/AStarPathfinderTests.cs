using System.Collections.Generic;
using NUnit.Framework;
using ReturnVector.Enemies;
using UnityEngine;

// Script summary: Contains Edit Mode coverage for A Star Pathfinder.

namespace ReturnVector.Tests
{
    public sealed class AStarPathfinderTests
    {
        /// <summary>
        /// Verifies that blocked strAIght route finds path around wall.
        /// </summary>
        [Test]
        public void BlockedStraightRoute_FindsPathAroundWall()
        {
            GameObject wall =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);

            wall.name = "Wall";
            wall.transform.position = new Vector3(0f, 0.95f, 0f);
            wall.transform.localScale = new Vector3(1.2f, 2f, 4f);
            Physics.SyncTransforms();

            Vector3 start = new Vector3(-3f, 0.95f, 0f);
            Vector3 goal = new Vector3(3f, 0.95f, 0f);
            List<Vector3> path = new List<Vector3>();

            Assert.IsFalse(
                AStarPathfinder.HasClearSegment(
                    start,
                    goal,
                    0.4f,
                    null));

            Assert.IsTrue(
                AStarPathfinder.TryFindPath(
                    start,
                    goal,
                    0.7f,
                    0.4f,
                    null,
                    path,
                    4f));

            Assert.Greater(path.Count, 1);

            Object.DestroyImmediate(wall);
        }
    }
}
