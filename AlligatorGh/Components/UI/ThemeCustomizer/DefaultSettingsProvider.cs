using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public static class DefaultSettingsProvider
    {
        private static bool _initialized = false;
        private static XmlDocument _guiDoc;
        private static XmlDocument _kernelDoc;

        private static void Initialize()
        {
            if (_initialized) return;

            Assembly assembly = Assembly.GetExecutingAssembly();

            _guiDoc = new XmlDocument();
            using (Stream stream = assembly.GetManifestResourceStream("AlligatorGh.Resources.grasshopper_gui.xml"))
            {
                if (stream != null)
                {
                    _guiDoc.Load(stream);
                }
            }

            _kernelDoc = new XmlDocument();
            using (Stream stream = assembly.GetManifestResourceStream("AlligatorGh.Resources.grasshopper_kernel.xml"))
            {
                if (stream != null)
                {
                    _kernelDoc.Load(stream);
                }
            }

            _initialized = true;
        }

        public static Color GetDefaultColor(string propertyName)
        {
            Initialize();

            if (_guiDoc != null)
            {
                XmlNode node = _guiDoc.SelectSingleNode($"//item[@name='{propertyName}']");
                if (node != null && node.Attributes["type_code"]?.Value == "36")
                {
                    XmlNode argbNode = node.SelectSingleNode("ARGB");
                    if (argbNode != null)
                    {
                        string[] parts = argbNode.InnerText.Split(';');
                        if (parts.Length == 4)
                        {
                            if (byte.TryParse(parts[0], out byte a) &&
                                byte.TryParse(parts[1], out byte r) &&
                                byte.TryParse(parts[2], out byte g) &&
                                byte.TryParse(parts[3], out byte b))
                            {
                                return Color.FromArgb(a, r, g, b);
                            }
                        }
                    }
                }
            }

            return Color.Empty;
        }

        public static string GetDefaultFont(string propertyName)
        {
            Initialize();

            if (_kernelDoc != null)
            {
                XmlNode node = _kernelDoc.SelectSingleNode($"//item[@name='{propertyName}']");
                if (node != null && node.Attributes["type_code"]?.Value == "10")
                {
                    return node.InnerText;
                }
            }

            return string.Empty;
        }
    }
}
