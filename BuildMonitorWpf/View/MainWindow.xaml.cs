namespace BuildMonitorWpf.View
{
   using System;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Windows.Controls;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.ViewModel;

   using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

   using MS.WindowsAPICodePack.Internal;

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow
   {
      private bool activated;

      internal const String APP_ID = "Artiso.WPF.BuildMonitor";

      public MainWindow()
      {
         Dispatcher.Thread.CurrentUICulture = Dispatcher.Thread.CurrentCulture;

         InitializeComponent();
         TryCreateShortcut();

         if (!MonitorViewModel.MonitorSettings.UpgradeNeeded)
         {
            return;
         }

         MonitorViewModel.MonitorSettings.UpgradeNeeded = false;

         const int ActualColumnNumber = 9;
         var widths = MonitorViewModel.MonitorSettings.ColumnWidths.Split(',').ToList();
         while (widths.Count < ActualColumnNumber)
         {
            widths.Add("100:true");
         }

         MonitorViewModel.MonitorSettings.ColumnWidths = string.Join(",", widths);
         MonitorViewModel.SaveMonitorSettings(MonitorViewModel.MonitorSettings);
      }

      // In order to display toasts, a desktop application must have a shortcut on the Start menu.
      // Also, an AppUserModelID must be set on that shortcut.
      // The shortcut should be created as part of the installer. The following code shows how to create
      // a shortcut and assign an AppUserModelID using Windows APIs. You must download and include the 
      // Windows API Code Pack for Microsoft .NET Framework for this code to function
      //
      // Included in this project is a wxs file that be used with the WiX toolkit
      // to make an installer that creates the necessary shortcut. One or the other should be used.
      private bool TryCreateShortcut()
      {
         var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\WPF.BuildMonitor.lnk";
         if (!File.Exists(shortcutPath))
         {
            InstallShortcut(shortcutPath);
            return true;
         }

         return false;
      }

      private void InstallShortcut(String shortcutPath)
      {
         // Find the path to the current executable
         var exePath = Process.GetCurrentProcess().MainModule.FileName;
         var newShortcut = (IShellLinkW)new CShellLink();

         // Create a shortcut to the exe
         ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
         ErrorHelper.VerifySucceeded(newShortcut.SetArguments(string.Empty));

         // Open the shortcut property store, set the AppUserModelId property
         var newShortcutProperties = (IPropertyStore)newShortcut;

         using (var appId = new PropVariant(APP_ID))
         {
            ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
            ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
         }

         // Commit the shortcut to disk
         var newShortcutSave = (IPersistFile)newShortcut;

         ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
      }

      protected override void OnActivated(EventArgs e)
      {
         base.OnActivated(e);

         if (activated)
         {
            return;
         }

         if (MonitorViewModel.MonitorSettings.WindowLeft >= 0 && MonitorViewModel.MonitorSettings.WindowTop >= 0)
         {
            Top = MonitorViewModel.MonitorSettings.WindowTop;
            Left = MonitorViewModel.MonitorSettings.WindowLeft;
         }

         DataContext = new MainWindowViewModel();
         activated = true;
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         var viewModel = DataContext as MainWindowViewModel;

         foreach (var buildServer in ((MainWindowViewModel)DataContext).MonitorViewModel.BuildServers)
         {
            foreach (var buildDefinition in buildServer.BuildDefinitions)
            {
               var adapter = viewModel.BuildAdapters.FirstOrDefault(x => x.BuildInformation.BuildDefinitionId == buildDefinition.Id);
               if (adapter == null)
               {
                  continue;
               }

               buildDefinition.Tags = adapter.Tags.ToArray();
            }
         }

         MonitorViewModel.MonitorSettings.WindowTop = (int)Top;
         MonitorViewModel.MonitorSettings.WindowLeft = (int)Left;
         MonitorViewModel.MonitorSettings.BigSize = viewModel.BigSizeMode;
         MonitorViewModel.MonitorSettings.UseFullWidth = viewModel.UseFullWidth;
         MonitorViewModel.MonitorSettings.ZoomFactor = viewModel.ZoomFactor;
         MonitorViewModel.MonitorSettings.RefreshInterval = viewModel.Maximum;
         MonitorViewModel.MonitorSettings.RibbonMinimized = viewModel.IsRibbonMinimized;
         MonitorViewModel.SaveMonitorSettings(MonitorViewModel.MonitorSettings);

         while (viewModel.PinBuildViews.Any())
         {
            viewModel.PinBuildViews[0].Close();
         }

         base.OnClosing(e);
      }

      private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         var listView = sender as ListView;
         var viewModel = DataContext as MainWindowViewModel;
         if (listView == null || viewModel == null)
         {
            return;
         }

         viewModel.SelectedBuildAdapters.Clear();
         foreach (var adapter in listView.SelectedItems.OfType<BuildAdapter>())
         {
            viewModel.SelectedBuildAdapters.Add(adapter);
         }
      }
   }
}
