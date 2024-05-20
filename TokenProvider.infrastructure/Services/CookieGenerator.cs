using Microsoft.AspNetCore.Http;

namespace TokenProvider.infrastructure.Services;

public class CookieGenerator
{
    public static CookieOptions GenerateCookie(DateTimeOffset expiryDate)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = expiryDate
        };
        return cookieOptions;

    }
}
