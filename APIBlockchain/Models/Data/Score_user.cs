namespace WebApplication3.Models.Data
{
    public class Score_user
    {
        public int? ID { get; set; }
        public int? Id_user { get; set; }
        public int? Id_subject { get; set; }
        public string? score { get; set; } = string.Empty;
        public string  user_name { get; set; } = string.Empty;
    }
}
