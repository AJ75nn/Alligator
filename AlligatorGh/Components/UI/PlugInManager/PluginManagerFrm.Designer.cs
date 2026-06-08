using System.Windows.Forms;

namespace AlligatorGh.Components.UI.PlugInManager
{
    partial class PluginManagerFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flpTabList = new FlowLayoutPanel();
            flpMainLayout = new TableLayoutPanel();
            tlpBottomPanel = new TableLayoutPanel();
            btnReset = new Button();
            btnSave = new Button();
            lblSummary = new Label();
            tlpTopPanel = new TableLayoutPanel();
            btnNone = new Button();
            btnAll = new Button();
            chbIcon = new CheckBox();
            flpMainLayout.SuspendLayout();
            tlpBottomPanel.SuspendLayout();
            tlpTopPanel.SuspendLayout();
            SuspendLayout();
            // 
            // flpTabList
            // 
            flpTabList.AutoScroll = true;
            flpTabList.BackColor = System.Drawing.Color.White;
            flpTabList.BorderStyle = BorderStyle.FixedSingle;
            flpTabList.Dock = DockStyle.Fill;
            flpTabList.FlowDirection = FlowDirection.TopDown;
            flpTabList.Location = new System.Drawing.Point(23, 76);
            flpTabList.Name = "flpTabList";
            flpTabList.Size = new System.Drawing.Size(284, 249);
            flpTabList.TabIndex = 0;
            flpTabList.WrapContents = false;
            flpTabList.DragDrop += flpTabList_DragDrop;
            flpTabList.DragEnter += flpTabList_DragEnter;
            flpTabList.DragOver += flpTabList_DragOver;
            // 
            // flpMainLayout
            // 
            flpMainLayout.ColumnCount = 1;
            flpMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            flpMainLayout.Controls.Add(flpTabList, 0, 1);
            flpMainLayout.Controls.Add(tlpBottomPanel, 0, 3);
            flpMainLayout.Controls.Add(lblSummary, 0, 2);
            flpMainLayout.Controls.Add(tlpTopPanel, 0, 0);
            flpMainLayout.Dock = DockStyle.Fill;
            flpMainLayout.Location = new System.Drawing.Point(0, 0);
            flpMainLayout.Name = "flpMainLayout";
            flpMainLayout.Padding = new Padding(20);
            flpMainLayout.RowCount = 4;
            flpMainLayout.RowStyles.Add(new RowStyle());
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            flpMainLayout.RowStyles.Add(new RowStyle());
            flpMainLayout.RowStyles.Add(new RowStyle());
            flpMainLayout.Size = new System.Drawing.Size(330, 461);
            flpMainLayout.TabIndex = 0;
            // 
            // tlpBottomPanel
            // 
            tlpBottomPanel.AutoSize = true;
            tlpBottomPanel.ColumnCount = 2;
            tlpBottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBottomPanel.Controls.Add(btnReset, 0, 0);
            tlpBottomPanel.Controls.Add(btnSave, 1, 0);
            tlpBottomPanel.Dock = DockStyle.Fill;
            tlpBottomPanel.Location = new System.Drawing.Point(20, 408);
            tlpBottomPanel.Margin = new Padding(0, 10, 0, 0);
            tlpBottomPanel.Name = "tlpBottomPanel";
            tlpBottomPanel.RowCount = 1;
            tlpBottomPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBottomPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBottomPanel.Size = new System.Drawing.Size(290, 33);
            tlpBottomPanel.TabIndex = 2;
            // 
            // btnReset
            // 
            btnReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnReset.Location = new System.Drawing.Point(3, 3);
            btnReset.Name = "btnReset";
            btnReset.Size = new System.Drawing.Size(124, 27);
            btnReset.TabIndex = 2;
            btnReset.Text = "Reset";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new System.Drawing.Point(163, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(124, 27);
            btnSave.TabIndex = 1;
            btnSave.Text = "Apply";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // lblSummary
            // 
            lblSummary.AutoSize = true;
            lblSummary.Dock = DockStyle.Fill;
            lblSummary.Location = new System.Drawing.Point(20, 348);
            lblSummary.Margin = new Padding(0, 20, 0, 20);
            lblSummary.Name = "lblSummary";
            lblSummary.Size = new System.Drawing.Size(290, 30);
            lblSummary.TabIndex = 3;
            lblSummary.Text = "Total Tabs : 51 Natives : 10 Installed Plugins : 41 Visible : 12 Hidden : 39";
            lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tlpTopPanel
            // 
            tlpTopPanel.AutoSize = true;
            tlpTopPanel.ColumnCount = 3;
            tlpTopPanel.ColumnStyles.Add(new ColumnStyle());
            tlpTopPanel.ColumnStyles.Add(new ColumnStyle());
            tlpTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpTopPanel.Controls.Add(btnNone, 1, 0);
            tlpTopPanel.Controls.Add(btnAll, 0, 0);
            tlpTopPanel.Controls.Add(chbIcon, 2, 0);
            tlpTopPanel.Dock = DockStyle.Fill;
            tlpTopPanel.Location = new System.Drawing.Point(20, 20);
            tlpTopPanel.Margin = new Padding(0, 0, 0, 20);
            tlpTopPanel.Name = "tlpTopPanel";
            tlpTopPanel.RowCount = 1;
            tlpTopPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpTopPanel.Size = new System.Drawing.Size(290, 33);
            tlpTopPanel.TabIndex = 4;
            // 
            // btnNone
            // 
            btnNone.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNone.Location = new System.Drawing.Point(133, 3);
            btnNone.Name = "btnNone";
            btnNone.Size = new System.Drawing.Size(124, 27);
            btnNone.TabIndex = 1;
            btnNone.Text = "Check None";
            btnNone.UseVisualStyleBackColor = true;
            btnNone.Click += btnNone_Click;
            // 
            // btnAll
            // 
            btnAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAll.Location = new System.Drawing.Point(3, 3);
            btnAll.Name = "btnAll";
            btnAll.Size = new System.Drawing.Size(124, 27);
            btnAll.TabIndex = 0;
            btnAll.Text = "Check All";
            btnAll.UseVisualStyleBackColor = true;
            btnAll.Click += btnAll_Click;
            // 
            // chbIcon
            // 
            chbIcon.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chbIcon.AutoSize = true;
            chbIcon.Checked = true;
            chbIcon.CheckState = CheckState.Checked;
            chbIcon.Location = new System.Drawing.Point(263, 11);
            chbIcon.Name = "chbIcon";
            chbIcon.Size = new System.Drawing.Size(24, 19);
            chbIcon.TabIndex = 2;
            chbIcon.Text = "ShowIcons";
            chbIcon.UseVisualStyleBackColor = true;
            chbIcon.CheckedChanged += chbIcon_CheckedChanged;
            // 
            // PluginManagerFrm
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            BackColor = System.Drawing.Color.WhiteSmoke;
            ClientSize = new System.Drawing.Size(330, 461);
            Controls.Add(flpMainLayout);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PluginManagerFrm";
            Text = "Alligator Plugin Manager";
            flpMainLayout.ResumeLayout(false);
            flpMainLayout.PerformLayout();
            tlpBottomPanel.ResumeLayout(false);
            tlpTopPanel.ResumeLayout(false);
            tlpTopPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpTabList;
        private TableLayoutPanel flpMainLayout;
        private Button btnSave;
        private TableLayoutPanel tlpBottomPanel;
        private Button btnReset;
        private Label lblSummary;
        private TableLayoutPanel tlpTopPanel;
        private Button btnNone;
        private Button btnAll;
        private CheckBox chbIcon;
    }
}