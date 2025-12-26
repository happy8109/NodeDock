using System.Collections.Generic;

namespace NodeDock.Models
{
    /// <summary>
    /// 全局应用配置设置
    /// </summary>
    public class NodeDockSettings
    {
        /// <summary>
        /// 受管理的应用程序列表
        /// </summary>
        public List<AppItem> AppList { get; set; } = new List<AppItem>();

        /// <summary>
        /// 是否最小化到系统托盘
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// 是否开机自动启动管理器
        /// </summary>
        public bool StartAtLogin { get; set; } = false;

        /// <summary>
        /// 默认使用的 Node 版本名称
        /// </summary>
        public string DefaultNodeVersion { get; set; }
    }
}
