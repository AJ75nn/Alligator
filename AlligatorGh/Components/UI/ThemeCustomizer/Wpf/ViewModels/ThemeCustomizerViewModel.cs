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

            UpdateHSVFromColor();

            ResetCommand = new RelayCommand(ResetColor);
            TogglePopupCommand = new RelayCommand(() => {
                IsPopupOpen = !IsPopupOpen;
                if (IsPopupOpen) UpdateHSVFromColor();
            });
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
                return $"{col.A:X2}{col.R:X2}{col.G:X2}{col.B:X2}";
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                try
                {
                    string hex = value.Trim().TrimStart('#');
                    if (hex.Length == 6)
                    {
                        Color c = ColorTranslator.FromHtml("#" + hex);
                        ThemeManager.SetCustomColor(_propertyKey, c, new GrasshopperUIFacade().GetDocumentEditor());
                    }
                    else if (hex.Length == 8)
                    {
                        byte a = Convert.ToByte(hex.Substring(0, 2), 16);
                        byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                        byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                        byte b = Convert.ToByte(hex.Substring(6, 2), 16);
                        ThemeManager.SetCustomColor(_propertyKey, Color.FromArgb(a, r, g, b), new GrasshopperUIFacade().GetDocumentEditor());
                    }

                    Refresh();
                }
                catch { /* Ignore invalid hex input gracefully while typing */ }
            }
        }

        public string RValue
        {
            get
            {
                var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                return col.R.ToString();
            }
            set
            {
                if (byte.TryParse(value, out byte r))
                {
                    var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                    ThemeManager.SetCustomColor(_propertyKey, Color.FromArgb(col.A, r, col.G, col.B), new GrasshopperUIFacade().GetDocumentEditor());
                    Refresh();
                }
            }
        }

        public string GValue
        {
            get
            {
                var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                return col.G.ToString();
            }
            set
            {
                if (byte.TryParse(value, out byte g))
                {
                    var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                    ThemeManager.SetCustomColor(_propertyKey, Color.FromArgb(col.A, col.R, g, col.B), new GrasshopperUIFacade().GetDocumentEditor());
                    Refresh();
                }
            }
        }

        public string BValue
        {
            get
            {
                var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                return col.B.ToString();
            }
            set
            {
                if (byte.TryParse(value, out byte b))
                {
                    var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                    ThemeManager.SetCustomColor(_propertyKey, Color.FromArgb(col.A, col.R, col.G, b), new GrasshopperUIFacade().GetDocumentEditor());
                    Refresh();
                }
            }
        }

        private double _hue = 0;
        private double _saturation = 0;
        private double _value = 0;

        private bool _isUpdatingFromHSV = false;

        public double Hue => _hue;
        public double Saturation => _saturation;
        public double Value => _value;

        public System.Windows.Media.Color HueColorMedia
        {
            get
            {
                Color c = ColorFromHSV(_hue, 1.0, 1.0);
                return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            }
        }

        public void SetHue(double hue)
        {
            _hue = hue;
            UpdateColorFromHSV();
            OnPropertyChanged(nameof(Hue));
            OnPropertyChanged(nameof(HueColorMedia));
        }

        public void SetSaturationValue(double saturation, double value)
        {
            _saturation = saturation;
            _value = value;
            UpdateColorFromHSV();
            OnPropertyChanged(nameof(Saturation));
            OnPropertyChanged(nameof(Value));
        }

        private void UpdateColorFromHSV()
        {
            _isUpdatingFromHSV = true;
            Color c = ColorFromHSV(_hue, _saturation, _value);
            ThemeManager.SetCustomColor(_propertyKey, c, new GrasshopperUIFacade().GetDocumentEditor());
            Refresh();
            _isUpdatingFromHSV = false;
        }

        private void UpdateHSVFromColor()
        {
            if (_isUpdatingFromHSV) return;

            var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
            ColorToHSV(col, out _hue, out _saturation, out _value);
            OnPropertyChanged(nameof(Hue));
            OnPropertyChanged(nameof(Saturation));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(HueColorMedia));
        }

        private static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0) return Color.FromArgb(255, v, t, p);
            else if (hi == 1) return Color.FromArgb(255, q, v, p);
            else if (hi == 2) return Color.FromArgb(255, p, v, t);
            else if (hi == 3) return Color.FromArgb(255, p, q, v);
            else if (hi == 4) return Color.FromArgb(255, t, p, v);
            else return Color.FromArgb(255, v, p, q);
        }

        public ICommand ResetCommand { get; }
        public ICommand TogglePopupCommand { get; }

        private void ResetColor()
        {
            ThemeManager.ClearCustomColor(_propertyKey, new GrasshopperUIFacade().GetDocumentEditor());
            Refresh();
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(CurrentColorMedia));
            OnPropertyChanged(nameof(HexColor));
            OnPropertyChanged(nameof(RValue));
            OnPropertyChanged(nameof(GValue));
            OnPropertyChanged(nameof(BValue));

            UpdateHSVFromColor();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class ThemeCustomizerViewModel : INotifyPropertyChanged
    {
        public ThemeGroupViewModel[] Groups { get; }

        public ThemeCustomizerViewModel()
        {
            Groups = new[]
            {
                new ThemeGroupViewModel("General",
                    new ThemePropertyViewModel("Canvas Background", "CustomCanvasBack"),
                    new ThemePropertyViewModel("Canvas Grid", "CustomCanvasGrid"),
                    new ThemePropertyViewModel("Canvas Edge", "CustomCanvasEdge"),
                    new ThemePropertyViewModel("Canvas Shade", "CustomCanvasShade")
                ),
                new ThemeGroupViewModel("Wiring",
                    new ThemePropertyViewModel("Wire Default", "CustomWireDefault"),
                    new ThemePropertyViewModel("Wire Selected A", "CustomWireSelectedA"),
                    new ThemePropertyViewModel("Wire Selected B", "CustomWireSelectedB"),
                    new ThemePropertyViewModel("Wire Empty", "CustomWireEmpty")
                ),
                new ThemeGroupViewModel("Components",
                    new ThemePropertyViewModel("Component Background", "CustomComponentBack"),
                    new ThemePropertyViewModel("Component Border", "CustomComponentBorder"),
                    new ThemePropertyViewModel("Component Warning", "CustomComponentWarning"),
                    new ThemePropertyViewModel("Component Error", "CustomComponentError"),
                    new ThemePropertyViewModel("Component Hidden", "CustomComponentHidden")
                ),
                new ThemeGroupViewModel("Elements",
                    new ThemePropertyViewModel("Ribbon Background", "CustomRibbonBack"),
                    new ThemePropertyViewModel("Ribbon Highlight", "CustomRibbonHighlight"),
                    new ThemePropertyViewModel("Ribbon Text", "CustomRibbonText"),
                    new ThemePropertyViewModel("Scribble Text", "CustomScribbleText")
                )
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
            ThemeManager.ApplyTheme(new GrasshopperUIFacade().GetDocumentEditor());

            OnPropertyChanged(nameof(IsDefaultBaseTheme));
            OnPropertyChanged(nameof(IsDarkBaseTheme));
            OnPropertyChanged(nameof(RibbonFontSizeText));
            OnPropertyChanged(nameof(CompFontSizeText));
            OnPropertyChanged(nameof(CompFontType));
            foreach (var group in Groups)
                foreach(var prop in group.Properties) prop.Refresh();
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
                ThemeManager.ClearAllCustomSettings(new GrasshopperUIFacade().GetDocumentEditor());
                ThemeManager.CurrentBaseTheme = "Default";
                ThemeManager.ApplyTheme(new GrasshopperUIFacade().GetDocumentEditor());

                OnPropertyChanged(nameof(IsDefaultBaseTheme));
                OnPropertyChanged(nameof(IsDarkBaseTheme));
                OnPropertyChanged(nameof(RibbonFontSizeText));
                OnPropertyChanged(nameof(CompFontSizeText));
                OnPropertyChanged(nameof(CompFontType));
                foreach (var group in Groups)
                    foreach(var prop in group.Properties) prop.Refresh();
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
                    ThemeManager.ApplyTheme(new GrasshopperUIFacade().GetDocumentEditor());
                }
                OnPropertyChanged();
            }
        }

        public string CompFontSizeText
        {
            get
            {
                int sz = Instances.Settings.GetValue("CustomCompFontSize", 0);
                return sz > 0 ? sz.ToString() : "";
            }
            set
            {
                if (int.TryParse(value, out int size) && size > 0)
                {
                    Instances.Settings.SetValue("CustomCompFontSize", size);
                    ThemeManager.ApplyTheme(new GrasshopperUIFacade().GetDocumentEditor());
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    Instances.Settings.SetValue("CustomCompFontSize", 0);
                    ThemeManager.ApplyTheme(new GrasshopperUIFacade().GetDocumentEditor());
                }
                OnPropertyChanged();
            }
        }

        public string CompFontType
        {
            get
            {
                return Instances.Settings.GetValue("CustomCompFontType", "");
            }
            set
            {
                Instances.Settings.SetValue("CustomCompFontType", value);
                ThemeManager.ApplyTheme(new GrasshopperUIFacade().GetDocumentEditor());
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}