
namespace BuildMonitorWpf.ViewModel
{
   using System;
   using System.ComponentModel;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.Commands;
   using BuildMonitorWpf.View;

   public class PinBuildViewModel : BuildAdapter
   {
      private int maximum;

      private int actualValue;

      public PinBuildViewModel(PinBuildView owner, MainWindowViewModel mainWindowViewModel, BuildInformation buildInformation)
         : base(mainWindowViewModel, buildInformation, true)
      {
         MainWindowViewModel = mainWindowViewModel;
         Maximum = ActualValue = mainWindowViewModel.Maximum;

         CloseCommand = new ClosePinBuildCommand(owner, buildInformation);

         Refresh();

         mainWindowViewModel.PropertyChanged += MainWindowViewModelOnPropertyChanged;
         mainWindowViewModel.Refreshing += MainWindowViewModelRefreshing;
      }

      public MainWindowViewModel MainWindowViewModel { get; }

      public ICommand CloseCommand { get; private set; }

      public int ActualValue
      {
         get
         {
            return actualValue;
         }

         set
         {
            if (actualValue == value)
            {
               return;
            }

            actualValue = value;
            OnPropertyChanged();
         }
      }

      public int Maximum
      {
         get
         {
            return maximum;
         }

         set
         {
            if (value == maximum)
            {
               return;
            }

            maximum = value;
            OnPropertyChanged();
         }
      }

      private void MainWindowViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         ActualValue = ((MainWindowViewModel)sender).ActualValue;
         Maximum = ((MainWindowViewModel)sender).Maximum;
      }

      private void MainWindowViewModelRefreshing(object sender, EventArgs e)
      {
         Refresh();
      }
   }
}
