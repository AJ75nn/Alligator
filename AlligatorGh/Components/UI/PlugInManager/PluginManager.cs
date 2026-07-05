using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grasshopper;
using Grasshopper.GUI.Ribbon;
using Rhino;

namespace AlligatorGh.Components.UI.PlugInManager
{
    public static class PluginManager
    {
        // Persistent store of every tab we have ever seen during this session.
        // Hidden tabs are removed from the live ribbon, so this backup is required
        // to remember them so the user can re-enable them later. Keyed by NameFull for
        // O(1) dedup instead of the previous O(n) list scan.
        private static readonly Dictionary<string, GH_RibbonTab> _knownTabs = new Dictionary<string, GH_RibbonTab>(StringComparer.Ordinal);

        public static List<GH_RibbonTab> GetAllTabs()
        {
            InitializeBackup();
            return _knownTabs.Values.ToList();
        }

        public static GH_Ribbon GetRibbon(Grasshopper.GUI.GH_DocumentEditor editor)
        {
            if (editor == null) return null;
            var prop = typeof(Grasshopper.GUI.GH_DocumentEditor).GetProperty("Ribbon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                return prop.GetValue(editor) as GH_Ribbon;
            }
            LogReflectionFailure("GH_DocumentEditor.Ribbon", null);
            return null;
        }

        private static void InitializeBackup()
        {
            if (Instances.DocumentEditor == null) return;
            var ribbon = GetRibbon(Instances.DocumentEditor);
            if (ribbon == null) return;

            // Sync with current tabs in the ribbon.
            // Some new plugins might have loaded since last time.
            var currentTabs = GetRibbonTabs(ribbon);
            if (currentTabs != null)
            {
                foreach (var tab in currentTabs)
                {
                    if (tab == null) continue;
                    if (!_knownTabs.ContainsKey(tab.NameFull))
                    {
                        _knownTabs[tab.NameFull] = tab;
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

            // Snapshot the live tabs BEFORE clearing so we can restore them if the
            // rebuild throws. Without this, an exception between Clear() and AddRange()
            // would leave the ribbon permanently empty with no recovery path.
            var liveSnapshot = ribbonTabs.ToList();

            try
            {
                ribbonTabs.Clear();

                // Reconstruct the list based on settings
                var sortedTabsToApply = new List<GH_RibbonTab>();

                if (settings != null)
                {
                    foreach (var setting in settings)
                    {
                        if (setting.Visible && _knownTabs.TryGetValue(setting.Name, out var tab))
                        {
                            sortedTabsToApply.Add(tab);
                        }
                    }
                }

                // Also add any tabs that are not in the settings (default to visible and put them at the end)
                foreach (var tab in _knownTabs.Values)
                {
                    if (settings == null || !settings.Any(s => s.Name == tab.NameFull))
                    {
                        sortedTabsToApply.Add(tab);
                    }
                }

                // Add them back to the ribbon
                ribbonTabs.AddRange(sortedTabsToApply);

                // Re-call layout mechanism internally.
                var mLayout = typeof(GH_Ribbon).GetMethod("LayoutRibbon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mLayout != null)
                {
                    mLayout.Invoke(ribbon, null);
                }
                else
                {
                    LogReflectionFailure("GH_Ribbon.LayoutRibbon", null);
                }
            }
            catch (Exception ex)
            {
                // Restore the pre-apply live state so the ribbon is never left empty.
                LogReflectionFailure("ribbon rebuild", ex);
                try
                {
                    ribbonTabs.Clear();
                    ribbonTabs.AddRange(liveSnapshot);
                }
                catch (Exception ex2)
                {
                    LogReflectionFailure("ribbon restore", ex2);
                }
            }
            finally
            {
                // Trigger a UI refresh regardless of success/failure.
                ribbon.PerformLayout();
                ribbon.Refresh();
            }
        }

        private static List<GH_RibbonTab> GetRibbonTabs(GH_Ribbon ribbon)
        {
            if (ribbon == null) return null;

            // GH_Ribbon.Tabs is List<GH_RibbonTab>. Try the public property first, then
            // fall back to the internal field. Failures are logged rather than swallowed.
            try
            {
                var prop = typeof(GH_Ribbon).GetProperty("Tabs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null)
                {
                    return prop.GetValue(ribbon) as List<GH_RibbonTab>;
                }

                var field = typeof(GH_Ribbon).GetField("m_tabs", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    return field.GetValue(ribbon) as List<GH_RibbonTab>;
                }

                LogReflectionFailure("GH_Ribbon.Tabs / m_tabs", null);
            }
            catch (Exception ex)
            {
                LogReflectionFailure("GH_Ribbon.Tabs", ex);
            }

            return null;
        }

        private static void LogReflectionFailure(string member, Exception ex)
        {
            string detail = ex == null ? "member not found" : $"{ex.GetType().Name}: {ex.Message}";
            RhinoApp.WriteLine($"[Alligator] Plugin Manager: could not access '{member}' ({detail}). Ribbon layout may not apply correctly.");
        }
    }
}
