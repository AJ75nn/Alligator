using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace AlligatorGh
{
    public class CanvasSettingsComponent : GH_Component
    {
        // Static backup of original canvas settings
        private static bool _originalSaved = false;
        private static Color _origBack;
        private static Color _origGrid;
        private static Color _origEdge;
        private static Color _origShade;

        public CanvasSettingsComponent()
          : base("CanvasSettings", "CanvasSet",
              "Customizes Grasshopper's canvas appearance.",
              "Alligator", "Canvas")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Apply", "A", "If true, applies the custom colors. If false, resets to default.", GH_ParamAccess.item, false);
            pManager.AddColourParameter("Background", "B", "Custom background color", GH_ParamAccess.item);
            pManager.AddColourParameter("Grid", "G", "Custom grid lines color", GH_ParamAccess.item);
            pManager.AddColourParameter("Edge", "E", "Custom edge color of the canvas", GH_ParamAccess.item);
            pManager.AddColourParameter("Shade", "S", "Custom shadow/shade color", GH_ParamAccess.item);

            // Make colors optional so users can pick and choose
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // No outputs needed for UI modification
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Save originals safely the first time
            if (!_originalSaved)
            {
                _origBack = GH_Skin.canvas_back;
                _origGrid = GH_Skin.canvas_grid;
                _origEdge = GH_Skin.canvas_edge;
                _origShade = GH_Skin.canvas_shade;
                _originalSaved = true;
            }

            bool apply = false;
            if (!DA.GetData(0, ref apply)) return;

            if (apply)
            {
                // Reset all to default first to ensure disconnected inputs fall back correctly
                GH_Skin.canvas_back = _origBack;
                GH_Skin.canvas_grid = _origGrid;
                GH_Skin.canvas_edge = _origEdge;
                GH_Skin.canvas_shade = _origShade;

                Color back = Color.Empty;
                Color grid = Color.Empty;
                Color edge = Color.Empty;
                Color shade = Color.Empty;

                if (DA.GetData(1, ref back)) GH_Skin.canvas_back = back;
                if (DA.GetData(2, ref grid)) GH_Skin.canvas_grid = grid;
                if (DA.GetData(3, ref edge)) GH_Skin.canvas_edge = edge;
                if (DA.GetData(4, ref shade)) GH_Skin.canvas_shade = shade;
            }
            else
            {
                // Reset to default
                GH_Skin.canvas_back = _origBack;
                GH_Skin.canvas_grid = _origGrid;
                GH_Skin.canvas_edge = _origEdge;
                GH_Skin.canvas_shade = _origShade;
            }

            // Force the canvas to redraw to show changes immediately
            if (Instances.ActiveCanvas != null)
            {
                Instances.ActiveCanvas.Invalidate();
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("4A2B9C81-8F33-4D90-BE2A-2C471E2B7A8D");
    }
}
