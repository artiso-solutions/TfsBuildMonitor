using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BuildMonitor.Logic.Contracts;
using BuildMonitorWpf.Properties;
using Newtonsoft.Json;

namespace BuildMonitorWpf.ViewModel
{
   public static class MonitorSettingsContainer
   {
      private const string SettingsFolder = "BuildMonitorWpf";
      private const string BuildServerConfigFileName = "buildServers.config";
      private const string MonitorSettingsFileName = "monitorSettings.config";
      private static readonly Lazy<MonitorSettings> monitorSettings = new Lazy<MonitorSettings>(LoadMonitorSettings);

      public static MonitorSettings MonitorSettings
      {
         get { return monitorSettings.Value; }
      }

      public static List<BuildServer> BuildServers { get; set; }

      public static void LoadAllSettings()
      {
         BuildServers = LoadBuildServerConfiguration();
      }

      public static void SaveBuildServers()
      {
         try
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
         catch (Exception exception)
         {
            Debug.WriteLine($"Failed to save build servers: {exception}");
         }
      }

      private static List<BuildServer> LoadBuildServerConfiguration()
      {
         try
         {
            var appDataFoder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsFolder = Path.Combine(appDataFoder, SettingsFolder);
            var buildServerConfigurationFilePath = Path.Combine(settingsFolder, BuildServerConfigFileName);
            if (File.Exists(buildServerConfigurationFilePath))
            {
               return JsonConvert.DeserializeObject<List<BuildServer>>(File.ReadAllText(buildServerConfigurationFilePath));
            }
         }
         catch (Exception exception)
         {
            Debug.WriteLine($"Failed to load build server configuration: {exception}");

         }

         return Settings.Default.BuildServers.BuildServers;
      }

      private static MonitorSettings LoadMonitorSettings()
      {
         try
         {
            var appDataFoder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsFolder = Path.Combine(appDataFoder, SettingsFolder);
            var monitorSettingsFilePath = Path.Combine(settingsFolder, MonitorSettingsFileName);
            if (File.Exists(monitorSettingsFilePath))
            {
               return JsonConvert.DeserializeObject<MonitorSettings>(File.ReadAllText(monitorSettingsFilePath));
            }
         }
         catch (Exception exception)
         {
            Debug.WriteLine($"Failed to load monitor settings: {exception}");
         }

         return new MonitorSettings
         {
            RefreshInterval = Settings.Default.RefreshInterval,
            ColumnWidths = Settings.Default.ColumnWidths,
            WindowTop = Settings.Default.WindowTop,
            WindowLeft = Settings.Default.WindowLeft,
            UpgradeNeeded = Settings.Default.UpgradeNeeded,
            BigSize = Settings.Default.BigSize,
            ZoomFactor = Settings.Default.ZoomFactor,
            UseFullWidth = Settings.Default.UseFullWidth,
            RibbonMinimized = Settings.Default.RibbonMinimized
         };
      }

      public static void SaveMonitorSettings()
      {
         SaveMonitorSettings(MonitorSettings);
      }

      public static void SaveMonitorSettings(MonitorSettings monitorSettings)
      {
         try
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
         catch (Exception exception)
         {
            Debug.WriteLine($"Failed to save monitor settings: {exception}");
         }
      }
   }
}