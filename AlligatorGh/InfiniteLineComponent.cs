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
            pManager.AddPointParameter("Base Point", "P", "Origin point for the infinite line", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "D", "Direction vector for the infinite line", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Infinite Line", "L", "Generated infinite line", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d basePoint = Point3d.Unset;
            Vector3d direction = Vector3d.Unset;

            if (!DA.GetData(0, ref basePoint)) return;
            if (!DA.GetData(1, ref direction)) return;

            try
            {
                // Rely on the Core engine for generating the XLine
                Line xLine = XLineEngine.CreateXLine(basePoint, direction);
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
