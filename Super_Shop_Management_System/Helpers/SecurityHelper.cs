using System;
using System.Security.Cryptography;
using System.Text;

namespace Super_Shop_Management_System.Helpers
{
    public static class SecurityHelper
    {
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
    }
}
