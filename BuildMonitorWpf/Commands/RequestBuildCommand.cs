namespace BuildMonitorWpf.Commands
{
   using System;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.Windows;
   using System.Windows.Input;

   using BuildMonitor.Logic.BuildExplorer;
   using BuildMonitor.Logic.Contracts;
   using BuildMonitor.Logic.Interfaces;

   using BuildMonitorWpf.Adapter;

   /// <summary>The request new build command.</summary>
   /// <seealso cref="System.Windows.Input.ICommand"/>
   public class RequestBuildCommand : ICommand
   {
      #region Constants and Fields

      private readonly BuildAdapter buildAdapter;

      private readonly IBuildExplorer buildExplorer;

      private readonly BuildInformation buildInformation;

      private bool isRequesting;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="RequestBuildCommand"/> class.</summary>
      /// <param name="buildAdapter">The build adapter.</param>
      /// <param name="buildInformation">The build information.</param>
      public RequestBuildCommand(BuildAdapter buildAdapter, BuildInformation buildInformation)
      {
         buildExplorer = BuildExplorerFactory.CreateBuildExplorer(buildInformation.TfsVersion);
         buildAdapter.PropertyChanged += buildAdapter_PropertyChanged;

         this.buildInformation = buildInformation;
         this.buildAdapter = buildAdapter;
      }

      #endregion

      #region ICommand Members

      /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      /// <returns>true if this command can be executed; otherwise, false.</returns>
      public bool CanExecute(object parameter)
      {
         if (isRequesting)
         {
            return false;
         }

         if (buildAdapter.Status == BuildStatus.Error || buildAdapter.Status == BuildStatus.Unknown || buildAdapter.Status == BuildStatus.Waiting)
         {
            return false;
         }

         return true;
      }

      /// <summary>Defines the method to be called when the command is invoked.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public async void Execute(object parameter)
      {
         var question = string.Format("Are you sure you want to request a new build '{0}'?", buildAdapter.Name);
         var answer = MessageBox.Show(question, "WPF.BuildMonitor", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
         if (answer == MessageBoxResult.Cancel)
         {
            return;
         }

         isRequesting = true;
         OnCanExecuteChanged();

         var buildResult = await buildExplorer.RequestBuild(buildInformation);
         var tfsUri = string.Concat(buildInformation.BuildDetailUrl, buildResult.TfsUri);
         ToastNotifications.CreateToastNotification(buildResult, false, (o, e) => Process.Start(tfsUri));

         isRequesting = false;
         OnCanExecuteChanged();
      }

      /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
      public event EventHandler CanExecuteChanged;

      #endregion

      #region Methods

      private void OnCanExecuteChanged()
      {
         var handler = CanExecuteChanged;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }

      private void buildAdapter_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "Status")
         {
            OnCanExecuteChanged();
         }
      }

      #endregion
   }
}