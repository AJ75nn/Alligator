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

            ThemeManager.Initialize(documentEditor);

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

            if (customizeUIMenu.DropDownItems.Find("mnuAlligatorTheme", false).Length > 0)
                return;

            ToolStripMenuItem themeMenu = new ToolStripMenuItem("Theme");
            themeMenu.Name = "mnuAlligatorTheme";
            customizeUIMenu.DropDownItems.Add(themeMenu);

            ToolStripMenuItem defaultMenuItem = new ToolStripMenuItem("Default");
            defaultMenuItem.Name = "mnuThemeDefault";
            defaultMenuItem.Click += (s, e) =>
            {
                ThemeManager.CurrentBaseTheme = "Default";
                ThemeManager.ApplyTheme(Instances.DocumentEditor);
                UpdateThemeCheckmarks();
            };

            ToolStripMenuItem darkMenuItem = new ToolStripMenuItem("Dark");
            darkMenuItem.Name = "mnuThemeDark";
            darkMenuItem.Click += (s, e) =>
            {
                ThemeManager.CurrentBaseTheme = "Dark";
                ThemeManager.ApplyTheme(Instances.DocumentEditor);
                UpdateThemeCheckmarks();
            };

            ToolStripMenuItem customMenuItem = new ToolStripMenuItem("Custom");
            customMenuItem.Name = "mnuThemeCustom";

            // Build the Custom Dropdown items inline
            BuildCustomColorMenu(customMenuItem, "Canvas Background", "CustomCanvasBack");
            BuildCustomColorMenu(customMenuItem, "Canvas Grid", "CustomCanvasGrid");
            BuildCustomColorMenu(customMenuItem, "Canvas Edge", "CustomCanvasEdge");
            BuildCustomColorMenu(customMenuItem, "Canvas Shade", "CustomCanvasShade");
            customMenuItem.DropDownItems.Add(new ToolStripSeparator());
            BuildCustomColorMenu(customMenuItem, "Wire Default", "CustomWireDefault");
            BuildCustomColorMenu(customMenuItem, "Wire Selected A", "CustomWireSelectedA");
            BuildCustomColorMenu(customMenuItem, "Wire Selected B", "CustomWireSelectedB");
            BuildCustomColorMenu(customMenuItem, "Wire Empty", "CustomWireEmpty");

            themeMenu.DropDownItems.Add(defaultMenuItem);
            themeMenu.DropDownItems.Add(darkMenuItem);
            themeMenu.DropDownItems.Add(new ToolStripSeparator());
            themeMenu.DropDownItems.Add(customMenuItem);

            documentEditor.Shown += DocumentEditor_Shown;
        }

        private void BuildCustomColorMenu(ToolStripMenuItem parent, string displayName, string propertyKey)
        {
            ToolStripMenuItem propertyItem = new ToolStripMenuItem(displayName);

            // Add a dummy item to ensure the dropdown arrow is shown
            propertyItem.DropDownItems.Add(new ToolStripMenuItem("..."));

            propertyItem.DropDownOpening += (s, e) => {
                propertyItem.DropDownItems.Clear();

                Color currentColor = ThemeManager.GetCustomColor(propertyKey) ?? ThemeManager.GetDefaultColorForProperty(propertyKey);

                GH_DocumentObject.Menu_AppendColourPicker(propertyItem.DropDown, currentColor, (sender, args) => {
                    ThemeManager.SetCustomColor(propertyKey, args.Colour, Instances.DocumentEditor);
                });

                propertyItem.DropDownItems.Add(new ToolStripSeparator());

                ToolStripMenuItem resetItem = new ToolStripMenuItem("Reset");
                resetItem.Click += (sender, args) => {
                    ThemeManager.ClearCustomColor(propertyKey, Instances.DocumentEditor);
                    propertyItem.DropDown.Close();
                };
                propertyItem.DropDownItems.Add(resetItem);
            };

            parent.DropDownItems.Add(propertyItem);
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
