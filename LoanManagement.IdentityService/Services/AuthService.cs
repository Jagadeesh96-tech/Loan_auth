using global::LoanManagement.IdentityService.Configurations;
using global::LoanManagement.IdentityService.DTOs;
using global::LoanManagement.IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoanManagement.IdentityService.Services
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtOptions.Value;
        }

        // REGISTER
        public async Task<LoginResponseDto> RegisterAsync(
            RegisterRequestDto request)
        {
            // 1. Check whether email already exists
            var existingUser = await _userManager.FindByEmailAsync(
                request.Email);

            if (existingUser != null)
                throw new InvalidOperationException(
                    "Email already registered.");

            // 2. Create user object
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName
            };

            // 3. Create user and hash password
            var result = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        result.Errors.Select(e => e.Description)));
            }

            // 4. Assign default User role
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "User");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                throw new InvalidOperationException(
                    "Unable to assign default User role.");
            }

            // 5. Generate JWT
            var token = await GenerateJwtTokenAsync(user);

            // 6. Return response
            return new LoginResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                AccessToken = token
            };
        }

        // LOGIN
        public async Task<LoginResponseDto> LoginAsync(
            LoginRequestDto request)
        {
            // 1. Find user using email
            var user = await _userManager.FindByEmailAsync(
                request.Email);

            if (user == null)
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");

            // 2. Verify password
            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            // 3. Generate JWT
            var token = await GenerateJwtTokenAsync(user);

            // 4. Return response
            return new LoginResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                AccessToken = token
            };
        }

        // RESET PASSWORD
        public async Task<bool> ResetPasswordAsync(
            ResetPasswordDto request)
        {
            var user = await _userManager.FindByEmailAsync(
                request.Email);

            if (user == null)
                return false;

            var result = await _userManager.ResetPasswordAsync(
                user,
                request.Token,
                request.NewPassword);

            return result.Succeeded;
        }

        // JWT GENERATION
        private async Task<string> GenerateJwtTokenAsync(
            ApplicationUser user)
        {
            // 1. Create signing key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            // 2. Create signing credentials
            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // 3. Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // 4. Create claims
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!)
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(
                    ClaimTypes.Role,
                    role));
            }

            // 5. Create JWT token
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _jwtSettings.ExpirationMinutes),
                signingCredentials: credentials
            );

            // 6. Convert token into string
            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }

}
