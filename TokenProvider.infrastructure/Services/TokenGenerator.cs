using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TokenProvider.infrastructure.Models;

namespace TokenProvider.infrastructure.Services;

public class TokenGenerator
{
    private readonly RefreshTokenService _refreshTokenService;

    public TokenGenerator(RefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }


    public async Task<RefreshTokenResult> GenerateRefreshToken(string userId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.BadRequest, Error = "Invalid bode request" };
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var token = GenerateJwtToken(new ClaimsIdentity(claims), DateTime.Now.AddMinutes(5));

            if (token == null)
            {
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = "An unexpected error occurred while token was generated" };

            }
            var cookieOptions = CookieGenerator.GenerateCookie(DateTimeOffset.Now.AddDays(7));
            if (cookieOptions == null)
            {
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = "An unexpected error occurred while cookie was generated" };
            }
            var result = await _refreshTokenService.SaveRefreshToken(token, userId, cancellationToken);

            if (!result)
            {
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = "An unexpected error occurred while saving refresh token" };

            }

            return new RefreshTokenResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                Token = token,
                cookieOptions = cookieOptions
            };

        }
        catch (Exception ex)
        {
            return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = ex.Message };
        }

    }

    public static string GenerateJwtToken(ClaimsIdentity claimsIdentity, DateTime dateTime)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = dateTime,
            Issuer = "TokenProvider",
            Audience = "Silicon",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1a7e90f5-4583-4744-b80f-28e9ac2843d1")), SecurityAlgorithms.HmacSha256Signature)

        };
        var token = tokenHandler.CreateToken(tokenDescription);
        return tokenHandler.WriteToken(token);

    }

    public AccessTokenResult GenerateAccessToken(TokenRequest tokenRequest, string refreshToken)
    {
        
        try
        {
            if (string.IsNullOrEmpty(tokenRequest.UserId) || string.IsNullOrEmpty(tokenRequest.Email))
            {
                return new AccessTokenResult { StatusCode = (int)HttpStatusCode.BadRequest, ErrorMessage = "Invalid request body. Parameters userId and email must be provided" };
                    
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, tokenRequest.UserId),
                new Claim(ClaimTypes.Name, tokenRequest.Email),
                new Claim(ClaimTypes.Email, tokenRequest.Email),
            };
            if (!string.IsNullOrEmpty(refreshToken))
            {
                claims = [.. claims, new Claim("refreshToken", refreshToken)];
            }
            var token = GenerateJwtToken(new ClaimsIdentity(claims), DateTime.Now.AddMinutes(5));
            if (token == null)
            {
                return new AccessTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, ErrorMessage = "An unexpected error occurred while access token was generated" };
            }
            return new AccessTokenResult { StatusCode = (int)HttpStatusCode.OK, Token = token};
        }
        catch (Exception ex)
        {
            return new AccessTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, ErrorMessage = ex.Message };
        }
    }
}

