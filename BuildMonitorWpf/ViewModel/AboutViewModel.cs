namespace BuildMonitorWpf.ViewModel
{
   using System.Collections.ObjectModel;
   using System.Diagnostics;
   using System.Windows.Input;
   using System.Xml;

   using BuildMonitorWpf.Commands;
   using BuildMonitorWpf.Contracts;
   using BuildMonitorWpf.Properties;
   using BuildMonitorWpf.View;

   /// <summary>The about view model.</summary>
   public class AboutViewModel
   {
      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="AboutViewModel"/> class.</summary>
      /// <param name="owner">The owner.</param>
      public AboutViewModel(AboutView owner)
      {
         CloseCommand = new CloseCommand(owner);
         SendMailCommand = new RelayCommand(o => Process.Start("mailto:ctissot@artiso.com;vgibilmanno@artiso.com;mrink@artiso.com"));
         OpenGitHubCommand = new RelayCommand(o => Process.Start("https://github.com/artiso-solutions/TfsBuildMonitor"));

         var assemblyName = typeof(AboutViewModel).Assembly.GetName();

         Version = assemblyName.Version.ToString();
         Product = assemblyName.Name;
         ReleaseNotes = new ObservableCollection<ReleaseNote>();
      }

      #endregion

      #region Public Properties

      /// <summary>Gets the close command.</summary>
      public ICommand CloseCommand { get; private set; }

      /// <summary>Gets the open git hub command.</summary>
      public ICommand OpenGitHubCommand { get; private set; }

      /// <summary>Gets or sets the product.</summary>
      public string Product { get; set; }

      /// <summary>Gets the release notes.</summary>
      public ObservableCollection<ReleaseNote> ReleaseNotes { get; private set; }

      /// <summary>Gets the send mail command.</summary>
      public ICommand SendMailCommand { get; private set; }

      /// <summary>Gets or sets the version.</summary>
      public string Version { get; set; }

      #endregion

      #region Methods

      /// <summary>Fills the release notes.</summary>
      internal void FillReleaseNotes()
      {
         ReleaseNotes.Clear();

         var releaseNotesXmlContent = Resources.ReleaseNotes;
         var xmlDocument = new XmlDocument();
         xmlDocument.LoadXml(releaseNotesXmlContent);

         foreach (XmlElement noteNode in xmlDocument.DocumentElement.GetElementsByTagName("Note"))
         {
            var note = new ReleaseNote { Version = noteNode.GetAttribute("Version"), Description = CleanWhiteSpace(noteNode.FirstChild.InnerText) };
            ReleaseNotes.Add(note);
         }
      }

      private string CleanWhiteSpace(string input)
      {
         var trimed = input.Trim();
         while (trimed.IndexOf("  ") >= 0)
         {
            trimed = trimed.Replace("  ", " ");
         }

         return trimed.Replace(" -", "-");
      }

      #endregion
   }
}