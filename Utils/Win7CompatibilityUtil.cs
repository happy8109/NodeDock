using System;
using System.Runtime.InteropServices;

namespace NodeDock.Utils
{
    public static class Win7CompatibilityUtil
    {
        /// <summary>
        /// 判断当前操作系统是否为 Windows 7 或更低版本
        /// Windows 7 = 6.1, Windows 8 = 6.2, Windows 10 = 10.0
        /// </summary>
        public static bool IsWindows7OrLower
        {
            get
            {
                var os = Environment.OSVersion.Version;
                return os.Major < 6 || (os.Major == 6 && os.Minor <= 1);
            }
        }

        private const string EnvVarName = "NODE_SKIP_PLATFORM_CHECK";
        private const int WM_SETTINGCHANGE = 0x001A;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

        private const uint SMTO_ABORTIFHUNG = 0x0002;

        /// <summary>
        /// 应用或移除 Windows 7 兼容性环境变量
        /// </summary>
        /// <param name="enable">是否开启</param>
        public static void Apply(bool enable)
        {
            string value = enable ? "1" : null;

            try
            {
                // 1. 修改当前进程的环境变量（立即对后续启动的子进程生效）
                Environment.SetEnvironmentVariable(EnvVarName, value, EnvironmentVariableTarget.Process);

                // 2. 修改用户级环境变量（持久化，重启 NodeDock 依然有效）
                Environment.SetEnvironmentVariable(EnvVarName, value, EnvironmentVariableTarget.User);

                // 3. 广播设置已更改消息
                IntPtr result;
                SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 5000, out result);
            }
            catch (Exception ex)
            {
                // 静默失败，或者在此记录日志
                Console.WriteLine($"无法更新环境变量: {ex.Message}");
            }
        }
    }
}
