using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace controller_ui
{
    public partial class processLister : Form
    {
        private Timer procUpdator;
        public processLister()
        {
            InitializeComponent();
            //
            // inint process lister
            //
            init_procs();
            this.procUpdator = new Timer();
            this.procUpdator.Tick += update_procs;
            this.procUpdator.Interval = 500;
            this.procUpdator.Start();
        }

        #region get processes
        private Process[] processes_list;
        private (ListViewItem, bool) procListViewItem(Process proc)
        {
            Icon procIcon = null;
            try
            {
                procIcon = Icon.ExtractAssociatedIcon(proc.MainModule.FileName);
            }
            catch
            {
                procIcon = Properties.Resources.defaultProcIcon;
            }
            if (procIcon == null)
            {
                procIcon = Properties.Resources.defaultProcIcon;
            }

            ListViewItem row = new ListViewItem();
            this.processViewList.SmallImageList.Images.Add(proc.Id.ToString(), procIcon);
            row.ImageIndex = this.processViewList.SmallImageList.Images.IndexOfKey(proc.Id.ToString());
            row.SubItems.Add(proc.ProcessName);
            row.SubItems.Add(proc.Id.ToString());
            string fileName;
            try
            {
                fileName = proc.MainModule.FileName;
                row.SubItems.Add(fileName);
                return (row, false);
            }
            catch
            {
                fileName = "";
                row.SubItems.Add(fileName);
                return (row, true);

            }
        }
        private void init_procs()
        {
            processes_list = Process.GetProcesses();
            this.processViewList.SmallImageList = new ImageList();
            this.processViewList.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
            this.processViewList.SmallImageList.TransparentColor = Color.Transparent;
            this.processViewList.SmallImageList.ImageSize = new Size(16, 16);
            foreach (Process proc in processes_list)
            {
                (ListViewItem, bool) row = procListViewItem(proc);
                if (row.Item2)
                {
                    this.processViewList.Items.Add(row.Item1);
                }
                else
                {
                    this.processViewList.Items.Insert(0, row.Item1);
                }

            }
        }
        private void update_procs(object sender, System.EventArgs e)
        {
            this.procUpdator.Stop();
            Process[] processes = Process.GetProcesses();
            // var newProcs = processes.Except(processes_list);
            var newProcs = processes.Where(p1 => !processes_list.Any(p2 => p2.Id == p1.Id));
            // var deadProcs=processes_list.Except(processes);
            var deadProcs = processes_list.Where(p1 => !processes.Any(p2 => p2.Id == p1.Id));
            foreach (Process proc in deadProcs)
            {
                this.processViewList.SmallImageList.Images.RemoveByKey(proc.Id.ToString());
                this.processViewList.Items.Remove(
                    this.processViewList.Items.Cast<ListViewItem>()
                    .Where(item => item.SubItems[2].Text == proc.Id.ToString()).First()
                );
            }
            foreach (Process proc in newProcs)
            {
                (ListViewItem, bool) row = procListViewItem(proc);
                if (row.Item2)
                {
                    this.processViewList.Items.Add(row.Item1);
                }
                else
                {
                    this.processViewList.Items.Insert(15, row.Item1);
                }
            }
            processes_list = processes;
            this.procUpdator.Start();
        }

        #endregion
        private void searchBox_TabChanged(object sender, EventArgs e)
        {
            //this.procUpdator.Stop();
            if (this.searchBox.Text != "")
            {
                this.processViewList.EnsureVisible(0);
                ListViewItem[] items = this.processViewList.Items.Cast<ListViewItem>()
                   .Where(item => item.SubItems[1].Text.ToLower().Contains(this.searchBox.Text.ToLower()) ||
                         item.SubItems[2].Text == this.searchBox.Text).ToArray();
                foreach (ListViewItem item in items)
                {
                    this.processViewList.Items.Remove(item);
                    this.processViewList.Items.Insert(0, item);
                }
            }
            //this.procUpdator.Start();
        }
        public int returnPid = -1;
        private void processViewList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null)
            {
                ListViewItem item = listView.GetItemAt(e.X, e.Y);
                if (item != null)
                {
                    this.procUpdator.Stop();
                    this.returnPid = int.Parse(item.SubItems[2].Text);
                    if (!utils.injecting(this.returnPid))
                    {
                        this.returnPid = -1;
                        this.procUpdator.Start();
                        MessageBox.Show("ACCESS DENIED!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    this.Close();
                }
            }
        }
    }
}
