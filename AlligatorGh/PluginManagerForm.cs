using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;

namespace AlligatorGh
{
    public class PluginManagerForm : Form
    {
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnReset;
        private Button _btnCheckAll;
        private Button _btnCheckNone;
        private CheckBox _chkShowIcons;
        private FlowLayoutPanel _listLayout;

        private DraggablePluginItem _lastSelected = null;
        private bool _isBulkUpdating = false;

        public PluginManagerForm()
        {
            InitializeComponent();
            LoadData();

            // Set up native drag-and-drop on the FlowLayoutPanel
            _listLayout.AllowDrop = true;
            _listLayout.DragEnter += ListLayout_DragEnter;
            _listLayout.DragOver += ListLayout_DragOver;
            _listLayout.DragDrop += ListLayout_DragDrop;
        }

        private void ListLayout_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DraggablePluginItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void ListLayout_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(DraggablePluginItem))) return;

            DraggablePluginItem draggingItem = (DraggablePluginItem)e.Data.GetData(typeof(DraggablePluginItem));

            Point clientPoint = _listLayout.PointToClient(new Point(e.X, e.Y));

            DraggablePluginItem targetItem = null;
            foreach (Control ctrl in _listLayout.Controls)
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
                int currentIndex = _listLayout.Controls.GetChildIndex(draggingItem);
                int targetIndex = _listLayout.Controls.GetChildIndex(targetItem);

                if (currentIndex != targetIndex)
                {
                    _listLayout.Controls.SetChildIndex(draggingItem, targetIndex);
                    ApplyLivePreview();
                }
            }
        }

        private void ListLayout_DragDrop(object sender, DragEventArgs e)
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
            for (int i = 0; i < _listLayout.Controls.Count; i++)
            {
                if (_listLayout.Controls[i] is DraggablePluginItem item)
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
        }

        private void InitializeComponent()
        {
            this.Text = "Alligator Plugin Manager";
            this.Size = new Size(400, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new Size(300, 400);

            var mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 4;
            mainLayout.ColumnCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Top buttons
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // List
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Bottom buttons
            mainLayout.Padding = new Padding(10);

            // Bulk actions layout
            var bulkLayout = new FlowLayoutPanel();
            bulkLayout.Dock = DockStyle.Fill;
            bulkLayout.FlowDirection = FlowDirection.LeftToRight;
            bulkLayout.AutoSize = true;
            bulkLayout.Margin = new Padding(0, 0, 0, 5);

            _btnCheckAll = new Button { Text = "Check All", AutoSize = true, MinimumSize = new Size(80, 30), Padding = new Padding(5) };
            _btnCheckAll.Click += BtnCheckAll_Click;

            _btnCheckNone = new Button { Text = "Check None", AutoSize = true, MinimumSize = new Size(80, 30), Padding = new Padding(5) };
            _btnCheckNone.Click += BtnCheckNone_Click;

            _chkShowIcons = new CheckBox { Text = "Show Icons", AutoSize = true, Padding = new Padding(10, 5, 5, 5) };
            _chkShowIcons.CheckedChanged += ChkShowIcons_CheckedChanged;

            bulkLayout.Controls.Add(_btnCheckAll);
            bulkLayout.Controls.Add(_btnCheckNone);
            bulkLayout.Controls.Add(_chkShowIcons);

            mainLayout.Controls.Add(bulkLayout, 0, 0);

            // Item list layout
            _listLayout = new FlowLayoutPanel();
            _listLayout.Dock = DockStyle.Fill;
            _listLayout.FlowDirection = FlowDirection.TopDown;
            _listLayout.WrapContents = false;
            _listLayout.AutoScroll = true;
            _listLayout.BackColor = Color.White;
            _listLayout.BorderStyle = BorderStyle.FixedSingle;

            mainLayout.Controls.Add(_listLayout, 0, 1);

            // Bottom buttons layout
            var bottomLayout = new FlowLayoutPanel();
            bottomLayout.Dock = DockStyle.Fill;
            bottomLayout.FlowDirection = FlowDirection.RightToLeft;
            bottomLayout.AutoSize = true;
            bottomLayout.Margin = new Padding(0, 10, 0, 0);

            _btnSave = new Button { Text = "Save && Apply", AutoSize = true, MinimumSize = new Size(100, 30), Padding = new Padding(5) };
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button { Text = "Cancel", AutoSize = true, MinimumSize = new Size(80, 30), Padding = new Padding(5) };
            _btnCancel.Click += BtnCancel_Click;

            _btnReset = new Button { Text = "Reset to Default", AutoSize = true, MinimumSize = new Size(100, 30), Padding = new Padding(5) };
            _btnReset.Click += BtnReset_Click;

            bottomLayout.Controls.Add(_btnSave);
            bottomLayout.Controls.Add(_btnCancel);
            bottomLayout.Controls.Add(_btnReset);

            mainLayout.Controls.Add(bottomLayout, 0, 2);

            this.Controls.Add(mainLayout);
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

            _listLayout.Controls.Clear();
            foreach (var item in items)
            {
                // Find icon from ComponentServer libraries matching the tab name
                Image icon = null;
                var lib = Instances.ComponentServer.Libraries.FirstOrDefault(l =>
                    l.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

                if (lib != null)
                {
                    icon = lib.Icon;
                }

                var row = new DraggablePluginItem
                {
                    PluginName = item.Name,
                    IsVisible = item.Visible,
                    PluginIcon = icon,
                    ShowIcon = _chkShowIcons.Checked,
                    Width = _listLayout.ClientSize.Width - 10
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
                row.VisibilityChanged += (s, e) => {
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

                _listLayout.Controls.Add(row);
            }

            // Handle resize
            _listLayout.Resize += (s, e) =>
            {
                foreach (Control ctrl in _listLayout.Controls)
                {
                    ctrl.Width = _listLayout.ClientSize.Width - 10;
                }
            };
        }

        private void BtnCheckAll_Click(object sender, EventArgs e)
        {
            _isBulkUpdating = true;

            // Specifically requested to check all items regardless of selection
            foreach (Control ctrl in _listLayout.Controls)
            {
                if (ctrl is DraggablePluginItem item)
                {
                    item.IsVisible = true;
                }
            }

            _isBulkUpdating = false;
            ApplyLivePreview();
        }

        private void BtnCheckNone_Click(object sender, EventArgs e)
        {
            _isBulkUpdating = true;

            // Specifically requested to uncheck all items regardless of selection
            foreach (Control ctrl in _listLayout.Controls)
            {
                if (ctrl is DraggablePluginItem item)
                {
                    item.IsVisible = false;
                }
            }

            _isBulkUpdating = false;
            ApplyLivePreview();
        }

        private void ChkShowIcons_CheckedChanged(object sender, EventArgs e)
        {
            bool show = _chkShowIcons.Checked;
            foreach (Control ctrl in _listLayout.Controls)
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
                    int startIndex = _listLayout.Controls.IndexOf(_lastSelected);
                    int endIndex = _listLayout.Controls.IndexOf(clickedItem);

                    int min = Math.Min(startIndex, endIndex);
                    int max = Math.Max(startIndex, endIndex);

                    for (int i = 0; i < _listLayout.Controls.Count; i++)
                    {
                        if (_listLayout.Controls[i] is DraggablePluginItem item)
                        {
                            item.IsSelected = i >= min && i <= max;
                        }
                    }
                }
                else
                {
                    // Select single
                    foreach (Control ctrl in _listLayout.Controls)
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
            var allItems = _listLayout.Controls.OfType<DraggablePluginItem>().ToList();
            var selectedItems = allItems.Where(i => i.IsSelected).ToList();
            return selectedItems.Count > 0 ? selectedItems : allItems;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var newSettings = new List<PluginTabSettings>();
            for (int i = 0; i < _listLayout.Controls.Count; i++)
            {
                if (_listLayout.Controls[i] is DraggablePluginItem item)
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

        private void BtnReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset the ribbon tabs to their native Grasshopper order?", "Reset Ribbon", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                PluginManagerSettings.SaveSettings(new List<PluginTabSettings>());
                PluginManager.ApplyLayout();
                this.Close();
            }
        }
    }
}
