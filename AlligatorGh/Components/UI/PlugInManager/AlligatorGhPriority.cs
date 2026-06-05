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

            // Find or create "Alligator" main menu
            ToolStripItem[] alligatorMenuArr = documentEditor.MainMenuStrip.Items.Find("mnuAlligator", false);
            ToolStripMenuItem alligatorMenu;
            if (alligatorMenuArr.Length == 0)
            {
                alligatorMenu = new ToolStripMenuItem("Alligator");
                alligatorMenu.Name = "mnuAlligator";
                documentEditor.MainMenuStrip.Items.Add(alligatorMenu);
            }
            else
            {
                alligatorMenu = alligatorMenuArr[0] as ToolStripMenuItem;
            }

            if (alligatorMenu == null)
                return;

            // Find or create "UI Control" submenu
            ToolStripItem[] uiControlMenuArr = alligatorMenu.DropDownItems.Find("mnuAlligatorUIControl", false);
            ToolStripMenuItem uiControlMenu;
            if (uiControlMenuArr.Length == 0)
            {
                uiControlMenu = new ToolStripMenuItem("UI Control");
                uiControlMenu.Name = "mnuAlligatorUIControl";
                alligatorMenu.DropDownItems.Add(uiControlMenu);
            }
            else
            {
                uiControlMenu = uiControlMenuArr[0] as ToolStripMenuItem;
            }

            if (uiControlMenu == null)
                return;

            // Check if "Plugin Manager" already exists
            if (uiControlMenu.DropDownItems.Find("PluginManager", false).Length > 0)
                return;

            ToolStripMenuItem managerMenuItem = new ToolStripMenuItem("Plugin Manager");
            managerMenuItem.Name = "PluginManager";
            managerMenuItem.Click += (s, e) =>
            {
                PluginManagerFrm form = new PluginManagerFrm();
                form.Show(documentEditor);
            };

            uiControlMenu.DropDownItems.Add(managerMenuItem);

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
