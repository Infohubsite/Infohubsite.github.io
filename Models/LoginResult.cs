namespace Frontend.Models
{
    public class LoginResult
    {
        public bool Succeeded { get; set; }
        public string[] ErrorList { get; set; } = [];
    }
}
