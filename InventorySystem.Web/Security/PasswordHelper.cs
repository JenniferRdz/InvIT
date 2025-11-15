using System.Security.Cryptography;

namespace InventorySystem.Web.Security
{
    public static class PasswordHelper
    {
        public const int Iterations = 100_000;
        public const int SaltSize = 32;
        public const int HashSize = 32;

        public static (byte[] hash, byte[] salt) Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);
            return (hash, salt);
        }

        public static bool Verify(string password, byte[] salt, byte[] expectedHash)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var actual = pbkdf2.GetBytes(HashSize);
            return CryptographicOperations.FixedTimeEquals(actual, expectedHash);
        }
    }
}
