namespace TokenProvider.infrastructure.Models;

public class AccessTokenResult
{
    public int? StatusCode { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }

}
