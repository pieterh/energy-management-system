using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EMS.WebHost.Helpers
{
    public interface IJwtService
    {
        string Generate(Guid userId, string username, string name);
        TokenValidationParameters GetTokenValidationParameters();
    }

    public class JwtConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
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
            JwtConfig j = new();
            configurations.GetSection("web:jwt").Bind(j);
            Settings = j;            
        }
        public string Generate(Guid userId, string username, string name)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Typ, "JWT"),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("name", name)                
            };

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

        public TokenValidationParameters GetTokenValidationParameters()
        {
            var retval = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Settings.Issuer,
                ValidAudience = Settings.Audience,
                IssuerSigningKey = new RsaSecurityKey(key.Rsa.ExportParameters(false)),
                ClockSkew = TimeSpan.FromSeconds(Settings.ClockSkew)
            };
            return retval;
        }
    }
}
