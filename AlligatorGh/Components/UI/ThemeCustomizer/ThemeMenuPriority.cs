using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public class ThemeMenuPriority : GH_AssemblyPriority
    {
        private static Image _resetImage;

        private static Image GetResetImage()
        {
            if (_resetImage == null)
            {
                try
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    // Do not wrap in a using statement; the stream must remain open for the lifetime of the GDI+ Image.
                    Stream stream = assembly.GetManifestResourceStream("AlligatorGh.Resources.reset.png");
                    if (stream != null)
                    {
                        _resetImage = Image.FromStream(stream);
                    }
                }
                catch { }
            }
            return _resetImage;
        }

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
                ThemeManager.ClearAllCustomSettings(Instances.DocumentEditor);
                ThemeManager.CurrentBaseTheme = "Default";
                ThemeManager.ApplyTheme(Instances.DocumentEditor);
                UpdateThemeCheckmarks();
            };

            ToolStripMenuItem darkMenuItem = new ToolStripMenuItem("Dark");
            darkMenuItem.Name = "mnuThemeDark";
            darkMenuItem.Click += (s, e) =>
            {
                ThemeManager.ClearAllCustomSettings(Instances.DocumentEditor);
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

            customMenuItem.DropDownItems.Add(new ToolStripSeparator());
            BuildCustomColorMenu(customMenuItem, "Ribbon Background", "CustomRibbonBack");
            BuildCustomColorMenu(customMenuItem, "Ribbon Highlight", "CustomRibbonHighlight");
            BuildCustomColorMenu(customMenuItem, "Ribbon Text", "CustomRibbonText");
            BuildCustomNumberMenu(customMenuItem, "Ribbon Font Size", "CustomRibbonFontSize");

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

            bool _isResetting = false;

            propertyItem.DropDown.Closing += (sender, eClosing) => {
                if (_isResetting)
                {
                    eClosing.Cancel = true;
                    _isResetting = false;
                }
            };

            propertyItem.DropDownOpening += (s, e) => {
                propertyItem.DropDownItems.Clear();

                Color currentColor = ThemeManager.GetCustomColor(propertyKey) ?? ThemeManager.GetDefaultColorForProperty(propertyKey);

                GH_DocumentObject.Menu_AppendColourPicker(propertyItem.DropDown, currentColor, (sender, args) => {
                    ThemeManager.SetCustomColor(propertyKey, args.Colour, Instances.DocumentEditor);
                });

                propertyItem.DropDownItems.Add(new ToolStripSeparator());

                ToolStripMenuItem resetItem = new ToolStripMenuItem("Reset");
                Image resetImg = GetResetImage();
                if (resetImg != null)
                {
                    resetItem.Image = resetImg;
                }

                resetItem.MouseEnter += (sender, args) => {
                    if (resetItem.GetCurrentParent() != null)
                        resetItem.GetCurrentParent().Cursor = Cursors.Hand;
                };

                resetItem.MouseLeave += (sender, args) => {
                    if (resetItem.GetCurrentParent() != null)
                        resetItem.GetCurrentParent().Cursor = Cursors.Default;
                };

                resetItem.Click += (sender, args) => {
                    _isResetting = true;
                    ThemeManager.ClearCustomColor(propertyKey, Instances.DocumentEditor);
                };

                propertyItem.DropDownItems.Add(resetItem);
            };

            parent.DropDownItems.Add(propertyItem);
        }

        private void BuildCustomNumberMenu(ToolStripMenuItem parent, string displayName, string propertyKey)
        {
            ToolStripMenuItem propertyItem = new ToolStripMenuItem(displayName);

            // Add a dummy item to ensure the dropdown arrow is shown
            propertyItem.DropDownItems.Add(new ToolStripMenuItem("..."));

            bool _isResetting = false;

            propertyItem.DropDown.Closing += (sender, eClosing) => {
                if (_isResetting)
                {
                    eClosing.Cancel = true;
                    _isResetting = false;
                }
            };

            propertyItem.DropDownOpening += (s, e) => {
                propertyItem.DropDownItems.Clear();

                int currentFontSize = ThemeManager.GetCustomRibbonFontSize();

                GH_DocumentObject.Menu_AppendTextItem(propertyItem.DropDown, currentFontSize.ToString(), null, (sender, textArgs) => {
                    if (sender is GH_MenuTextBox txt && int.TryParse(txt.Text, out int size) && size > 0)
                    {
                        ThemeManager.SetCustomRibbonFontSize(size);
                    }
                }, true, 100, true);

                propertyItem.DropDownItems.Add(new ToolStripSeparator());

                ToolStripMenuItem resetItem = new ToolStripMenuItem("Reset");
                Image resetImg = GetResetImage();
                if (resetImg != null)
                {
                    resetItem.Image = resetImg;
                }

                resetItem.MouseEnter += (sender, args) => {
                    if (resetItem.GetCurrentParent() != null)
                        resetItem.GetCurrentParent().Cursor = Cursors.Hand;
                };

                resetItem.MouseLeave += (sender, args) => {
                    if (resetItem.GetCurrentParent() != null)
                        resetItem.GetCurrentParent().Cursor = Cursors.Default;
                };

                resetItem.Click += (sender, args) => {
                    _isResetting = true;
                    Instances.Settings.SetValue(propertyKey, 0);
                    ThemeManager.ApplyTheme(Instances.DocumentEditor);
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
