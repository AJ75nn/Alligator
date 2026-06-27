using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AlligatorGh.Components.UI.ThemeCustomizer;
using Grasshopper;
using Grasshopper.GUI;

namespace AlligatorGh.Components.UI.ThemeCustomizer.Wpf.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public event EventHandler CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
    }

    public class ThemePropertyViewModel : INotifyPropertyChanged
    {
        private readonly string _propertyKey;
        private readonly string _displayName;
        private bool _isPopupOpen;

        public ThemePropertyViewModel(string displayName, string propertyKey)
        {
            _displayName = displayName;
            _propertyKey = propertyKey;

            ResetCommand = new RelayCommand(ResetColor);
            TogglePopupCommand = new RelayCommand(() => IsPopupOpen = !IsPopupOpen);
        }

        public string DisplayName => _displayName;

        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                if (_isPopupOpen != value)
                {
                    _isPopupOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        public System.Windows.Media.Color CurrentColorMedia
        {
            get
            {
                var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                return System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B);
            }
        }

        public string HexColor
        {
            get
            {
                var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                return $"#{col.A:X2}{col.R:X2}{col.G:X2}{col.B:X2}";
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                try
                {
                    // Basic parsing. ColorTranslator.FromHtml handles "#RRGGBB" or "RRGGBB", but not always AARRGGBB properly
                    // We'll parse the hex string manually to ensure Alpha is supported
                    string hex = value.Trim().TrimStart('#');
                    if (hex.Length == 6)
                    {
                        Color c = ColorTranslator.FromHtml("#" + hex);
                        ThemeManager.SetCustomColor(_propertyKey, c, Instances.DocumentEditor);
                    }
                    else if (hex.Length == 8)
                    {
                        byte a = Convert.ToByte(hex.Substring(0, 2), 16);
                        byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                        byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                        byte b = Convert.ToByte(hex.Substring(6, 2), 16);
                        ThemeManager.SetCustomColor(_propertyKey, Color.FromArgb(a, r, g, b), Instances.DocumentEditor);
                    }

                    OnPropertyChanged(nameof(HexColor));
                    OnPropertyChanged(nameof(CurrentColorMedia));
                }
                catch { /* Ignore invalid hex input gracefully while typing */ }
            }
        }

        public ICommand ResetCommand { get; }
        public ICommand TogglePopupCommand { get; }

        private void ResetColor()
        {
            ThemeManager.ClearCustomColor(_propertyKey, Instances.DocumentEditor);
            Refresh();
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(CurrentColorMedia));
            OnPropertyChanged(nameof(HexColor));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class ThemeCustomizerViewModel : INotifyPropertyChanged
    {
        public ThemePropertyViewModel[] ColorProperties { get; }

        public ThemeCustomizerViewModel()
        {
            ColorProperties = new[]
            {
                new ThemePropertyViewModel("Canvas Background", "CustomCanvasBack"),
                new ThemePropertyViewModel("Canvas Grid", "CustomCanvasGrid"),
                new ThemePropertyViewModel("Canvas Edge", "CustomCanvasEdge"),
                new ThemePropertyViewModel("Canvas Shade", "CustomCanvasShade"),
                new ThemePropertyViewModel("Wire Default", "CustomWireDefault"),
                new ThemePropertyViewModel("Wire Selected A", "CustomWireSelectedA"),
                new ThemePropertyViewModel("Wire Selected B", "CustomWireSelectedB"),
                new ThemePropertyViewModel("Wire Empty", "CustomWireEmpty"),
                new ThemePropertyViewModel("Ribbon Background", "CustomRibbonBack"),
                new ThemePropertyViewModel("Ribbon Highlight", "CustomRibbonHighlight"),
                new ThemePropertyViewModel("Ribbon Text", "CustomRibbonText")
            };

            SetDefaultBaseThemeCommand = new RelayCommand(() => SetBaseTheme("Default"));
            SetDarkBaseThemeCommand = new RelayCommand(() => SetBaseTheme("Dark"));
            ResetAllCommand = new RelayCommand(ResetAllSettings);
        }

        public bool IsDefaultBaseTheme => ThemeManager.CurrentBaseTheme == "Default";
        public bool IsDarkBaseTheme => ThemeManager.CurrentBaseTheme == "Dark";

        public ICommand SetDefaultBaseThemeCommand { get; }
        public ICommand SetDarkBaseThemeCommand { get; }
        public ICommand ResetAllCommand { get; }

        private void SetBaseTheme(string theme)
        {
            ThemeManager.CurrentBaseTheme = theme;
            ThemeManager.ApplyTheme(Instances.DocumentEditor);

            OnPropertyChanged(nameof(IsDefaultBaseTheme));
            OnPropertyChanged(nameof(IsDarkBaseTheme));
            OnPropertyChanged(nameof(RibbonFontSizeText));
            foreach (var prop in ColorProperties)
                prop.Refresh();
        }

        private void ResetAllSettings()
        {
            var result = System.Windows.MessageBox.Show(
                "Are you sure you want to reset all theme settings to default?",
                "Reset Theme",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                ThemeManager.ClearAllCustomSettings(Instances.DocumentEditor);
                ThemeManager.CurrentBaseTheme = "Default";
                ThemeManager.ApplyTheme(Instances.DocumentEditor);

                OnPropertyChanged(nameof(IsDefaultBaseTheme));
                OnPropertyChanged(nameof(IsDarkBaseTheme));
                OnPropertyChanged(nameof(RibbonFontSizeText));
                foreach (var prop in ColorProperties)
                {
                    prop.Refresh();
                }
            }
        }

        public string RibbonFontSizeText
        {
            get
            {
                int sz = ThemeManager.GetCustomRibbonFontSize();
                return sz > 0 ? sz.ToString() : "";
            }
            set
            {
                if (int.TryParse(value, out int size) && size > 0)
                {
                    ThemeManager.SetCustomRibbonFontSize(size);
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    Instances.Settings.SetValue("CustomRibbonFontSize", 0);
                    ThemeManager.ApplyTheme(Instances.DocumentEditor);
                }
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}