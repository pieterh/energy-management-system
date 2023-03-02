using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.BlazorWasm.Client.Services.Auth
{
    public record Response
    {
        public int Status { get; set; } = 204;
        public string StatusText { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public record LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public record UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public record LoginResponse : Response
    {
        public string Token { get; set; } = string.Empty;
        public UserModel? User { get; set; }

        internal void Deconstruct(out UserModel? user, out string token)
        {
            user = User;
            token = Token;
        }
    }

    public record PingResponse : Response
    {
        public UserModel? User { get; set; }
    }

    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(LoginModel model);
        void LogoutAsync();
        Task<PingResponse> Ping();
    }
}