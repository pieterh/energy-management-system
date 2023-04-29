using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Library.Shared.DTO;
using EMS.Library.Shared.DTO.Users;

namespace EMS.BlazorWasm.Client.Services.Auth
{
    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(LoginModel model);
        Task<LoginResponse> LoginAsync(LoginModel model, CancellationToken cancellationToken);
        void LogoutAsync();
        Task<SetPasswordResponse> SetPasswordAsync(SetPasswordModel model, CancellationToken cancellationToken);
        Task<PingResponse> Ping();
    }
}