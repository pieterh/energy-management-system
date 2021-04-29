using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace EMS.WebHost.Helpers
{
    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int MinutesToExpiration { get; set; }
        public TimeSpan ClockSkew { get; set; }
        public TimeSpan Expire => TimeSpan.FromMinutes(MinutesToExpiration);
    }

    public class JwtTokens
    {
        // after restart we have a new key and a client needs to login again
        private static readonly RsaSecurityKey key = new(RSA.Create(2048));
        public static RSAParameters KeyParamaters { get { return key.Rsa.ExportParameters(false); } }

        private readonly JwtSettings _settings;

        public JwtTokens(JwtSettings settings)
        {
            _settings = settings;
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
                _settings.Issuer,
                _settings.Audience,
                claims,
                expires: DateTime.UtcNow + _settings.Expire,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
