public class Self
{
   #region Public Properties

   public string href { get; set; }

   #endregion
}

public class Changes
{
   #region Public Properties

   public string href { get; set; }

   #endregion
}

public class WorkItems
{
   #region Public Properties

   public string href { get; set; }

   #endregion
}

public class Web
{
   #region Public Properties

   public string href { get; set; }

   #endregion
}

public class Author2
{
   #region Public Properties

   public string href { get; set; }

   #endregion
}

public class CheckedInBy2
{
   #region Public Properties

   public string href { get; set; }

   #endregion
}

public class Links
{
   #region Public Properties

   public Author2 author { get; set; }

   public Changes changes { get; set; }

   public CheckedInBy2 checkedInBy { get; set; }

   public Self self { get; set; }

   public Web web { get; set; }

   public WorkItems workItems { get; set; }

   #endregion
}