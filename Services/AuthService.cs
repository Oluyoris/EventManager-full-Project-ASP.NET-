using EventManager.Api.Data;
using EventManager.Api.Models;
using EventManager.Api.Dtos;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace EventManager.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly EventManagerDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(EventManagerDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(UserLoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.UsernameOrEmail) || string.IsNullOrEmpty(loginDto.Password))
            {
                throw new ArgumentException("Username or password cannot be empty.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.UsernameOrEmail || u.Email == loginDto.UsernameOrEmail);
            if (user == null)
            {
                Console.WriteLine($"[AuthService] User not found for UsernameOrEmail: {loginDto.UsernameOrEmail}");
                throw new ArgumentException("Invalid username or password.");
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                Console.WriteLine("[AuthService] Password verification failed.");
                throw new ArgumentException("Invalid username or password.");
            }

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);
            // Map UserRole enum to string based on database values
            string roleName = user.Role switch
            {
                UserRole.Admin => "Admin",
                UserRole.Planner => "Planner",
                _ => throw new ArgumentException($"Unknown role value: {user.Role}")
            };
            Console.WriteLine($"[AuthService] User role in database: {user.Role}, Mapped role in JWT: {roleName}");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", roleName) // Align with admin login
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            Console.WriteLine($"[AuthService] Generated token for {user.Username}: {tokenString}");
            // Log token claims for debugging
            var tokenHandlerRead = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandlerRead.ReadJwtToken(tokenString);
            Console.WriteLine($"[AuthService] Token claims: {string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}: {c.Value}"))}");
            return tokenString;
        }

        public async Task<User> RegisterAsync(UserRegisterDto registerDto)
        {
            if (registerDto == null || string.IsNullOrEmpty(registerDto.Username) || string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password))
            {
                throw new ArgumentException("Username, email, or password cannot be empty.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);

            if (existingUser != null)
            {
                throw new ArgumentException("User with this username or email already exists.");
            }

            UserRole role = UserRole.Planner;
            if (!string.IsNullOrEmpty(registerDto.Role) && Enum.TryParse<UserRole>(registerDto.Role, true, out var parsedRole))
            {
                role = parsedRole;
            }

            var newUser = new User
            {
                Id = 0,
                Username = registerDto.Username,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber,
                Country = registerDto.Country,
                CountryCode = registerDto.CountryCode,
                CompanyName = registerDto.CompanyName,
                PasswordHash = HashPassword(registerDto.Password),
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Balance = 0m,
                IsBlocked = false,
                IsEmailVerified = false
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        public async Task UpdatePasswordAsync(string userId, PasswordUpdateDto passwordDto)
        {
            if (string.IsNullOrEmpty(userId) || passwordDto == null || string.IsNullOrEmpty(passwordDto.NewPassword))
            {
                throw new ArgumentException("User ID or new password cannot be empty.");
            }

            if (!int.TryParse(userId, out int parsedUserId))
            {
                throw new ArgumentException("Invalid user ID format.");
            }

            var user = await _context.Users.FindAsync(parsedUserId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            user.PasswordHash = HashPassword(passwordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be empty.");
            }
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            {
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}