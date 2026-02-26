using CareerOps.Frontend.Services;
using System.Net.Http.Headers;

public class AuthenticatedHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider; 
    public AuthenticatedHandler(ITokenProvider tokenProvider) => _tokenProvider = tokenProvider;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
