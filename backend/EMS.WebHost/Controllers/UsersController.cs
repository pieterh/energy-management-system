using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using EMS.DataStore;
using EMS.Library;
using EMS.Library.Passwords;
using EMS.WebHost.Helpers;

namespace EMS.WebHost.Controllers;

public class Response
{
    public required int Status { get; set; }
    public required string StatusText { get; init; }
    public string? Message { get; init; }
}

public class LoginResponse : Response
{
    public required string Token { get; init; }
    public required UserModelLogon User { get; init; }
}

public class PingResponse : Response
{
    public required UserModelBasic User { get; init; }
}

public class LoginModel
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public class UserModelBasic
{
    public Guid Id { get; set; }
    public required string Username { get; init; }
    public required string Name { get; init; }

    public UserModelBasic() { }

    [SetsRequiredMembers]
    public UserModelBasic(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        Id = user.ID;
        Username = user.Username;
        Name = user.Name;
    }
}

public class UserModelLogon : UserModelBasic
{
    public required bool NeedPasswordChange { get; init; }

    [SetsRequiredMembers]
    public UserModelLogon(User user) : base(user)
    {
        ArgumentNullException.ThrowIfNull(user);
        NeedPasswordChange = (user.LastPasswordChangedDate.Ticks <= 0) || (DateTime.UtcNow - user.LastPasswordChangedDate).TotalDays > 3;
    }
}

public class SetPasswordModel
{
    public required string OldPassword { get; init; }
    public required string NewPassword { get; init; }
}

public class SetPasswordResponse : Response
{
    public UserModelBasic? User { get; init; }
}

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
            return new JsonResult(result);
        }
        else
        {
            return Unauthorized();
        }
    }

    [HttpPost("setpassword")]
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
                        return new JsonResult(new SetPasswordResponse() { Status = 500, StatusText = "Old password is wrong.", User = new UserModelBasic(userFromDb) });

                    case PasswordVerificationResult.Success:
                    case PasswordVerificationResult.SuccessRehashNeeded:
                        if (PasswordQualityChecker.IsValidPassword(model.NewPassword))
                        {
                            userFromDb.Password = HashPasword(userFromDb, model.NewPassword);
                            db.Entry(userFromDb).Property(x => x.Password).IsModified = true;

                            userFromDb.LastPasswordChangedDate = DateTime.UtcNow;
                            db.Entry(userFromDb).Property(x => x.LastPasswordChangedDate).IsModified = true;

                            db.SaveChanges();

                            return new JsonResult(new SetPasswordResponse() { Status = 200, StatusText = "", User = new UserModelBasic(userFromDb) });
                        }
                        else
                        {
                            return new JsonResult(new SetPasswordResponse() { Status = 500, StatusText = "New password is to week.", User = new UserModelBasic(userFromDb) });
                        }

                    default:
                        break;
                }
            }
        }
        return new JsonResult(new SetPasswordResponse() { Status = 500, StatusText = "Password was not changed" });
    }

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
            User? userFromDb = null;
            User? found = db.FindUserByUsername(model.Username);

            if (found == null)
            {
                // create the admin user if it doesn't exist
                if (model.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    StaticLogger.Warn("The user 'admin' doesn't exist yet. Creating it with a default password.");
#pragma warning disable S2068
                    userFromDb = new User() { Username = "admin", Name = "Administrator", LastLogonDate = DateTime.UtcNow, Password = "tbd" };
#pragma warning restore
                    var passwordHashed = HashPasword(userFromDb, "admin");
                    userFromDb.Password = passwordHashed;
                    db.Add<User>(userFromDb);
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

                var user = new UserModelLogon(userFromDb);
                return user;

            }
            else
                return null;

        }
    }

    private static PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
    public static string HashPasword(User user, string password)
    {
        string hash = _passwordHasher.HashPassword(user, password);
        return hash;
    }

    public static PasswordVerificationResult VerifyPassword(User user, string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result;
    }
}

public static class UserExtension
{
    public static User? FindUserByUsername(this HEMSContext db, string username)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNullOrEmpty(username);
        var upper = username.ToUpperInvariant();
        return db.Users.Where(x => x.Username == upper).FirstOrDefault();

    }

    public static User? FindUserById(this HEMSContext db, Guid id)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(id);
        return db.Users.Where((x) => x.ID.Equals(id)).FirstOrDefault();
    }
}
