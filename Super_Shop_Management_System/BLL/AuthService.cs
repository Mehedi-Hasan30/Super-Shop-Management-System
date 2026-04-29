using System;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class AuthService
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private readonly AuditLogService _auditLogService = new AuditLogService();

        public User Login(string username, string password)
        {
            if (ValidationHelper.IsNullOrWhiteSpace(username) || ValidationHelper.IsNullOrWhiteSpace(password))
            {
                throw new ApplicationException("Username and password are required.");
            }

            string normalizedUsername = username.Trim();
            string passwordHash = SecurityHelper.HashValue(password);
            User user = _userRepository.Authenticate(normalizedUsername, passwordHash);

            if (user == null)
            {
                _auditLogService.Log("LOGIN_FAIL", "Users", normalizedUsername, normalizedUsername, "Failed login attempt.");
                throw new ApplicationException("Invalid username or password.");
            }

            SessionManager.StartSession(user.UserID, user.Username, user.FullName, user.Role);
            _auditLogService.Log("LOGIN_SUCCESS", "Users", user.UserID.ToString(), user.Username, "Successful login.");
            return user;
        }

        public User FindUserForPasswordReset(string username, string email)
        {
            if (ValidationHelper.IsNullOrWhiteSpace(username) || ValidationHelper.IsNullOrWhiteSpace(email))
            {
                throw new ApplicationException("Username and email are required.");
            }

            User user = _userRepository.GetByUsernameAndEmail(username.Trim(), email.Trim());
            if (user == null)
            {
                throw new ApplicationException("No user found with provided credentials.");
            }

            return user;
        }

        public bool ResetPassword(int userId, string securityAnswer, string newPassword, string expectedHashedAnswer)
        {
            if (ValidationHelper.IsNullOrWhiteSpace(securityAnswer) || ValidationHelper.IsNullOrWhiteSpace(newPassword))
            {
                throw new ApplicationException("Security answer and new password are required.");
            }

            string providedAnswerHash = SecurityHelper.HashValue(securityAnswer.Trim().ToLowerInvariant());
            if (!string.Equals(providedAnswerHash, expectedHashedAnswer, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException("Security answer is incorrect.");
            }

            bool success = _userRepository.UpdatePassword(userId, SecurityHelper.HashValue(newPassword));
            if (success)
            {
                _auditLogService.Log("UPDATE", "Users", userId.ToString(), SessionManager.Username, "Password reset executed.");
            }
            return success;
        }

        public void Logout()
        {
            _auditLogService.Log("LOGOUT", "Users", SessionManager.UserID.ToString(), SessionManager.Username, "User logged out.");
            SessionManager.EndSession();
        }
    }
}
