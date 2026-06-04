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
            this.Size = new Size(350, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblInfo = new Label();
            lblInfo.Text = "Manage Grasshopper Ribbon Tabs (Plugins):\nCheck to show, uncheck to hide. Use buttons to reorder.";
            lblInfo.Location = new Point(10, 10);
            lblInfo.Size = new Size(310, 40);

            _checkedListBox = new CheckedListBox();
            _checkedListBox.Location = new Point(10, 55);
            _checkedListBox.Size = new Size(240, 300);
            _checkedListBox.CheckOnClick = true;

            _btnUp = new Button();
            _btnUp.Text = "▲ Up";
            _btnUp.Location = new Point(260, 55);
            _btnUp.Size = new Size(60, 30);
            _btnUp.Click += BtnUp_Click;

            _btnDown = new Button();
            _btnDown.Text = "▼ Down";
            _btnDown.Location = new Point(260, 90);
            _btnDown.Size = new Size(60, 30);
            _btnDown.Click += BtnDown_Click;

            _btnSave = new Button();
            _btnSave.Text = "Save && Apply";
            _btnSave.Location = new Point(160, 370);
            _btnSave.Size = new Size(90, 30);
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button();
            _btnCancel.Text = "Cancel";
            _btnCancel.Location = new Point(60, 370);
            _btnCancel.Size = new Size(90, 30);
            _btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblInfo);
            this.Controls.Add(_checkedListBox);
            this.Controls.Add(_btnUp);
            this.Controls.Add(_btnDown);
            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnCancel);
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
                        Visible = true, // default visible
                        Order = int.MaxValue // put at the end
                    });
                }
            }

            // Sort by order, then alphabetically for those without specific order
            items = items.OrderBy(x => x.Order).ThenBy(x => x.Name).ToList();

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
