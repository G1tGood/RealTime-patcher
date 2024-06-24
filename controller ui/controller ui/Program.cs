using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace controller_ui
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            utils.Iintilizing_events();



            int pid = -1;
            //System.Diagnostics.Process.Start("notepad.exe");
           
            foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcesses())
            {
                if (proc.ProcessName.ToLower().StartsWith("dummy"))
                {
                    pid = proc.Id;
                    utils.injecting(proc.Id);
                    break;
                }
            }
            if (pid == -1)
            {
                MessageBox.Show("not found");
                return;
            }


            Application.Run(new Patcher(pid));
            //System.Diagnostics.Process.GetProcessById(pid).Kill();

            //processLister chooseProc = new processLister();
            // Application.Run(chooseProc);
            // Application.Run(new Patcher(chooseProc.returnPid));




            //chooseProc = null;

            //while (true) { }
            //int pid = -1;
            //do
            //{
            //    if (pid != -1)
            //    {
            //        MessageBox.Show("ACCESS DENIED!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //    processLister chooseProc = new processLister();
            //    Application.Run(chooseProc);
            //    pid = chooseProc.returnPid;
            //} while (!injecting(pid));
        }
    }
}
