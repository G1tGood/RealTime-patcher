using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace controller_ui
{
    public partial class Patcher : Form
    {
        private System.Diagnostics.Process targetProc;
        private System.Threading.Thread updator;
        private loaderCommander commander;
        private WorkerController worker_controller;
        private List<string> breakPoints;
        public Patcher(int pid)
        {
            this.targetProc = System.Diagnostics.Process.GetProcessById(pid);

            InitializeComponent();
            //this.loaderCommands.BringToFront();
            this.breakPoints = new List<string>();
            inint_data_table();

            this.worker_controller = new WorkerController(WorkerController.BitMode.x86_64);

            typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this.HexView, true, null);

            get_modules();
            this.updator = new System.Threading.Thread(new System.Threading.ThreadStart(updatorTask));
            this.updator.Start();
            commander = new UIloaderCommander(this);

        }
        private void updatorTask()
        {
            for (int i = 0;; i++)
            {
                System.Threading.Thread.Sleep(100);
                if (this.selected_section != null)
                {
                    load_section_memory(Convert.ToUInt64(this.selected_section.BaseAdress, 16), this.selected_section.RegionSize, true);
                }
                //if (i % 200 == 199) //TODO: UPDATE MODULES
                //{
                //    get_modules(true);
                //}
            }
        }


        #region get modules
        private void get_modules(bool isUpdate=false)
        {
            foreach (ProcessModule module in this.targetProc.Modules)
            {

                //if (module.ModuleName.ToLower().StartsWith("notepad"))
                // {
                MyModule myModule = new MyModule(module.ModuleName, module.FileName);
                TreeNode moduleNode = null ;
                if (isUpdate)
                {
                    bool isNew = true;
                    foreach (TreeNode node in this.sectionsTree.Nodes)
                    {
                        if (myModule == node.Tag as MyModule)
                        {
                            moduleNode = node;
                            isNew = false;
                            break;
                        }
                    }
                    if (isNew)
                    {
                        moduleNode = new TreeNode(module.ModuleName);
                        this.sectionsTree.Invoke((Action)(() => this.sectionsTree.Nodes.Add(moduleNode)));
                        moduleNode.Tag = myModule;
                    }
                }
                else
                {
                    moduleNode = new TreeNode(module.ModuleName);
                    this.sectionsTree.Nodes.Add(moduleNode);
                    moduleNode.Tag = myModule;
                }

                myModule.Sections.AddRange(this.worker_controller.get_module_sections(myModule));
                //existingList.AddRange(newData.Except(existingList));
                this.worker_controller.update_module_PE_sections(myModule);
                
                foreach (MyModule.MySection sect in myModule.Sections)
                {
                    if (isUpdate) {
                        bool isNew = true;  
                        foreach (TreeNode node in moduleNode.Nodes)
                        {
                            if(sect== node.Tag as MyModule.MySection)
                            {
                                isNew = false;
                                break;
                            }
                        }
                        if(isNew)
                        {
                            this.sectionsTree.Invoke((Action)(() =>{
                                TreeNode node = new TreeNode(sect.Name);
                                node.Tag = sect;
                                moduleNode.Nodes.Add(node);
                            }));
                        }
                    }
                    else
                    {
                        TreeNode node = new TreeNode(sect.Name);
                        node.Tag = sect;
                        moduleNode.Nodes.Add(node);
                    }

                }
            }
        }
        #endregion

        private static readonly object _load_section_memory_lock = new object();
        private void load_section_memory(ulong address, ulong len, bool isUpdate = false)
        {
            lock (_load_section_memory_lock)
            {
                if (!isUpdate)
                {
                    this.dataTable.Rows.Clear();
                }
                //if (dataTable.Rows.Count == len)
                //    return;
                dataTable.BeginLoadData();

                byte[] buffer = this.read_memory(address, len);

                // DataGridViewRowCollection rows = new DataGridViewRowCollection(this.HexView);
                // while (true)
                for (ulong j = 0; j < len; j += 16)
                {

                    // byte[] buffer = new byte[len];
                    //if (server.ReadByte() == -1)
                    //{
                    //    break;
                    //}
                    //server.Read(buffer);

                    object[] cells = new object[18];
                    cells[0] = (address + (ulong)j).ToString("X16");
                    byte[] dump_bytes = new byte[16];
                    for (ulong i = 1; i <= 16; i++)
                    {
                        cells[i] = buffer[(ulong)j + i - 1].ToString("X");
                        dump_bytes[i - 1] = buffer[(ulong)j + i - 1];
                    }
                    cells[17] = utils.dumpHex(dump_bytes);//Encoding.ASCII.GetString(dump_bytes);//
                    // this.hexDump.Text += utils.dumpHex(buffer);
                    if (!isUpdate)
                    {
                        this.dataTable.Rows.Add(cells);
                    }
                    else
                    {
                        if (!Enumerable.SequenceEqual(this.dataTable.Rows[Convert.ToInt32(j / 16)].ItemArray, cells))
                        {
                            this.dataTable.Rows[Convert.ToInt32(j / 16)].ItemArray = cells;
                        }

                    }
                }
                dataTable.EndLoadData();
            }
        }
        
        public byte[] read_memory(ulong address, ulong len)
        {
            return this.worker_controller.read_memory(address,len);
        }
        private static readonly object _write_memory_lock = new object();
        public void write_memory(ulong address, byte[] data)
        {
            this.worker_controller.write_memory(address, data);
        }
        private DataTable dataTable;
        private void inint_data_table()
        {
            this.dataTable = new DataTable();
            this.dataTable.Columns.Add("address", typeof(string));
            this.dataTable.Columns.Add("00", typeof(string));
            this.dataTable.Columns.Add("01", typeof(string));
            this.dataTable.Columns.Add("02", typeof(string));
            this.dataTable.Columns.Add("03", typeof(string));
            this.dataTable.Columns.Add("04", typeof(string));
            this.dataTable.Columns.Add("05", typeof(string));
            this.dataTable.Columns.Add("06", typeof(string));
            this.dataTable.Columns.Add("07", typeof(string));
            this.dataTable.Columns.Add("08", typeof(string));
            this.dataTable.Columns.Add("09", typeof(string));
            this.dataTable.Columns.Add("0A", typeof(string));
            this.dataTable.Columns.Add("0B", typeof(string));
            this.dataTable.Columns.Add("0C", typeof(string));
            this.dataTable.Columns.Add("0D", typeof(string));
            this.dataTable.Columns.Add("0E", typeof(string));
            this.dataTable.Columns.Add("0F", typeof(string));
            this.dataTable.Columns.Add("dump", typeof(string));
            this.HexView.VirtualMode = true;
            this.HexView.CellValueNeeded += DataGridView_CellValueNeeded;
            this.HexView.DataSource = dataTable;

            foreach (string i in new string[]{ "address","00","01", "02", "03", "04", "05"
            , "06", "07", "08", "09", "0A", "0B", "0C", "0D", "0E", "0F","dump"}){
                this.HexView.Columns[i].Frozen = true;
                this.HexView.Columns[i].HeaderText = i;
                this.HexView.Columns[i].Name = i;
                this.HexView.Columns[i].MinimumWidth = 6;
                if (i == "address")
                {
                    this.HexView.Columns[i].ReadOnly = true;
                    this.HexView.Columns["address"].Width = 160;
                }
                else if (i == "dump")
                {
                    this.HexView.Columns[i].ReadOnly = true;
                    this.HexView.Columns["dump"].Width = 371;
                    (this.HexView.Columns[i] as DataGridViewTextBoxColumn).MaxInputLength = 16;
                }
                else 
                {
                    this.HexView.Columns[i].Width = 31;
                    (this.HexView.Columns[i] as DataGridViewTextBoxColumn).MaxInputLength = 2;
                }
            }
            
            //// 
            //// address
            //// 
            //this.address.Frozen = true;
            //this.address.HeaderText = "address";
            //this.address.MinimumWidth = 6;
            //this.address.Name = "address";
            //this.address.ReadOnly = true;
            //this.address.Width = 110;
            //// 
            //// col0
            //// 
            //this.col0.Frozen = true;
            //this.col0.HeaderText = "00";
            //this.col0.MaxInputLength = 2;
            //this.col0.MinimumWidth = 6;
            //this.col0.Name = "col0";
            //this.col0.Width = 31;
        }

        private MyModule.MySection selected_section = null;
        private void sectionsTree_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode item = sectionsTree.SelectedNode;

            if (item != null)
            {
                this.selected_section = item.Tag as MyModule.MySection;
                if (this.selected_section != null)
                {
                    load_section_memory(Convert.ToUInt64(this.selected_section.BaseAdress, 16), this.selected_section.RegionSize);
                }
            }
          //  this.HexView.Rows.Clear();
           //this.HexView.Rows.Add(reade_memory(Convert.ToUInt64(sect.BaseAdress,16),sect.RegionSize));
        }
        private void SectionsTree_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode item =this.sectionsTree.GetNodeAt(new Point(e.X,e.Y));// sectionsTree.SelectedNode;
                if (item != null)
                {
                    MyModule selected_module = item.Tag as MyModule;
                    if (selected_module != null)
                    {
                        this.sectionsTreeChooseIfAssemblyStart.Items.Clear();
                        ToolStripMenuItem startItem = new ToolStripMenuItem("disassembling from entry point");
                        startItem.Click += (sender, e) => {
                            ulong addr=Convert.ToUInt64(this.targetProc.Modules.Cast<ProcessModule>()
                                .FirstOrDefault(module => module.FileName == selected_module.Path &&
                                    module.ModuleName == selected_module.Name)
                                .EntryPointAddress.ToInt64());
                            this.dissasemblingStartAddressBox.Text = addr.ToString("X");

                            //MyModule.MySection addrSec = this.worker_controller.get_address_section(addr);
                            //byte[] data = this.worker_controller.read_memory(addr,
                            //     Convert.ToUInt32(addrSec.RegionSize - (addr - Convert.ToUInt64(addrSec.BaseAdress, 16)))
                            //);
                            //dissasembling(data, addr);
                        };
                         this.sectionsTreeChooseIfAssemblyStart.Items.Add(startItem);
                        this.sectionsTreeChooseIfAssemblyStart.Show(this.sectionsTree, e.Location);
                    }
                    //this.sectionsTreeChooseIfAssemblyStart.Items.Clear();
                }
            }
        }
        private void DataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < dataTable.Rows.Count && e.ColumnIndex < dataTable.Columns.Count)
            {
                if (e.ColumnIndex == 17)
                {
                    byte[] bbb = new byte[16];
                    for(int i = 0; i < 16; i++)
                    {
                        bbb[i]=(byte)dataTable.Rows[e.RowIndex][i+1];
                    }
                    e.Value = utils.dumpHex(bbb);//((ulong)dataTable.Rows[e.RowIndex][e.ColumnIndex]).ToString("X");
                }
                else
                {
                    e.Value = ((byte)dataTable.Rows[e.RowIndex][e.ColumnIndex]).ToString("X");
                }

            }
        }


        //private void HexView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) //DataGridViewCellEventArgs   //
        //{
        //    if (e.ColumnIndex >= 1 && e.ColumnIndex <= 16)
        //    {

        //    }
        //    else if (e.ColumnIndex == 17)
        //    {
        //        e.Cancel = true;
        //        //Monitor.Enter(_read_memory_lock);//to 
        //        //byte[] bdata = new byte[16];
        //        //for (int i = 1; i <= 16; i++)
        //        //{
        //        //    bdata[i - 1] = Convert.ToByte((string)this.HexView.Rows[e.RowIndex].Cells[i].Value, 16);
        //        //}
        //        //this.HexView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Encoding.ASCII.GetString(bdata);
        //        //MessageBox.Show(Encoding.ASCII.GetString(bdata));
        //        //MessageBox.Show((string)this.HexView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
        //    }
        //}

        private void HexView_CellEndEdit(object sender, DataGridViewCellEventArgs e)//some fixes needed to support 32 bit
        {
            if (e.ColumnIndex >= 1 && e.ColumnIndex <= 16)
            {
                string value = this.HexView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string address = this.HexView.Rows[e.RowIndex].Cells[0].Value.ToString();
                if (value.Length==0)
                {
                    value = "0";
                }
                byte[] data;
                try
                {
                    data = new byte[1] { Convert.ToByte(value, 16) };
                }
                catch
                {
                    MessageBox.Show("please enter data in hex format");
                    return;
                }
                write_memory(Convert.ToUInt64(address, 16) + (ulong)(e.ColumnIndex - 1), data);

            }
            else if (e.ColumnIndex == 17)
            {
                string address = this.HexView.Rows[e.RowIndex].Cells[0].Value.ToString();

                string data = (string)this.HexView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                while (data.Length < 16)
                {
                    data += "\0";
                }

                byte[] bdata = Encoding.ASCII.GetBytes(data);
                //this.HexView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = utils.dumpHex(bdata);
                write_memory(Convert.ToUInt64(address, 16), bdata);
                //Monitor.Exit(_read_memory_lock);//
            }
        }
        private void HexView_MouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if(e.RowIndex != -1 && e.ColumnIndex > 0)
                {
                    string address = (Convert.ToUInt64((string)this.HexView.Rows[e.RowIndex].Cells[0].Value, 16) +
                            Convert.ToUInt64(e.ColumnIndex - 1)).ToString("X");

                    this.temporaryContextMenuStrip.Items.Clear();
                    ToolStripMenuItem startItem = new ToolStripMenuItem("disassembling from this address");
                    startItem.Click += (sender, _e) => {
                        this.dissasemblingStartAddressBox.Text = address;
                    };
                    
                    ToolStripMenuItem brpt = new ToolStripMenuItem();
                    if (this.breakPoints.FindIndex((s) => s == address) == -1)
                    {
                        brpt.Text = "add break point";
                        brpt.Click += DisassemblyText_MouseContextMenuAddbpt;
                        brpt.Click += (s, e) => {
                            int line = this.disassemblyText.SelectionStart;
                            if (line != -1)
                            {
                                this.addBreakPoint(address);
                            }
                        };
                    }
                    else {
                        brpt.Text = "remove break point";
                        brpt.Click += (s, e) => {
                            int line = this.disassemblyText.SelectionStart;
                            if (line != -1)
                            {
                                this.removeBreakPoint(address);
                            }
                        };
                    }
                    this.temporaryContextMenuStrip.Items.Add(startItem);
                    this.temporaryContextMenuStrip.Items.Add(brpt);

                    Point position = this.HexView.PointToClient(Cursor.Position);
                    this.temporaryContextMenuStrip.Show(this.HexView, position);
                }
                else if (e.RowIndex == -1 && e.ColumnIndex == 0)
                {
                    this.temporaryContextMenuStrip.Items.Clear();
                    ToolStripTextBox gotoAddressItem = new ToolStripTextBox();
                    //gotoAddressItem.Text = "goto address:";
                    gotoAddressItem.TextChanged += (sender, _e) => {
                        string saddr = (sender as ToolStripTextBox).Text;
                        try
                        {
                            ulong addr = Convert.ToUInt64(saddr, 16);
                            foreach(TreeNode mnode in this.sectionsTree.Nodes)
                            {
                                foreach (TreeNode snode in mnode.Nodes)
                                {
                                    MyModule.MySection sect = (snode.Tag as MyModule.MySection);
                                    ulong badddr = Convert.ToUInt64(sect.BaseAdress, 16);
                                    if (addr >= badddr && addr <= badddr + sect.RegionSize)
                                    {
                                        this.selected_section = sect;
                                        if (this.selected_section != null)
                                        {
                                            load_section_memory(Convert.ToUInt64(this.selected_section.BaseAdress, 16), this.selected_section.RegionSize);
                                            foreach (DataGridViewRow row in this.HexView.Rows)
                                            {
                                               if(Convert.ToUInt64(row.Cells[0].Value as string,16) == addr)
                                                {
                                                    this.HexView.FirstDisplayedScrollingRowIndex = row.Index;
                                                    break;
                                                }
                                            }
                                        }
                                        return;
                                    }
                                }
                            }
                            MessageBox.Show("address not in program's regions");
                        }
                        catch
                        {
                            MessageBox.Show("please enter data in hex format");
                            return;
                        }
                    };

                    this.temporaryContextMenuStrip.Items.Add(gotoAddressItem);

                    Point position = this.HexView.PointToClient(Cursor.Position);
                    this.temporaryContextMenuStrip.Show(this.HexView, position);
                }
            }
        }

        private void controlsRunCodeButton_Click(object sender, EventArgs e)
        {
            this.commander = new UIloaderCommander(this);

            var t = this.commander.Run(this.loaderCommands.Text);
            if (!t.Item1)
                MessageBox.Show("err at line" + t.Item2.ToString());
        }


        private void mainViewchooseContentClick(object sender, EventArgs e)
        {
            this.showMemoryButton.BackColor = SystemColors.Control;
            this.showDissasemblerButton.BackColor = SystemColors.Control;
            (sender as ToolStripButton).BackColor= SystemColors.Highlight;
            this.HexViewPanel.Visible = false;
            this.disassemblerPanel.Visible = false;
            ((sender as ToolStripButton).Tag as Panel).Visible = true;
        }



        private void dissasembling(byte[] data,ulong address)//fixes needded for support x32
        {
            SharpDisasm.Disassembler disasm = new SharpDisasm.Disassembler(data, SharpDisasm.ArchitectureMode.x86_64, address, true);

            var instructions= disasm.Disassemble().ToList();
            StringBuilder sb = new StringBuilder();
            List<int> brpts = new List<int>();
            int i = 0;
            foreach (var instruction in instructions)
            {
                string addr = instruction.Offset.ToString("X");
                sb.AppendLine(addr + ": "+instruction.ToString());
                if (this.breakPoints.FindIndex((s) => s== addr) != -1)
                {
                    brpts.Add(i);
                }
                i++;
            }
            this.disassemblyText.Text = sb.ToString();
            foreach (var index in brpts)
            {
                this.disassemblyText.Select(
                        this.disassemblyText.GetFirstCharIndexFromLine(index), this.disassemblyText.Lines[index].Length
                    );
                    this.disassemblyText.SelectionColor = Color.Red;
            }
            //for (int i = 0; i < this.disassemblyText.Lines.Length; i++)
            //{
            //    string line = this.disassemblyText.Lines[i];
            //    if (this.breakPoints.FindIndex((s) => line.StartsWith(s)) != -1)
            //    {
            //        this.disassemblyText.Select(
            //            this.disassemblyText.GetFirstCharIndexFromLine(i), this.disassemblyText.Lines[i].Length
            //        );
            //        this.disassemblyText.SelectionColor = Color.Red;
            //    }
            //}
        }
        private void DissasemblingStartAddressBox_TextChanged(object sender, System.EventArgs e)
        {
            ulong addr;
            try
            {
                addr = Convert.ToUInt64(this.dissasemblingStartAddressBox.Text, 16);
                MyModule.MySection addrSec = this.worker_controller.get_address_section(addr);

                byte[] data = this.worker_controller.read_memory(addr,
                     Convert.ToUInt32(addrSec.RegionSize-(addr - Convert.ToUInt64(addrSec.BaseAdress, 16)))
                );

                dissasembling(data, addr);
            }
            catch
            {
                MessageBox.Show("enter valid address in hex");
            }
        }




        private int find_DisassemblyTextAddressIndex(string address)
        {
            int index = -1;
            for (int i = 0; i < this.disassemblyText.Lines.Length; i++)
            {
                string line = this.disassemblyText.Lines[i];
                if (line.StartsWith(address))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        private bool addBreakPoint(string address)
        {
            if(!new System.Text.RegularExpressions.Regex("^[0-9A-Fa-f]+$").IsMatch(address))
            {
                return false;
            }
            if (this.breakPoints.FindIndex((s) => s == address) == -1)
            {
                this.breakPoints.Add(address);
                int index = find_DisassemblyTextAddressIndex(address);
                if (index != -1)
                {
                    this.disassemblyText.Select(
                        this.disassemblyText.GetFirstCharIndexFromLine(index), this.disassemblyText.Lines[index].Length
                    );
                    this.disassemblyText.SelectionColor = Color.Red;
                }
            }
            return true;
        }
        private bool removeBreakPoint(string address)
        {
            if (!this.breakPoints.Remove(address))
                return false;
            int index = find_DisassemblyTextAddressIndex(address);
            if (index != -1)
            {
                this.disassemblyText.Select(
                    this.disassemblyText.GetFirstCharIndexFromLine(index), this.disassemblyText.Lines[index].Length
                );
                this.disassemblyText.SelectionColor = this.disassemblyText.ForeColor;
            }
            return true;
        }




        ////
        ////disassemblyTextContextMenuStrip
        ////
        //ToolStripMenuItem addBrkPnt = new ToolStripMenuItem("add break point");
        //addBrkPnt.Click +=DisassemblyText_MouseContextMenuAddbpt
        //    this.disassemblyTextContextMenuStrip.Items.Add(addBrkPnt);
        private void DisassemblyText_MouseContextMenuAddbpt(object sender, EventArgs e)
        {
            int line = this.disassemblyText.SelectionStart;
            if (line != -1)
            {
                int lineNumber = this.disassemblyText.GetLineFromCharIndex(line);
                this.addBreakPoint(this.disassemblyText.Lines[lineNumber].Split(':')[0]);
            }
        }
        private void DisassemblyText_keyPress(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.B)
            {
                int line = this.disassemblyText.SelectionStart;
                if (line != -1)
                {
                    int lineNumber = this.disassemblyText.GetLineFromCharIndex(line);
                    string addr;
                    try { addr = this.disassemblyText.Lines[lineNumber].Split(':')[0]; }
                    catch { return; }
                    if (this.breakPoints.FindIndex((s) => s == addr) == -1)
                    {
                        this.addBreakPoint(this.disassemblyText.Lines[lineNumber].Split(':')[0]);
                    }
                    else
                    {
                        this.removeBreakPoint(this.disassemblyText.Lines[lineNumber].Split(':')[0]);
                    }
                }
            }
        }
        private void DisassemblyText_MouseClick(object sender, MouseEventArgs e)
        {
            //MessageBox.Show(e.Button.ToString());
            if (e.Button == MouseButtons.Right)
            {
                int line = this.disassemblyText.SelectionStart;
                if (line!=-1)
                {
                    int lineNumber = this.disassemblyText.GetLineFromCharIndex(line);
                    string addr;
                    try {addr = this.disassemblyText.Lines[lineNumber].Split(':')[0]; }
                    catch { return; }
                    

                    this.temporaryContextMenuStrip.Items.Clear();
                    ToolStripMenuItem option = new ToolStripMenuItem();
                    if (this.breakPoints.FindIndex((s) => s == addr)==-1)
                    {
                        option.Text = "add break point";
                        option.Click += DisassemblyText_MouseContextMenuAddbpt;
                        option.Click += (s, e) => {
                            int line = this.disassemblyText.SelectionStart;
                            if (line != -1)
                            {
                                this.addBreakPoint(this.disassemblyText.Lines[lineNumber].Split(':')[0]);
                            }
                        };
                    }
                    else
                    {
                        option.Text = "remove break point";
                        option.Click += (s,e) => {
                            int line = this.disassemblyText.SelectionStart;
                            if (line != -1)
                            {
                                this.removeBreakPoint(this.disassemblyText.Lines[lineNumber].Split(':')[0]);
                            }
                        };
                    }
                    this.temporaryContextMenuStrip.Items.Add(option);
                    Point position = this.disassemblyText.PointToClient(Cursor.Position);
                    this.temporaryContextMenuStrip.Show(this.disassemblyText, position);
                }
            }
        }
        //ToolStripMenuItem addBrkPnt = new ToolStripMenuItem("add break point");
        //addBrkPnt.Click += DisassemblyText_MouseContextMenuAddbpt;
        //this.disassemblyTextContextMenuStrip.Items.Add(addBrkPnt);

    }
}
