using NUnit.Framework;
using ReturnVector.Enemies;
using UnityEngine;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for EnemyProjectileMath.
    /// </summary>
    public sealed class EnemyProjectileMathTests
    {
        [Test]
        public void PointCrossingSegment_HasZeroDistance()
        {
            float distance =
                EnemyProjectileMath.DistancePointToSegment(
                    new Vector3(1f, 0f, 0f),
                    Vector3.zero,
                    new Vector3(2f, 0f, 0f));

            Assert.AreEqual(0f, distance, 0.0001f);
        }

        [Test]
        public void PointPastSegment_UsesNearestEndpoint()
        {
            float distance =
                EnemyProjectileMath.DistancePointToSegment(
                    new Vector3(3f, 0f, 0f),
                    Vector3.zero,
                    new Vector3(2f, 0f, 0f));

            Assert.AreEqual(1f, distance, 0.0001f);
        }
    }
}
