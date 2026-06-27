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

        public ThemePropertyViewModel(string displayName, string propertyKey)
        {
            _displayName = displayName;
            _propertyKey = propertyKey;

            ResetCommand = new RelayCommand(ResetColor);
            PickColorCommand = new RelayCommand(PickColor);
        }

        public string DisplayName => _displayName;

        public System.Windows.Media.Color CurrentColorMedia
        {
            get
            {
                var col = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);
                return System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B);
            }
        }

        public ICommand ResetCommand { get; }
        public ICommand PickColorCommand { get; }

        private void ResetColor()
        {
            ThemeManager.ClearCustomColor(_propertyKey, Instances.DocumentEditor);
            OnPropertyChanged(nameof(CurrentColorMedia));
        }

        private void PickColor()
        {
            var currentColor = ThemeManager.GetCustomColor(_propertyKey) ?? ThemeManager.GetDefaultColorForProperty(_propertyKey);

            var pickerForm = new System.Windows.Forms.Form
            {
                Text = "Pick Color",
                Size = new System.Drawing.Size(250, 320),
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            };

            var picker = new GH_ColourPicker
            {
                Colour = currentColor,
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            pickerForm.Controls.Add(picker);

            var panel = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Bottom, Height = 40 };
            var okBtn = new System.Windows.Forms.Button { Text = "OK", DialogResult = System.Windows.Forms.DialogResult.OK, Dock = System.Windows.Forms.DockStyle.Right, Width = 80 };
            panel.Controls.Add(okBtn);
            pickerForm.AcceptButton = okBtn;

            if (pickerForm.ShowDialog(Instances.DocumentEditor) == System.Windows.Forms.DialogResult.OK)
            {
                ThemeManager.SetCustomColor(_propertyKey, picker.Colour, Instances.DocumentEditor);
                OnPropertyChanged(nameof(CurrentColorMedia));
            }
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
        }

        public bool IsDefaultBaseTheme => ThemeManager.CurrentBaseTheme == "Default";
        public bool IsDarkBaseTheme => ThemeManager.CurrentBaseTheme == "Dark";

        public ICommand SetDefaultBaseThemeCommand { get; }
        public ICommand SetDarkBaseThemeCommand { get; }

        private void SetBaseTheme(string theme)
        {
            ThemeManager.ClearAllCustomSettings(Instances.DocumentEditor);
            ThemeManager.CurrentBaseTheme = theme;
            ThemeManager.ApplyTheme(Instances.DocumentEditor);

            OnPropertyChanged(nameof(IsDefaultBaseTheme));
            OnPropertyChanged(nameof(IsDarkBaseTheme));
            OnPropertyChanged(nameof(RibbonFontSizeText));
            foreach (var prop in ColorProperties)
                prop.GetType().GetMethod("OnPropertyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(prop, new object[] { "CurrentColorMedia" });
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