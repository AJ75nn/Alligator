using System.Windows.Media;
using AlligatorGh.Components.UI.ThemeCustomizer;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf.Theming
{
    /// <summary>
    /// A complete set of colours for one theme (light or dark). Every themable brush/colour in the
    /// Plugin Manager window resolves from one of these fields, so a theme switch is a field-by-field
    /// crossfade between two palettes.
    /// </summary>
    public sealed class ThemePalette
    {
        /// <summary>Outermost window background (the flat neumorphic base).</summary>
        public Color WindowBackground { get; init; }

        /// <summary>Inset "well" behind the scrolling row list.</summary>
        public Color SurfaceAlt { get; init; }

        /// <summary>Raised card / row surface.</summary>
        public Color CardBackground { get; init; }

        /// <summary>Primary text colour.</summary>
        public Color Text { get; init; }

        /// <summary>Secondary / muted text (summary footer, drag handle).</summary>
        public Color MutedText { get; init; }

        /// <summary>Single accent colour (primary button, toggle "on", selection ring).</summary>
        public Color Accent { get; init; }

        /// <summary>Text/glyph colour drawn on top of the accent.</summary>
        public Color AccentText { get; init; }

        /// <summary>Hairline border / divider colour (used sparingly).</summary>
        public Color Border { get; init; }

        /// <summary>Bottom-right neumorphic shade.</summary>
        public Color ShadowDark { get; init; }

        /// <summary>Top-left neumorphic highlight.</summary>
        public Color ShadowLight { get; init; }

        /// <summary>Background of a selected row.</summary>
        public Color RowSelected { get; init; }

        /// <summary>Toggle-switch track colour in the "off" (light) position.</summary>
        public Color TrackOff { get; init; }

        /// <summary>
        /// The light theme — a soft off-white neumorphic palette sharing the accent hue with dark.
        /// </summary>
        public static ThemePalette Light { get; } = new ThemePalette
        {
            WindowBackground = Color.FromRgb(0xE6, 0xEB, 0xF0),
            SurfaceAlt = Color.FromRgb(0xDF, 0xE5, 0xEC),
            CardBackground = Color.FromRgb(0xED, 0xF1, 0xF6),
            Text = Color.FromRgb(0x2B, 0x34, 0x40),
            MutedText = Color.FromRgb(0x6B, 0x76, 0x86),
            Accent = Color.FromRgb(0x46, 0x91, 0xCF),
            AccentText = Color.FromRgb(0xFF, 0xFF, 0xFF),
            Border = Color.FromRgb(0xD5, 0xDC, 0xE4),
            ShadowDark = Color.FromRgb(0xC3, 0xCA, 0xD3),
            ShadowLight = Color.FromRgb(0xFF, 0xFF, 0xFF),
            RowSelected = Color.FromRgb(0xDC, 0xE6, 0xF2),
            TrackOff = Color.FromRgb(0xC9, 0xD2, 0xDC),
        };

        /// <summary>
        /// The dark theme — seeded from <see cref="ThemeManager"/>'s canvas/menu colours so the
        /// window matches the rest of the Alligator dark UI rather than an invented palette.
        /// </summary>
        public static ThemePalette Dark { get; } = new ThemePalette
        {
            WindowBackground = ToMedia(ThemeManager.DarkCanvasBack),   // 34,41,51
            SurfaceAlt = Color.FromRgb(0x1C, 0x22, 0x2B),
            CardBackground = ToMedia(ThemeManager.DarkUIMenuBack),     // 45,53,66
            Text = ToMedia(ThemeManager.DarkUIMenuText),               // 220,225,235
            MutedText = Color.FromRgb(0x96, 0xA0, 0xAF),
            Accent = ToMedia(ThemeManager.DarkUIHighlight),            // 70,145,207
            AccentText = Color.FromRgb(0xF5, 0xF7, 0xFA),
            Border = ToMedia(ThemeManager.DarkCanvasEdge),             // 65,78,97
            ShadowDark = Color.FromRgb(0x14, 0x19, 0x20),
            ShadowLight = Color.FromRgb(0x3C, 0x48, 0x58),
            RowSelected = Color.FromRgb(0x3A, 0x46, 0x58),
            TrackOff = Color.FromRgb(0x3C, 0x48, 0x58),
        };

        /// <summary>
        /// Returns the palette for a theme name as understood by <see cref="ThemeManager"/>.
        /// </summary>
        /// <param name="isDark">True for the dark palette, false for light.</param>
        /// <returns>The matching palette.</returns>
        public static ThemePalette For(bool isDark) => isDark ? Dark : Light;

        private static Color ToMedia(System.Drawing.Color c) => Color.FromArgb(c.A, c.R, c.G, c.B);
    }
}
