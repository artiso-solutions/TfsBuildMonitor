
namespace BuildMonitor.Logic.Crypter
{
   using System;
   using System.Security.Cryptography;
   using System.Text;

   using BuildMonitor.Logic.Interfaces;

   public class Crypter : ICrypter
   {
      private const string Key = "aAzZeErRtTyYuUiIoOpPqQsS";

      /// <summary>Encrypts the specified <paramref name="input" />.</summary>
      /// <param name="input">The input.</param>
      /// <returns>An encrypted string representing the input.</returns>
      public string Encrypt(string input)
      {
         using (
            var cryptoServiceProvider = new TripleDESCryptoServiceProvider { Key = Encoding.UTF8.GetBytes(Key), Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
         {
            using (var encryptor = cryptoServiceProvider.CreateEncryptor())
            {
               var inputBuffer = Encoding.UTF8.GetBytes(input);
               var crypted = encryptor.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);

               return BitConverter.ToString(crypted).Replace("-", string.Empty);
            }
         }
      }

      /// <summary>Decrypts the specified <paramref name="input" />.</summary>
      /// <param name="input">The input.</param>
      /// <returns>A decrypted string.</returns>
      public string Decrypt(string input)
      {
         if (string.IsNullOrEmpty(input))
         {
            return string.Empty;
         }

         using (
            var cryptoServiceProvider = new TripleDESCryptoServiceProvider { Key = Encoding.UTF8.GetBytes(Key), Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
         {
            using (var decryptor = cryptoServiceProvider.CreateDecryptor())
            {
               var inputBuffer = StringToByteArray(input);

               var decrypted = decryptor.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
               return Encoding.UTF8.GetString(decrypted);
            }
         }
      }

      private static byte[] StringToByteArray(string hex)
      {
         var numberChars = hex.Length;
         var bytes = new byte[numberChars / 2];
         for (var i = 0; i < numberChars; i += 2)
         {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
         }

         return bytes;
      }
   }
}
