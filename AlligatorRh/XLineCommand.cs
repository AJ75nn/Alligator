using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using AlligatorCore;

namespace AlligatorRh
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
            // 1. Get Base Point
            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Specify a point");
            var result = gp.Get();

            if (result != GetResult.Point)
                return Result.Cancel;

            Point3d basePoint = gp.Point();

            // 2. Continually ask for through points
            while (true)
            {
                GetPoint gpThrough = new GetPoint();
                gpThrough.SetCommandPrompt("Specify through point");
                gpThrough.SetBasePoint(basePoint, true);

                // Add dynamic draw event to preview the XLine
                gpThrough.DynamicDraw += (sender, e) =>
                {
                    Point3d currentPoint = e.CurrentPoint;
                    try
                    {
                        // Calculate XLine preview
                        Line xline = XLineEngine.CreateXLine(basePoint, currentPoint);
                        e.Display.DrawLine(xline, System.Drawing.Color.Black);
                    }
                    catch (ArgumentException)
                    {
                        // Coincident points or other issues, just don't draw anything
                    }
                };

                var throughResult = gpThrough.Get();

                if (throughResult == GetResult.Cancel)
                    break;

                if (throughResult == GetResult.Point)
                {
                    Point3d throughPoint = gpThrough.Point();

                    try
                    {
                        Line xline = XLineEngine.CreateXLine(basePoint, throughPoint);
                        doc.Objects.AddLine(xline);
                        doc.Views.Redraw();
                    }
                    catch (ArgumentException ex)
                    {
                        RhinoApp.WriteLine($"Error creating XLine: {ex.Message}");
                    }
                }
                else
                {
                    // If user pressed enter, esc, etc.
                    break;
                }
            }

            return Result.Success;
        }
    }
}
