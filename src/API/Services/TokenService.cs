using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TCGOrderManagement.Api.Models;

namespace TCGOrderManagement.Api.Services
{
    /// <summary>
    /// Implementation of token service for JWT token generation and validation
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        
        /// <summary>
        /// Initializes a new instance of the TokenService
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        /// <inheritdoc />
        public string GenerateAccessToken(UserDto user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            
            // Add roles as claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                signingCredentials: credentials);
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        /// <inheritdoc />
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        
        /// <inheritdoc />
        public bool ValidateRefreshToken(string refreshToken)
        {
            // In a real application, this would validate against stored refresh tokens
            // For demo purposes, we'll just check if it's not null or empty
            return !string.IsNullOrEmpty(refreshToken);
        }
        
        /// <inheritdoc />
        public Guid? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false // Don't validate lifetime here
                }, out SecurityToken validatedToken);
                
                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                
                return Guid.Parse(userId);
            }
            catch
            {
                return null;
            }
        }
    }
} 