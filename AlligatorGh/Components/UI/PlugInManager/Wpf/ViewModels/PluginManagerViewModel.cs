using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AlligatorGh.Components.UI.PlugInManager.Wpf.Services;
using AlligatorGh.Components.UI.PlugInManager.Wpf.Theming;
using Grasshopper;
using Grasshopper.GUI.Ribbon;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf.ViewModels
{
    /// <summary>
    /// View-model for the Plugin Manager window. Owns the row collection, the live-preview pipeline,
    /// multi-select state, the summary footer, and the Light/Dark toggle. All ribbon manipulation is
    /// delegated to the existing <see cref="PluginManager"/> / <see cref="PluginManagerSettings"/>.
    /// </summary>
    public sealed class PluginManagerViewModel : INotifyPropertyChanged
    {
        private bool _showIcons = true;
        private bool _isDark;
        private string _summaryText = string.Empty;

        // Guards mirroring the original form: suppress per-item sync/preview during bulk + sync ops.
        private bool _suppressVisibilitySync;
        private PluginRowViewModel _lastSelected;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raised when the window should close (after Apply / Reset).</summary>
        public event EventHandler CloseRequested;

        /// <summary>Raised when the user flips the theme toggle; carries the new "is dark" value.</summary>
        public event EventHandler<bool> ThemeChangeRequested;

        /// <summary>
        /// Optional confirmation hook for Reset (the window supplies a dialog). Returns true to proceed.
        /// </summary>
        public Func<bool> ResetConfirmation { get; set; }

        /// <summary>The ribbon-tab rows, in display/order.</summary>
        public ObservableCollection<PluginRowViewModel> Rows { get; } = new ObservableCollection<PluginRowViewModel>();

        /// <summary>True once the user committed via Apply or Reset (so closing does not revert).</summary>
        public bool Committed { get; private set; }

        /// <summary>Check-all command (sets every row visible).</summary>
        public RelayCommand CheckAllCommand { get; }

        /// <summary>Check-none command (sets every row hidden).</summary>
        public RelayCommand CheckNoneCommand { get; }

        /// <summary>Apply command: persists settings, applies the layout, and closes.</summary>
        public RelayCommand ApplyCommand { get; }

        /// <summary>Reset command: clears settings (native order) and closes.</summary>
        public RelayCommand ResetCommand { get; }

        /// <summary>
        /// Builds the view-model and loads the current ribbon tabs.
        /// </summary>
        /// <param name="initialDark">The starting theme (from ThemeManager).</param>
        public PluginManagerViewModel(bool initialDark)
        {
            _isDark = initialDark;

            CheckAllCommand = new RelayCommand(() => SetAllVisible(true));
            CheckNoneCommand = new RelayCommand(() => SetAllVisible(false));
            ApplyCommand = new RelayCommand(Apply);
            ResetCommand = new RelayCommand(Reset);

            Load();
            UpdateSummary();
        }

        /// <summary>Whether the icon column is shown for every row.</summary>
        public bool ShowIcons
        {
            get => _showIcons;
            set
            {
                if (_showIcons == value)
                {
                    return;
                }

                _showIcons = value;
                OnPropertyChanged();
                foreach (var row in Rows)
                {
                    row.ShowIcon = value;
                }
            }
        }

        /// <summary>The active theme; setting it requests an animated theme change.</summary>
        public bool IsDark
        {
            get => _isDark;
            set
            {
                if (_isDark == value)
                {
                    return;
                }

                _isDark = value;
                OnPropertyChanged();
                ThemeChangeRequested?.Invoke(this, value);
            }
        }

        /// <summary>The summary footer text (tab/native/plugin/visible/hidden counts).</summary>
        public string SummaryText
        {
            get => _summaryText;
            private set
            {
                if (_summaryText == value)
                {
                    return;
                }

                _summaryText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Moves a row from one index to another (used by drag-reorder) and refreshes the live preview.
        /// </summary>
        /// <param name="from">Source index.</param>
        /// <param name="to">Destination index.</param>
        public void MoveRow(int from, int to)
        {
            if (from < 0 || to < 0 || from >= Rows.Count || to >= Rows.Count || from == to)
            {
                return;
            }

            Rows.Move(from, to);
            ApplyLivePreview();
        }

        /// <summary>
        /// Handles a row click for multi-selection (mirrors the original Ctrl/Shift/single logic).
        /// </summary>
        /// <param name="clicked">The clicked row.</param>
        /// <param name="isCtrl">Whether Ctrl was held (toggle).</param>
        /// <param name="isShift">Whether Shift was held (range).</param>
        public void HandleRowClick(PluginRowViewModel clicked, bool isCtrl, bool isShift)
        {
            if (clicked == null)
            {
                return;
            }

            if (isCtrl)
            {
                clicked.IsSelected = !clicked.IsSelected;
                _lastSelected = clicked;
            }
            else if (isShift && _lastSelected != null)
            {
                int start = Rows.IndexOf(_lastSelected);
                int end = Rows.IndexOf(clicked);
                int min = Math.Min(start, end);
                int max = Math.Max(start, end);

                for (int i = 0; i < Rows.Count; i++)
                {
                    Rows[i].IsSelected = i >= min && i <= max;
                }
            }
            else
            {
                foreach (var row in Rows)
                {
                    row.IsSelected = row == clicked;
                }

                _lastSelected = clicked;
            }
        }

        /// <summary>
        /// Rebuilds the ribbon preview from the current row order/visibility (without persisting),
        /// then refreshes the summary footer.
        /// </summary>
        public void ApplyLivePreview()
        {
            PluginManager.ApplyLayoutPreview(BuildSettings());
            UpdateSummary();
        }

        private void Load()
        {
            if (new GrasshopperUIFacade().GetDocumentEditor() == null)
            {
                return;
            }

            var ribbon = new GrasshopperUIFacade().GetRibbon();
            if (ribbon == null)
            {
                return;
            }

            List<PluginTabSettings> savedSettings = PluginManagerSettings.LoadSettings();
            var allTabs = PluginManager.GetAllTabs();

            // Reconcile saved settings against the live tab list, defaulting new tabs to visible and
            // ordering them after known tabs (same logic as the original form).
            var items = new List<PluginTabSettings>();
            int initialOrder = 0;
            foreach (var tab in allTabs)
            {
                var existing = savedSettings.FirstOrDefault(s => s.Name == tab.NameFull);
                items.Add(new PluginTabSettings
                {
                    Name = tab.NameFull,
                    Visible = existing?.Visible ?? true,
                    Order = existing?.Order ?? (savedSettings.Count == 0 ? initialOrder : int.MaxValue),
                });
                initialOrder++;
            }

            items = items
                .OrderBy(x => x.Order)
                .ThenBy(x => allTabs.FindIndex(t => t.NameFull == x.Name))
                .ToList();

            System.Drawing.Color symbolColor = SymbolColor(_isDark);

            foreach (var item in items)
            {
                var row = BuildRow(item, allTabs, symbolColor);
                Rows.Add(row);
            }
        }

        private PluginRowViewModel BuildRow(PluginTabSettings item, IReadOnlyList<GH_RibbonTab> allTabs, System.Drawing.Color symbolColor)
        {
            var icon = PluginIconProvider.ResolveIcon(item.Name, allTabs, symbolColor);

            var row = new PluginRowViewModel
            {
                Name = item.Name,
                Icon = ImageBridge.ToImageSource(icon),
                IsNative = PluginIconProvider.IsNativeCategory(item.Name),
            };

            row.SetVisibleSilently(item.Visible);
            row.ShowIcon = _showIcons;
            row.VisibilityChanged += OnRowVisibilityChanged;
            return row;
        }

        private void OnRowVisibilityChanged(object sender, EventArgs e)
        {
            if (_suppressVisibilitySync)
            {
                return;
            }

            var row = (PluginRowViewModel)sender;

            // If the toggled row is part of a selection, mirror its new visibility to the whole
            // selection; otherwise just the one row changes. Targets are synced silently so this
            // handler does not re-enter.
            if (row.IsSelected)
            {
                _suppressVisibilitySync = true;
                foreach (var target in GetTargetItems())
                {
                    if (target != row)
                    {
                        target.SetVisibleSilently(row.IsVisible);
                    }
                }

                _suppressVisibilitySync = false;
            }

            ApplyLivePreview();
        }

        private void SetAllVisible(bool visible)
        {
            _suppressVisibilitySync = true;
            foreach (var row in Rows)
            {
                row.SetVisibleSilently(visible);
            }

            _suppressVisibilitySync = false;
            ApplyLivePreview();
        }

        private IEnumerable<PluginRowViewModel> GetTargetItems()
        {
            var selected = Rows.Where(r => r.IsSelected).ToList();
            return selected.Count > 0 ? selected : Rows.ToList();
        }

        private List<PluginTabSettings> BuildSettings()
        {
            var settings = new List<PluginTabSettings>();
            for (int i = 0; i < Rows.Count; i++)
            {
                settings.Add(new PluginTabSettings
                {
                    Name = Rows[i].Name,
                    Visible = Rows[i].IsVisible,
                    Order = i,
                });
            }

            return settings;
        }

        private void Apply()
        {
            Committed = true;
            PluginManagerSettings.SaveSettings(BuildSettings());
            PluginManager.ApplyLayout();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Reset()
        {
            if (ResetConfirmation != null && !ResetConfirmation())
            {
                return;
            }

            Committed = true;
            PluginManagerSettings.SaveSettings(new List<PluginTabSettings>());
            PluginManager.ApplyLayout();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateSummary()
        {
            int total = Rows.Count;
            int natives = Rows.Count(r => r.IsNative);
            int plugins = total - natives;
            int visible = Rows.Count(r => r.IsVisible);
            int hidden = total - visible;

            SummaryText = $"Total {total}     Natives {natives}     Plugins {plugins}     Visible {visible}     Hidden {hidden}";
        }

        private static System.Drawing.Color SymbolColor(bool isDark)
        {
            var c = ThemePalette.For(isDark).Text;
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
