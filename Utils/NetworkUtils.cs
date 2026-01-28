using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace NodeDock.Utils
{
    public static class NetworkUtils
    {
        /// <summary>
        /// 获取当前系统正在监听的所有 TCP 端口
        /// </summary>
        public static HashSet<int> GetUsedPorts()
        {
            var usedPorts = new HashSet<int>();
            try
            {
                var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();
                
                foreach (var endpoint in tcpListeners)
                {
                    usedPorts.Add(endpoint.Port);
                }
            }
            catch (Exception)
            {
                // 忽略可能的权限或其他异常
            }
            return usedPorts;
        }

        /// <summary>
        /// 检查指定端口是否已被占用
        /// </summary>
        public static bool IsPortBusy(int port)
        {
            return GetUsedPorts().Contains(port);
        }
    }
}
