namespace WebApplication3.Models.Data
{
    public class User
    {
        public int? ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? Permisson { get; set; }
        public string StudentId { get; set; } = string.Empty;

    }
}
