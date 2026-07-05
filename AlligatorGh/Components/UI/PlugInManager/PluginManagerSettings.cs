using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper;

namespace AlligatorGh.Components.UI.PlugInManager
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
                            Name = Unescape(props[0]),
                            Visible = bool.Parse(props[1]),
                            Order = int.Parse(props[2])
                        });
                    }
                }
                return settingsList.OrderBy(s => s.Order).ToList();
            }
            catch
            {
                // Corrupt store: reset to empty rather than crashing the host.
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

            // Format: EscapedName|Visible|Order;EscapedName|Visible|Order
            // Names are escaped so that '|' or ';' inside a tab name cannot corrupt
            // the delimiter-based format.
            var parts = settings.Select(s => $"{Escape(s.Name)}|{s.Visible}|{s.Order}");
            string rawData = string.Join(";", parts);

            Instances.Settings.SetValue(SettingsKey, rawData);
            Instances.Settings.WritePersistentSettings();
        }

        // Escape delimiters and the escape char itself so tab names containing '|',
        // ';' or '\' round-trip safely through the delimited format.
        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c == '\\') sb.Append("\\\\");
                else if (c == '|') sb.Append("\\p");
                else if (c == ';') sb.Append("\\s");
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private static string Unescape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\\' && i + 1 < value.Length)
                {
                    char next = value[i + 1];
                    if (next == '\\') { sb.Append('\\'); i++; }
                    else if (next == 'p') { sb.Append('|'); i++; }
                    else if (next == 's') { sb.Append(';'); i++; }
                    else { sb.Append(value[i]); }
                }
                else
                {
                    sb.Append(value[i]);
                }
            }
            return sb.ToString();
        }
    }
}
