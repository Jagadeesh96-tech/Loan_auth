using LoanManagement.IdentityService.DTOs;

namespace LoanManagement.IdentityService.Services;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(
        RegisterRequestDto request);

    Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request);

    Task<bool> ResetPasswordAsync(
        ResetPasswordDto request);
}