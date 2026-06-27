using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf.ViewModels
{
    /// <summary>
    /// View-model for a single ribbon-tab row in the Plugin Manager: its name, icon, visibility,
    /// selection state, and whether it is a native Grasshopper category (for the summary footer).
    /// </summary>
    public sealed class PluginRowViewModel : INotifyPropertyChanged
    {
        private bool _isVisible;
        private bool _isSelected;
        private bool _showIcon;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raised when <see cref="IsVisible"/> changes due to user interaction, so the parent
        /// view-model can run the live preview and selection-visibility sync.
        /// </summary>
        public event EventHandler VisibilityChanged;

        /// <summary>The full tab name (matches GH_RibbonTab.NameFull).</summary>
        public string Name { get; init; }

        /// <summary>The resolved 16px icon, already converted to a WPF source; may be null.</summary>
        public ImageSource Icon { get; init; }

        /// <summary>True when this tab is a built-in Grasshopper category.</summary>
        public bool IsNative { get; init; }

        /// <summary>True when an icon was resolved (drives the icon placeholder vs image).</summary>
        public bool HasIcon => Icon != null;

        /// <summary>Whether the tab is shown on the ribbon.</summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                _isVisible = value;
                OnPropertyChanged();
                VisibilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Whether the row is part of the current multi-selection.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Whether the icon column is shown (toggled globally by "Show Icons").</summary>
        public bool ShowIcon
        {
            get => _showIcon;
            set
            {
                if (_showIcon == value)
                {
                    return;
                }

                _showIcon = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Sets <see cref="IsVisible"/> without raising <see cref="VisibilityChanged"/>, used during
        /// bulk operations and selection-sync to avoid re-entrancy and redundant preview passes.
        /// </summary>
        /// <param name="value">The new visibility.</param>
        public void SetVisibleSilently(bool value)
        {
            if (_isVisible == value)
            {
                return;
            }

            _isVisible = value;
            OnPropertyChanged(nameof(IsVisible));
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
