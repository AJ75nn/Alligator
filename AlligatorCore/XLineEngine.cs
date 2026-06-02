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
        // We use an extremely large number close to Rhino's maximum extents.
        // Rhino generally uses double precision. 1e15 is right at the boundary of safe integer precision for doubles (2^53 ~ 9e15).
        private const double InfinityScale = 1e15;

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

        /// <summary>
        /// Creates a horizontal XLine through a point (using World XY plane X-axis).
        /// </summary>
        public static Line CreateHorizontalXLine(Point3d point)
        {
            return CreateXLine(point, Vector3d.XAxis);
        }

        /// <summary>
        /// Creates a vertical XLine through a point (using World XY plane Y-axis).
        /// </summary>
        public static Line CreateVerticalXLine(Point3d point)
        {
            return CreateXLine(point, Vector3d.YAxis);
        }

        /// <summary>
        /// Creates an XLine at a specific angle from the World X-axis.
        /// </summary>
        public static Line CreateAngledXLine(Point3d point, double radians)
        {
            Vector3d direction = new Vector3d(Math.Cos(radians), Math.Sin(radians), 0);
            return CreateXLine(point, direction);
        }

        /// <summary>
        /// Creates an XLine that bisects the angle defined by three points (vertex, start, end).
        /// </summary>
        public static Line CreateBisectingXLine(Point3d vertex, Point3d p1, Point3d p2)
        {
            Vector3d v1 = p1 - vertex;
            Vector3d v2 = p2 - vertex;

            if (v1.IsZero || v2.IsZero)
                throw new ArgumentException("Bisect points cannot be coincident with the vertex.");

            v1.Unitize();
            v2.Unitize();

            Vector3d bisector = v1 + v2;

            // If vectors are exactly opposite, the sum is zero
            if (bisector.IsZero)
            {
                // In this case, rotate v1 by 90 degrees around Z axis (assuming XY plane for 2D AutoCAD parity)
                bisector = new Vector3d(-v1.Y, v1.X, v1.Z);
            }

            return CreateXLine(vertex, bisector);
        }

        /// <summary>
        /// Creates an XLine offset from an existing line by a given distance.
        /// </summary>
        public static Line CreateOffsetXLine(Line referenceLine, double distance, Point3d sidePoint)
        {
            Vector3d dir = referenceLine.Direction;
            if (dir.IsZero)
                throw new ArgumentException("Reference line has zero length.");

            dir.Unitize();

            // Assume working in XY plane for offset direction (similar to AutoCAD default behavior)
            // Normal to the line in XY plane: (-Y, X, 0)
            Vector3d normal = new Vector3d(-dir.Y, dir.X, 0);
            normal.Unitize();

            // Determine if sidePoint is in the direction of the normal or opposite
            Vector3d toSide = sidePoint - referenceLine.From;
            double dot = normal * toSide;

            if (dot < 0)
            {
                normal = -normal;
            }

            Point3d newBasePoint = referenceLine.From + normal * distance;
            return CreateXLine(newBasePoint, referenceLine.Direction);
        }
    }
}
