
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
namespace controller_ui
{
    partial class processLister
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.processViewList = new System.Windows.Forms.ListView();
            this.icon = new System.Windows.Forms.ColumnHeader();
            this.name = new System.Windows.Forms.ColumnHeader();
            this.pid = new System.Windows.Forms.ColumnHeader();
            this.file = new System.Windows.Forms.ColumnHeader();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // processViewList
            // 
            this.processViewList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.processViewList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.icon,
            this.name,
            this.pid,
            this.file});
            this.processViewList.FullRowSelect = true;
            this.processViewList.HideSelection = false;
            this.processViewList.Location = new System.Drawing.Point(0, 27);
            this.processViewList.Name = "processViewList";
            this.processViewList.Size = new System.Drawing.Size(800, 423);
            this.processViewList.TabIndex = 0;
            this.processViewList.UseCompatibleStateImageBehavior = false;
            this.processViewList.View = System.Windows.Forms.View.Details;
            this.processViewList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.processViewList_MouseDoubleClick);
            // 
            // icon
            // 
            this.icon.Text = "";
            this.icon.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.icon.Width = 20;
            // 
            // name
            // 
            this.name.Text = "name";
            this.name.Width = 100;
            // 
            // pid
            // 
            this.pid.Text = "pid";
            this.pid.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // file
            // 
            this.file.Text = "file path";
            this.file.Width = 500;
            // 
            // procUpdator
            // 
           // this.procUpdator.Enabled = true;
            //this.procUpdator.Interval = 500;
            // 
            // searchBox
            // 
            this.searchBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.searchBox.Location = new System.Drawing.Point(274, 1);
            this.searchBox.Name = "searchBox";
            this.searchBox.PlaceholderText = "type name or pid";
            this.searchBox.Size = new System.Drawing.Size(267, 27);
            this.searchBox.TabIndex = 1;
            this.searchBox.TextChanged += searchBox_TabChanged;
            // 
            // processLister
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.processViewList);
            this.Name = "processLister";
            this.Text = "choose a target process";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
        
        private System.Windows.Forms.ListView processViewList;
        private System.Windows.Forms.ColumnHeader icon;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.ColumnHeader pid;
        private System.Windows.Forms.ColumnHeader file;
        private TextBox searchBox;
    }
}

