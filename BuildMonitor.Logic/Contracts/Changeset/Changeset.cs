public class Changeset
{
   #region Public Properties

   public Links _links { get; set; }

   public Author author { get; set; }

   public int changesetId { get; set; }

   public CheckedInBy checkedInBy { get; set; }

   public string comment { get; set; }

   public string createdDate { get; set; }

   public string url { get; set; }

   #endregion
}