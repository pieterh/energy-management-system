using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EMS.WebHost.Helpers;
using Microsoft.AspNetCore.Hosting;

namespace EMS.WebHost.Controllers
{
    public class Response
    {
        public required int Status { get; set; }
        public required string StatusText { get; init; }
        public string? Message { get; init; }
    }

    public class LoginResponse : Response
    {
        public required string Token { get; init; }
        public required UserModel User { get; init; }
    }

    public class PingResponse : Response
    {
        public required UserModel User { get; init; }
    }

    public class LoginModel
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }

    public class UserModel
    {
        public Guid Id { get; set; }
        public required string Username { get; init; }
        public required string Name { get; init; }
    }   

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private ILogger Logger { get; init; }
        private readonly IJwtService _jwtService;

        public UsersController(ILogger<UsersController> logger, IJwtService jwtService)
        {
            Logger = logger;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        [Consumes("application/json")]
        [Produces("application/json")]

        public IActionResult Authenticate([FromBody] LoginModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            var user = PerformAuth(model);
            if (user != null)
            {
                var token = _jwtService.Generate(user.Id, user.Username, user.Name);

                var result = new LoginResponse
                {
                    Status = 200,
                    StatusText = "OK",
                    Token = token,
                    User = user
                };
                return new JsonResult(result);
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {


            var id = User.Claims.First<Claim>((x) => x.Type == JwtRegisteredClaimNames.Sub).Value;
            var name = User.Claims.First<Claim>((x) => x.Type == "name").Value;
            foreach (var c in User.Claims)
            {
                Logger.LogDebug("{Type} - {Value}", c.Type, c.Value);
            }
            var user = new UserModel
            {
                Id = Guid.Parse(id),
                Username = "admin",
                Name = name
            };

            return new JsonResult(new PingResponse() { Status = 200, StatusText = "OK", User = user });
        }

        private static UserModel? PerformAuth(LoginModel model)
        {
            var user = (model.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) &&
                    model.Password.Equals("admin", StringComparison.Ordinal)) ? new UserModel
                    {
                        Id = Guid.NewGuid(),
                        Username = "admin",
                        Name = "Pieter Hilkemeijer"
                    } : null;
            return user;
        }
    }
}
