using System.Diagnostics;
using System.Windows.Forms;

namespace controller_ui
{
    partial class Patcher
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Patcher));
            this.controls = new System.Windows.Forms.ToolStrip();
            this.controlsRunCodeButton = new System.Windows.Forms.ToolStripButton();
            this.sectionsTree = new System.Windows.Forms.TreeView();
            this.HexView = new System.Windows.Forms.DataGridView();
            this.loaderCommands = new System.Windows.Forms.TextBox();
            this.mainViewPanel = new System.Windows.Forms.Panel();
            this.controls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HexView)).BeginInit();
            this.mainViewPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // controls
            // 
            this.controls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.controls.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.controls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.controlsRunCodeButton});
            this.controls.Location = new System.Drawing.Point(0, 0);
            this.controls.Name = "controls";
            this.controls.Size = new System.Drawing.Size(1532, 27);
            this.controls.TabIndex = 0;
            this.controls.Text = "toolStrip1";
            // 
            // controlsRunCodeButton
            // 
            this.controlsRunCodeButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.controlsRunCodeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.controlsRunCodeButton.Image = ((System.Drawing.Image)(resources.GetObject("controlsRunCodeButton.Image")));
            this.controlsRunCodeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.controlsRunCodeButton.Name = "controlsRunCodeButton";
            this.controlsRunCodeButton.Size = new System.Drawing.Size(29, 24);
            this.controlsRunCodeButton.Text = "controlsRunCodeButton";
            this.controlsRunCodeButton.Click += new System.EventHandler(this.controlsRunCodeButton_Click);
            // 
            // sectionsTree
            // 
            this.sectionsTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.sectionsTree.Location = new System.Drawing.Point(0, 27);
            this.sectionsTree.Name = "sectionsTree";
            this.sectionsTree.Size = new System.Drawing.Size(214, 659);
            this.sectionsTree.TabIndex = 1;
            this.sectionsTree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.sectionsTree_MouseDoubleClick);
            // 
            // HexView
            // 
            this.HexView.AllowUserToAddRows = false;
            this.HexView.AllowUserToDeleteRows = false;
            this.HexView.AllowUserToResizeColumns = false;
            this.HexView.AllowUserToResizeRows = false;
            this.HexView.ColumnHeadersHeight = 29;
            this.HexView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.HexView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HexView.Location = new System.Drawing.Point(0, 0);
            this.HexView.Name = "HexView";
            this.HexView.RowHeadersVisible = false;
            this.HexView.RowHeadersWidth = 51;
            this.HexView.RowTemplate.Height = 29;
            this.HexView.Size = new System.Drawing.Size(1045, 661);
            this.HexView.TabIndex = 1;
            this.HexView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.HexView_CellEndEdit);
            // 
            // loaderCommands
            // 
            this.loaderCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loaderCommands.Location = new System.Drawing.Point(1259, 27);
            this.loaderCommands.Multiline = true;
            this.loaderCommands.Name = "loaderCommands";
            this.loaderCommands.Size = new System.Drawing.Size(273, 659);
            this.loaderCommands.TabIndex = 2;
            // 
            // mainViewPanel
            // 
            this.mainViewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.mainViewPanel.Controls.Add(this.HexView);
            this.mainViewPanel.Location = new System.Drawing.Point(214, 27);
            this.mainViewPanel.Name = "mainViewPanel";
            this.mainViewPanel.Size = new System.Drawing.Size(1045, 659);
            this.mainViewPanel.TabIndex = 3;
            // 
            // Patcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1532, 686);
            this.Controls.Add(this.mainViewPanel);
            this.Controls.Add(this.loaderCommands);
            this.Controls.Add(this.sectionsTree);
            this.Controls.Add(this.controls);
            this.Name = "Patcher";
            this.Text = "memory patcher";
            this.controls.ResumeLayout(false);
            this.controls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HexView)).EndInit();
            this.mainViewPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ToolStrip controls;
        private TreeView sectionsTree;
     //   private SplitContainer sectionView;
     //   private ToolStrip toolStrip1;
     //   private ToolStrip toolStrip2;
        private DataGridView HexView;
        private TextBox loaderCommands;
        private ToolStripButton controlsRunCodeButton;
        private Panel mainViewPanel;
        // private TextBox hexDump;
    }
}