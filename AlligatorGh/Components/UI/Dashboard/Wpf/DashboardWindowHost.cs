using System;
using System.Windows.Interop;
using AlligatorGh.Components.UI.Dashboard.Wpf.ViewModels;
using AlligatorGh.Components.UI.PlugInManager.Wpf.ViewModels;
using AlligatorGh.Components.UI.ThemeCustomizer.Wpf.ViewModels;
using AlligatorGh.Components.UI.ThemeCustomizer;
using Grasshopper.GUI;

namespace AlligatorGh.Components.UI.Dashboard.Wpf
{
    public static class DashboardWindowHost
    {
        private static DashboardWindow _current;

        public static void ShowOrActivate(GH_DocumentEditor editor)
        {
            if (_current != null)
            {
                _current.Activate();
                return;
            }

            bool isDark = ThemeManager.CurrentBaseTheme == "Dark";

            var pluginManagerViewModel = new PluginManagerViewModel(isDark);
            var themeCustomizerViewModel = new ThemeCustomizerViewModel();

            var dashboardViewModel = new DashboardViewModel
            {
                PluginManagerViewModel = pluginManagerViewModel,
                ThemeCustomizerViewModel = themeCustomizerViewModel
            };

            var window = new DashboardWindow
            {
                DataContext = dashboardViewModel
            };

            dashboardViewModel.CloseRequested += (_, _) => window.Close();
            pluginManagerViewModel.CloseRequested += (_, _) => window.Close();

            var themeService = new AlligatorGh.Components.UI.PlugInManager.Wpf.Services.ThemeService(window.Resources, isDark);

            pluginManagerViewModel.ThemeChangeRequested += (_, requestedDark) => themeService.SetTheme(requestedDark);

            themeCustomizerViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(themeCustomizerViewModel.IsDarkBaseTheme) || e.PropertyName == nameof(themeCustomizerViewModel.IsDefaultBaseTheme))
                {
                    bool isNowDark = ThemeManager.CurrentBaseTheme == "Dark";
                    themeService.SetTheme(isNowDark);
                    pluginManagerViewModel.IsDark = isNowDark;
                }
            };

            if (editor != null)
            {
                new WindowInteropHelper(window).Owner = editor.Handle;
            }

            window.Closed += (_, _) =>
            {
                _current = null;
                if (!pluginManagerViewModel.Committed)
                {
                    AlligatorGh.Components.UI.PlugInManager.PluginManager.ApplyLayout();
                }
                themeService.PersistChoice();
            };

            _current = window;
            dashboardViewModel.UpdateCurrentViewModel();
            window.Show();
        }
    }
}