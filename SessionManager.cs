using ChicoDesktopApp.Models;

namespace ChicoDesktopApp
{
    public static class SessionManager
    {
        private static User? _currentUser;

        public static User? CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        public static bool IsAuthenticated => _currentUser != null;

        public static bool IsAdmin => _currentUser?.Role == UserRole.Admin;

        public static bool IsCashier => _currentUser?.Role == UserRole.Cashier;

        public static void Logout()
        {
            _currentUser = null;
        }

        public static string GetCurrentUserDisplay()
        {
            return _currentUser?.DisplayName ?? "غير مسجل";
        }

        public static bool HasPermission(string requiredRole)
        {
            if (!IsAuthenticated) return false;
            
            if (requiredRole == "Admin")
            {
                return IsAdmin;
            }
            
            return true; // Both roles can access non-admin features
        }
    }
}
