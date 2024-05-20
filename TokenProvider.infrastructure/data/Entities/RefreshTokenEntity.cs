﻿using System.ComponentModel.DataAnnotations;

namespace TokenProvider.infrastructure.data.Entities;

public class RefreshTokenEntity
{
    [Key]
    public string RefreshToken { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ExpiryDate { get; set; } = DateTime.Now.AddDays(7);
}
