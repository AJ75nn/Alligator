using System;
using Rhino.Geometry;

namespace AlligatorCore
{
    /// <summary>
    /// Stateless engine responsible for generating XLines (infinite lines).
    /// </summary>
    public static class XLineEngine
    {
        // Arbitrary large scale factor to simulate "infinity" in Rhino space.
        // Rhino's typical model space limit is roughly 1e5 to 1e8 before precision issues occur.
        // We will use 1e6 which is usually safe for both float and double precision and represents 1000km if in mm.
        private const double InfinityScale = 1e6;

        /// <summary>
        /// Creates an XLine (infinite line) extending infinitely in both directions from the base point.
        /// </summary>
        /// <param name="basePoint">The origin point of the line.</param>
        /// <param name="direction">The direction vector.</param>
        /// <returns>A Line geometry that extends infinitely.</returns>
        public static Line CreateXLine(Point3d basePoint, Vector3d direction)
        {
            if (!basePoint.IsValid)
                throw new ArgumentException("Base point is invalid.", nameof(basePoint));

            if (!direction.IsValid || direction.IsZero)
                throw new ArgumentException("Direction vector is invalid or zero.", nameof(direction));

            direction.Unitize();

            // Extend in both directions
            Point3d start = basePoint - (direction * InfinityScale);
            Point3d end = basePoint + (direction * InfinityScale);

            return new Line(start, end);
        }

        /// <summary>
        /// Creates an XLine passing through two points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns>A Line geometry that extends infinitely.</returns>
        public static Line CreateXLine(Point3d p1, Point3d p2)
        {
            if (!p1.IsValid)
                throw new ArgumentException("First point is invalid.", nameof(p1));

            if (!p2.IsValid)
                throw new ArgumentException("Second point is invalid.", nameof(p2));

            Vector3d direction = p2 - p1;

            if (direction.IsZero)
                throw new ArgumentException("Points are coincident; cannot determine direction.", nameof(p2));

            return CreateXLine(p1, direction);
        }
    }
}
