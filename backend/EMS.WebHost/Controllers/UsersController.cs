using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using EMS.Library.Passwords;
using EMS.WebHost.Helpers;
using EMS.Library.Shared.DTO;
using EMS.Library.Shared.DTO.Users;
using EMS.DataStore;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

namespace EMS.WebHost.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private ILogger Logger { get; init; }
    private static readonly NLog.Logger StaticLogger = NLog.LogManager.GetCurrentClassLogger();

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
            var token = _jwtService.Generate(user.Id, user.Username, user.Name, user.NeedPasswordChange);

            var result = new LoginResponse
            {
                Status = 200,
                StatusText = "OK",
                Token = token,
                User = user
            };
            return Ok((Response)result);
        }
        else
        {
            var result = new Response
            {
                Status = 401,
                StatusText = "Je mag er niet in",
                Message = "Helaas ;-)"
            };
            return Unauthorized(result);
        }
    }

    /// <summary>
    /// Update password
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Sample request:
    /// POST /api/Users/setpassword
    /// {
    ///  "oldPassword": "string",
    ///  "newPassword": "string"
    /// }
    /// </remarks>
    /// <response code="200">Password is changed</response>
    [HttpPost("setpassword")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult SetPassword([FromBody] SetPasswordModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (Guid.TryParse(User.Claims.First<Claim>((x) => x.Type == JwtRegisteredClaimNames.Sub).Value, out var userid))
        {
            using (var db = new HEMSContext())
            {
                var userFromDb = db.FindUserById(userid);
                if (userFromDb == null)
                    return ValidationProblem();

                switch (VerifyPassword(userFromDb, userFromDb.Password, model.OldPassword))
                {
                    case PasswordVerificationResult.Failed:
                        return new JsonResult(new SetPasswordResponse() { Status = 500, StatusText = "Old password is wrong.", User = userFromDb.CreateUserModelBasic() });

                    case PasswordVerificationResult.Success:
                    case PasswordVerificationResult.SuccessRehashNeeded:
                        if (PasswordQualityChecker.IsValidPassword(model.NewPassword))
                        {
                            userFromDb.Password = HashPasword(userFromDb, model.NewPassword);
                            db.Entry(userFromDb).Property(x => x.Password).IsModified = true;

                            userFromDb.LastPasswordChangedDate = DateTime.UtcNow;
                            db.Entry(userFromDb).Property(x => x.LastPasswordChangedDate).IsModified = true;

                            db.SaveChanges();

                            return new JsonResult(new SetPasswordResponse() { Status = 200, StatusText = "", User = userFromDb.CreateUserModelBasic() });
                        }
                        else
                        {
                            return new JsonResult(new SetPasswordResponse() { Status = 500, StatusText = "New password is to week.", User = userFromDb.CreateUserModelBasic() });
                        }

                    default:
                        break;
                }
            }
        }
        return new JsonResult(new SetPasswordResponse() { Status = 500, StatusText = "Password was not changed" });
    }

    /// <summary>
    /// Ping
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Sample request:
    /// GET /ping
    /// </remarks>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        var id = User.Claims.First<Claim>((x) => x.Type == JwtRegisteredClaimNames.Sub).Value;
        var name = User.Claims.First<Claim>((x) => x.Type == "name").Value;
        var username = User.Claims.First<Claim>((x) => x.Type == "username").Value;

        foreach (var c in User.Claims)
        {
            Logger.LogDebug("{Type} - {Value}", c.Type, c.Value);
        }

        var user = new UserModelBasic
        {
            Id = Guid.Parse(id),
            Username = username,
            Name = name
        };

        return new JsonResult(new PingResponse() { Status = 200, StatusText = "OK", User = user });
    }

    private static UserModelLogon? PerformAuth(LoginModel model)
    {

        using (HEMSContext db = new HEMSContext())
        {
            DataStore.User? userFromDb = null;
            DataStore.User? found = db.FindUserByUsername(model.Username);

            if (found == null)
            {
                // create the admin user if it doesn't exist
                if (model.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    StaticLogger.Warn("The user 'admin' doesn't exist yet. Creating it with a default password.");
#pragma warning disable S2068
                    userFromDb = new DataStore.User() { Username = "admin", Name = "Administrator", LastLogonDate = DateTime.UtcNow, Password = "tbd" };
#pragma warning restore
                    var passwordHashed = HashPasword(userFromDb, "admin");
                    userFromDb.Password = passwordHashed;
                    db.Add<DataStore.User>(userFromDb);
                    db.SaveChanges();
                }
            }
            else
                userFromDb = found;

            if (userFromDb != null)
            {
                var verifyResult = VerifyPassword(userFromDb, userFromDb.Password, model.Password);
                if (verifyResult == PasswordVerificationResult.Failed)
                    return null;
                if (verifyResult != PasswordVerificationResult.Success && verifyResult != PasswordVerificationResult.SuccessRehashNeeded)
                    return null;

                // update last logon time
                userFromDb.LastLogonDate = DateTime.UtcNow;
                db.Entry(userFromDb).Property(x => x.LastLogonDate).IsModified = true;

                if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    string h = HashPasword(userFromDb, model.Password);
                    userFromDb.Password = h;
                    db.Entry(userFromDb).Property(x => x.Password).IsModified = true;
                }

                db.SaveChanges();

                return userFromDb.CreateUserModelLogon();
            }
            else
                return null;
        }
    }

    private static PasswordHasher<DataStore.User> _passwordHasher = new PasswordHasher<DataStore.User>();
    public static string HashPasword(DataStore.User user, string password)
    {
        string hash = _passwordHasher.HashPassword(user, password);
        return hash;
    }

    public static PasswordVerificationResult VerifyPassword(DataStore.User user, string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result;
    }
}

public static class UserExtension
{
    public static DataStore.User? FindUserByUsername(this HEMSContext db, string username)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNullOrEmpty(username);
        var upper = username.ToUpperInvariant();
        return db.Users.Where(x => x.Username == upper).FirstOrDefault();

    }

    public static DataStore.User? FindUserById(this HEMSContext db, Guid id)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(id);
        return db.Users.Where((x) => x.ID.Equals(id)).FirstOrDefault();
    }

    public static UserModelBasic CreateUserModelBasic(this DataStore.User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var umd = new UserModelBasic(user.ID, user.Username, user.Name);
        return umd;
    }

    public static UserModelLogon CreateUserModelLogon(this DataStore.User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        bool needPasswordChange = (user.LastPasswordChangedDate.Ticks <= 0) || (DateTime.UtcNow - user.LastPasswordChangedDate).TotalDays > 3;
        var uml = new UserModelLogon(user.ID, user.Username, user.Name, needPasswordChange);
        return uml;
    }
}
