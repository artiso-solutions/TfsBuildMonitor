
namespace BuildMonitor.Logic.Interfaces
{
   /// <summary>Defines an interface to encrypt and decrypt a password.</summary>
   public interface ICrypter
   {
      /// <summary>Encrypts the specified <paramref name="input"/>.</summary>
      /// <param name="input">The input.</param>
      /// <returns>An encrypted string representing the input.</returns>
      string Encrypt(string input);

      /// <summary>Decrypts the specified <paramref name="input"/>.</summary>
      /// <param name="input">The input.</param>
      /// <returns>A decrypted string.</returns>
      string Decrypt(string input);
   }
}
