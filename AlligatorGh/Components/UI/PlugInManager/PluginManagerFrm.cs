using Grasshopper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlligatorGh.Components.UI.PlugInManager
{
    public partial class PluginManagerFrm : Form
    {



        private DraggablePluginItem _lastSelected = null;
        private bool _isBulkUpdating = false;
        private bool _saved = false;


        public PluginManagerFrm()
        {
            InitializeComponent();
            LoadData();


            // Set up native drag-and-drop on the FlowLayoutPanel
            flpTabList.AllowDrop = true;


        }

        private void flpTabList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DraggablePluginItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void flpTabList_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(DraggablePluginItem))) return;

            DraggablePluginItem draggingItem = (DraggablePluginItem)e.Data.GetData(typeof(DraggablePluginItem));

            Point clientPoint = flpTabList.PointToClient(new Point(e.X, e.Y));

            DraggablePluginItem targetItem = null;
            foreach (Control ctrl in flpTabList.Controls)
            {
                if (ctrl is DraggablePluginItem item && item != draggingItem)
                {
                    if (clientPoint.Y > item.Top && clientPoint.Y < item.Bottom)
                    {
                        targetItem = item;
                        break;
                    }
                }
            }

            if (targetItem != null)
            {
                int currentIndex = flpTabList.Controls.GetChildIndex(draggingItem);
                int targetIndex = flpTabList.Controls.GetChildIndex(targetItem);

                if (currentIndex != targetIndex)
                {
                    flpTabList.Controls.SetChildIndex(draggingItem, targetIndex);
                    ApplyLivePreview();
                }
            }
        }

        private void flpTabList_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DraggablePluginItem)))
            {
                DraggablePluginItem draggingItem = (DraggablePluginItem)e.Data.GetData(typeof(DraggablePluginItem));
                draggingItem.BackColor = draggingItem.IsSelected ? Color.LightBlue : Color.White;
            }
        }

        private void ApplyLivePreview()
        {
            // Gather current ordered settings based on UI
            var newSettings = new List<PluginTabSettings>();
            for (int i = 0; i < flpTabList.Controls.Count; i++)
            {
                if (flpTabList.Controls[i] is DraggablePluginItem item)
                {
                    newSettings.Add(new PluginTabSettings
                    {
                        Name = item.PluginName,
                        Visible = item.IsVisible,
                        Order = i
                    });
                }
            }

            // Temporarily apply to layout but DO NOT save to disk yet
            PluginManager.ApplyLayoutPreview(newSettings);

            UpdateSummaryFooter();
        }

        private void UpdateSummaryFooter()
        {
            int totalTabs = flpTabList.Controls.Count;
            int natives = 0;
            int plugins = 0;
            int visible = 0;
            int hidden = 0;

            foreach (Control ctrl in flpTabList.Controls)
            {
                if (ctrl is DraggablePluginItem item)
                {
                    if (item.IsVisible) visible++;
                    else hidden++;

                    // Simple heuristic for native tabs: usually have fewer components or specific names
                    // However, actual native detection is tricky. Let's use a known list or assume based on icons if needed.
                    // For now, let's look for standard Grasshopper categories:
                    string[] nativeCats = { "Params", "Maths", "Sets", "Vector", "Curve", "Surface", "Mesh", "Intersect", "Transform", "Display" };
                    if (nativeCats.Contains(item.PluginName)) natives++;
                    else plugins++;
                }
            }

            lblSummary.Text = @$"Total Tabs : {totalTabs}    Natives : {natives}    Installed Plugins : {plugins}    Visible : {visible}    Hidden : {hidden}";
        }

        private void LoadData()
        {
            if (Instances.DocumentEditor == null)
                return;

            var ribbon = PluginManager.GetRibbon(Instances.DocumentEditor);
            if (ribbon == null)
                return;

            List<PluginTabSettings> savedSettings = PluginManagerSettings.LoadSettings();
            var allTabs = PluginManager.GetAllTabs();

            var items = new List<PluginTabSettings>();

            int initialOrder = 0;
            foreach (var tab in allTabs)
            {
                var existingSetting = savedSettings.FirstOrDefault(s => s.Name == tab.NameFull);
                if (existingSetting != null)
                {
                    items.Add(new PluginTabSettings
                    {
                        Name = tab.NameFull,
                        Visible = existingSetting.Visible,
                        Order = existingSetting.Order
                    });
                }
                else
                {
                    items.Add(new PluginTabSettings
                    {
                        Name = tab.NameFull,
                        Visible = true,
                        Order = savedSettings.Count == 0 ? initialOrder : int.MaxValue
                    });
                }
                initialOrder++;
            }

            items = items.OrderBy(x => x.Order).ThenBy(x => allTabs.FindIndex(t => t.NameFull == x.Name)).ToList();

            flpTabList.Controls.Clear();
            foreach (var item in items)
            {
                // Find icon from ComponentServer libraries matching the tab name
                Image icon = null;
                var lib = Instances.ComponentServer.Libraries.FirstOrDefault(l =>
                    l.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

                if (lib != null && lib.Icon != null)
                {
                    icon = lib.Icon;
                }
                else
                {
                    // Fallback to internal grasshopper resources for native tabs (e.g. "Maths" -> "Category_Maths_16x16")
                    var type = typeof(Instances).Assembly.GetType("Grasshopper.My.Resources.Res_CategoryIcons");
                    if (type != null)
                    {
                        string safeName = item.Name.Replace(" ", "");

                        var prop = type.GetProperty($"Category_{safeName}_16x16", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                        // Fallback 1: Try plural form (e.g., "Surface" -> "Surfaces")
                        if (prop == null && !safeName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                        {
                            prop = type.GetProperty($"Category_{safeName}s_16x16", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        }

                        // Fallback 2: Try without 's' at the end
                        if (prop == null && safeName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                        {
                            prop = type.GetProperty($"Category_{safeName.Substring(0, safeName.Length - 1)}_16x16", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        }

                        if (prop != null)
                        {
                            icon = prop.GetValue(null) as Image;
                        }
                    }

                    // Fallback to NameSymbol generation for 3rd-party plugins without library icons
                    if (icon == null)
                    {
                        var tab = allTabs.FirstOrDefault(t => t.NameFull == item.Name);
                        if (tab != null && !string.IsNullOrEmpty(tab.NameSymbol))
                        {
                            icon = CreateSymbolIcon(tab.NameSymbol);
                        }
                    }
                }

                int catCount = 0;
                int compCount = 0;

                var ghTab = allTabs.FirstOrDefault(t => t.NameFull == item.Name);
                if (ghTab != null && ghTab.Panels != null)
                {
                    catCount = ghTab.Panels.Count;
                    foreach (var panel in ghTab.Panels)
                    {
                        var buttonsProp = typeof(Grasshopper.GUI.Ribbon.GH_RibbonPanel).GetProperty("Buttons", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        if (buttonsProp != null)
                        {
                            var buttons = buttonsProp.GetValue(panel) as System.Collections.IList;
                            if (buttons != null)
                            {
                                compCount += buttons.Count;
                            }
                        }
                    }
                }

                var row = new DraggablePluginItem
                {
                    PluginName = item.Name,
                    IsVisible = item.Visible,
                    PluginIcon = icon,
                    ShowIcon = chbIcon.Checked,
                    Width = flpTabList.ClientSize.Width - 10
                };

                row.HandleMouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        row.BackColor = Color.LightGray; // Highlight while dragging
                        row.DoDragDrop(row, DragDropEffects.Move);
                        // Once DoDragDrop returns, dragging has finished
                        row.BackColor = row.IsSelected ? Color.LightBlue : Color.White;
                    }
                };

                // If this item's visibility changes and it's selected, we sync it to other selected items.
                bool isSyncingVisibility = false;
                row.VisibilityChanged += (s, e) =>
                {
                    if (isSyncingVisibility || _isBulkUpdating) return; // Prevent infinite recursion or UI lockup during mass check

                    if (row.IsSelected)
                    {
                        isSyncingVisibility = true;
                        var itemsToUpdate = GetTargetItems();
                        foreach (var target in itemsToUpdate)
                        {
                            if (target != row)
                            {
                                target.IsVisible = row.IsVisible;
                            }
                        }
                        isSyncingVisibility = false;
                    }

                    ApplyLivePreview();
                };

                // Forward click to form for multi-select logic
                WireUpClickRecursive(row);

                flpTabList.Controls.Add(row);
            }

            // Handle resize
            flpTabList.Resize += (s, e) =>
            {
                foreach (Control ctrl in flpTabList.Controls)
                {
                    ctrl.Width = flpTabList.ClientSize.Width - 10;
                }
            };

            UpdateSummaryFooter();
        }
        private Image CreateSymbolIcon(string symbol)
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Use default text color for icon drawing (typically black or dark gray)
                using (var brush = new SolidBrush(Color.Black))
                using (var font = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(symbol, font, brush, new RectangleF(0, 0, 16, 16), sf);
                }
            }
            return bmp;
        }

        private void btnAll_Click(object sender, EventArgs e)
        {
            _isBulkUpdating = true;

            // Specifically requested to check all items regardless of selection
            foreach (Control ctrl in flpTabList.Controls)
            {
                if (ctrl is DraggablePluginItem item)
                {
                    item.IsVisible = true;
                }
            }

            _isBulkUpdating = false;
            ApplyLivePreview();
        }

        private void btnNone_Click(object sender, EventArgs e)
        {
            _isBulkUpdating = true;

            // Specifically requested to uncheck all items regardless of selection
            foreach (Control ctrl in flpTabList.Controls)
            {
                if (ctrl is DraggablePluginItem item)
                {
                    item.IsVisible = false;
                }
            }

            _isBulkUpdating = false;
            ApplyLivePreview();
        }

        private void chbIcon_CheckedChanged(object sender, EventArgs e)
        {
            bool show = chbIcon.Checked;
            foreach (Control ctrl in flpTabList.Controls)
            {
                if (ctrl is DraggablePluginItem item)
                {
                    item.ShowIcon = show;
                }
            }
        }
        private void WireUpClickRecursive(Control ctrl)
        {
            // Do not wire up the CheckBox itself, so it strictly toggles visibility
            if (!(ctrl is CheckBox))
            {
                ctrl.Click += Row_Click;
            }

            foreach (Control child in ctrl.Controls)
            {
                WireUpClickRecursive(child);
            }
        }
        private void Row_Click(object sender, EventArgs e)
        {
            // Find which item was actually clicked
            Control source = sender as Control;
            while (source != null && !(source is DraggablePluginItem))
            {
                source = source.Parent;
            }

            if (source is DraggablePluginItem clickedItem)
            {
                bool isShift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                bool isCtrl = (ModifierKeys & Keys.Control) == Keys.Control;

                if (isCtrl)
                {
                    // Toggle selection
                    clickedItem.IsSelected = !clickedItem.IsSelected;
                    _lastSelected = clickedItem;
                }
                else if (isShift && _lastSelected != null)
                {
                    // Select range
                    int startIndex = flpTabList.Controls.IndexOf(_lastSelected);
                    int endIndex = flpTabList.Controls.IndexOf(clickedItem);

                    int min = Math.Min(startIndex, endIndex);
                    int max = Math.Max(startIndex, endIndex);

                    for (int i = 0; i < flpTabList.Controls.Count; i++)
                    {
                        if (flpTabList.Controls[i] is DraggablePluginItem item)
                        {
                            item.IsSelected = i >= min && i <= max;
                        }
                    }
                }
                else
                {
                    // Select single
                    foreach (Control ctrl in flpTabList.Controls)
                    {
                        if (ctrl is DraggablePluginItem item)
                        {
                            item.IsSelected = (item == clickedItem);
                        }
                    }
                    _lastSelected = clickedItem;
                }
            }
        }
        private IEnumerable<DraggablePluginItem> GetTargetItems()
        {
            var allItems = flpTabList.Controls.OfType<DraggablePluginItem>().ToList();
            var selectedItems = allItems.Where(i => i.IsSelected).ToList();
            return selectedItems.Count > 0 ? selectedItems : allItems;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_saved)
            {
                // Revert to saved settings
                PluginManager.ApplyLayout();
            }
            base.OnFormClosing(e);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _saved = true;
            var newSettings = new List<PluginTabSettings>();
            for (int i = 0; i < flpTabList.Controls.Count; i++)
            {
                if (flpTabList.Controls[i] is DraggablePluginItem item)
                {
                    newSettings.Add(new PluginTabSettings
                    {
                        Name = item.PluginName,
                        Visible = item.IsVisible,
                        Order = i
                    });
                }
            }

            PluginManagerSettings.SaveSettings(newSettings);
            PluginManager.ApplyLayout();
            this.Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you want to reset the ribbon tabs to their native Grasshopper order?", @"Reset Ribbon", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _saved = true;
                PluginManagerSettings.SaveSettings(new List<PluginTabSettings>());
                PluginManager.ApplyLayout();
                this.Close();
            }
        }
    }
}
