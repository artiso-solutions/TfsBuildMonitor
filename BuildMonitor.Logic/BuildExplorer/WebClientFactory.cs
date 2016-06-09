
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BuildMonitor.Logic.BuildExplorer
{
   using System.Net.Http;

   using BuildMonitor.Logic.Crypter;
   using BuildMonitor.Logic.Interfaces;

   /// <summary>The web client factory.</summary>
   public static class WebClientFactory
   {
      private static readonly ICrypter Crypter = new Crypter();

      /// <summary>Creates the HTTP client.</summary>
      /// <param name="domain">The domain.</param>
      /// <param name="login">The login.</param>
      /// <param name="cryptedPassword">The crypted password.</param>
      /// <returns>The <see cref="HttpClient"/>.</returns>
      public static HttpClient CreateHttpClient(string domain, string login, string cryptedPassword)
      {
         var handler = new HttpClientHandler();
         // on premise TFS authentication
         if (!string.IsNullOrEmpty(login))
         {
            handler.Credentials = new NetworkCredential(login, Crypter.Decrypt(cryptedPassword), domain);
         }

         var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };

         // VSTS authentication with personal access token
         if (string.IsNullOrEmpty(login))
         {
            var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(":{0}", Crypter.Decrypt(cryptedPassword))));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
         }

         return client;
      }
   }
}
