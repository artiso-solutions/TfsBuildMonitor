
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Windows.Input;

   public class RelayCommand : ICommand
   {
      #region Constants and Fields

      private readonly Action<object> executeAction;

      #endregion

      #region Constructors and Destructors

      public RelayCommand(Action<object> executeAction)
      {
         this.executeAction = executeAction;
      }

      #endregion

      #region Public Events

      /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
      public event EventHandler CanExecuteChanged;

      #endregion

      #region ICommand Members

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
         executeAction(parameter);
      }

      #endregion
   }
}