using System;
using System.IO;
using Newtonsoft.Json;
using NodeDock.Models;

namespace NodeDock.Services
{
    public class ConfigService
    {
        private readonly string _configPath;
        private static ConfigService _instance;
        public static ConfigService Instance => _instance ?? (_instance = new ConfigService());

        public NodeDockSettings Settings { get; private set; }

        private ConfigService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "settings.json");
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    Settings = JsonConvert.DeserializeObject<NodeDockSettings>(json) ?? new NodeDockSettings();
                }
                else
                {
                    Settings = new NodeDockSettings();
                    Save(); // 创建初始文件
                }
            }
            catch (Exception)
            {
                Settings = new NodeDockSettings();
            }
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                // TODO: 记录日志
                Console.WriteLine("保存配置失败: " + ex.Message);
            }
        }
    }
}
