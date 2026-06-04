using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using AlligatorCore;

namespace AlligatorGh
{
    public class InfiniteLineComponent : GH_Component
    {
        public InfiniteLineComponent()
          : base("InfiniteLine", "XLine",
              "Creates an infinite line from a base point and direction.",
              "Alligator", "Geometry")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Base Point", "P", "Origin point for the line", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "D", "Direction vector for the line", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "The length to extend the line", GH_ParamAccess.item, 1000.0);
            pManager.AddBooleanParameter("Both Sides", "B", "If true, acts as an XLine extending both ways. If false, acts as a Ray.", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Line", "L", "Generated line (XLine or Ray)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d basePoint = Point3d.Unset;
            Vector3d direction = Vector3d.Unset;
            double length = 1000.0;
            bool bothSides = true;

            if (!DA.GetData(0, ref basePoint)) return;
            if (!DA.GetData(1, ref direction)) return;
            DA.GetData(2, ref length);
            DA.GetData(3, ref bothSides);

            try
            {
                // Rely on the Core engine for generating the XLine or Ray
                Curve xLine = XLineEngine.CreateXLine(basePoint, direction, length, bothSides);
                DA.SetData(0, xLine);
            }
            catch (ArgumentException ex)
            {
                // Surface validation errors as Grasshopper component errors
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        // Generate a stable new Guid for this component
        public override Guid ComponentGuid => new Guid("B5D0F55A-8E2E-4AB8-9E11-209B1A0E4822");
    }
}
