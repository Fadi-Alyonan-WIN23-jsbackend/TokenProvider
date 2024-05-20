using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TokenProvider.infrastructure.Models;
using TokenProvider.infrastructure.Services;

namespace TokenProvider.Functions
{
    public class GenerateToken
    {
        private readonly ILogger<GenerateToken> _logger;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly TokenGenerator _tokenGenerator;
        public GenerateToken(ILogger<GenerateToken> logger, RefreshTokenService refreshTokenService, TokenGenerator tokenGenerator)
        {
            _logger = logger;
            _refreshTokenService = refreshTokenService;
            _tokenGenerator = tokenGenerator;
        }

        [Function("GenerateToken")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($" StreamReader GenerateToken :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            var tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(body);
            if (tokenRequest == null || tokenRequest.Email == null || tokenRequest.UserId == null)
            {
                return new BadRequestObjectResult(new {error = "Please provide a valid user id and email address"});
            }

            try
            {
                RefreshTokenResult refreshTokenResult = null!;
                AccessTokenResult accessTokenResult = null!;
                using var ctsTimeOut = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeOut.Token, req.HttpContext.RequestAborted);

                req.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    refreshTokenResult = await _refreshTokenService.GetRefreshToken(refreshToken, cts.Token);
                }
                
                if (refreshTokenResult == null || refreshTokenResult.ExpiryDate < DateTime.Now.AddDays(1)) 
                {
                    refreshTokenResult = await _tokenGenerator.GenerateRefreshToken(tokenRequest.UserId , cts.Token);
                }

                accessTokenResult = _tokenGenerator.GenerateAccessToken(tokenRequest, refreshTokenResult.Token);

                if (refreshTokenResult.Token != null && refreshTokenResult.cookieOptions != null)
                {
                    req.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenResult.Token, refreshTokenResult.cookieOptions);
                }

                if (accessTokenResult != null && accessTokenResult.Token != null )
                {
                    return new ObjectResult(new {AccessToken = accessTokenResult.Token, RefreshToken = refreshTokenResult.Token});
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error : GenerateToken :: {ex.Message}");
            }
            return new ObjectResult(new { Error = "An unexepected error occurred while generating tokens." }) { StatusCode = 500 };
        }
    }
}
