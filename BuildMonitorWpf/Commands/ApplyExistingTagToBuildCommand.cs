
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.ComponentModel;
   using System.Linq;
   using System.Windows.Input;

   using BuildMonitorWpf.ViewModel;

   public class ApplyExistingTagToBuildCommand : ICommand
   {
      #region Constants and Fields

      private readonly MainWindowViewModel viewModel;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="ApplyExistingTagToBuildCommand"/> class.</summary>
      /// <param name="viewModel">The view model.</param>
      public ApplyExistingTagToBuildCommand(MainWindowViewModel viewModel)
      {
         this.viewModel = viewModel;
         viewModel.PropertyChanged += ViewModelPropertyChanged;
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
         return viewModel.SelectedBuildAdapter != null && viewModel.SelectedExistingTag!=null;
      }

      /// <summary>Defines the method to be called when the command is invoked.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public void Execute(object parameter)
      {
         if (viewModel.SelectedBuildAdapter.Tags.Any(x => string.Equals(x, viewModel.SelectedExistingTag.Label, StringComparison.InvariantCultureIgnoreCase)))
         {
            return;
         }

         viewModel.SelectedBuildAdapter.Tags.Add(viewModel.SelectedExistingTag.Label);
      }

      #endregion

      #region Methods

      private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "SelectedBuildAdapter" || e.PropertyName == "SelectedExistingTag")
         {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
         }
      }

      #endregion
   }
}