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

        public bool IsLts => Lts != null && Lts.ToString().ToLower() != "false";
        
        public string LtsName => IsLts ? Lts.ToString() : "";

        public string DisplayName => IsLts ? $"{Version} (LTS: {LtsName})" : Version;

        public override string ToString() => DisplayName;
    }
}
