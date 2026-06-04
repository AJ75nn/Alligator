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
        private static Color _origWireDefault;
        private static Color _origWireSelectedA;
        private static Color _origWireSelectedB;
        private static Color _origWireEmpty;

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
            pManager.AddColourParameter("Wire Default", "WD", "Custom default wire color", GH_ParamAccess.item);
            pManager.AddColourParameter("Wire Selected A", "WA", "Custom selected wire color (A)", GH_ParamAccess.item);
            pManager.AddColourParameter("Wire Selected B", "WB", "Custom selected wire color (B)", GH_ParamAccess.item);
            pManager.AddColourParameter("Wire Empty", "WE", "Custom empty/null wire color", GH_ParamAccess.item);

            // Make colors optional so users can pick and choose
            for (int i = 1; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }
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
                _origWireDefault = GH_Skin.wire_default;
                _origWireSelectedA = GH_Skin.wire_selected_a;
                _origWireSelectedB = GH_Skin.wire_selected_b;
                _origWireEmpty = GH_Skin.wire_empty;
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
                GH_Skin.wire_default = _origWireDefault;
                GH_Skin.wire_selected_a = _origWireSelectedA;
                GH_Skin.wire_selected_b = _origWireSelectedB;
                GH_Skin.wire_empty = _origWireEmpty;

                Color back = Color.Empty;
                Color grid = Color.Empty;
                Color edge = Color.Empty;
                Color shade = Color.Empty;
                Color wDef = Color.Empty;
                Color wSelA = Color.Empty;
                Color wSelB = Color.Empty;
                Color wEmp = Color.Empty;

                if (DA.GetData(1, ref back)) GH_Skin.canvas_back = back;
                if (DA.GetData(2, ref grid)) GH_Skin.canvas_grid = grid;
                if (DA.GetData(3, ref edge)) GH_Skin.canvas_edge = edge;
                if (DA.GetData(4, ref shade)) GH_Skin.canvas_shade = shade;
                if (DA.GetData(5, ref wDef)) GH_Skin.wire_default = wDef;
                if (DA.GetData(6, ref wSelA)) GH_Skin.wire_selected_a = wSelA;
                if (DA.GetData(7, ref wSelB)) GH_Skin.wire_selected_b = wSelB;
                if (DA.GetData(8, ref wEmp)) GH_Skin.wire_empty = wEmp;
            }
            else
            {
                // Reset to default
                GH_Skin.canvas_back = _origBack;
                GH_Skin.canvas_grid = _origGrid;
                GH_Skin.canvas_edge = _origEdge;
                GH_Skin.canvas_shade = _origShade;
                GH_Skin.wire_default = _origWireDefault;
                GH_Skin.wire_selected_a = _origWireSelectedA;
                GH_Skin.wire_selected_b = _origWireSelectedB;
                GH_Skin.wire_empty = _origWireEmpty;
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
