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

namespace EMS.WebHost.Controllers
{
    public class Response
    {
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string Message { get; set; }
    }

    public class LoginResponse : Response
    {
        public string Token { get; set; }
        public UserModel User { get; set; }
    }
    public class PingResponse : Response
    {
        public UserModel User { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
    }
     
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private ILogger Logger { get; init; }
        private readonly IJWTService _jwtService;

        public UsersController(ILogger<UsersController> logger, IJWTService jwtService)
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
            var user = PerformAuth(model);
            if (user != null)
            {
                var token = _jwtService.Generate(user.Id, user.Username, user.Name);

                var result = new LoginResponse
                {
                    Status = 200,
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
        public IActionResult Ping() {
            var id = User.Claims.FirstOrDefault<Claim>((x) => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var name = User.Claims.FirstOrDefault<Claim>((x) => x.Type == "name")?.Value;            
            foreach(var c in User.Claims)
            {
                Logger.LogDebug("{type} - {value}", c.Type, c.Value);
            }
            var user = new UserModel
            {
                Id = Guid.Parse(id),
                Username = "admin",
                Name = name
            };

            return new JsonResult(new PingResponse() { Status = 200, User = user });
        }

        private static UserModel PerformAuth(LoginModel model)
        {
            var user = (model.Username.Equals("admin") &&
                    model.Password.Equals("admin")) ? new UserModel
                    {
                        Id = Guid.NewGuid(),
                        Username = "admin",
                        Name = "Pieter Hilkemeijer"
                    } : null;
            return user;
        }
    }
}
