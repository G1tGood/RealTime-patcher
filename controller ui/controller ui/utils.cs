using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Pipes;

namespace controller_ui
{
    class utils
    {
        static string dll32path = System.IO.Path.GetFullPath("..\\..\\..\\..\\..\\dll worker\\Debug\\dll worker.dll");
        static string dll64path = System.IO.Path.GetFullPath("..\\..\\..\\..\\..\\dll worker\\x64\\Debug\\dll worker.dll");
        public static bool injecting(int pid)
        {
            [DllImport("C:\\Users\\user\\Documents\\קורסים\\יג\\א\\יסודות באבטחת תוכנה\\RealTime-patcher\\contoller util\\x64\\Release\\contoller util.dll")]
            static extern int inject(int pid, IntPtr dll_path);
            IntPtr ptr = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(dll64path);

            if (inject(pid, ptr) == 5)
                return false;

            return true;
        }
        private static void register_handler(string name, Action handler)
        {
            ThreadPool.RegisterWaitForSingleObject(
          new EventWaitHandle(false, EventResetMode.AutoReset, name),
          (state, timedOut) => { handler(); }, false, -1, false);

        }
        public static byte[] longToBytes(long val)
        {
            byte[] res = BitConverter.GetBytes(val);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(res);
            return res;
        }
        #region transleMemoryProtection
        private static Dictionary<int, string> Memory_Protection_Constants = new Dictionary<int, string>(){
            { 0x10,"PAGE_EXECUTE" },
            {0x20,"PAGE_EXECUTE_READ" },
            {0x40,"PAGE_EXECUTE_READWRITE" },
            {0x80,"PAGE_EXECUTE_WRITECOPY" },
            {0x01,"PAGE_NOACCESS" },
            {0x02,"PAGE_READONLY" },
            {0x04,"PAGE_READWRITE" },
            {0x08,"PAGE_WRITECOPY" },
            {0x40000000,"PAGE_TARGETS_INVALID/PAGE_TARGETS_NO_UPDATE" },
            {0x00,"0" }
        };
        public static string transleMemoryProtection(int protection)
        {
            string basic = Memory_Protection_Constants[(int)(protection & 0xfffff8ff)];

            return basic +
                ((protection & 0x100) == 1 ? "|PAGE_GUARD" : "") +
                ((protection & 0x200) == 1 ? "|PAGE_NOCACHE" : "") +
                ((protection & 0x400) == 1 ? "|PAGE_WRITECOMBINE" : "");
            
        }
        #endregion
        private static char translateByte(byte val)
        {
            if (val < 32) return '.';
            if (val < 127) return (char)val;
            if (val == 127) return '.';
            if (val < 0x90) return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[val & 0xF];
            if (val < 0xA0) return ".‘’“”•–—˜™š›œ.žŸ"[val & 0xF];
            if (val == 0xAD) return '.'; 
            return (char)val;
        }
        public static string dumpHex(byte[] bytes)
        {
            return new string(bytes.Select(translateByte).ToArray());
        }

        public static void triggerEvent(string eventName)
        {
            using (EventWaitHandle eventWaitHandle =
            new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\yesodot_memory_Dynamic_Memory_Patcher_" + eventName))
            {
                eventWaitHandle.Set();
            }
        }
        private static void registerEvent(string name, WaitOrTimerCallback doo)
        {
            ThreadPool.RegisterWaitForSingleObject(
                new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\yesodot_memory_Dynamic_Memory_Patcher_" + name),
                doo, false, -1, false);
        }
        public static NamedPipeServerStream write_memory_server =
            new NamedPipeServerStream("yesodot_memory_Dynamic_Memory_Patcher_write_memory",
                PipeDirection.InOut, 1, PipeTransmissionMode.Message);
        public static void Iintilizing_events()
        {
            registerEvent("cannotWriteEvent_show",(state, timedOut) =>{
                MessageBox.Show("cannot get access to write");
            });

            //ThreadPool.RegisterWaitForSingleObject(
            //    new EventWaitHandle(false,EventResetMode.AutoReset,"Global\\yesodot_memory_Dynamic_Memory_Patcher_test"),
            //    (state, timedOut) =>{
            //        MessageBox.Show("work");
            //        using (EventWaitHandle eventWaitHandle =
            //        new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\yesodot_memory_Dynamic_Memory_Patcher_test2"))
            //        {
            //           eventWaitHandle.Set();
            //        }
            //    },false, -1, false);

        }
    }
}
