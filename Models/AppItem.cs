using System;

namespace NodeDock.Models
{
    /// <summary>
    /// 表示一个受管理的 Node.js 应用项
    /// </summary>
    public class AppItem
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 项目工作目录（绝对路径）
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// 入口脚本路径（相对于 WorkingDirectory 或绝对路径）
        /// </summary>
        public string EntryScript { get; set; }

        /// <summary>
        /// 关联的 Node 运行版本（对应 runtimes 下的目录名）
        /// </summary>
        public string NodeVersion { get; set; }

        /// <summary>
        /// 附加启动参数
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// 是否随管理器启动而自动启动
        /// </summary>
        public bool AutoStart { get; set; }

        /// <summary>
        /// 运行状态（内部运行态，不序列化到 JSON）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public AppStatus Status { get; set; } = AppStatus.Stopped;

        /// <summary>
        /// 启动时间（内部运行态，不序列化到 JSON）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime? StartTime { get; set; }
    }

    public enum AppStatus
    {
        Stopped,
        Starting,
        Running,
        Error
    }
}
