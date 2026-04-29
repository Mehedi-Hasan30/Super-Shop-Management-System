namespace Super_Shop_Management_System.Helpers
{
    public static class SessionManager
    {
        public static int UserID { get; private set; }
        public static string Username { get; private set; }
        public static string FullName { get; private set; }
        public static string Role { get; private set; }

        public static bool IsLoggedIn => UserID > 0;

        public static void StartSession(int userId, string username, string fullName, string role)
        {
            UserID = userId;
            Username = username;
            FullName = fullName;
            Role = role;
        }

        public static void EndSession()
        {
            UserID = 0;
            Username = string.Empty;
            FullName = string.Empty;
            Role = string.Empty;
        }
    }
}
