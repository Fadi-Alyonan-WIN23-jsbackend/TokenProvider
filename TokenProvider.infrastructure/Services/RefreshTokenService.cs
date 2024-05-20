using Microsoft.EntityFrameworkCore;
using System.Net;
using TokenProvider.infrastructure.data.Context;
using TokenProvider.infrastructure.data.Entities;
using TokenProvider.infrastructure.Models;

namespace TokenProvider.infrastructure.Services;

public class RefreshTokenService
{
    private readonly IDbContextFactory<DataContext> _contextFactory;

    public RefreshTokenService(IDbContextFactory<DataContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<RefreshTokenResult> GetRefreshToken(string refreshtoken, CancellationToken cancellationToken)
    {
        await using var context = _contextFactory.CreateDbContext();
        var refreshTokenEntity = await context.RefreshTokens.FirstOrDefaultAsync(x => x.RefreshToken == refreshtoken && x.ExpiryDate > DateTime.Now, cancellationToken);
        if (refreshTokenEntity == null)
        {
            return new RefreshTokenResult
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Token = null,
                ExpiryDate = null,
            };
        }
        else
        {
            return new RefreshTokenResult 
            { 
                StatusCode = (int) HttpStatusCode.OK,
                Token = refreshTokenEntity.RefreshToken,
                ExpiryDate = refreshTokenEntity.ExpiryDate,
            };
        }

    }
    public async Task<bool> SaveRefreshToken(string refreshtoken, string userId,CancellationToken cancellationToken)
    {
        try
        {
            await using var context = _contextFactory.CreateDbContext();
            var refreshTokenEntity = new RefreshTokenEntity()
            {
                RefreshToken = refreshtoken,
                UserId = userId,
                ExpiryDate = DateTime.Now.AddDays(7)
            };
            context.RefreshTokens.Add(refreshTokenEntity);
            await context.SaveChangesAsync(cancellationToken);
            return true;

        } catch { return false; }
        
        

    }
}
