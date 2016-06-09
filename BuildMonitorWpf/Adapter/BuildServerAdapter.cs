namespace BuildMonitorWpf.Adapter
{
   using System;
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.ComponentModel;
   using System.Linq;
   using System.Runtime.CompilerServices;
   using System.Windows.Data;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;
   using BuildMonitor.Logic.Crypter;
   using BuildMonitor.Logic.Interfaces;

   using BuildMonitorWpf.Commands;

   /// <summary>The build server adapter.</summary>
   /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
   public class BuildServerAdapter : INotifyPropertyChanged
   {
      #region Constants and Fields

      /// <summary>All string</summary>
      internal const string AllString = "--- All ---";

      private readonly ICrypter crypter = new Crypter();

      private string connectionProgress;

      private string detailBuildUrl;

      private string domainName;

      private string filter;

      private int filteredCount;

      private string login;

      private bool onlySelected;

      private string passwordBytes;

      private string selectedProjectName;

      private string serverName;

      private TfsVersion tfsVersion;

      private int totalCount;

      private string url;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="BuildServerAdapter"/> class.</summary>
      /// <param name="buildServer">The build server.</param>
      /// <param name="removeServerCommand">The remove server command.</param>
      public BuildServerAdapter(BuildServer buildServer, ICommand removeServerCommand)
      {
         detailBuildUrl = buildServer.DetailBuildUrl;
         domainName = buildServer.DomainName;
         login = buildServer.Login;
         passwordBytes = buildServer.PasswordBytes;
         serverName = buildServer.ServerName;
         tfsVersion = buildServer.TfsVersion;
         url = buildServer.Url;
         BuildDefinitions = new List<BuildDefinition>(buildServer.BuildDefinitions);

         TfsConnectCommand = new TfsConnectCommand(this, crypter);
         RemoveServerCommand = removeServerCommand;

         ProjectNames = new ObservableCollection<string>();

         BuildDefinitionResults = new ObservableCollection<BuildDefinitionResult>();
         CollectionViewSourceBuildDefinitions = new CollectionViewSource { Source = BuildDefinitionResults };
         CollectionViewSourceBuildDefinitions.Filter += CollectionViewSourceBuildDefinitions_Filter;

         if (buildServer.BuildDefinitions.Any())
         {
            OnlySelected = true;
         }

         if (TfsConnectCommand.CanExecute(null))
         {
            TfsConnectCommand.Execute(null);
         }
      }

      #endregion

      #region INotifyPropertyChanged Members

      /// <summary>Occurs when a property value changes.</summary>
      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      #region Public Properties

      /// <summary>Gets or sets the connection progress.</summary>
      public string ConnectionProgress
      {
         get
         {
            return connectionProgress;
         }

         set
         {
            if (connectionProgress == value)
            {
               return;
            }

            connectionProgress = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the crypted password.</summary>
      public string CryptedPassword
      {
         get
         {
            return passwordBytes;
         }

         set
         {
            passwordBytes = value;
         }
      }

      /// <summary>Gets or sets the detail build URL.</summary>
      public string DetailBuildUrl
      {
         get
         {
            return detailBuildUrl;
         }

         set
         {
            if (detailBuildUrl == value)
            {
               return;
            }

            detailBuildUrl = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the domain.</summary>
      public string Domain
      {
         get
         {
            return domainName;
         }

         set
         {
            if (domainName == value)
            {
               return;
            }

            domainName = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the filter.</summary>
      public string Filter
      {
         get
         {
            return filter;
         }

         set
         {
            if (filter == value)
            {
               return;
            }

            filter = value;
            OnFilterChanged();
         }
      }

      /// <summary>Gets the filtered build definition results.</summary>
      public ICollectionView FilteredBuildDefinitionResults
      {
         get
         {
            return CollectionViewSourceBuildDefinitions.View;
         }
      }

      /// <summary>Gets or sets the filtered count.</summary>
      public int FilteredCount
      {
         get
         {
            return filteredCount;
         }

         set
         {
            if (filteredCount == value)
            {
               return;
            }

            filteredCount = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the login.</summary>
      public string Login
      {
         get
         {
            return login;
         }

         set
         {
            if (login == value)
            {
               return;
            }

            login = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets a value indicating whether [only selected].</summary>
      public bool OnlySelected
      {
         get
         {
            return onlySelected;
         }

         set
         {
            if (onlySelected == value)
            {
               return;
            }

            onlySelected = value;
            OnFilterChanged();
         }
      }

      /// <summary>Gets the project names.</summary>
      public ObservableCollection<string> ProjectNames { get; private set; }

      /// <summary>Gets the remove server command.</summary>
      public ICommand RemoveServerCommand { get; private set; }

      /// <summary>Gets or sets the name of the selected project.</summary>
      public string SelectedProjectName
      {
         get
         {
            return selectedProjectName;
         }

         set
         {
            if (selectedProjectName == value)
            {
               return;
            }

            selectedProjectName = value;
            OnPropertyChanged();
            OnFilterChanged();
         }
      }

      /// <summary>Gets or sets the name of the server.</summary>
      public string ServerName
      {
         get
         {
            return serverName;
         }

         set
         {
            if (serverName == value)
            {
               return;
            }

            serverName = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the TFS connect command.</summary>
      public ICommand TfsConnectCommand { get; private set; }

      /// <summary>Gets or sets the TFS URL.</summary>
      public string TfsUrl
      {
         get
         {
            return url;
         }

         set
         {
            if (url == value)
            {
               return;
            }

            url = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the TFS version.</summary>
      public TfsVersion TfsVersion
      {
         get
         {
            return tfsVersion;
         }

         set
         {
            if (tfsVersion == value)
            {
               return;
            }

            tfsVersion = value;
            OnPropertyChanged();
            OnTfsVersionChanged();
         }
      }

      /// <summary>Gets or sets the total count.</summary>
      public int TotalCount
      {
         get
         {
            return totalCount;
         }

         set
         {
            if (totalCount == value)
            {
               return;
            }

            totalCount = value;
            OnPropertyChanged();
         }
      }

      #endregion

      #region Properties

      /// <summary>Gets the build definition results.</summary>
      internal ObservableCollection<BuildDefinitionResult> BuildDefinitionResults { get; private set; }

      /// <summary>Gets the build definitions.</summary>
      internal IList<BuildDefinition> BuildDefinitions { get; private set; }

      /// <summary>Gets the collection view source build definitions.</summary>
      internal CollectionViewSource CollectionViewSourceBuildDefinitions { get; private set; }

      #endregion

      #region Methods

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         var handler = PropertyChanged;
         if (handler != null)
            handler(this, new PropertyChangedEventArgs(propertyName));
      }

      private void CollectionViewSourceBuildDefinitions_Filter(object sender, FilterEventArgs e)
      {
         if (SelectedProjectName == AllString && string.IsNullOrEmpty(Filter) && !OnlySelected)
         {
            e.Accepted = true;
            return;
         }

         var buildDefinitionResult = e.Item as BuildDefinitionResult;
         if (buildDefinitionResult == null)
         {
            e.Accepted = false;
            return;
         }

         var selected = true;
         if (OnlySelected)
         {
            selected = buildDefinitionResult.Selected;
         }

         var filtered = true;
         if (!string.IsNullOrEmpty(Filter))
         {
            filtered = buildDefinitionResult.Name.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
         }

         var projectName = true;
         if (!string.Equals(SelectedProjectName, AllString) && !string.IsNullOrEmpty(SelectedProjectName))
         {
            projectName = string.Equals(buildDefinitionResult.ProjectName, SelectedProjectName);
         }

         e.Accepted = selected && filtered && projectName;
      }

      private void CountItems()
      {
         TotalCount = CollectionViewSourceBuildDefinitions.View.SourceCollection.OfType<BuildDefinitionResult>().Count();

         FilteredCount = CollectionViewSourceBuildDefinitions.View.OfType<BuildDefinitionResult>().Count();
      }

      private void OnFilterChanged()
      {
         CollectionViewSourceBuildDefinitions.View.Refresh();
         CountItems();
      }

      private void OnTfsVersionChanged()
      {
         switch (TfsVersion)
         {
            case TfsVersion.Version2013:
               TfsUrl = @"http://tfs:8080/tfs/Collection/_apis/build/Definitions";
               DetailBuildUrl = "http://tfs:8080/tfs/Collection/Project/_build#_a=summary&buildUri=";
               break;

            case TfsVersion.Version2015:
               TfsUrl = "http://tfs:8080/tfs/Collection";
               DetailBuildUrl = string.Empty;
               break;
         }
      }

      #endregion
   }
}