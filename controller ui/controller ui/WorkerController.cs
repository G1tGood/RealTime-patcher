using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controller_ui
{
    class WorkerController
    {
        public enum BitMode { x86_32, x86_64 };
        private BitMode bitMode;
        public WorkerController(BitMode bitMode = BitMode.x86_64)
        {
            this.bitMode = bitMode;
        }
        public WorkerController(BitMode bitMode, int pid)
        {
            this.bitMode = bitMode;
            utils.injecting(pid, this.bitMode == BitMode.x86_32);
        }

        private readonly object _read_memory_lock = new object();
        public byte[] read_memory(ulong address, ulong len)
        {
            lock (_read_memory_lock)
            {
                using (var server = new NamedPipeServerStream(
                "yesodot_memory_Dynamic_Memory_Patcher_read_memory", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                {
                    utils.triggerEvent("read_memory");
                    server.WaitForConnection();
                    byte[] addressBytes = BitConverter.GetBytes(
                        this.bitMode == BitMode.x86_64?
                        address:
                        Convert.ToUInt32(address));

                    byte[] lenBytes = BitConverter.GetBytes(len);
                    byte[] addressLenBytes = new byte[addressBytes.Length + lenBytes.Length];
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(addressBytes);
                        Array.Reverse(lenBytes);
                    }
                    addressBytes.CopyTo(addressLenBytes, 0);
                    lenBytes.CopyTo(addressLenBytes, addressBytes.Length);

                    server.Write(addressLenBytes);

                    server.WaitForPipeDrain();
                    byte[] buffer = new byte[len];
                    server.Read(buffer);
                    return buffer;
                }
            }
        }

        private readonly object _write_memory_lock = new object();
        public void write_memory(ulong address, byte[] data)
        {
            lock (_write_memory_lock)
            {

                byte[] addressBytes = BitConverter.GetBytes(
                        this.bitMode == BitMode.x86_64 ?
                        address :
                        Convert.ToUInt32(address));

                byte[] lenBytes = BitConverter.GetBytes(data.Length);
                byte[] addressLenBytes = new byte[addressBytes.Length + lenBytes.Length];
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(addressBytes);
                    Array.Reverse(lenBytes);
                }
                addressBytes.CopyTo(addressLenBytes, 0);
                lenBytes.CopyTo(addressLenBytes, addressBytes.Length);

                utils.triggerEvent("write_memory");
                utils.write_memory_server.WaitForConnection();

                utils.write_memory_server.Write(addressLenBytes);
                utils.write_memory_server.Flush();

                utils.write_memory_server.Write(data);
                utils.write_memory_server.Flush();
                utils.write_memory_server.WaitForPipeDrain();

                utils.write_memory_server.Close();
                utils.write_memory_server = new NamedPipeServerStream("yesodot_memory_Dynamic_Memory_Patcher_write_memory",
                PipeDirection.InOut, 1, PipeTransmissionMode.Message);
            }
        }

        private readonly object _get_module_sections_lock = new object();
        public List<MyModule.MySection> get_module_sections(MyModule mymodule)
        {
            lock (_get_module_sections_lock)
            {
                List<MyModule.MySection> rett = new List<MyModule.MySection>();
                using (var server = new NamedPipeServerStream(
                     "yesodot_memory_Dynamic_Memory_Patcher_get_module_sections", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                {
                    utils.triggerEvent("get_module_sections");
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

                        rett.Add(new MyModule.MySection(
                            sliced[0],
                            Convert.ToUInt64(sliced[1]),
                            Convert.ToInt32(sliced[2]))
                        );
                    }
                    return rett;
                }
            }
        }

        private static readonly object _get_module_PE_sections_lock = new object();
        public void update_module_PE_sections(MyModule mymodule)
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
                            if (Convert.ToUInt64(sliced[1], 16) == Convert.ToUInt64(sec.BaseAdress, 16))
                            {
                                sec.Name = sliced[0];
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static readonly object _get_address_section_lock = new object();
        public MyModule.MySection get_address_section(ulong address)
        {
            lock (_get_address_section_lock)
            {
                using (var server = new NamedPipeServerStream(
                "yesodot_memory_Dynamic_Memory_Patcher_get_address_section", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                {
                    utils.triggerEvent("get_address_section");
                    server.WaitForConnection();
                    byte[] addressBytes = BitConverter.GetBytes(
                        this.bitMode == BitMode.x86_64 ?
                        address :
                        Convert.ToUInt32(address));
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(addressBytes);
                    }
                    server.Write(addressBytes);
                    server.WaitForPipeDrain();

                    byte[] buffer = new byte[100];
                    server.Read(buffer);
                    string strBuffer = Encoding.ASCII.GetString(buffer);
                    string[] sliced = strBuffer.Split('\n');

                    return new MyModule.MySection(
                        sliced[0],
                        Convert.ToUInt32(sliced[1]),
                        Convert.ToInt32(sliced[2])
                    );
                }
            }
        }

    }



}
