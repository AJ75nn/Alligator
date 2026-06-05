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
            flpMainLayout.SuspendLayout();
            // 
            // flpMainLayout
            // 
            flpMainLayout.Dock = DockStyle.Fill;
            flpMainLayout.RowCount = 4;
            flpMainLayout.ColumnCount = 1;
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Top buttons
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // List
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Summary Footer
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Bottom buttons
            flpMainLayout.Padding = new Padding(20);



            flpMainLayout.Controls.Add(flpTabList, 0, 1);
            flpMainLayout.Dock = DockStyle.Fill;
            flpMainLayout.Location = new System.Drawing.Point(0, 0);
            flpMainLayout.Name = "flpMainLayout";
            flpMainLayout.RowCount = 4;
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            flpMainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            flpMainLayout.Size = new System.Drawing.Size(828, 894);
            
            SuspendLayout();
            // 
            // flpTabList
            // 
            flpTabList.AutoScroll = true;
            flpTabList.BackColor = System.Drawing.Color.White;
            flpTabList.BorderStyle = BorderStyle.FixedSingle;
            flpTabList.FlowDirection = FlowDirection.TopDown;
            flpTabList.Location = new System.Drawing.Point(3, 23);
            flpTabList.Name = "flpTabList";
            flpTabList.Size = new System.Drawing.Size(732, 14);
            flpTabList.TabIndex = 0;
            flpTabList.WrapContents = false;
           
            // 
            // PluginManagerFrm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(828, 894);
            Controls.Add(flpMainLayout);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PluginManagerFrm";
            Text = "PluginManagerFrm";
            flpMainLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpTabList;
        private TableLayoutPanel flpMainLayout;
    }
}