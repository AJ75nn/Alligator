using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public static class ThemeManager
    {
        private static bool _initialized = false;

        // Default Canvas Theme (Loaded Dynamically)
        public static Color DefaultCanvasBack => DefaultSettingsProvider.GetDefaultColor("canvas_backcolor");
        public static Color DefaultCanvasGrid => DefaultSettingsProvider.GetDefaultColor("canvas_gridcolor");
        public static Color DefaultCanvasEdge => DefaultSettingsProvider.GetDefaultColor("canvas_edgecolor");
        public static Color DefaultCanvasShade => DefaultSettingsProvider.GetDefaultColor("canvas_shadecolor");
        public static Color DefaultWireDefault => DefaultSettingsProvider.GetDefaultColor("wire_default");
        public static Color DefaultWireSelectedA => DefaultSettingsProvider.GetDefaultColor("wire_selected_a");
        public static Color DefaultWireSelectedB => DefaultSettingsProvider.GetDefaultColor("wire_selected_b");
        public static Color DefaultWireEmpty => DefaultSettingsProvider.GetDefaultColor("wire_empty");

        public static Color DefaultComponentFill => DefaultSettingsProvider.GetDefaultColor("normal.std.fill");
        public static Color DefaultComponentEdge => DefaultSettingsProvider.GetDefaultColor("normal.std.edge");
        public static Color DefaultComponentText => DefaultSettingsProvider.GetDefaultColor("normal.std.text");
        public static Color DefaultWarningFill => DefaultSettingsProvider.GetDefaultColor("warning.std.fill");
        public static Color DefaultWarningEdge => DefaultSettingsProvider.GetDefaultColor("warning.std.edge");
        public static Color DefaultWarningText => DefaultSettingsProvider.GetDefaultColor("warning.std.text");
        public static Color DefaultErrorFill => DefaultSettingsProvider.GetDefaultColor("error.std.fill");
        public static Color DefaultErrorEdge => DefaultSettingsProvider.GetDefaultColor("error.std.edge");
        public static Color DefaultErrorText => DefaultSettingsProvider.GetDefaultColor("error.std.text");
        public static Color DefaultHiddenFill => DefaultSettingsProvider.GetDefaultColor("hidden.std.fill");
        public static Color DefaultHiddenEdge => DefaultSettingsProvider.GetDefaultColor("hidden.std.edge");
        public static Color DefaultHiddenText => DefaultSettingsProvider.GetDefaultColor("hidden.std.text");

        public static Color DefaultPanelBack => DefaultSettingsProvider.GetDefaultColor("panel_backcolor");
        public static Color DefaultGroupBack => DefaultSettingsProvider.GetDefaultColor("group_backcolor");

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

            Color? customCompBack = GetCustomColor("CustomComponentBack");
            Color? customCompBorder = GetCustomColor("CustomComponentBorder");
            Color? customCompWarn = GetCustomColor("CustomComponentWarning");
            Color? customCompErr = GetCustomColor("CustomComponentError");
            Color? customCompHidden = GetCustomColor("CustomComponentHidden");
            int compTransparency = Instances.Settings.GetValue("CustomCompTransparency", 255);

            if (!isDark && !customCompBack.HasValue && !customCompBorder.HasValue && compTransparency == 255)
                GH_Skin.palette_normal_standard = new GH_PaletteStyle(DefaultComponentFill, DefaultComponentEdge, DefaultComponentText);
            else if (customCompBack.HasValue || customCompBorder.HasValue || compTransparency != 255)
            {
                Color fill = customCompBack ?? (isDark ? GH_Skin.palette_normal_standard.Fill : DefaultComponentFill);
                fill = Color.FromArgb(compTransparency, fill.R, fill.G, fill.B);
                GH_Skin.palette_normal_standard = new GH_PaletteStyle(fill, customCompBorder ?? (isDark ? GH_Skin.palette_normal_standard.Edge : DefaultComponentEdge), isDark ? GH_Skin.palette_normal_standard.Text : DefaultComponentText);
            }

            if (!isDark && !customCompWarn.HasValue && !customCompBorder.HasValue && compTransparency == 255)
                GH_Skin.palette_warning_standard = new GH_PaletteStyle(DefaultWarningFill, DefaultWarningEdge, DefaultWarningText);
            else if (customCompWarn.HasValue || customCompBorder.HasValue || compTransparency != 255)
            {
                Color fill = customCompWarn ?? (isDark ? GH_Skin.palette_warning_standard.Fill : DefaultWarningFill);
                fill = Color.FromArgb(compTransparency, fill.R, fill.G, fill.B);
                GH_Skin.palette_warning_standard = new GH_PaletteStyle(fill, customCompBorder ?? (isDark ? GH_Skin.palette_warning_standard.Edge : DefaultWarningEdge), isDark ? GH_Skin.palette_warning_standard.Text : DefaultWarningText);
            }

            if (!isDark && !customCompErr.HasValue && !customCompBorder.HasValue && compTransparency == 255)
                GH_Skin.palette_error_standard = new GH_PaletteStyle(DefaultErrorFill, DefaultErrorEdge, DefaultErrorText);
            else if (customCompErr.HasValue || customCompBorder.HasValue || compTransparency != 255)
            {
                Color fill = customCompErr ?? (isDark ? GH_Skin.palette_error_standard.Fill : DefaultErrorFill);
                fill = Color.FromArgb(compTransparency, fill.R, fill.G, fill.B);
                GH_Skin.palette_error_standard = new GH_PaletteStyle(fill, customCompBorder ?? (isDark ? GH_Skin.palette_error_standard.Edge : DefaultErrorEdge), isDark ? GH_Skin.palette_error_standard.Text : DefaultErrorText);
            }

            if (!isDark && !customCompHidden.HasValue && !customCompBorder.HasValue && compTransparency == 255)
                GH_Skin.palette_hidden_standard = new GH_PaletteStyle(DefaultHiddenFill, DefaultHiddenEdge, DefaultHiddenText);
            else if (customCompHidden.HasValue || customCompBorder.HasValue || compTransparency != 255)
            {
                Color fill = customCompHidden ?? (isDark ? GH_Skin.palette_hidden_standard.Fill : DefaultHiddenFill);
                fill = Color.FromArgb(compTransparency, fill.R, fill.G, fill.B);
                GH_Skin.palette_hidden_standard = new GH_PaletteStyle(fill, customCompBorder ?? (isDark ? GH_Skin.palette_hidden_standard.Edge : DefaultHiddenEdge), isDark ? GH_Skin.palette_hidden_standard.Text : DefaultHiddenText);
            }

            Color oldPanelBack = GH_Skin.panel_back;
            Color oldGroupBack = GH_Skin.group_back;
            Color? customPanelBack = GetCustomColor("CustomPanelBack");
            Color? customGroupBack = GetCustomColor("CustomGroupBack");

            if (customPanelBack.HasValue) GH_Skin.panel_back = customPanelBack.Value;
            else if (!isDark) GH_Skin.panel_back = DefaultPanelBack;

            if (customGroupBack.HasValue) GH_Skin.group_back = customGroupBack.Value;
            else if (!isDark) GH_Skin.group_back = DefaultGroupBack;

            if (Instances.ActiveCanvas != null && Instances.ActiveCanvas.Document != null)
            {
                bool docChanged = false;
                foreach (var obj in Instances.ActiveCanvas.Document.Objects)
                {
                    if (obj is Grasshopper.Kernel.Special.GH_Panel panel)
                    {
                        if (panel.Properties.Colour == oldPanelBack)
                        {
                            panel.Properties.Colour = GH_Skin.panel_back;
                            docChanged = true;
                        }
                    }
                    else if (obj is Grasshopper.Kernel.Special.GH_Group group)
                    {
                        if (group.Colour == oldGroupBack)
                        {
                            group.Colour = GH_Skin.group_back;
                            docChanged = true;
                        }
                    }
                }
                if(docChanged) Instances.ActiveCanvas.Document.DestroyAttributeCache();
            }
            try { GH_Skin.SaveSkin(); } catch { }

            string compFontType = Instances.Settings.GetValue("CustomCompFontType", "");
            if (!string.IsNullOrEmpty(compFontType))
            {
                try
                {
                    var newFamily = new FontFamily(compFontType);
                    GH_FontServer.FamilyStandard = newFamily;
                    GH_FontServer.FamilyConsole = newFamily;
                }
                catch { }
            }
            else
            {
                try
                {
                    string standardFont = DefaultSettingsProvider.GetDefaultFont("Font:Standard");
                    string consoleFont = DefaultSettingsProvider.GetDefaultFont("Font:Console");
                    if (!string.IsNullOrEmpty(standardFont)) GH_FontServer.FamilyStandard = new FontFamily(standardFont);
                    if (!string.IsNullOrEmpty(consoleFont)) GH_FontServer.FamilyConsole = new FontFamily(consoleFont);
                }
                catch { }
            }

            int compFontSize = Instances.Settings.GetValue("CustomCompFontSize", 0);
            if (compFontSize > 0) ApplyComponentFontSize(compFontSize);

            Color? scribbleColor = GetCustomColor("CustomScribbleText");
            ScribbleThemePatcher.ScribbleTextColor = scribbleColor ?? (isDark ? Color.White : Color.Black);
            ScribbleThemePatcher.ApplyPatch();

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
                    splitter.BackColor = ribbonBack;
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
                "CustomRibbonBack", "CustomRibbonHighlight", "CustomRibbonText", "CustomRibbonFontSize",
                "CustomComponentBack", "CustomComponentBorder", "CustomComponentWarning", "CustomComponentError", "CustomComponentHidden",
                "CustomPanelBack", "CustomGroupBack", "CustomScribbleText", "CustomCompTransparency"
            };

            foreach (var key in keys)
            {
                if (key == "CustomRibbonFontSize")
                    Instances.Settings.SetValue(key, 0);
                else if (key == "CustomCompTransparency")
                    Instances.Settings.SetValue(key, 255);
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
            if (propName == "CustomComponentBack") return baseTheme == "Dark" ? GH_Skin.palette_normal_standard.Fill : DefaultComponentFill;
            if (propName == "CustomComponentBorder") return baseTheme == "Dark" ? GH_Skin.palette_normal_standard.Edge : DefaultComponentEdge;
            if (propName == "CustomComponentWarning") return baseTheme == "Dark" ? GH_Skin.palette_warning_standard.Fill : DefaultWarningFill;
            if (propName == "CustomComponentError") return baseTheme == "Dark" ? GH_Skin.palette_error_standard.Fill : DefaultErrorFill;
            if (propName == "CustomComponentHidden") return baseTheme == "Dark" ? GH_Skin.palette_hidden_standard.Fill : DefaultHiddenFill;
            if (propName == "CustomPanelBack") return baseTheme == "Dark" ? GH_Skin.panel_back : DefaultPanelBack;
            if (propName == "CustomGroupBack") return baseTheme == "Dark" ? GH_Skin.group_back : DefaultGroupBack;
            if (propName == "CustomScribbleText") return ScribbleThemePatcher.ScribbleTextColor;

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
            ApplyTheme(new GrasshopperUIFacade().GetDocumentEditor());
        }

        private static void ApplyComponentFontSize(float newSize)
        {
            UpdateFontProperty("Standard", newSize);
            UpdateFontProperty("StandardBold", newSize);
            UpdateFontProperty("StandardItalic", newSize);

            if (Instances.ActiveCanvas == null || Instances.ActiveCanvas.Document == null)
                return;

            foreach (IGH_DocumentObject docObject in Instances.ActiveCanvas.Document.Objects)
            {
                docObject.Attributes.ExpireLayout();
            }

            Instances.ActiveCanvas.Document.DestroyAttributeCache();
            Instances.ActiveCanvas.Refresh();
        }

        private static void UpdateFontProperty(string propertyName, float newSize)
        {
            Type fontServerType = typeof(GH_FontServer);
            System.Reflection.PropertyInfo propInfo = fontServerType.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (propInfo == null) return;

            Font currentFont = propInfo.GetValue(null, null) as Font;
            if (currentFont == null) return;

            Font newFont = new Font(currentFont.FontFamily, newSize, currentFont.Style, GraphicsUnit.Point);
            System.Reflection.FieldInfo backingField = GetBackingField(fontServerType, propertyName);

            if (backingField != null)
            {
                backingField.SetValue(null, newFont);
            }
        }

        private static System.Reflection.FieldInfo GetBackingField(Type type, string propertyName)
        {
            System.Reflection.FieldInfo field = type.GetField($"m_{propertyName.ToLower()}", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
            if (field != null) return field;

            field = type.GetField($"_{propertyName}", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
            if (field != null) return field;

            field = type.GetField($"_{char.ToLower(propertyName[0])}{propertyName.Substring(1)}", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (field != null) return field;

            field = type.GetField($"_font{propertyName}", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
            if (field != null) return field;

            field = type.GetField($"_font{propertyName}8", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
            if (field != null) return field;

            field = type.GetField($"_font{propertyName}10", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
            if (field != null) return field;

            field = type.GetField($"<{propertyName}>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return field;
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
