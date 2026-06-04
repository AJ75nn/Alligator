using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI.Ribbon;

namespace AlligatorGh
{
    public class PluginManagerForm : Form
    {
        private CheckedListBox _checkedListBox;
        private Button _btnUp;
        private Button _btnDown;
        private Button _btnSave;
        private Button _btnCancel;

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

            _checkedListBox = new CheckedListBox();
            _checkedListBox.Dock = DockStyle.Fill;
            _checkedListBox.CheckOnClick = true;
            _checkedListBox.IntegralHeight = false;
            listLayout.Controls.Add(_checkedListBox, 0, 0);

            var btnLayout = new FlowLayoutPanel();
            btnLayout.Dock = DockStyle.Fill;
            btnLayout.FlowDirection = FlowDirection.TopDown;
            btnLayout.AutoSize = true;
            btnLayout.WrapContents = false;
            btnLayout.Padding = new Padding(10, 0, 0, 0);

            _btnUp = new Button();
            _btnUp.Text = "▲ Up";
            _btnUp.AutoSize = true;
            _btnUp.Click += BtnUp_Click;

            _btnDown = new Button();
            _btnDown.Text = "▼ Down";
            _btnDown.AutoSize = true;
            _btnDown.Click += BtnDown_Click;

            btnLayout.Controls.Add(_btnUp);
            btnLayout.Controls.Add(_btnDown);
            listLayout.Controls.Add(btnLayout, 1, 0);

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

            bottomLayout.Controls.Add(_btnSave);
            bottomLayout.Controls.Add(_btnCancel);

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
                    // If no setting exists, preserve the tab's current order in the ribbon
                    // rather than sticking it at the end alphabetically
                    int indexInRibbon = ribbon.Tabs.FindIndex(t => t.NameFull == tab.NameFull);
                    items.Add(new PluginTabSettings
                    {
                        Name = tab.NameFull,
                        Visible = true, // default visible
                        Order = savedSettings.Count == 0 ? initialOrder : int.MaxValue // if no settings exist at all, use current loaded order, otherwise put new tabs at end
                    });
                }
                initialOrder++;
            }

            // Sort by order, then by their current order in the ribbon if they have int.MaxValue order
            items = items.OrderBy(x => x.Order).ThenBy(x => allTabs.FindIndex(t => t.NameFull == x.Name)).ToList();

            foreach (var item in items)
            {
                _checkedListBox.Items.Add(item.Name, item.Visible);
            }
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            if (_checkedListBox.SelectedIndex > 0)
            {
                int newIndex = _checkedListBox.SelectedIndex - 1;
                SwapItems(_checkedListBox.SelectedIndex, newIndex);
                _checkedListBox.SelectedIndex = newIndex;
            }
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            if (_checkedListBox.SelectedIndex >= 0 && _checkedListBox.SelectedIndex < _checkedListBox.Items.Count - 1)
            {
                int newIndex = _checkedListBox.SelectedIndex + 1;
                SwapItems(_checkedListBox.SelectedIndex, newIndex);
                _checkedListBox.SelectedIndex = newIndex;
            }
        }

        private void SwapItems(int index1, int index2)
        {
            var item1 = _checkedListBox.Items[index1];
            bool check1 = _checkedListBox.GetItemChecked(index1);

            var item2 = _checkedListBox.Items[index2];
            bool check2 = _checkedListBox.GetItemChecked(index2);

            _checkedListBox.Items[index1] = item2;
            _checkedListBox.SetItemChecked(index1, check2);

            _checkedListBox.Items[index2] = item1;
            _checkedListBox.SetItemChecked(index2, check1);
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
    }
}
