
namespace BuildMonitorWpf.Test.ToastNotificationsTests
{
   using System;
   using System.Threading;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;

   using Microsoft.VisualStudio.TestTools.UnitTesting;

   [TestClass]
   public class CreateToastNotificationTests
   {
      [TestMethod]
      public void CreateToastNotificationErrorDesignTest()
      {
         var result = new BuildResult { Name = "Product-Main", Status = BuildStatus.Failed, RequestedBy = "Some user" };
         ToastNotifications.CreateToastNotification(result, false, null);

         Thread.Sleep(TimeSpan.FromSeconds(5));
      }
   }
}
