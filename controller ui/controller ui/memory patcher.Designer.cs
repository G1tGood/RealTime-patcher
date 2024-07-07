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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Patcher));
            this.controls = new System.Windows.Forms.ToolStrip();
            this.controlsRunCodeButton = new System.Windows.Forms.ToolStripButton();
            this.emptySpace1 = new System.Windows.Forms.ToolStripLabel();
            this.showMemoryButton = new System.Windows.Forms.ToolStripButton();
            this.HexViewPanel = new System.Windows.Forms.Panel();
            this.HexView = new System.Windows.Forms.DataGridView();
            this.showDissasemblerButton = new System.Windows.Forms.ToolStripButton();
            this.disassemblerPanel = new System.Windows.Forms.Panel();
            this.disassemblyText = new System.Windows.Forms.RichTextBox();
            this.dissasemblingStartAddressBox = new System.Windows.Forms.ToolStripTextBox();
            this.sectionsTree = new System.Windows.Forms.TreeView();
            this.loaderCommands = new System.Windows.Forms.TextBox();
            this.mainViewPanel = new System.Windows.Forms.Panel();
            this.sectionsTreeChooseIfAssemblyStart = new System.Windows.Forms.ContextMenuStrip();
            this.temporaryContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.disassemblyTextContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.controls.SuspendLayout();
            this.HexViewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HexView)).BeginInit();
            this.disassemblerPanel.SuspendLayout();
            this.mainViewPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // controls
            // 
            this.controls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.controls.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.controls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.controlsRunCodeButton,
            this.emptySpace1,
            this.showMemoryButton,
            this.showDissasemblerButton,
            this.dissasemblingStartAddressBox});
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
            // emptySpace1
            // 
            this.emptySpace1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.emptySpace1.Name = "emptySpace1";
            this.emptySpace1.Padding = new System.Windows.Forms.Padding(0, 0, 214, 0);
            this.emptySpace1.Size = new System.Drawing.Size(214, 24);
            // 
            // showMemoryButton
            // 
            this.showMemoryButton.BackColor = System.Drawing.SystemColors.Control;
            this.showMemoryButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.showMemoryButton.Image = ((System.Drawing.Image)(resources.GetObject("showMemoryButton.Image")));
            this.showMemoryButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showMemoryButton.Name = "showMemoryButton";
            this.showMemoryButton.Size = new System.Drawing.Size(68, 24);
            this.showMemoryButton.Tag = this.HexViewPanel;
            this.showMemoryButton.Text = "memory";
            this.showMemoryButton.Click += mainViewchooseContentClick;
            // 
            // HexViewPanel
            // 
            this.HexViewPanel.Controls.Add(this.HexView);
            this.HexViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HexViewPanel.Location = new System.Drawing.Point(0, 0);
            this.HexViewPanel.Name = "HexViewPanel";
            this.HexViewPanel.Size = new System.Drawing.Size(1045, 659);
            this.HexViewPanel.TabIndex = 4;
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
            this.HexView.Size = new System.Drawing.Size(1045, 659);
            this.HexView.TabIndex = 5;
            this.HexView.CellMouseClick += HexView_MouseClick;
            this.HexView.CellEndEdit += HexView_CellEndEdit;
            this.HexView.DataError += null;
            // 
            // showDissasemblerButton
            // 
            this.showDissasemblerButton.CheckOnClick = true;
            this.showDissasemblerButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.showDissasemblerButton.Image = ((System.Drawing.Image)(resources.GetObject("showDissasemblerButton.Image")));
            this.showDissasemblerButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showDissasemblerButton.Name = "showDissasemblerButton";
            this.showDissasemblerButton.Size = new System.Drawing.Size(56, 24);
            this.showDissasemblerButton.Tag = this.disassemblerPanel;
            this.showDissasemblerButton.Text = "debug";
            this.showDissasemblerButton.Click += mainViewchooseContentClick;
            // 
            // disassemblerPanel
            // 
            this.disassemblerPanel.Controls.Add(this.disassemblyText);
            this.disassemblerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.disassemblerPanel.Location = new System.Drawing.Point(0, 0);
            this.disassemblerPanel.Name = "disassemblerPanel";
            this.disassemblerPanel.Size = new System.Drawing.Size(1045, 659);
            this.disassemblerPanel.TabIndex = 1;
            this.disassemblerPanel.Visible = false;
            // 
            // disassemblyText
            // 
            this.disassemblyText.BackColor = System.Drawing.Color.White;
            this.disassemblyText.Dock = System.Windows.Forms.DockStyle.Left;
            this.disassemblyText.Location = new System.Drawing.Point(0, 0);
            this.disassemblyText.Multiline = true;
            this.disassemblyText.Name = "disassemblyText";
            this.disassemblyText.ReadOnly = true;
            this.disassemblyText.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.disassemblyText.Size = new System.Drawing.Size(427, 659);
            this.disassemblyText.TabIndex = 2;
            this.disassemblyText.ContextMenuStrip = null;
            this.disassemblyText.MouseDown += DisassemblyText_MouseClick;
            this.disassemblyText.KeyDown += DisassemblyText_keyPress;
            // 
            // dissasemblingStartAddressBox
            // 
            this.dissasemblingStartAddressBox.Name = "dissasemblingStartAddressBox";
            this.dissasemblingStartAddressBox.Size = new System.Drawing.Size(155, 27);
            this.dissasemblingStartAddressBox.TextChanged+=DissasemblingStartAddressBox_TextChanged;
            // 
            // sectionsTree
            // 
            this.sectionsTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.sectionsTree.Location = new System.Drawing.Point(0, 27);
            this.sectionsTree.Name = "sectionsTree";
            this.sectionsTree.Size = new System.Drawing.Size(214, 659);
            this.sectionsTree.TabIndex = 1;
            this.sectionsTree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.sectionsTree_MouseDoubleClick);
            this.sectionsTree.MouseClick += SectionsTree_MouseClick;
            // 
            // loaderCommands
            // 
            this.loaderCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loaderCommands.Location = new System.Drawing.Point(1259, 27);
            this.loaderCommands.Multiline = true;
            this.loaderCommands.Name = "loaderCommands";
            this.loaderCommands.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.loaderCommands.Size = new System.Drawing.Size(273, 659);
            this.loaderCommands.TabIndex = 2;
            // 
            // mainViewPanel
            // 
            this.mainViewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.mainViewPanel.Controls.Add(this.HexViewPanel);
            this.mainViewPanel.Controls.Add(this.disassemblerPanel);
            this.mainViewPanel.Location = new System.Drawing.Point(214, 27);
            this.mainViewPanel.Name = "mainViewPanel";
            this.mainViewPanel.Size = new System.Drawing.Size(1045, 659);
            this.mainViewPanel.TabIndex = 3;
            // 
            // sectionsTreeChooseIfAssemblyStart
            // 
            this.sectionsTreeChooseIfAssemblyStart.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.sectionsTreeChooseIfAssemblyStart.Name = "sectionsTreeChooseIfAssemblyStart";
            this.sectionsTreeChooseIfAssemblyStart.ShowImageMargin = false;
            this.sectionsTreeChooseIfAssemblyStart.Size = new System.Drawing.Size(186, 32);
            // 
            // memoryByteMenuStrip
            // 
            this.temporaryContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.temporaryContextMenuStrip.Name = "sectionsTreeChooseIfAssemblyStart";
            this.temporaryContextMenuStrip.ShowImageMargin = false;
            this.temporaryContextMenuStrip.Size = new System.Drawing.Size(186, 32);
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
            this.HexViewPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.HexView)).EndInit();
            this.disassemblerPanel.ResumeLayout(false);
            this.disassemblerPanel.PerformLayout();
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
        private Panel HexViewPanel;
        private Panel disassemblerPanel;
        private ToolStripButton showMemoryButton;
        private ToolStripButton showDissasemblerButton;
        private ToolStripLabel emptySpace1;
        private RichTextBox disassemblyText;
        private ToolStripTextBox dissasemblingStartAddressBox;
        private ContextMenuStrip sectionsTreeChooseIfAssemblyStart;
        private ContextMenuStrip temporaryContextMenuStrip;
        private ContextMenuStrip disassemblyTextContextMenuStrip;
        // private TextBox hexDump;
    }
}