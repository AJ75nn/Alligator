using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace AlligatorGh.Components.UI.PlugInManager
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
                PluginManagerFrm form = new PluginManagerFrm();
                form.Show(documentEditor);
            };

            // Insert near the top, after standard GH options
            displayMenu.DropDownItems.Insert(3, managerMenuItem);

            // Wait for the document editor to fully load and show
            documentEditor.Shown += DocumentEditor_Shown;
        }

        private void DocumentEditor_Shown(object sender, System.EventArgs e)
        {
            // Grasshopper editor has shown, ribbon is populated.
            // We apply our layout slightly delayed to ensure all 3rd party plugins finished injecting tabs.
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 500; // 500ms delay
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                timer.Dispose();
                PluginManager.ApplyLayout();
            };
            timer.Start();
        }
    }
}
