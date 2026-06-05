using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI.Ribbon;

namespace AlligatorGh
{
    public class DragDropCheckedListBox : CheckedListBox
    {
        private int _draggedIndex = -1;
        private int _hoveredIndex = -1;
        private bool _isDragging = false;
        private Point _dragStartPoint;

        public DragDropCheckedListBox()
        {
            this.DoubleBuffered = true;
            this.AllowDrop = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (this.Items.Count == 0) return;

            int index = this.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                // Check if click is on the checkbox part. Usually checkbox is around 16px wide at the start.
                // Or let standard behavior toggle it if we don't start dragging immediately.
                _draggedIndex = index;
                _dragStartPoint = e.Location;
                _isDragging = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left && _draggedIndex != -1)
            {
                if (!_isDragging)
                {
                    // threshold to start drag
                    if (Math.Abs(e.Y - _dragStartPoint.Y) > 5 || Math.Abs(e.X - _dragStartPoint.X) > 5)
                    {
                        _isDragging = true;
                    }
                }

                if (_isDragging)
                {
                    int index = this.IndexFromPoint(e.Location);
                    if (index == ListBox.NoMatches)
                    {
                        if (e.Y > this.GetItemRectangle(this.Items.Count - 1).Bottom)
                            index = this.Items.Count - 1;
                        else
                            index = 0;
                    }

                    if (index != _hoveredIndex)
                    {
                        _hoveredIndex = index;
                        this.Invalidate(); // trigger redraw to show drop line
                    }
                }
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_isDragging && _draggedIndex != -1 && _hoveredIndex != -1 && _draggedIndex != _hoveredIndex)
            {
                // perform swap or insert
                var item = this.Items[_draggedIndex];
                bool isChecked = this.GetItemChecked(_draggedIndex);

                this.Items.RemoveAt(_draggedIndex);
                this.Items.Insert(_hoveredIndex, item);
                this.SetItemChecked(_hoveredIndex, isChecked);
                this.SelectedIndex = _hoveredIndex;
            }

            _isDragging = false;
            _draggedIndex = -1;
            _hoveredIndex = -1;
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_isDragging)
            {
                _isDragging = false;
                _draggedIndex = -1;
                _hoveredIndex = -1;
                this.Invalidate();
            }
        }

        // To draw custom items and drag handle, we would need DrawMode = DrawMode.OwnerDrawFixed,
        // but CheckedListBox doesn't support OwnerDrawFixed properly without losing native checkboxes.
        // So we will stick to standard drawing but draw a line overlay during drag.

        private const int WM_PAINT = 0x000F;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                if (_isDragging && _hoveredIndex != -1)
                {
                    using (Graphics g = Graphics.FromHwnd(this.Handle))
                    {
                        Rectangle rect = this.GetItemRectangle(_hoveredIndex);
                        bool insertBelow = _hoveredIndex > _draggedIndex;

                        int y = insertBelow ? rect.Bottom : rect.Top;

                        using (Pen p = new Pen(Color.Black, 2))
                        {
                            g.DrawLine(p, rect.Left, y, rect.Right, y);
                        }
                    }
                }
            }
        }
    }

    public class PluginManagerForm : Form
    {
        private DragDropCheckedListBox _checkedListBox;
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnReset;

        public PluginManagerForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Alligator Plugin Manager";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            var mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.ColumnCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.Padding = new Padding(10);

            var lblInfo = new Label();
            lblInfo.Text = "Manage Grasshopper Ribbon Tabs (Plugins):\nCheck to show, uncheck to hide. Use buttons to reorder.";
            lblInfo.AutoSize = true;
            lblInfo.Margin = new Padding(0, 0, 0, 10);
            mainLayout.Controls.Add(lblInfo, 0, 0);

            var listLayout = new TableLayoutPanel();
            listLayout.Dock = DockStyle.Fill;
            listLayout.RowCount = 1;
            listLayout.ColumnCount = 2;
            listLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            listLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            listLayout.Margin = new Padding(0);

            _checkedListBox = new DragDropCheckedListBox();
            _checkedListBox.Dock = DockStyle.Fill;
            _checkedListBox.CheckOnClick = true;
            _checkedListBox.IntegralHeight = false;
            listLayout.Controls.Add(_checkedListBox, 0, 0);

            // Removing the Up/Down buttons layout as requested

            mainLayout.Controls.Add(listLayout, 0, 1);

            var bottomLayout = new FlowLayoutPanel();
            bottomLayout.Dock = DockStyle.Fill;
            bottomLayout.FlowDirection = FlowDirection.RightToLeft;
            bottomLayout.AutoSize = true;
            bottomLayout.Margin = new Padding(0, 10, 0, 0);

            _btnSave = new Button();
            _btnSave.Text = "Save && Apply";
            _btnSave.AutoSize = true;
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button();
            _btnCancel.Text = "Cancel";
            _btnCancel.AutoSize = true;
            _btnCancel.Click += (s, e) => this.Close();

            _btnReset = new Button();
            _btnReset.Text = "Reset to Default";
            _btnReset.AutoSize = true;
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

            // Get all current tabs. Because we might have hidden some, we need to gather from both visible and hidden.
            // Actually, we keep backups in PluginManager.
            var allTabs = PluginManager.GetAllTabs();

            // Create a temporary list to sort them according to settings
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
                    // If no setting exists, preserve the tab's absolute layout order (based on _originalTabs populated at startup)
                    items.Add(new PluginTabSettings
                    {
                        Name = tab.NameFull,
                        Visible = true, // default visible
                        Order = savedSettings.Count == 0 ? initialOrder : int.MaxValue // if no settings exist at all, use native layout order, otherwise put new tabs at end
                    });
                }
                initialOrder++;
            }

            // Sort by order, then by their native layout order if they have int.MaxValue order
            items = items.OrderBy(x => x.Order).ThenBy(x => allTabs.FindIndex(t => t.NameFull == x.Name)).ToList();

            foreach (var item in items)
            {
                _checkedListBox.Items.Add(item.Name, item.Visible);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var newSettings = new List<PluginTabSettings>();
            for (int i = 0; i < _checkedListBox.Items.Count; i++)
            {
                newSettings.Add(new PluginTabSettings
                {
                    Name = _checkedListBox.Items[i].ToString(),
                    Visible = _checkedListBox.GetItemChecked(i),
                    Order = i
                });
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
