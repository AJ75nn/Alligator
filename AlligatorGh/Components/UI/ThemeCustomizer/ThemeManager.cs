using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI.Canvas;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public static class ThemeManager
    {
        private static bool _initialized = false;

        // Native Grasshopper original defaults (captured at startup)
        public static Color NativeCanvasBack;
        public static Color NativeWireSelectedA;
        public static Color NativeWireSelectedB;

        // Base theme definitions
        public static readonly Color DarkCanvasBack = Color.FromArgb(255, 45, 45, 48);
        public static readonly Color DarkWireSelectedA = Color.FromArgb(255, 0, 122, 204); // Example
        public static readonly Color DarkWireSelectedB = Color.FromArgb(255, 28, 151, 234); // Example

        public static string CurrentBaseTheme
        {
            get => Instances.Settings.GetValue("Alligator_BaseTheme", "Default");
            set => Instances.Settings.SetValue("Alligator_BaseTheme", value);
        }

        public static void Initialize()
        {
            if (_initialized) return;

            // Capture the true native Grasshopper defaults before we override them
            NativeCanvasBack = GH_Skin.canvas_back;
            NativeWireSelectedA = GH_Skin.wire_selected_a;
            NativeWireSelectedB = GH_Skin.wire_selected_b;

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
                GH_Skin.wire_selected_a = DarkWireSelectedA;
                GH_Skin.wire_selected_b = DarkWireSelectedB;
            }
            else
            {
                GH_Skin.canvas_back = NativeCanvasBack;
                GH_Skin.wire_selected_a = NativeWireSelectedA;
                GH_Skin.wire_selected_b = NativeWireSelectedB;
            }

            // Apply custom overrides if they exist
            Color? customBack = GetCustomColor("CustomCanvasBack");
            if (customBack.HasValue) GH_Skin.canvas_back = customBack.Value;

            Color? customWireA = GetCustomColor("CustomWireSelectedA");
            if (customWireA.HasValue) GH_Skin.wire_selected_a = customWireA.Value;

            Color? customWireB = GetCustomColor("CustomWireSelectedB");
            if (customWireB.HasValue) GH_Skin.wire_selected_b = customWireB.Value;

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

            if (propName == "CustomCanvasBack")
            {
                return baseTheme == "Dark" ? DarkCanvasBack : NativeCanvasBack;
            }
            if (propName == "CustomWireSelectedA")
            {
                return baseTheme == "Dark" ? DarkWireSelectedA : NativeWireSelectedA;
            }
            if (propName == "CustomWireSelectedB")
            {
                return baseTheme == "Dark" ? DarkWireSelectedB : NativeWireSelectedB;
            }

            return Color.Empty;
        }
    }
}
