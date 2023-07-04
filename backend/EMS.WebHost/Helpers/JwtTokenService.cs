using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EMS.WebHost.Helpers;

public interface IJwtService
{
    string Generate(Guid userId, string username, string name, bool needPasswordChange);
}

public class JwtConfig
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public ushort Expiration { get; set; }
    public ushort ClockSkew { get; set; }
}

public class JwtTokenService : IJwtService
{
    // after restart we have a new key and a client needs to login again
    private static readonly RsaSecurityKey key = new(RSA.Create(2048));
    public JwtConfig Settings { get; init; }

    public JwtTokenService(IConfiguration configurations)
    {
        ArgumentNullException.ThrowIfNull(configurations);
        JwtConfig j = new();
        configurations.GetSection("web:jwt").Bind(j);
        Settings = j;
    }

    public string Generate(Guid userId, string username, string name, bool needPasswordChange)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Typ, "JWT"),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("name", name),
            new Claim("username", username)                 
        };

        if (needPasswordChange)
            claims.Add(new Claim("needpasswordchange", needPasswordChange.ToString()));

        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSsaPssSha256);

        var token = new JwtSecurityToken(
            Settings.Issuer,
            Settings.Audience,
            claims,
            expires: DateTime.UtcNow + TimeSpan.FromMinutes(Settings.Expiration),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static TokenValidationParameters CreateTokenValidationParameters(IConfiguration configurations)
    {
        ArgumentNullException.ThrowIfNull(configurations);
        JwtConfig j = new();
        configurations.GetSection("web:jwt").Bind(j);
        var retval = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = j.Issuer,
            ValidAudience = j.Audience,
            IssuerSigningKey = new RsaSecurityKey(key.Rsa.ExportParameters(false)),
            ClockSkew = TimeSpan.FromSeconds(j.ClockSkew)
        };
        return retval;
    }
}
