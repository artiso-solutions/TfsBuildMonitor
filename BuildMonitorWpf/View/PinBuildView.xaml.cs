using System.Windows.Input;

namespace BuildMonitorWpf.View
{
   using System.Linq;

   using BuildMonitorWpf.Properties;
   using BuildMonitorWpf.ViewModel;

   /// <summary>
   /// Interaction logic for PinBuildView.xaml
   /// </summary>
   public partial class PinBuildView
   {
      public PinBuildView()
      {
         InitializeComponent();
      }

      private void PinBuildView_OnMouseDown(object sender, MouseButtonEventArgs e)
      {
         if (e.ChangedButton == MouseButton.Left)
            DragMove();

         var pinBuildViewModel = DataContext as PinBuildViewModel;
         if (pinBuildViewModel == null)
         {
            return;
         }

         var buildDefinition = pinBuildViewModel.MainWindowViewModel.BuildServers.SelectMany(x => x.BuildDefinitions).FirstOrDefault(x => x.Id == pinBuildViewModel.BuildInformation.BuildDefinitionId);
         if (buildDefinition != null)
         {
            buildDefinition.PinLeft = (int)((PinBuildView)sender).Left;
            buildDefinition.PinTop = (int)((PinBuildView)sender).Top;
            Settings.Default.Save();
         }
      }
   }
}
