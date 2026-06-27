using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AlligatorGh.Components.UI.Dashboard.Wpf.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private bool _isSidebarExpanded = true;
        private bool _isPluginManagerSelected = true;
        private bool _isThemeCustomizerSelected = false;
        private object _currentViewModel;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CloseRequested;

        public DashboardViewModel()
        {
        }

        public bool IsSidebarExpanded
        {
            get => _isSidebarExpanded;
            set
            {
                if (_isSidebarExpanded != value)
                {
                    _isSidebarExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPluginManagerSelected
        {
            get => _isPluginManagerSelected;
            set
            {
                if (_isPluginManagerSelected != value)
                {
                    _isPluginManagerSelected = value;
                    OnPropertyChanged();
                    if (value) UpdateCurrentViewModel();
                }
            }
        }

        public bool IsThemeCustomizerSelected
        {
            get => _isThemeCustomizerSelected;
            set
            {
                if (_isThemeCustomizerSelected != value)
                {
                    _isThemeCustomizerSelected = value;
                    OnPropertyChanged();
                    if (value) UpdateCurrentViewModel();
                }
            }
        }

        public object CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public object PluginManagerViewModel { get; set; }
        public object ThemeCustomizerViewModel { get; set; }

        public void UpdateCurrentViewModel()
        {
            if (IsPluginManagerSelected)
                CurrentViewModel = PluginManagerViewModel;
            else if (IsThemeCustomizerSelected)
                CurrentViewModel = ThemeCustomizerViewModel;
        }

        public void RequestClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}