namespace CareerOps.Frontend.Services;

public interface ITokenProvider
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task RemoveTokenAsync();
}