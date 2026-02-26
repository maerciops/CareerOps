using Microsoft.JSInterop;

namespace CareerOps.Frontend.Services;

public class LocalStorageTokenProvider : ITokenProvider
{
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "authToken";

    public LocalStorageTokenProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync() =>
        await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    public async Task SetTokenAsync(string token) =>
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);

    public async Task RemoveTokenAsync() =>
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
}