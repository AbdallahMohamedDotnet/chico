namespace ChicoDesktopApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameArabic { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
