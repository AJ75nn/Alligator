using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grasshopper;
using Grasshopper.GUI.Ribbon;

namespace AlligatorGh.Components.UI.PlugInManager
{
    public static class PluginManager
    {
        // We need to keep a backup of all original tabs because if we remove them from the ribbon,
        // we can't easily retrieve them to show them again later.
        private static List<GH_RibbonTab> _originalTabs = null;

        public static List<GH_RibbonTab> GetAllTabs()
        {
            InitializeBackup();
            return _originalTabs.ToList();
        }

        public static GH_Ribbon GetRibbon(Grasshopper.GUI.GH_DocumentEditor editor)
        {
            if (editor == null) return null;
            var prop = typeof(Grasshopper.GUI.GH_DocumentEditor).GetProperty("Ribbon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                return prop.GetValue(editor) as GH_Ribbon;
            }
            return null;
        }

        private static void InitializeBackup()
        {
            if (Instances.DocumentEditor == null) return;
            var ribbon = GetRibbon(Instances.DocumentEditor);
            if (ribbon == null) return;

            if (_originalTabs == null)
            {
                _originalTabs = new List<GH_RibbonTab>();
            }

            // Sync with current tabs in the ribbon
            // Some new plugins might have loaded since last time.
            var currentTabs = GetRibbonTabs(ribbon);
            if (currentTabs != null)
            {
                foreach (var tab in currentTabs)
                {
                    if (!_originalTabs.Any(t => t.NameFull == tab.NameFull))
                    {
                        _originalTabs.Add(tab);
                    }
                }
            }
        }

        public static void ApplyLayoutPreview(List<PluginTabSettings> temporarySettings)
        {
            ApplyLayoutInternal(temporarySettings);
        }

        public static void ApplyLayout()
        {
            var settings = PluginManagerSettings.LoadSettings();
            ApplyLayoutInternal(settings);
        }

        private static void ApplyLayoutInternal(List<PluginTabSettings> settings)
        {
            if (Instances.DocumentEditor == null) return;
            var ribbon = GetRibbon(Instances.DocumentEditor);
            if (ribbon == null) return;

            InitializeBackup();

            var ribbonTabs = GetRibbonTabs(ribbon);
            if (ribbonTabs == null) return;

            ribbonTabs.Clear();

            // Reconstruct the list based on settings
            var sortedTabsToApply = new List<GH_RibbonTab>();

            foreach (var setting in settings)
            {
                if (setting.Visible)
                {
                    var tab = _originalTabs.FirstOrDefault(t => t.NameFull == setting.Name);
                    if (tab != null)
                    {
                        sortedTabsToApply.Add(tab);
                    }
                }
            }

            // Also add any new tabs that are not in the settings (default to visible and put them at the end)
            foreach (var tab in _originalTabs)
            {
                if (!settings.Any(s => s.Name == tab.NameFull))
                {
                    sortedTabsToApply.Add(tab);
                }
            }

            // Add them back to the ribbon
            ribbonTabs.AddRange(sortedTabsToApply);

            // Calling PopulateRibbon replaces the ribbon tabs with defaults, destroying our custom order.
            // Instead, we just need to re-layout the ribbon to fix any gaps left by removed/reordered tabs.
            // Grasshopper Ribbon dynamically recalculates rects when LayoutRibbon is called.

            // Re-call layout mechanism internally
            var mLayout = typeof(GH_Ribbon).GetMethod("LayoutRibbon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mLayout != null)
            {
                mLayout.Invoke(ribbon, null);
            }

            // Trigger a UI refresh
            ribbon.PerformLayout();
            ribbon.Refresh();
        }

        private static List<GH_RibbonTab> GetRibbonTabs(GH_Ribbon ribbon)
        {
            if (ribbon == null) return null;

            // In Grasshopper, Ribbon.Tabs is public property but might return a read-only collection?
            // Wait, looking at the dump, Tabs is a property. Let's try reflection to get the underlying list
            // because Ribbon.Tabs is usually List<GH_RibbonTab> or IEnumerable.

            // Actually, GH_Ribbon.Tabs is List<GH_RibbonTab> which is a reference type.
            // However, modifying it via property getter directly should work if it returns the list.
            try
            {
                var prop = typeof(GH_Ribbon).GetProperty("Tabs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null)
                {
                    return prop.GetValue(ribbon) as List<GH_RibbonTab>;
                }

                // Fallback to internal field if Property is somehow read-only.
                var field = typeof(GH_Ribbon).GetField("m_tabs", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    return field.GetValue(ribbon) as List<GH_RibbonTab>;
                }
            }
            catch { }

            return null;
        }
    }
}
