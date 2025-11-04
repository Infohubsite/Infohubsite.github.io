using Shared.DTO.Server;
using Shared.Interface;
using System.Diagnostics.CodeAnalysis;

namespace Frontend.Models.Converted
{
    public record LoginResponse : IConverterBi<LoginResponse, LoginResponseDto>
    {
        public required string Token { get; set; }

        [SetsRequiredMembers]
        public LoginResponse(LoginResponseDto dto)
        {
            Token = dto.Token;
        }

        public static LoginResponse Convert(LoginResponseDto from) => new(from);
        public static LoginResponseDto Convert(LoginResponse from) => new()
        {
            Token = from.Token
        };
    }
}
