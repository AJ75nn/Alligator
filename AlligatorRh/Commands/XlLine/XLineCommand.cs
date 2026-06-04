using System;
using AlligatorCore;
using AlligatorCore.Curve;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace AlligatorRh.Commands.XlLine
{
    public class XLineCommand : Command
    {
        public XLineCommand()
        {
            Instance = this;
        }

        public static XLineCommand Instance { get; private set; }

        public override string EnglishName => "XLine";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Specify a point");

            int opHor = gp.AddOption("Hor");
            int opVer = gp.AddOption("Ver");
            int opAng = gp.AddOption("Ang");
            int opBisect = gp.AddOption("Bisect");
            int opOffset = gp.AddOption("Offset");

            var result = gp.Get();

            if (result == GetResult.Cancel)
                return Result.Cancel;

            System.Drawing.Color layerColor = doc.Layers.CurrentLayer.Color;

            if (result == GetResult.Option)
            {
                int opIndex = gp.Option().Index;
                if (opIndex == opHor)
                    return RunDirectionalMode(doc, Vector3d.XAxis, layerColor);
                if (opIndex == opVer)
                    return RunDirectionalMode(doc, Vector3d.YAxis, layerColor);
                if (opIndex == opAng)
                    return RunAngledMode(doc, layerColor);
                if (opIndex == opBisect)
                    return RunBisectMode(doc, layerColor);
                if (opIndex == opOffset)
                    return RunOffsetMode(doc, layerColor);

                return Result.Cancel;
            }

            if (result == GetResult.Point)
            {
                Point3d basePoint = gp.Point();
                return RunThroughMode(doc, basePoint, layerColor);
            }

            return Result.Success;
        }

        private double GetFrustumLength(RhinoDoc doc)
        {
            if (doc.Views.ActiveView != null && doc.Views.ActiveView.ActiveViewport != null)
            {
                var bbox = doc.Views.ActiveView.ActiveViewport.GetFrustumBoundingBox();
                if (bbox.IsValid)
                {
                    return bbox.Diagonal.Length * 1000.0;
                }
            }
            return 1e9; // Fallback
        }

        private Result RunDirectionalMode(RhinoDoc doc, Vector3d direction, System.Drawing.Color layerColor)
        {
            while (true)
            {
                GetPoint gp = new GetPoint();
                gp.SetCommandPrompt("Specify through point");

                gp.DynamicDraw += (sender, e) =>
                {
                    try
                    {
                        double dynamicLength = GetFrustumLength(doc);
                        Curve xline = XLineEngine.CreateXLine(e.CurrentPoint, direction, dynamicLength, true);
                        e.Display.DrawCurve(xline, layerColor);
                    }
                    catch (ArgumentException) {}
                };

                var result = gp.Get();

                if (result == GetResult.Cancel || result != GetResult.Point)
                    break;

                try
                {
                    double dynamicLength = GetFrustumLength(doc);
                    Curve xline = XLineEngine.CreateXLine(gp.Point(), direction, dynamicLength, true);
                    doc.Objects.AddCurve(xline);
                    doc.Views.Redraw();
                }
                catch (ArgumentException ex)
                {
                    RhinoApp.WriteLine($"Error creating XLine: {ex.Message}");
                }
            }
            return Result.Success;
        }

        private Result RunThroughMode(RhinoDoc doc, Point3d basePoint, System.Drawing.Color layerColor)
        {
            while (true)
            {
                GetPoint gpThrough = new GetPoint();
                gpThrough.SetCommandPrompt("Specify through point");
                gpThrough.SetBasePoint(basePoint, true);

                gpThrough.DynamicDraw += (sender, e) =>
                {
                    try
                    {
                        double dynamicLength = GetFrustumLength(doc);
                        Curve xline = XLineEngine.CreateXLine(basePoint, e.CurrentPoint, dynamicLength, true);
                        e.Display.DrawCurve(xline, layerColor);
                    }
                    catch (ArgumentException) {}
                };

                var throughResult = gpThrough.Get();

                if (throughResult == GetResult.Cancel || throughResult != GetResult.Point)
                    break;

                try
                {
                    double dynamicLength = GetFrustumLength(doc);
                    Curve xline = XLineEngine.CreateXLine(basePoint, gpThrough.Point(), dynamicLength, true);
                    doc.Objects.AddCurve(xline);
                    doc.Views.Redraw();
                }
                catch (ArgumentException ex)
                {
                    RhinoApp.WriteLine($"Error creating XLine: {ex.Message}");
                }
            }
            return Result.Success;
        }

        private Result RunAngledMode(RhinoDoc doc, System.Drawing.Color layerColor)
        {
            GetNumber gn = new GetNumber();
            gn.SetCommandPrompt("Enter angle of xline");
            gn.SetDefaultNumber(0.0);

            int opReference = gn.AddOption("Reference");

            var resNum = gn.Get();
            if (resNum == GetResult.Cancel) return Result.Cancel;

            double angleRad = 0;

            if (resNum == GetResult.Option && gn.Option().Index == opReference)
            {
                Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject();
                go.SetCommandPrompt("Select a line object");
                go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
                go.Get();
                if (go.CommandResult() != Result.Success) return go.CommandResult();

                Curve crv = go.Object(0).Curve();
                if (crv != null && crv.IsLinear())
                {
                    Line refLine = new Line(crv.PointAtStart, crv.PointAtEnd);

                    GetNumber gnRefAngle = new GetNumber();
                    gnRefAngle.SetCommandPrompt("Enter angle of xline from reference");
                    gnRefAngle.Get();
                    if (gnRefAngle.CommandResult() != Result.Success) return gnRefAngle.CommandResult();

                    Vector3d dir = refLine.Direction;
                    dir.Unitize();
                    dir.Rotate(RhinoMath.ToRadians(gnRefAngle.Number()), Vector3d.ZAxis);
                    return RunDirectionalMode(doc, dir, layerColor);
                }
                else
                {
                    RhinoApp.WriteLine("Invalid reference object.");
                    return Result.Failure;
                }
            }
            else if (resNum == GetResult.Number)
            {
                angleRad = RhinoMath.ToRadians(gn.Number());
            }
            else
            {
                return Result.Cancel;
            }

            Vector3d direction = new Vector3d(Math.Cos(angleRad), Math.Sin(angleRad), 0);
            return RunDirectionalMode(doc, direction, layerColor);
        }

        private Result RunBisectMode(RhinoDoc doc, System.Drawing.Color layerColor)
        {
            GetPoint gpVertex = new GetPoint();
            gpVertex.SetCommandPrompt("Specify angle vertex point");
            if (gpVertex.Get() != GetResult.Point) return Result.Cancel;
            Point3d vertex = gpVertex.Point();

            GetPoint gpStart = new GetPoint();
            gpStart.SetCommandPrompt("Specify angle start point");
            gpStart.SetBasePoint(vertex, true);
            if (gpStart.Get() != GetResult.Point) return Result.Cancel;
            Point3d p1 = gpStart.Point();

            while(true)
            {
                GetPoint gpEnd = new GetPoint();
                gpEnd.SetCommandPrompt("Specify angle end point");
                gpEnd.SetBasePoint(vertex, true);

                gpEnd.DynamicDraw += (sender, e) =>
                {
                    try
                    {
                        double dynamicLength = GetFrustumLength(doc);
                        Curve xline = XLineEngine.CreateBisectingXLine(vertex, p1, e.CurrentPoint, dynamicLength, true);
                        e.Display.DrawCurve(xline, layerColor);
                    }
                    catch (ArgumentException) {}
                };

                var resEnd = gpEnd.Get();
                if (resEnd == GetResult.Cancel || resEnd != GetResult.Point) break;

                try
                {
                    double dynamicLength = GetFrustumLength(doc);
                    Curve xline = XLineEngine.CreateBisectingXLine(vertex, p1, gpEnd.Point(), dynamicLength, true);
                    doc.Objects.AddCurve(xline);
                    doc.Views.Redraw();
                }
                catch(ArgumentException ex)
                {
                    RhinoApp.WriteLine($"Error creating XLine: {ex.Message}");
                }
            }

            return Result.Success;
        }

        private Result RunOffsetMode(RhinoDoc doc, System.Drawing.Color layerColor)
        {
            GetNumber gn = new GetNumber();
            gn.SetCommandPrompt("Specify offset distance");
            int opThrough = gn.AddOption("Through");
            var resDist = gn.Get();

            if (resDist == GetResult.Cancel) return Result.Cancel;

            bool isThrough = false;
            double distance = 0;

            if (resDist == GetResult.Option && gn.Option().Index == opThrough)
            {
                isThrough = true;
            }
            else if (resDist == GetResult.Number)
            {
                distance = gn.Number();
                if (distance <= 0)
                {
                    RhinoApp.WriteLine("Distance must be positive.");
                    return Result.Failure;
                }
            }
            else
            {
                return Result.Cancel;
            }

            Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject();
            go.SetCommandPrompt("Select a line object");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
            go.Get();
            if (go.CommandResult() != Result.Success) return go.CommandResult();

            Curve crv = go.Object(0).Curve();
            if (crv == null || !crv.IsLinear())
            {
                RhinoApp.WriteLine("Object is not a line.");
                return Result.Failure;
            }

            // Using the curve start/end directly for direction derivation
            Vector3d refDirection = crv.PointAtEnd - crv.PointAtStart;
            refDirection.Unitize();

            if (isThrough)
            {
                while (true)
                {
                    GetPoint gpThrough = new GetPoint();
                    gpThrough.SetCommandPrompt("Specify through point");

                    gpThrough.DynamicDraw += (sender, e) =>
                    {
                        try
                        {
                            double dynamicLength = GetFrustumLength(doc);
                            Curve xline = XLineEngine.CreateXLine(e.CurrentPoint, refDirection, dynamicLength, true);
                            e.Display.DrawCurve(xline, layerColor);
                        }
                        catch(ArgumentException) {}
                    };

                    var resThrough = gpThrough.Get();
                    if (resThrough == GetResult.Cancel || resThrough != GetResult.Point) break;

                    try
                    {
                        double dynamicLength = GetFrustumLength(doc);
                        Curve xline = XLineEngine.CreateXLine(gpThrough.Point(), refDirection, dynamicLength, true);
                        doc.Objects.AddCurve(xline);
                        doc.Views.Redraw();
                    }
                    catch (ArgumentException ex)
                    {
                        RhinoApp.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            else
            {
                while(true)
                {
                    GetPoint gpSide = new GetPoint();
                    gpSide.SetCommandPrompt("Specify side to offset");

                    gpSide.DynamicDraw += (sender, e) =>
                    {
                        try
                        {
                            double dynamicLength = GetFrustumLength(doc);
                            Curve xline = XLineEngine.CreateOffsetXLine(crv, distance, e.CurrentPoint, dynamicLength, true);
                            e.Display.DrawCurve(xline, layerColor);
                        }
                        catch(ArgumentException) {}
                    };

                    var resSide = gpSide.Get();
                    if (resSide == GetResult.Cancel || resSide != GetResult.Point) break;

                    try
                    {
                        double dynamicLength = GetFrustumLength(doc);
                        Curve xline = XLineEngine.CreateOffsetXLine(crv, distance, gpSide.Point(), dynamicLength, true);
                        doc.Objects.AddCurve(xline);
                        doc.Views.Redraw();
                    }
                    catch (ArgumentException ex)
                    {
                        RhinoApp.WriteLine($"Error: {ex.Message}");
                    }
                }
            }

            return Result.Success;
        }
    }
}
