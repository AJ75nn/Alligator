using System.Linq;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Ribbon;

namespace AlligatorGh.Components.UI
{
    /// <summary>
    /// Provides a facade for safely accessing and retrieving core Grasshopper UI controls.
    /// </summary>
    public class GrasshopperUIFacade
    {
        /// <summary>
        /// Retrieves the main Grasshopper Document Editor instance.
        /// </summary>
        /// <returns>The active GH_DocumentEditor, or null if it has not been instantiated.</returns>
        public GH_DocumentEditor GetDocumentEditor()
        {
            // The active document editor is a singleton managed by the Grasshopper Instances class.
            // It must be validated against null to prevent runtime exceptions during early Rhino startup.
            GH_DocumentEditor editor = Grasshopper.Instances.DocumentEditor;

            if (editor == null)
            {
                Rhino.RhinoApp.WriteLine("Warning: Grasshopper Document Editor is not currently loaded.");
                return null;
            }

            return editor;
        }

        /// <summary>
        /// Retrieves the Main Menu Strip (Area 1) from the Grasshopper UI.
        /// </summary>
        /// <returns>The MenuStrip object, or null if not found.</returns>
        public MenuStrip GetMainMenu()
        {
            GH_DocumentEditor editor = GetDocumentEditor();
            if (editor == null) return null;

            // The MainMenuStrip is explicitly exposed as a public property on the GH_DocumentEditor.
            return editor.MainMenuStrip;
        }

        /// <summary>
        /// Retrieves the custom Grasshopper Ribbon control (Areas 2, 3, and 4).
        /// </summary>
        /// <returns>The GH_Ribbon control, or null if not found.</returns>
        public GH_Ribbon GetRibbon()
        {
            GH_DocumentEditor editor = GetDocumentEditor();
            if (editor == null) return null;

            // The GH_Ribbon is a custom WinForms control embedded within the editor's Controls collection.
            // We iterate through the top-level controls to find the first instance matching the GH_Ribbon type.
            return editor.Controls.OfType<GH_Ribbon>().FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the Document Tab strip (Area 5) handling multiple open files.
        /// </summary>
        /// <returns>The Control, or null if not found.</returns>
        public Control GetDocumentTabs()
        {
            GH_DocumentEditor editor = GetDocumentEditor();
            if (editor == null) return null;

            // Similar to the ribbon, the document tab strip is a child control of the main editor form.
            return editor.Controls.Cast<Control>().FirstOrDefault(c => c.GetType().Name == "GH_DocumentTabs" || c.GetType().Name == "GH_DocumentTabStrip");
        }

        /// <summary>
        /// Retrieves the Canvas Toolbar (Area 6).
        /// </summary>
        /// <returns>The Control, or null if not found.</returns>
        public Control GetCanvasToolbar()
        {
            GH_DocumentEditor editor = GetDocumentEditor();
            if (editor == null) return null;

            // The canvas toolbar is nested alongside the canvas itself.
            // We extract it directly from the editor's control collection.
            return editor.Controls.Cast<Control>().FirstOrDefault(c => c.GetType().Name == "GH_CanvasToolbar");
        }
    }
}
