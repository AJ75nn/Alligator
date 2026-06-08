using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public static class ThemeManager
    {
        private static bool _initialized = false;

        // Default Canvas Theme (Grasshopper Native XML)
        public static readonly Color DefaultCanvasBack = Color.FromArgb(255, 212, 208, 200);
        public static readonly Color DefaultCanvasGrid = Color.FromArgb(30, 0, 0, 0);
        public static readonly Color DefaultCanvasEdge = Color.FromArgb(255, 0, 0, 0);
        public static readonly Color DefaultCanvasShade = Color.FromArgb(80, 0, 0, 0);
        public static readonly Color DefaultWireDefault = Color.FromArgb(150, 0, 0, 0);
        public static readonly Color DefaultWireSelectedA = Color.FromArgb(255, 125, 210, 40);
        public static readonly Color DefaultWireSelectedB = Color.FromArgb(50, 0, 0, 0);
        public static readonly Color DefaultWireEmpty = Color.FromArgb(180, 255, 60, 0);

        // Default UI Chrome Theme
        public static readonly Color DefaultUIControlBack = Color.FromArgb(255, 240, 240, 240);
        public static readonly Color DefaultUIControlText = Color.FromArgb(255, 0, 0, 0);
        public static readonly Color DefaultUIHighlight = Color.FromArgb(255, 153, 209, 255);
        public static readonly int DefaultRibbonFontSize = 9;

        // Dark Canvas Theme
        public static readonly Color DarkCanvasBack = Color.FromArgb(255, 34, 41, 51);
        public static readonly Color DarkCanvasGrid = Color.FromArgb(255, 41, 49, 61);
        public static readonly Color DarkCanvasEdge = Color.FromArgb(255, 65, 78, 97);
        public static readonly Color DarkCanvasShade = Color.FromArgb(255, 50, 60, 74);
        public static readonly Color DarkWireDefault = Color.FromArgb(255, 97, 116, 143);
        public static readonly Color DarkWireSelectedA = Color.FromArgb(255, 70, 145, 207);
        public static readonly Color DarkWireSelectedB = Color.FromArgb(255, 70, 207, 150);
        public static readonly Color DarkWireEmpty = Color.FromArgb(255, 255, 206, 150);

        // Dark UI Elements
        public static readonly Color DarkUIMenuBack = Color.FromArgb(255, 45, 53, 66);
        public static readonly Color DarkUIMenuText = Color.FromArgb(255, 220, 225, 235);
        public static readonly Color DarkUIHighlight = Color.FromArgb(255, 70, 145, 207);

        public static string CurrentBaseTheme
        {
            get => Instances.Settings.GetValue("Alligator_BaseTheme", "Default");
            set => Instances.Settings.SetValue("Alligator_BaseTheme", value);
        }

        public static void Initialize(GH_DocumentEditor editor)
        {
            if (_initialized) return;

            ApplyTheme(editor);
            _initialized = true;
        }

        public static void ApplyTheme(GH_DocumentEditor editor)
        {
            string baseTheme = CurrentBaseTheme;
            bool isDark = baseTheme == "Dark";

            // 1. Apply Canvas Properties
            if (isDark)
            {
                GH_Skin.canvas_back = DarkCanvasBack;
                GH_Skin.canvas_grid = DarkCanvasGrid;
                GH_Skin.canvas_edge = DarkCanvasEdge;
                GH_Skin.canvas_shade = DarkCanvasShade;
                GH_Skin.wire_default = DarkWireDefault;
                GH_Skin.wire_selected_a = DarkWireSelectedA;
                GH_Skin.wire_selected_b = DarkWireSelectedB;
                GH_Skin.wire_empty = DarkWireEmpty;
            }
            else
            {
                GH_Skin.canvas_back = DefaultCanvasBack;
                GH_Skin.canvas_grid = DefaultCanvasGrid;
                GH_Skin.canvas_edge = DefaultCanvasEdge;
                GH_Skin.canvas_shade = DefaultCanvasShade;
                GH_Skin.wire_default = DefaultWireDefault;
                GH_Skin.wire_selected_a = DefaultWireSelectedA;
                GH_Skin.wire_selected_b = DefaultWireSelectedB;
                GH_Skin.wire_empty = DefaultWireEmpty;
            }

            // Apply custom overrides if they exist
            Color? customBack = GetCustomColor("CustomCanvasBack");
            if (customBack.HasValue) GH_Skin.canvas_back = customBack.Value;

            Color? customGrid = GetCustomColor("CustomCanvasGrid");
            if (customGrid.HasValue) GH_Skin.canvas_grid = customGrid.Value;

            Color? customEdge = GetCustomColor("CustomCanvasEdge");
            if (customEdge.HasValue) GH_Skin.canvas_edge = customEdge.Value;

            Color? customShade = GetCustomColor("CustomCanvasShade");
            if (customShade.HasValue) GH_Skin.canvas_shade = customShade.Value;

            Color? customWireDef = GetCustomColor("CustomWireDefault");
            if (customWireDef.HasValue) GH_Skin.wire_default = customWireDef.Value;

            Color? customWireA = GetCustomColor("CustomWireSelectedA");
            if (customWireA.HasValue) GH_Skin.wire_selected_a = customWireA.Value;

            Color? customWireB = GetCustomColor("CustomWireSelectedB");
            if (customWireB.HasValue) GH_Skin.wire_selected_b = customWireB.Value;

            Color? customWireEmpty = GetCustomColor("CustomWireEmpty");
            if (customWireEmpty.HasValue) GH_Skin.wire_empty = customWireEmpty.Value;

            // 2. Apply UI Chrome Properties
            ApplyUITheme(editor, isDark);

            // 3. Refresh canvas
            if (Instances.ActiveCanvas != null)
            {
                Instances.ActiveCanvas.Invalidate();
            }
        }

        private static void ApplyUITheme(GH_DocumentEditor editor, bool isDark)
        {
            if (editor == null) return;

            editor.BackColor = isDark ? DarkCanvasBack : DefaultUIControlBack;

            // Fetch custom UI overrides
            Color ribbonBack = GetCustomColor("CustomRibbonBack") ?? (isDark ? DarkUIMenuBack : DefaultUIControlBack);
            Color ribbonText = GetCustomColor("CustomRibbonText") ?? (isDark ? DarkUIMenuText : DefaultUIControlText);
            Color ribbonHighlight = GetCustomColor("CustomRibbonHighlight") ?? (isDark ? DarkUIHighlight : DefaultUIHighlight);
            int ribbonFontSize = GetCustomRibbonFontSize();

            // Execute recursive traversal to style all toolbars, menus, splitters, and panels
            StyleUIElementsRecursively(editor, isDark, ribbonBack, ribbonText, ribbonHighlight, ribbonFontSize);
        }

        /// <summary>
        /// Recursively searches for and themes ToolStrips, Splitters, and structural Panels.
        /// </summary>
        private static void StyleUIElementsRecursively(Control parent, bool isDark, Color ribbonBack, Color ribbonText, Color ribbonHighlight, int ribbonFontSize)
        {
            foreach (Control control in parent.Controls)
            {
                // 1. ToolStrips (Menus, Toolbars, StatusBars, Ribbon)
                if (control is ToolStrip toolStrip)
                {
                    toolStrip.BackColor = ribbonBack;
                    toolStrip.ForeColor = ribbonText;

                    if (toolStrip.Font.Size != ribbonFontSize)
                    {
                        toolStrip.Font = new Font(toolStrip.Font.FontFamily, ribbonFontSize, toolStrip.Font.Style);
                    }

                    toolStrip.Renderer = new AlligatorDarkMenuRenderer(ribbonBack, ribbonText, ribbonHighlight, isDark);
                }
                // 2. Splitters (The horizontal resize bar beneath the ribbon)
                else if (control is Splitter splitter)
                {
                    // Using CanvasEdge provides a crisp, subtle separator line in dark mode
                    splitter.BackColor = isDark ? DarkCanvasEdge : DefaultUIControlBack;
                }
                // 3. Structural Panels (These containers often cause the light padding frames)
                else if (control is Panel)
                {
                    control.BackColor = isDark ? DarkCanvasBack : DefaultUIControlBack;
                }

                // Recurse into child containers
                if (control.HasChildren)
                {
                    StyleUIElementsRecursively(control, isDark, ribbonBack, ribbonText, ribbonHighlight, ribbonFontSize);
                }
            }
        }

        public static void SetCustomColor(string key, Color color, GH_DocumentEditor editor)
        {
            Instances.Settings.SetValue(key, color);
            ApplyTheme(editor);
        }

        public static void ClearCustomColor(string key, GH_DocumentEditor editor)
        {
            Instances.Settings.SetValue(key, Color.Empty);
            ApplyTheme(editor);
        }

        public static void ClearAllCustomSettings(GH_DocumentEditor editor)
        {
            string[] keys = new string[] {
                "CustomCanvasBack", "CustomCanvasGrid", "CustomCanvasEdge", "CustomCanvasShade",
                "CustomWireDefault", "CustomWireSelectedA", "CustomWireSelectedB", "CustomWireEmpty",
                "CustomRibbonBack", "CustomRibbonHighlight", "CustomRibbonText", "CustomRibbonFontSize"
            };

            foreach (var key in keys)
            {
                if (key == "CustomRibbonFontSize")
                    Instances.Settings.SetValue(key, 0); // 0 indicates empty for font size
                else
                    Instances.Settings.SetValue(key, Color.Empty);
            }
        }

        public static Color? GetCustomColor(string key)
        {
            Color c = Instances.Settings.GetValue(key, Color.Empty);
            if (c == Color.Empty) return null;
            return c;
        }

        public static Color GetDefaultColorForProperty(string propName)
        {
            string baseTheme = CurrentBaseTheme;

            if (propName == "CustomCanvasBack") return baseTheme == "Dark" ? DarkCanvasBack : DefaultCanvasBack;
            if (propName == "CustomCanvasGrid") return baseTheme == "Dark" ? DarkCanvasGrid : DefaultCanvasGrid;
            if (propName == "CustomCanvasEdge") return baseTheme == "Dark" ? DarkCanvasEdge : DefaultCanvasEdge;
            if (propName == "CustomCanvasShade") return baseTheme == "Dark" ? DarkCanvasShade : DefaultCanvasShade;
            if (propName == "CustomWireDefault") return baseTheme == "Dark" ? DarkWireDefault : DefaultWireDefault;
            if (propName == "CustomWireSelectedA") return baseTheme == "Dark" ? DarkWireSelectedA : DefaultWireSelectedA;
            if (propName == "CustomWireSelectedB") return baseTheme == "Dark" ? DarkWireSelectedB : DefaultWireSelectedB;
            if (propName == "CustomWireEmpty") return baseTheme == "Dark" ? DarkWireEmpty : DefaultWireEmpty;
            if (propName == "CustomRibbonBack") return baseTheme == "Dark" ? DarkUIMenuBack : DefaultUIControlBack;
            if (propName == "CustomRibbonHighlight") return baseTheme == "Dark" ? DarkUIHighlight : DefaultUIHighlight;
            if (propName == "CustomRibbonText") return baseTheme == "Dark" ? DarkUIMenuText : DefaultUIControlText;

            return Color.Empty;
        }

        public static int GetCustomRibbonFontSize()
        {
            int size = Instances.Settings.GetValue("CustomRibbonFontSize", 0);
            if (size > 0) return size;
            return DefaultRibbonFontSize; // Applies to both Default and Dark normally
        }

        public static void SetCustomRibbonFontSize(int size)
        {
            Instances.Settings.SetValue("CustomRibbonFontSize", size);
            ApplyTheme(Instances.DocumentEditor);
        }
    }

    /// <summary>
    /// Custom Renderer handling standard WinForms Menu and Toolbar styling.
    /// Aggressively strips out legacy 3D highlights for a modern, flat UI.
    /// </summary>
    public class AlligatorDarkMenuRenderer : ToolStripProfessionalRenderer
    {
        private Color _ribbonText;
        private Color _ribbonHighlight;
        private bool _isDark;

        public AlligatorDarkMenuRenderer(Color ribbonBack, Color ribbonText, Color ribbonHighlight, bool isDark)
            : base(new AlligatorDarkColorTable(ribbonBack, ribbonHighlight, isDark))
        {
            this.RoundedEdges = false;
            _ribbonText = ribbonText;
            _ribbonHighlight = ribbonHighlight;
            _isDark = isDark;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                using (SolidBrush brush = new SolidBrush(_ribbonHighlight))
                {
                    e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
                }
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = _ribbonText;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // Specifically DO NOT call base.OnRenderToolStripBorder(e).
            // WinForms hardcodes a 1px white highlight line here that ignores ColorTables.
            // By overriding it, we enforce a perfectly flat UI.

            Rectangle bounds = new Rectangle(0, 0, e.ToolStrip.Width, e.ToolStrip.Height);
            using (Pen borderPen = new Pen(_isDark ? ThemeManager.DarkCanvasEdge : ThemeManager.DefaultCanvasEdge))
            {
                // Draw a simple, subtle 1px flat edge at the bottom of the toolbar
                e.Graphics.DrawLine(borderPen, 0, bounds.Height - 1, bounds.Width, bounds.Height - 1);
            }
        }

        protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e)
        {
            // Suppress the default dotted light-grey sizing grip in the bottom right corner
            // to maintain a clean visual frame.
        }
    }

    /// <summary>
    /// Color table defining specific overrides for ToolStrip menus, toolbars, and buttons.
    /// </summary>
    public class AlligatorDarkColorTable : ProfessionalColorTable
    {
        private Color _ribbonBack;
        private Color _ribbonHighlight;
        private Color _edgeColor;

        public AlligatorDarkColorTable(Color ribbonBack, Color ribbonHighlight, bool isDark)
        {
            _ribbonBack = ribbonBack;
            _ribbonHighlight = ribbonHighlight;
            _edgeColor = isDark ? ThemeManager.DarkCanvasEdge : ThemeManager.DefaultCanvasEdge;
        }

        // Menu Elements
        public override Color ToolStripDropDownBackground => _ribbonBack;
        public override Color ImageMarginGradientBegin => _ribbonBack;
        public override Color ImageMarginGradientMiddle => _ribbonBack;
        public override Color ImageMarginGradientEnd => _ribbonBack;
        public override Color MenuBorder => _edgeColor;
        public override Color MenuItemBorder => _ribbonHighlight;
        public override Color MenuItemSelected => _ribbonHighlight;
        public override Color MenuStripGradientBegin => _ribbonBack;
        public override Color MenuStripGradientEnd => _ribbonBack;

        // Standard Toolbar Elements (Canvas Toolbar & Status Bar)
        public override Color ToolStripBorder => _edgeColor;
        public override Color ToolStripGradientBegin => _ribbonBack;
        public override Color ToolStripGradientMiddle => _ribbonBack;
        public override Color ToolStripGradientEnd => _ribbonBack;
        public override Color ToolStripPanelGradientBegin => _ribbonBack;
        public override Color ToolStripPanelGradientEnd => _ribbonBack;
        public override Color ButtonSelectedHighlight => _ribbonHighlight;
        public override Color ButtonSelectedBorder => _ribbonHighlight;
        public override Color ButtonPressedHighlight => _edgeColor;
        public override Color ButtonPressedBorder => _edgeColor;
        public override Color ButtonCheckedHighlight => _edgeColor;
    }
}