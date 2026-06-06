using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlligatorGh.Components.UI.ThemeCustomizer
{
    public class ThemeCustomizerFrm : Form
    {
        public ThemeCustomizerFrm()
        {
            InitializeComponent();
            PopulateControls();
        }

        private void InitializeComponent()
        {
            this.Text = "Custom Theme Settings";
            this.Size = new Size(350, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowIcon = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            FlowLayoutPanel layoutPanel = new FlowLayoutPanel();
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.FlowDirection = FlowDirection.TopDown;
            layoutPanel.WrapContents = false;
            layoutPanel.AutoScroll = true;
            layoutPanel.Padding = new Padding(10);

            this.Controls.Add(layoutPanel);

            // Create Color Controls for properties
            layoutPanel.Controls.Add(CreateColorSettingControl("Canvas Background", "CustomCanvasBack"));
            layoutPanel.Controls.Add(CreateColorSettingControl("Canvas Grid", "CustomCanvasGrid"));
            layoutPanel.Controls.Add(CreateColorSettingControl("Canvas Edge", "CustomCanvasEdge"));
            layoutPanel.Controls.Add(CreateColorSettingControl("Canvas Shade", "CustomCanvasShade"));
            layoutPanel.Controls.Add(CreateColorSettingControl("Wire Default Color", "CustomWireDefault"));
            layoutPanel.Controls.Add(CreateColorSettingControl("Wire Selected A Color", "CustomWireSelectedA"));
            layoutPanel.Controls.Add(CreateColorSettingControl("Wire Selected B Color", "CustomWireSelectedB"));
            layoutPanel.Controls.Add(CreateColorSettingControl("Wire Empty Color", "CustomWireEmpty"));
        }

        private void PopulateControls()
        {
            // Initial population is handled inside CreateColorSettingControl
        }

        private Control CreateColorSettingControl(string displayName, string settingKey)
        {
            Panel pnl = new Panel();
            pnl.Size = new Size(300, 60);

            Label lbl = new Label();
            lbl.Text = displayName;
            lbl.Location = new Point(0, 5);
            lbl.AutoSize = true;
            pnl.Controls.Add(lbl);

            Panel colorPnl = new Panel();
            colorPnl.Size = new Size(100, 25);
            colorPnl.Location = new Point(0, 25);
            colorPnl.BorderStyle = BorderStyle.FixedSingle;
            colorPnl.BackColor = ThemeManager.GetCustomColor(settingKey) ?? ThemeManager.GetDefaultColorForProperty(settingKey);
            colorPnl.Click += (s, e) =>
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    cd.Color = colorPnl.BackColor;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        colorPnl.BackColor = cd.Color;
                        ThemeManager.SetCustomColor(settingKey, cd.Color);
                        ThemeMenuPriority.UpdateThemeCheckmarks(); // Optional, UI refresh
                    }
                }
            };
            pnl.Controls.Add(colorPnl);

            Button btnReset = new Button();
            btnReset.Text = "Reset Value";
            btnReset.Location = new Point(110, 25);
            btnReset.Size = new Size(80, 25);
            btnReset.Click += (s, e) =>
            {
                ThemeManager.ClearCustomColor(settingKey);
                colorPnl.BackColor = ThemeManager.GetDefaultColorForProperty(settingKey);
                ThemeMenuPriority.UpdateThemeCheckmarks(); // Optional, UI refresh
            };
            pnl.Controls.Add(btnReset);

            return pnl;
        }
    }
}
