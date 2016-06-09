namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Windows;
   using System.Windows.Input;

   using BuildMonitorWpf.View;
   using BuildMonitorWpf.ViewModel;

   /// <summary>The about comment.</summary>
   /// <seealso cref="System.Windows.Input.ICommand"/>
   public class AboutCommand : ICommand
   {
      #region ICommand Members

      /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
      public event EventHandler CanExecuteChanged;

      /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      /// <returns>true if this command can be executed; otherwise, false.</returns>
      public bool CanExecute(object parameter)
      {
         return true;
      }

      /// <summary>Defines the method to be called when the command is invoked.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public void Execute(object parameter)
      {
         var aboutDialog = new AboutView { Owner = Application.Current.MainWindow };
         var aboutViewModel = new AboutViewModel(aboutDialog);
         aboutViewModel.FillReleaseNotes();
         aboutDialog.DataContext = aboutViewModel;
         aboutDialog.ShowDialog();
      }

      #endregion
   }
}