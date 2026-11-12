namespace LoanManagement.IdentityService.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
