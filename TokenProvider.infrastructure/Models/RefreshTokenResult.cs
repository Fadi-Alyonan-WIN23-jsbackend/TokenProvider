using Microsoft.AspNetCore.Http;

namespace TokenProvider.infrastructure.Models;

public class RefreshTokenResult
{
    public int? StatusCode { get; set; }
    public string? Token { get; set; }
    public CookieOptions? cookieOptions { get; set; }
    public string? Error { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
