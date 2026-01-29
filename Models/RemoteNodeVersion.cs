using Newtonsoft.Json;

namespace NodeDock.Models
{
    public class RemoteNodeVersion
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("files")]
        public string[] Files { get; set; }

        [JsonProperty("lts")]
        public object Lts { get; set; }

        /// <summary>
        /// 标记该版本是否为当前项目推荐的版本（基于 package.json 的 engines 字段）
        /// </summary>
        public bool IsRecommended { get; set; }

        /// <summary>
        /// 标记该版本是否为系统环境中安装的版本
        /// </summary>
        public bool IsSystemVersion { get; set; }

        /// <summary>
        /// 标记该版本是否为 Windows 7 最后支持的版本 (v13.14.0)
        /// </summary>
        public bool IsWin7Last { get; set; }

        public bool IsLts => Lts != null && Lts.ToString().ToLower() != "false";
        
        public string LtsName => IsLts ? Lts.ToString() : "";

        public string DisplayName
        {
            get
            {
                var baseName = IsLts ? $"{Version} (LTS: {LtsName})" : Version;
                
                // 添加标记
                var tags = "";
                if (IsSystemVersion) tags += " [系统环境]";
                if (IsRecommended) tags += " [推荐]";
                if (IsWin7Last) tags += " [Win7最后支持版本]";
                
                return baseName + tags;
            }
        }

        public override string ToString() => DisplayName;
    }
}

