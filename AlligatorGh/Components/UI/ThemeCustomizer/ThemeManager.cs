using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI.Canvas;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public static class ThemeManager
    {
        private static bool _initialized = false;

        // Default theme definitions (Grasshopper Native XML)
        public static readonly Color DefaultCanvasBack = Color.FromArgb(255, 212, 208, 200);
        public static readonly Color DefaultCanvasGrid = Color.FromArgb(30, 0, 0, 0);
        public static readonly Color DefaultCanvasEdge = Color.FromArgb(255, 0, 0, 0);
        public static readonly Color DefaultCanvasShade = Color.FromArgb(80, 0, 0, 0);
        public static readonly Color DefaultWireDefault = Color.FromArgb(150, 0, 0, 0);
        public static readonly Color DefaultWireSelectedA = Color.FromArgb(255, 125, 210, 40);
        public static readonly Color DefaultWireSelectedB = Color.FromArgb(50, 0, 0, 0);
        public static readonly Color DefaultWireEmpty = Color.FromArgb(180, 255, 60, 0);

        // Dark theme definitions
        public static readonly Color DarkCanvasBack = Color.FromArgb(255, 34, 41, 51);
        public static readonly Color DarkCanvasGrid = Color.FromArgb(255, 41, 49, 61);
        public static readonly Color DarkCanvasEdge = Color.FromArgb(255, 65, 78, 97);
        public static readonly Color DarkCanvasShade = Color.FromArgb(255, 50, 60, 74);
        public static readonly Color DarkWireDefault = Color.FromArgb(255, 97, 116, 143);
        public static readonly Color DarkWireSelectedA = Color.FromArgb(255, 70, 145, 207);
        public static readonly Color DarkWireSelectedB = Color.FromArgb(255, 70, 207, 150);
        public static readonly Color DarkWireEmpty = Color.FromArgb(255, 255, 206, 150);

        public static string CurrentBaseTheme
        {
            get => Instances.Settings.GetValue("Alligator_BaseTheme", "Default");
            set => Instances.Settings.SetValue("Alligator_BaseTheme", value);
        }

        public static void Initialize()
        {
            if (_initialized) return;

            // Load saved settings and apply
            ApplyTheme();

            _initialized = true;
        }

        public static void ApplyTheme()
        {
            string baseTheme = CurrentBaseTheme;

            // Base properties
            if (baseTheme == "Dark")
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

            // Refresh canvas
            if (Instances.ActiveCanvas != null)
            {
                Instances.ActiveCanvas.Invalidate();
            }
        }

        public static void SetCustomColor(string key, Color color)
        {
            Instances.Settings.SetValue(key, color);
            ApplyTheme();
        }

        public static void ClearCustomColor(string key)
        {
            // Grasshopper settings doesn't have a direct "Remove" or "Clear" method,
            // so we set it to Color.Empty to signify it's cleared.
            Instances.Settings.SetValue(key, Color.Empty);
            ApplyTheme();
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

            return Color.Empty;
        }
    }
}
