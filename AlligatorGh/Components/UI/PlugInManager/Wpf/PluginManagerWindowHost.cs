using System;
using System.Windows;
using System.Windows.Interop;
using AlligatorGh.Components.UI.PlugInManager.Wpf.ViewModels;
using AlligatorGh.Components.UI.ThemeCustomizer;
using Grasshopper.GUI;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf
{
    /// <summary>
    /// Bridges the WinForms-hosted Grasshopper editor to the WPF Plugin Manager window: it owns the
    /// single live instance, parents the window to the editor's HWND, and shows it modeless.
    /// </summary>
    public static class PluginManagerWindowHost
    {
        private static PluginManagerWindow _current;

        /// <summary>
        /// Shows the Plugin Manager, or activates the existing one if it is already open (a single
        /// instance avoids two windows driving the live ribbon preview at once).
        /// </summary>
        /// <param name="editor">The Grasshopper document editor that owns the window.</param>
        public static void ShowOrActivate(GH_DocumentEditor editor)
        {
            if (_current != null)
            {
                _current.Activate();
                return;
            }

            bool isDark = ThemeManager.CurrentBaseTheme == "Dark";
            var viewModel = new PluginManagerViewModel(isDark);
            var window = new PluginManagerWindow(viewModel, isDark);

            // Parent the WPF window to the GH editor's Win32 handle so it stays above the editor and
            // minimises/restores with it. Must be set before Show().
            if (editor != null)
            {
                new WindowInteropHelper(window).Owner = editor.Handle;
            }

            window.Closed += (_, _) => _current = null;
            _current = window;
            window.Show();
        }
    }
}
