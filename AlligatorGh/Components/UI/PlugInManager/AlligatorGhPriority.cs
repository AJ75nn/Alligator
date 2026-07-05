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

            // Find or create "Alligator" main menu and "UI Control" submenu via the
            // shared bootstrapper so this logic is not duplicated across modules.
            ToolStripMenuItem alligatorMenu = AlligatorMenuBootstrapper.GetOrCreateAlligatorMenu(documentEditor);
            if (alligatorMenu == null)
                return;

            ToolStripMenuItem uiControlMenu = AlligatorMenuBootstrapper.GetOrCreateSubMenu(alligatorMenu, "mnuAlligatorUIControl", "UI Control");
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
            // 3rd-party plugins inject ribbon tabs asynchronously after the editor shows.
            // A fixed 500ms delay misses slow-loading plugins. Instead, poll until the tab
            // set stabilizes (unchanged across two consecutive samples), with a hard timeout
            // so the saved layout is always applied eventually.
            var timer = new System.Windows.Forms.Timer();
            int lastCount = -1;
            int stableTicks = 0;
            int samples = 0;
            const int SampleIntervalMs = 250;
            const int RequiredStableSamples = 2;
            const int MaxSamples = 24; // ~6s hard timeout

            timer.Interval = SampleIntervalMs;
            timer.Tick += (s, args) =>
            {
                samples++;
                int currentCount = PluginManager.GetAllTabs().Count;

                if (currentCount == lastCount)
                {
                    stableTicks++;
                }
                else
                {
                    stableTicks = 0;
                    lastCount = currentCount;
                }

                if (stableTicks >= RequiredStableSamples || samples >= MaxSamples)
                {
                    timer.Stop();
                    timer.Dispose();
                    PluginManager.ApplyLayout();
                }
            };
            timer.Start();
        }
    }
}
