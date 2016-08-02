namespace BuildMonitor.Logic.Contracts
{
   public class MonitorSettings
   {
      public int RefreshInterval { get; set; }

      public string ColumnWidths { get; set; }

      public int WindowTop { get; set; }

      public int WindowLeft { get; set; }

      public bool UpgradeNeeded { get; set; }

      public bool BigSize { get; set; }

      public double ZoomFactor { get; set; }

      public bool UseFullWidth { get; set; }

      public bool RibbonMinimized { get; set; }
   }
}