using System;
using System.Collections.Generic;
using System.IO;
using BuildMonitor.Logic.Contracts;
using Newtonsoft.Json;

namespace BuildMonitorWpf.ViewModel
{
   public class MonitorViewModel
   {
      private static Lazy<MonitorSettings> monitorSettings = new Lazy<MonitorSettings>(LoadMonitorSettings);

      private const string SettingsFolder = "BuildMonitorWpf";
      private const string BuildServerConfigFileName = "buildServers.config";
      private const string MonitorSettingsFileName = "monitorSettings.config";

      public static MonitorSettings MonitorSettings
      {
         get { return monitorSettings.Value; }
      }

      public List<BuildServer> BuildServers { get; set; }

      public void LoadAllSettings()
      {
         BuildServers = LoadBuildServerConfiguration();
      }

      public void SaveBuildServers()
      {
         var appDataFoder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
         var settingsFolder = Path.Combine(appDataFoder, SettingsFolder);
         if (!Directory.Exists(settingsFolder))
         {
            Directory.CreateDirectory(settingsFolder);
         }

         var buildServerConfigurationFilePath = Path.Combine(settingsFolder, BuildServerConfigFileName);
         File.WriteAllText(buildServerConfigurationFilePath, JsonConvert.SerializeObject(BuildServers));
      }

      private List<BuildServer> LoadBuildServerConfiguration()
      {
         var appDataFoder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
         var settingsFolder = Path.Combine(appDataFoder, SettingsFolder);
         var buildServerConfigurationFilePath = Path.Combine(settingsFolder, BuildServerConfigFileName);
         if (!File.Exists(buildServerConfigurationFilePath))
         {
            return new List<BuildServer>();
         }

         return JsonConvert.DeserializeObject<List<BuildServer>>(File.ReadAllText(buildServerConfigurationFilePath));
      }

      public static MonitorSettings LoadMonitorSettings()
      {
         var appDataFoder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
         var settingsFolder = Path.Combine(appDataFoder, SettingsFolder);
         var monitorSettingsFilePath = Path.Combine(settingsFolder, MonitorSettingsFileName);
         if (!File.Exists(monitorSettingsFilePath))
         {
            return new MonitorSettings
            {
               RefreshInterval = 60,
               ColumnWidths = "30:true,35:true,110:true,250:true,350:true,120:true,100:true,35:true,120:true",
               WindowTop = -1,
               WindowLeft = -1,
               UpgradeNeeded = true,
               BigSize = false,
               ZoomFactor = 2,
               UseFullWidth = false,
               RibbonMinimized = true
            };
         }

         return JsonConvert.DeserializeObject<MonitorSettings>(File.ReadAllText(monitorSettingsFilePath));
      }

      public void SaveMonitorSettings()
      {
         SaveMonitorSettings(MonitorSettings);
      }

      public static void SaveMonitorSettings(MonitorSettings monitorSettings)
      {
         var appDataFoder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
         var settingsFolder = Path.Combine(appDataFoder, SettingsFolder);
         if (!Directory.Exists(settingsFolder))
         {
            Directory.CreateDirectory(settingsFolder);
         }

         var monitorSettingsFilePath = Path.Combine(settingsFolder, MonitorSettingsFileName);
         File.WriteAllText(monitorSettingsFilePath, JsonConvert.SerializeObject(monitorSettings));
      }
   }
}