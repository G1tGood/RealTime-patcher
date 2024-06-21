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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace controller_ui
{
    public partial class Patcher : Form
    {
        private System.Diagnostics.Process targetProc;
        private System.Threading.Thread updator;
        private loaderCommander commander;
        public Patcher(int pid)
        {
            this.targetProc = System.Diagnostics.Process.GetProcessById(pid);

            InitializeComponent();

            inint_data_table();


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
                    read_memory(Convert.ToUInt64(this.selected_section.BaseAdress, 16), this.selected_section.RegionSize, true);
                }
                //if (i % 200 == 199) //TODO: UPDATE MODULES
                //{
                //    get_modules(true);
                //}
            }
        }


        #region get modules
        private static readonly object _get_module_sections_lock = new object();
        private void get_module_sections(MyModule mymodule)
        {
            lock (_get_module_sections_lock)
            {

                using (var server = new NamedPipeServerStream(
                "yesodot_memory_Dynamic_Memory_Patcher_get_module_sections", PipeDirection.InOut,1,PipeTransmissionMode.Message))
                {
                    utils.triggerEvent("get_module_sections");
                    server.WaitForConnection();
                    byte[] pathh= Encoding.Unicode.GetBytes(mymodule.Path + '\0');
                    server.Write(pathh);
                    server.Flush();
                    server.WaitForPipeDrain();
                    //
                    while (true)
                    {
                        byte[] buffer = new byte[100];
                        if (server.ReadByte()==-1){
                            break;
                        }
                        server.Read(buffer);
                        string strBuffer = Encoding.ASCII.GetString(buffer);
                        string[] sliced=strBuffer.Split('\n');

                        mymodule.Sections.Add(new MyModule.MySection(
                            sliced[0],
                            Convert.ToUInt32(sliced[1]),
                            Convert.ToInt32(sliced[2]))
                        );
                    }
                }
            }
        }
        
        private static readonly object _get_module_PE_sections_lock = new object();
        private void get_module_PE_sections(MyModule mymodule)
        {
            lock (_get_module_PE_sections_lock)
            {
                using (var server = new NamedPipeServerStream(
                "yesodot_memory_Dynamic_Memory_Patcher_get_module_PE_sections", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                {
                    utils.triggerEvent("get_module_PE_sections");
                    server.WaitForConnection();
                    byte[] pathh = Encoding.Unicode.GetBytes(mymodule.Path + '\0');
                    server.Write(pathh);
                    server.Flush();
                    server.WaitForPipeDrain();
                    //
                    while (true)
                    {
                        byte[] buffer = new byte[100];
                        if (server.ReadByte() == -1)
                        {
                            break;
                        }
                        server.Read(buffer);
                        string strBuffer = Encoding.ASCII.GetString(buffer);
                        string[] sliced = strBuffer.Split('\n');

                        foreach (MyModule.MySection sec in mymodule.Sections)
                        {
                            if (Convert.ToUInt64(sliced[1],16)== Convert.ToUInt64(sec.BaseAdress,16))
                            {
                                sec.Name = sliced[0];
                                break;
                            }
                        }
                    }
                }
            }
        }

        private MyModule[] myModules;
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

                get_module_sections(myModule);
                get_module_PE_sections(myModule);
                
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

        private static readonly object _read_memory_lock = new object();//need to change for support 32 bit
        public void read_memory(ulong address, uint len, bool isUpdate=false)
        {
            lock (_read_memory_lock)
            {
                if (!isUpdate)
                {
                    this.dataTable.Rows.Clear();
                }
                //if (dataTable.Rows.Count == len)
                //    return;
                dataTable.BeginLoadData();
                using (var server = new NamedPipeServerStream(
                "yesodot_memory_Dynamic_Memory_Patcher_read_memory", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                {
                    utils.triggerEvent("read_memory");
                    server.WaitForConnection();
                    byte[] addressBytes = BitConverter.GetBytes(address);
                    byte[] lenBytes = BitConverter.GetBytes(len);
                    byte[] addressLenBytes = new byte[addressBytes.Length+ lenBytes.Length];
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(addressBytes);
                        Array.Reverse(lenBytes);
                    }
                    addressBytes.CopyTo(addressLenBytes,0);
                    lenBytes.CopyTo(addressLenBytes, addressBytes.Length);

                    server.Write(addressLenBytes);

                    server.WaitForPipeDrain();
                    byte[] buffer = new byte[len];
                    server.Read(buffer);

                    // DataGridViewRowCollection rows = new DataGridViewRowCollection(this.HexView);
                   // while (true)
                    for(int j=0;j<len;j+=16)
                    {

                       // byte[] buffer = new byte[len];
                        //if (server.ReadByte() == -1)
                        //{
                        //    break;
                        //}
                        //server.Read(buffer);
                        
                        object[] cells = new object[18];
                        cells[0] = (address+(ulong)j).ToString("X");
                        byte[] dump_bytes=new byte[16];
                        for (ulong i = 1; i <= 16; i++)
                        {
                            cells[i] = buffer[(ulong)j +i - 1].ToString("X");
                            dump_bytes[i - 1] = buffer[(ulong)j + i - 1];
                        }
                        cells[17] = utils.dumpHex(dump_bytes);
                        // this.hexDump.Text += utils.dumpHex(buffer);
                        if (!isUpdate)
                        {
                            this.dataTable.Rows.Add(cells);
                        }
                        else
                        {
                            if(!Enumerable.SequenceEqual(this.dataTable.Rows[j / 16].ItemArray, cells))
                            {
                                this.dataTable.Rows[j / 16].ItemArray = cells;
                            }
                            
                        }
                     //   rows.Add(cells);
                    }
                   // return rows;
                }
                dataTable.EndLoadData();
            }
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
                    this.HexView.Columns["address"].Width = 111;
                }
                else if (i == "dump")
                {
                    //this.HexView.Columns[i].ReadOnly = true;
                    this.HexView.Columns["dump"].Width = 420;
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
            if (item != null&&item.Tag != null)
            {
                this.selected_section = item.Tag as MyModule.MySection;
                read_memory(Convert.ToUInt64(this.selected_section.BaseAdress, 16), this.selected_section.RegionSize);
            }
          //  this.HexView.Rows.Clear();
           //this.HexView.Rows.Add(reade_memory(Convert.ToUInt64(sect.BaseAdress,16),sect.RegionSize));
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

        private static readonly object _write_memory_lock = new object();
        public void write_memory(ulong address, byte[] data)
        {
            lock (_write_memory_lock)
            {
                
                byte[] addressLength = new byte[sizeof(ulong) + sizeof(uint)];//take care in case of 32 bit
                BitConverter.GetBytes(address).CopyTo(addressLength, 0);
                BitConverter.GetBytes(data.Length).CopyTo(addressLength, 8);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(addressLength, 0, sizeof(ulong));//take care in case of 32 bit
                    Array.Reverse(addressLength, 8, sizeof(uint));
                }

                utils.triggerEvent("write_memory");
                utils.write_memory_server.WaitForConnection();

                utils.write_memory_server.Write(addressLength);
                utils.write_memory_server.Flush();

                utils.write_memory_server.Write(data);
                utils.write_memory_server.Flush();
                utils.write_memory_server.WaitForPipeDrain();

                utils.write_memory_server.Close();
                utils.write_memory_server = new NamedPipeServerStream("yesodot_memory_Dynamic_Memory_Patcher_write_memory",
                PipeDirection.InOut, 1, PipeTransmissionMode.Message);
            }
        }
        private void HexView_CellEndEdit(object sender, DataGridViewCellEventArgs e)//some fixes needed to support 32 bit
        {
            if (e.ColumnIndex >= 1 && e.ColumnIndex <= 16)
            {
                string value = this.HexView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string address = this.HexView.Rows[e.RowIndex].Cells[0].Value.ToString();
                if (value.Length < 2)
                {
                    value = "0" + value;
                }
                if (value.Length < 2)
                {
                    value = "0" + value;
                }
                byte[] data = new byte[1] { Convert.ToByte(this.HexView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), 16) };
                write_memory(Convert.ToUInt64(address, 16), data);
                   
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
                write_memory(Convert.ToUInt64(address, 16), bdata);
            }
        }


        private void controlsRunCodeButton_Click(object sender, EventArgs e)
        {
            string[] lines = this.loaderCommands.Text.Split("\n");
            this.commander = new UIloaderCommander(this);
            for(int i=0;i< lines.Length; i++)
            {
                if (!this.commander.Run(lines[i]))
                {
                    MessageBox.Show("err at line:" + i.ToString());
                    break;
                }
            }
        }
    }



}
