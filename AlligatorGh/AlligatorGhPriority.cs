using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace AlligatorGh
{
    public class AlligatorGhPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += Instances_CanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private void Instances_CanvasCreated(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= Instances_CanvasCreated;

            GH_DocumentEditor documentEditor = Instances.DocumentEditor;
            if (documentEditor == null)
                return;

            ToolStripItem[] displayMenuArr = documentEditor.MainMenuStrip.Items.Find("mnuDisplay", false);
            if (displayMenuArr.Length == 0)
                return;

            ToolStripDropDownItem displayMenu = displayMenuArr[0] as ToolStripDropDownItem;
            if (displayMenu == null)
                return;

            // Check if already exists
            if (displayMenu.DropDownItems.Find("AlligatorPluginManager", false).Length > 0)
                return;

            ToolStripMenuItem managerMenuItem = new ToolStripMenuItem("Alligator Plugin Manager");
            managerMenuItem.Name = "AlligatorPluginManager";
            managerMenuItem.Click += (s, e) =>
            {
                PluginManagerForm form = new PluginManagerForm();
                form.Show(documentEditor);
            };

            // Insert near the top, after standard GH options
            displayMenu.DropDownItems.Insert(3, managerMenuItem);

            // Wait for application idle to apply initial layout so that all plugins have had time to load
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, System.EventArgs e)
        {
            Application.Idle -= Application_Idle;
            PluginManager.ApplyLayout();
        }
    }
}
