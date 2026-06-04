using System;
using Rhino;
using Rhino.Geometry;

namespace AlligatorCore
{
    /// <summary>
    /// Stateless engine responsible for generating XLines and Rays based on dynamic lengths.
    /// </summary>
    public static class XLineEngine
    {
        /// <summary>
        /// Creates a line (XLine or Ray) based on a length and a direction.
        /// By returning a PolylineCurve with the basePoint as an explicit vertex, we prevent Rhino's display pipeline
        /// from losing precision when interpolating massive lengths, preventing flickering.
        /// </summary>
        /// <param name="basePoint">The origin point of the line.</param>
        /// <param name="direction">The direction vector.</param>
        /// <param name="length">The distance to extend the line.</param>
        /// <param name="bothSides">If true, extends in both directions (XLine). If false, extends only forward (Ray).</param>
        /// <returns>A Curve geometry.</returns>
        public static Curve CreateXLine(Point3d basePoint, Vector3d direction, double length, bool bothSides)
        {
            if (!basePoint.IsValid)
                throw new ArgumentException("Base point is invalid.", nameof(basePoint));

            if (!direction.IsValid || direction.IsZero)
                throw new ArgumentException("Direction vector is invalid or zero.", nameof(direction));

            if (length <= 0)
                throw new ArgumentException("Length must be positive.", nameof(length));

            direction.Unitize();

            Point3d end = basePoint + (direction * length);

            if (bothSides)
            {
                Point3d start = basePoint - (direction * length);
                Polyline poly = new Polyline() { start, basePoint, end };
                return poly.ToNurbsCurve();
            }
            else
            {
                Polyline poly = new Polyline() { basePoint, end };
                return poly.ToNurbsCurve();
            }
        }

        /// <summary>
        /// Creates an XLine passing through two points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="length">The distance to extend the line.</param>
        /// <param name="bothSides">If true, extends in both directions (XLine). If false, extends only forward (Ray).</param>
        /// <returns>A Curve geometry.</returns>
        public static Curve CreateXLine(Point3d p1, Point3d p2, double length, bool bothSides)
        {
            if (!p1.IsValid)
                throw new ArgumentException("First point is invalid.", nameof(p1));

            if (!p2.IsValid)
                throw new ArgumentException("Second point is invalid.", nameof(p2));

            Vector3d direction = p2 - p1;

            if (direction.IsZero)
                throw new ArgumentException("Points are coincident; cannot determine direction.", nameof(p2));

            direction.Unitize();

            Point3d end = p1 + (direction * length);

            if (bothSides)
            {
                Point3d start = p1 - (direction * length);
                Polyline poly = new Polyline() { start, p1, p2, end };
                return poly.ToNurbsCurve();
            }
            else
            {
                Polyline poly = new Polyline() { p1, p2, end };
                return poly.ToNurbsCurve();
            }
        }

        /// <summary>
        /// Creates a horizontal XLine through a point.
        /// </summary>
        public static Curve CreateHorizontalXLine(Point3d point, double length, bool bothSides)
        {
            return CreateXLine(point, Vector3d.XAxis, length, bothSides);
        }

        /// <summary>
        /// Creates a vertical XLine through a point.
        /// </summary>
        public static Curve CreateVerticalXLine(Point3d point, double length, bool bothSides)
        {
            return CreateXLine(point, Vector3d.YAxis, length, bothSides);
        }

        /// <summary>
        /// Creates an XLine at a specific angle from the World X-axis.
        /// </summary>
        public static Curve CreateAngledXLine(Point3d point, double radians, double length, bool bothSides)
        {
            Vector3d direction = new Vector3d(Math.Cos(radians), Math.Sin(radians), 0);
            return CreateXLine(point, direction, length, bothSides);
        }

        /// <summary>
        /// Creates an XLine that bisects the angle defined by three points (vertex, start, end).
        /// </summary>
        public static Curve CreateBisectingXLine(Point3d vertex, Point3d p1, Point3d p2, double length, bool bothSides)
        {
            Vector3d v1 = p1 - vertex;
            Vector3d v2 = p2 - vertex;

            if (v1.IsZero || v2.IsZero)
                throw new ArgumentException("Bisect points cannot be coincident with the vertex.");

            v1.Unitize();
            v2.Unitize();

            Vector3d bisector = v1 + v2;

            if (bisector.IsZero)
            {
                bisector = new Vector3d(-v1.Y, v1.X, v1.Z);
            }

            return CreateXLine(vertex, bisector, length, bothSides);
        }

        /// <summary>
        /// Creates an XLine offset from an existing curve by a given distance.
        /// </summary>
        public static Curve CreateOffsetXLine(Curve referenceCurve, double distance, Point3d sidePoint, double length, bool bothSides)
        {
            if (referenceCurve == null || !referenceCurve.IsLinear(RhinoMath.ZeroTolerance))
                throw new ArgumentException("Reference curve is not linear.");

            Line refLine = new Line(referenceCurve.PointAtStart, referenceCurve.PointAtEnd);
            Vector3d dir = refLine.Direction;
            if (dir.IsZero)
                throw new ArgumentException("Reference line has zero length.");

            dir.Unitize();

            // Assume working in XY plane for offset direction (similar to AutoCAD default behavior)
            // Normal to the line in XY plane: (-Y, X, 0)
            Vector3d normal = new Vector3d(-dir.Y, dir.X, 0);
            normal.Unitize();

            // Determine if sidePoint is in the direction of the normal or opposite
            Vector3d toSide = sidePoint - refLine.From;
            double dot = normal * toSide;

            if (dot < 0)
            {
                normal = -normal;
            }

            Point3d newBasePoint = refLine.From + normal * distance;
            return CreateXLine(newBasePoint, refLine.Direction, length, bothSides);
        }
    }
}
