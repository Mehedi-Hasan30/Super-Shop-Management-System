using System;

namespace Super_Shop_Management_System.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Which algorithm the stored Password hash uses: "SHA256" (legacy)
        /// or "BCrypt" (current). Drives backward-compatible verification
        /// and transparent migration in AuthService.
        /// </summary>
        public string PasswordAlgorithm { get; set; } = "SHA256";
    }
}