using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MongoDB.Driver;
using CalendarAPI.Models;
using CalendarAPI.Services;
using Microsoft.Extensions.Logging;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _users;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMongoDatabase database, IConfiguration configuration, IEmailService emailService, ILogger<AuthController> logger)
    {
        _users = database.GetCollection<User>("Users");
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Geçersiz email veya şifre" });
        }

        var token = GenerateJwtToken(user);

        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _users.Find(u => u.Email == request.Email).AnyAsync())
        {
            return BadRequest(new { message = "Bu email adresi zaten kullanımda" });
        }

        if (await _users.Find(u => u.Username == request.Username).AnyAsync())
        {
            return BadRequest(new { message = "Bu kullanıcı adı zaten kullanımda" });
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password)
        };

        await _users.InsertOneAsync(user);
        return Ok(new { message = "Kayıt başarılı" });
    }

    [HttpPost("verify")]
    public IActionResult VerifyToken([FromBody] TokenVerifyRequest request)
    {
        try
        {
            _logger.LogInformation($"Verifying token: {request?.Token}");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            
            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            _logger.LogInformation("Token validation successful");
            return Ok(new { isValid = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Token validation failed: {ex.Message}");
            return Ok(new { isValid = false });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (user == null)
        {
            return Ok(new { message = "Şifre sıfırlama bağlantısı gönderildi" });
        }

        var resetToken = GenerateResetToken(user);
        
        var update = Builders<User>.Update.Set(u => u.ResetPasswordToken, resetToken)
                                        .Set(u => u.ResetPasswordExpiry, DateTime.UtcNow.AddHours(1));
        await _users.UpdateOneAsync(u => u.Id == user.Id, update);

        Console.WriteLine($"Reset Token for {user.Email}: {resetToken}");
        _logger.LogInformation($"Reset Token for {user.Email}: {resetToken}");

        return Ok(new { message = "Şifre sıfırlama bağlantısı gönderildi" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _users.Find(u => u.ResetPasswordToken == request.Token 
                                      && u.ResetPasswordExpiry > DateTime.UtcNow)
                              .FirstOrDefaultAsync();

        if (user == null)
        {
            return BadRequest(new { message = "Geçersiz veya süresi dolmuş token" });
        }

        var update = Builders<User>.Update.Set(u => u.PasswordHash, HashPassword(request.NewPassword))
                                        .Set(u => u.ResetPasswordToken, null)
                                        .Set(u => u.ResetPasswordExpiry, null);
        await _users.UpdateOneAsync(u => u.Id == user.Id, update);

        return Ok(new { message = "Şifreniz başarıyla güncellendi" });
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["Jwt:Key"] ?? 
            throw new InvalidOperationException("JWT key is not configured");
        var key = Encoding.ASCII.GetBytes(jwtKey);

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
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string GenerateResetToken(User user)
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
} 