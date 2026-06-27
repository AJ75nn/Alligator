using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AlligatorGh.Components.UI.PlugInManager.Wpf.Services;
using AlligatorGh.Components.UI.PlugInManager.Wpf.ViewModels;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf
{
    /// <summary>
    /// The Plugin Manager window. Hosts the themed view, drives drag-reorder and multi-select on the
    /// row list, wires the Light/Dark <see cref="ThemeService"/>, and preserves the revert-on-close
    /// contract (closing without Apply restores the saved layout).
    /// </summary>
    public partial class PluginManagerWindow : Window
    {
        private readonly PluginManagerViewModel _viewModel;
        private readonly ThemeService _themeService;

        private Point _dragStart;
        private bool _mouseDownOnHandle;
        private PluginRowViewModel _dragCandidate;
        private PluginRowViewModel _draggingRow;
        private InsertionAdorner _adorner;

        /// <summary>
        /// Creates the window for the given view-model and starting theme.
        /// </summary>
        /// <param name="viewModel">The window's view-model.</param>
        /// <param name="initialDark">The starting theme (from ThemeManager).</param>
        public PluginManagerWindow(PluginManagerViewModel viewModel, bool initialDark)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            // Install themed brushes into this window's resources before the visual tree resolves
            // the DynamicResource references.
            _themeService = new ThemeService(Resources, initialDark);

            DataContext = _viewModel;

            _viewModel.ResetConfirmation = ConfirmReset;
            _viewModel.CloseRequested += (_, _) => Close();
            _viewModel.ThemeChangeRequested += (_, isDark) => _themeService.SetTheme(isDark);

            Closing += PluginManagerWindow_Closing;
        }

        private void PluginManagerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Revert the live preview to the saved layout unless the user committed via Apply/Reset.
            if (!_viewModel.Committed)
            {
                PluginManager.ApplyLayout();
            }

            // Persist the theme choice so the window reopens on the last-used theme.
            _themeService.PersistChoice();
        }

        private bool ConfirmReset()
        {
            return MessageBox.Show(
                this,
                "Reset the ribbon tabs to their native Grasshopper order?",
                "Reset ribbon",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
