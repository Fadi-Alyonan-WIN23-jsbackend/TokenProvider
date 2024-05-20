using Microsoft.EntityFrameworkCore;
using TokenProvider.infrastructure.data.Entities;

namespace TokenProvider.infrastructure.data.Context;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

}
