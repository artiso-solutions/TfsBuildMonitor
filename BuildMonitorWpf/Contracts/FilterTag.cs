namespace BuildMonitorWpf.Contracts
{
   using System.ComponentModel;
   using System.Runtime.CompilerServices;

   /// <summary>The filter tag.</summary>
   public class FilterTag : INotifyPropertyChanged
   {
      #region Constants and Fields

      private bool isSelected;

      #endregion

      #region Public Events

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      #region Public Properties

      /// <summary>Gets or sets a value indicating whether this filter is all filter.</summary>
      public bool IsAllFilter { get; set; }

      /// <summary>Gets or sets a value indicating whether this filter is selected.</summary>
      public bool IsSelected
      {
         get
         {
            return isSelected;
         }
         set
         {
            if (isSelected == value)
            {
               return;
            }

            isSelected = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the label.</summary>
      public string Label { get; set; }

      #endregion

      #region Methods

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      #endregion
   }
}