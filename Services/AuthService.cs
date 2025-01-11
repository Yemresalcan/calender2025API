using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using CalendarAPI.Models;
using CalendarAPI.DTOs;
using BCrypt.Net;
using MongoDB.Bson;

namespace CalendarAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponse> LoginAsync(LoginDto loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _users;
        private readonly string _jwtSecret;

        public AuthService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(config["MongoDbSettings:DatabaseName"]);
            _users = database.GetCollection<User>("Users");
            _jwtSecret = config["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto)
        {
            // Email kontrolü
            if (await _users.Find(u => u.Email == registerDto.Email).AnyAsync())
                throw new Exception("Email already exists");

            // Yeni kullanıcı oluştur
            var user = new User
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Email = registerDto.Email,
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            await _users.InsertOneAsync(user);

            // Token oluştur
            return new AuthResponse
            {
                Token = GenerateJwtToken(user),
                Username = user.Username
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
        {
            var user = await _users.Find(u => u.Email == loginDto.Email).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            return new AuthResponse
            {
                Token = GenerateJwtToken(user),
                Username = user.Username
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
} 