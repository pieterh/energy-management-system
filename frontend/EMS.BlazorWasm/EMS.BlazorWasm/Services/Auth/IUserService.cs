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
        public bool NeedPasswordChange { get; set; }
    }

    public record SetPasswordModel
    {
        public required string OldPassword { get; init; }
        public required string NewPassword { get; init; }
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

    public record SetPasswordResponse : Response
    {
        public UserModel? User { get; set; }
    }

    public record PingResponse : Response
    {
        public UserModel? User { get; set; }
    }

    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(LoginModel model);
        Task<LoginResponse> LoginAsync(LoginModel model, CancellationToken cancellationToken);
        void LogoutAsync();
        Task<SetPasswordResponse> SetPasswordAsync(SetPasswordModel model, CancellationToken cancellationToken);
        Task<PingResponse> Ping();
    }
}