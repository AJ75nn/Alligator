using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;

namespace AlligatorGh
{
    public class PluginTabSettings
    {
        public string Name { get; set; }
        public bool Visible { get; set; }
        public int Order { get; set; }
    }

    public static class PluginManagerSettings
    {
        private const string SettingsKey = "Alligator.PluginManager.Tabs";

        public static List<PluginTabSettings> LoadSettings()
        {
            string rawData = Instances.Settings.GetValue(SettingsKey, string.Empty);
            if (string.IsNullOrEmpty(rawData))
            {
                return new List<PluginTabSettings>();
            }

            try
            {
                var settingsList = new List<PluginTabSettings>();
                var parts = rawData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var props = part.Split('|');
                    if (props.Length >= 3)
                    {
                        settingsList.Add(new PluginTabSettings
                        {
                            Name = props[0],
                            Visible = bool.Parse(props[1]),
                            Order = int.Parse(props[2])
                        });
                    }
                }
                return settingsList.OrderBy(s => s.Order).ToList();
            }
            catch
            {
                return new List<PluginTabSettings>();
            }
        }

        public static void SaveSettings(List<PluginTabSettings> settings)
        {
            if (settings == null || settings.Count == 0)
            {
                Instances.Settings.SetValue(SettingsKey, string.Empty);
                Instances.Settings.WritePersistentSettings();
                return;
            }

            // Format: Name|Visible|Order;Name|Visible|Order
            var parts = settings.Select(s => $"{s.Name}|{s.Visible}|{s.Order}");
            string rawData = string.Join(";", parts);

            Instances.Settings.SetValue(SettingsKey, rawData);
            Instances.Settings.WritePersistentSettings();
        }
    }
}
