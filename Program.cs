using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NodeDock
{
    static class Program
    {
        // 用于单例模式的 Mutex 名称
        private const string MutexName = "NodeDock_SingleInstance_Mutex";
        
        // 自定义 Windows 消息，用于通知已运行的实例显示窗口
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME_NODEDOCK");
        
        [DllImport("user32.dll")]
        private static extern int RegisterWindowMessage(string message);
        
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (createdNew)
                {
                    // 这是第一个实例，正常启动程序
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    // 已有实例在运行，广播自定义消息通知其显示窗口
                    PostMessage(HWND_BROADCAST, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }
    }
}
