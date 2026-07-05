using System.Windows.Forms;
using Grasshopper.GUI;

namespace AlligatorGh.Components.UI
{
    /// <summary>
    /// Centralizes creation of the shared "Alligator" top-level menu and its submenus
    /// so independent feature modules (Plugin Manager, Theme Customizer, ...) do not
    /// each duplicate the find-or-create logic with divergent behavior.
    /// </summary>
    internal static class AlligatorMenuBootstrapper
    {
        public const string AlligatorMenuName = "mnuAlligator";

        public static ToolStripMenuItem GetOrCreateAlligatorMenu(GH_DocumentEditor editor)
        {
            if (editor == null || editor.MainMenuStrip == null) return null;
            return GetOrCreateMenuItem(editor.MainMenuStrip.Items, AlligatorMenuName, "Alligator");
        }

        public static ToolStripMenuItem GetOrCreateSubMenu(ToolStripMenuItem parent, string name, string text)
        {
            if (parent == null) return null;
            return GetOrCreateMenuItem(parent.DropDownItems, name, text);
        }

        private static ToolStripMenuItem GetOrCreateMenuItem(ToolStripItemCollection collection, string name, string text)
        {
            if (collection == null) return null;

            ToolStripItem[] found = collection.Find(name, false);
            if (found != null && found.Length > 0)
            {
                return found[0] as ToolStripMenuItem;
            }

            var item = new ToolStripMenuItem(text)
            {
                Name = name
            };
            collection.Add(item);
            return item;
        }
    }
}
