namespace Frontend.Model
{
    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public Dictionary<string, string> Claims { get; set; } = [];
    }
}
