namespace ChicoDesktopApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? FullNameArabic { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // Display properties
        public string DisplayName => FullNameArabic ?? FullName;
        public string RoleDisplay => Role == UserRole.Admin ? "مدير" : "أمين الصندوق";
    }

    public enum UserRole
    {
        Admin,
        Cashier
    }
}
