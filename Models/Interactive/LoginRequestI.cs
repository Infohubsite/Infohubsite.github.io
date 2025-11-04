using Shared.DTO.Client;
using Shared.Interface;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Interactive
{
    public record LoginRequestI : IConverterTo<LoginRequestI, LoginRequestDto>
    {
        [Required(ErrorMessage = "Username is required")] public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")] public string Password { get; set; } = string.Empty;

        public static LoginRequestDto Convert(LoginRequestI from) => new()
        {
            Username = from.Username,
            Password = from.Password
        };
    }
}
