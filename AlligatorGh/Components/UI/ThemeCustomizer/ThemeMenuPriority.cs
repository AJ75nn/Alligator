using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public class ThemeMenuPriority : GH_AssemblyPriority
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

            // Pass the editor to Initialize so it can apply WinForms UI skinning immediately
            ThemeManager.Initialize(documentEditor);

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

            if (alligatorMenu == null) return;

            // Find or create "Customize UI" submenu
            ToolStripItem[] customizeUIMenuArr = alligatorMenu.DropDownItems.Find("mnuAlligatorCustomizeUI", false);
            ToolStripMenuItem customizeUIMenu;
            if (customizeUIMenuArr.Length == 0)
            {
                customizeUIMenu = new ToolStripMenuItem("Customize UI");
                customizeUIMenu.Name = "mnuAlligatorCustomizeUI";
                alligatorMenu.DropDownItems.Add(customizeUIMenu);
            }
            else
            {
                customizeUIMenu = customizeUIMenuArr[0] as ToolStripMenuItem;
            }

            // Create "Theme" submenu
            if (customizeUIMenu.DropDownItems.Find("mnuAlligatorTheme", false).Length > 0)
                return;

            ToolStripMenuItem themeMenu = new ToolStripMenuItem("Theme");
            themeMenu.Name = "mnuAlligatorTheme";
            customizeUIMenu.DropDownItems.Add(themeMenu);

            // Default
            ToolStripMenuItem defaultMenuItem = new ToolStripMenuItem("Default");
            defaultMenuItem.Name = "mnuThemeDefault";
            defaultMenuItem.Click += (s, e) =>
            {
                ThemeManager.CurrentBaseTheme = "Default";
                ThemeManager.ApplyTheme(Instances.DocumentEditor);
                UpdateThemeCheckmarks();
            };

            // Dark
            ToolStripMenuItem darkMenuItem = new ToolStripMenuItem("Dark");
            darkMenuItem.Name = "mnuThemeDark";
            darkMenuItem.Click += (s, e) =>
            {
                ThemeManager.CurrentBaseTheme = "Dark";
                ThemeManager.ApplyTheme(Instances.DocumentEditor);
                UpdateThemeCheckmarks();
            };

            // Custom
            ToolStripMenuItem customMenuItem = new ToolStripMenuItem("Custom");
            customMenuItem.Name = "mnuThemeCustom";
            customMenuItem.Click += (s, e) =>
            {
                // Note: If ThemeCustomizerFrm also updates colors, ensure you update 
                // those method calls inside the form to pass Instances.DocumentEditor
                // e.g., ThemeManager.SetCustomColor("Key", color, Instances.DocumentEditor);
                //ThemeCustomizerFrm frm = new ThemeCustomizerFrm();
                //frm.Show(documentEditor);
            };

            themeMenu.DropDownItems.Add(defaultMenuItem);
            themeMenu.DropDownItems.Add(darkMenuItem);
            themeMenu.DropDownItems.Add(new ToolStripSeparator());
            themeMenu.DropDownItems.Add(customMenuItem);

            // Wait for the document editor to fully load to show proper checkmarks
            documentEditor.Shown += DocumentEditor_Shown;
        }

        private void DocumentEditor_Shown(object sender, EventArgs e)
        {
            UpdateThemeCheckmarks();
        }

        public static void UpdateThemeCheckmarks()
        {
            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null) return;

            ToolStripItem[] themeMenuArr = editor.MainMenuStrip.Items.Find("mnuAlligatorTheme", true);
            if (themeMenuArr.Length == 0) return;

            ToolStripMenuItem themeMenu = themeMenuArr[0] as ToolStripMenuItem;
            if (themeMenu == null) return;

            ToolStripMenuItem defaultItem = themeMenu.DropDownItems.Find("mnuThemeDefault", false)[0] as ToolStripMenuItem;
            ToolStripMenuItem darkItem = themeMenu.DropDownItems.Find("mnuThemeDark", false)[0] as ToolStripMenuItem;

            string currentBase = ThemeManager.CurrentBaseTheme;

            if (defaultItem != null) defaultItem.Checked = (currentBase == "Default");
            if (darkItem != null) darkItem.Checked = (currentBase == "Dark");
        }
    }
}