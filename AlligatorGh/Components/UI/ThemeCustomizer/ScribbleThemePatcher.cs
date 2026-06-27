using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Special;
using HarmonyLib;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public static class ScribbleThemePatcher
    {
        private static bool _isPatched = false;
        private static Color _scribbleTextColor = Color.White;

        public static Color ScribbleTextColor
        {
            get { return _scribbleTextColor; }
            set
            {
                if (_scribbleTextColor == value) return;

                _scribbleTextColor = value;

                if (Instances.ActiveCanvas != null)
                {
                    Instances.ActiveCanvas.Refresh();
                }
            }
        }

        public static void ApplyPatch()
        {
            if (_isPatched) return;

            var harmonyInstance = new Harmony("com.alligator.theme.scribble");
            MethodInfo originalMethod = typeof(GH_ScribbleAttributes).GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo prefixMethod = typeof(ScribbleThemePatcher).GetMethod(nameof(RenderPrefix), BindingFlags.NonPublic | BindingFlags.Static);

            if (originalMethod != null && prefixMethod != null)
            {
                harmonyInstance.Patch(originalMethod, prefix: new HarmonyMethod(prefixMethod));
                _isPatched = true;
            }
        }

        private static bool RenderPrefix(GH_ScribbleAttributes __instance, GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
                return true;

            GH_Scribble owner = __instance.Owner as GH_Scribble;
            if (owner == null)
                return false;

            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                using (SolidBrush customBrush = new SolidBrush(_scribbleTextColor))
                {
                    graphics.DrawString(owner.Text, owner.Font, customBrush, __instance.Bounds, format);
                }
            }

            if (__instance.Selected)
            {
                Rectangle selectionRect = Rectangle.Round(__instance.Bounds);
                selectionRect.Inflate(4, 4);

                using (Pen dashPen = new Pen(Color.FromArgb(150, 0, 0, 0), 1.5f))
                {
                    dashPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    graphics.DrawRectangle(dashPen, selectionRect);
                }
            }

            return false;
        }
    }
}
