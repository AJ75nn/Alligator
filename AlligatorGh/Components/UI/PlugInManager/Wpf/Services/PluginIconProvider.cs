using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Grasshopper;
using Grasshopper.GUI.Ribbon;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf.Services
{
    /// <summary>
    /// Resolves per-tab metadata for the Plugin Manager: the tab's 16px icon and whether it is a
    /// native Grasshopper category. Pure GDI / reflection — no WPF dependency, so it can be unit
    /// reasoned about independently of the window. Conversion of the returned <see cref="Image"/>
    /// to a WPF <c>ImageSource</c> happens at the view-model boundary.
    /// </summary>
    /// <remarks>
    /// Extracted verbatim (behaviour-preserving) from the former <c>PluginManagerFrm.LoadData</c>
    /// icon-resolution block so both the resolution order and the reflection fallbacks stay identical.
    /// </remarks>
    public static class PluginIconProvider
    {
        // Standard Grasshopper category tabs, used only to classify a row as native vs installed
        // plugin for the summary footer. Mirrors the heuristic in the original form.
        private static readonly string[] NativeCategories =
        {
            "Params", "Maths", "Sets", "Vector", "Curve", "Surface", "Mesh", "Intersect", "Transform", "Display"
        };

        /// <summary>
        /// Returns true when the tab name is one of Grasshopper's built-in component categories.
        /// </summary>
        /// <param name="tabName">The full tab name.</param>
        /// <returns>True if the tab is a native category.</returns>
        public static bool IsNativeCategory(string tabName)
        {
            return NativeCategories.Contains(tabName);
        }

        /// <summary>
        /// Resolves the 16px icon for a ribbon tab, trying, in order: the matching component-library
        /// icon, the reflected native category icon (with singular/plural fallbacks), and finally a
        /// generated text-symbol bitmap from the tab's <c>NameSymbol</c>.
        /// </summary>
        /// <param name="tabName">The full tab name (matches <c>GH_RibbonTab.NameFull</c>).</param>
        /// <param name="allTabs">All known ribbon tabs (used for the NameSymbol fallback).</param>
        /// <param name="symbolColor">Colour for the generated symbol glyph fallback so it stays legible on the active theme.</param>
        /// <returns>An icon image, or null when none could be resolved.</returns>
        public static Image ResolveIcon(string tabName, IReadOnlyList<GH_RibbonTab> allTabs, Color symbolColor)
        {
            // 1. Component-library icon matching the tab name.
            var lib = Instances.ComponentServer.Libraries.FirstOrDefault(l =>
                l.Name.Equals(tabName, StringComparison.OrdinalIgnoreCase));
            if (lib != null && lib.Icon != null)
            {
                // GH-owned shared image — return as-is, never dispose it.
                return lib.Icon;
            }

            // 2. Reflected native-category icon (e.g. "Maths" -> "Category_Maths_16x16").
            Image categoryIcon = ResolveCategoryIcon(tabName);
            if (categoryIcon != null)
            {
                return categoryIcon;
            }

            // 3. Generated symbol bitmap from the tab's NameSymbol (for 3rd-party plugins w/o icons).
            var tab = allTabs?.FirstOrDefault(t => t.NameFull == tabName);
            if (tab != null && !string.IsNullOrEmpty(tab.NameSymbol))
            {
                return CreateSymbolIcon(tab.NameSymbol, symbolColor);
            }

            return null;
        }

        /// <summary>
        /// Counts the components across all panels of a ribbon tab (best-effort via reflection).
        /// </summary>
        /// <param name="tab">The ribbon tab.</param>
        /// <returns>The total number of component buttons, or 0 if it could not be determined.</returns>
        public static int CountComponents(GH_RibbonTab tab)
        {
            if (tab == null || tab.Panels == null)
            {
                return 0;
            }

            int compCount = 0;
            var buttonsProp = typeof(GH_RibbonPanel).GetProperty(
                "Buttons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var panel in tab.Panels)
            {
                if (buttonsProp?.GetValue(panel) is IList buttons)
                {
                    compCount += buttons.Count;
                }
            }

            return compCount;
        }

        private static Image ResolveCategoryIcon(string tabName)
        {
            var type = typeof(Instances).Assembly.GetType("Grasshopper.My.Resources.Res_CategoryIcons");
            if (type == null)
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            string safeName = tabName.Replace(" ", string.Empty);

            var prop = type.GetProperty($"Category_{safeName}_16x16", flags);

            // Fallback 1: plural form (e.g. "Surface" -> "Surfaces").
            if (prop == null && !safeName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                prop = type.GetProperty($"Category_{safeName}s_16x16", flags);
            }

            // Fallback 2: singular form (e.g. "Surfaces" -> "Surface").
            if (prop == null && safeName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                prop = type.GetProperty($"Category_{safeName.Substring(0, safeName.Length - 1)}_16x16", flags);
            }

            return prop?.GetValue(null) as Image;
        }

        /// <summary>
        /// Draws a small bitmap containing the tab's symbol glyph. The colour is supplied by the
        /// caller (the active theme's text colour) so the glyph stays legible in both light and dark.
        /// </summary>
        /// <param name="symbol">The symbol text to render.</param>
        /// <param name="color">The glyph colour.</param>
        /// <returns>A freshly created 16x16 bitmap; the caller owns and may dispose it.</returns>
        public static Bitmap CreateSymbolIcon(string symbol, Color color)
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using (var brush = new SolidBrush(color))
                using (var font = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(symbol, font, brush, new RectangleF(0, 0, 16, 16), sf);
                }
            }

            return bmp;
        }
    }
}
