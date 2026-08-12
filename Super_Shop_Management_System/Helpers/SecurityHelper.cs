using System;
using System.Security.Cryptography;
using System.Text;

namespace Super_Shop_Management_System.Helpers
{
    public static class SecurityHelper
    {
        public const string AlgorithmSha256 = "SHA256";
        public const string AlgorithmBCrypt = "BCrypt";

        public static readonly string CurrentAlgorithm = AlgorithmBCrypt;

        public static string HashValue(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();

                foreach (byte item in hashBytes)
                {
                    builder.Append(item.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public static string HashPasswordForStorage(string plainTextPassword)
        {
            if (plainTextPassword == null) plainTextPassword = string.Empty;
            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, workFactor: 12);
        }

        public static bool VerifyPassword(string plainTextPassword, string storedHash, string algorithm)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;
            plainTextPassword = plainTextPassword ?? string.Empty;

            if (string.Equals(algorithm, AlgorithmBCrypt, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(plainTextPassword, storedHash);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    return false;
                }
            }

            string computedSha256 = HashValue(plainTextPassword);
            return string.Equals(computedSha256, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        public static bool NeedsMigration(string algorithm)
        {
            return !string.Equals(algorithm, AlgorithmBCrypt, StringComparison.OrdinalIgnoreCase);
        }
    }
}