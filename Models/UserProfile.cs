namespace Frontend.Models
{
    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public Dictionary<string, string> Claims { get; set; } = [];
    }
}
