using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EMS.WebHost.Helpers;

namespace EMS.WebHost.Controllers
{
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
        private readonly ILogger _logger;
        private ILogger Logger => _logger;
        private readonly JwtTokens _jwtCreator;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
            var settings = new JwtSettings() {
                Key = "72cc7881-297d-4670-8d95-54a00692f1ab",
                Issuer = "http://petteflet.org",
                Audience = "Test",
                MinutesToExpiration = 5
            };


            _jwtCreator = new JwtTokens(settings);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] LoginModel model)
        {
            var user = PerformAuth(model);
            if (user != null)
            {
                var token = _jwtCreator.Generate(user.Id, user.Username, user.Name);
                Response.Cookies.Append("X-Access-Token", token, new CookieOptions() { Path = "/api", HttpOnly = true, SameSite = SameSiteMode.Strict });
                return Ok(user);
            }
            else
            {
                return Unauthorized();
            }
        }
        
        [HttpGet("ping")]
        public string Ping() {
            var id = User.Claims.FirstOrDefault<Claim>((x) => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var name = User.Claims.FirstOrDefault<Claim>((x) => x.Type == "name")?.Value;
            var s = new StringBuilder();
            foreach(var c in User.Claims)
            {
                _logger.LogDebug($"{c.Type} - {c.Value}");
            }
            return $"pong{Environment.NewLine}Hello {name}";
        }

        private UserModel PerformAuth(LoginModel model)
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
