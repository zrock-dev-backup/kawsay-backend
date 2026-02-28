using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastructure.External;

public class AcademicAuthHandler : DelegatingHandler
{
    private string? _cachedToken;
    private readonly string _username;
    private readonly string _role;

    public AcademicAuthHandler(string username = "admin", string role = "admin")
    {
        _username = username;
        _role = role;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Don't intercept the login call itself
        if (request.RequestUri?.AbsolutePath.Contains("/login") == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        if (string.IsNullOrEmpty(_cachedToken))
        {
            await AuthenticateAsync(request, cancellationToken);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);
        var response = await base.SendAsync(request, cancellationToken);

        // Simple token refresh logic if expired (401)
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await AuthenticateAsync(request, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);
            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }

    private async Task AuthenticateAsync(HttpRequestMessage originalRequest, CancellationToken cancellationToken)
    {
        if (originalRequest.RequestUri == null)
            throw new InvalidOperationException("Original request URI is null.");

        var baseUri = new Uri(originalRequest.RequestUri.GetLeftPart(UriPartial.Authority));
        
        // 2. Create an ABSOLUTE URI for the login endpoint
        var loginUri = new Uri(baseUri, $"/login?username={_username}&role={_role}");
        
        // 3. Send the new absolute request
        var loginRequest = new HttpRequestMessage(HttpMethod.Post, loginUri);
        
        var response = await base.SendAsync(loginRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        
        using var jsonDoc = JsonDocument.Parse(responseBody);
        if (jsonDoc.RootElement.TryGetProperty("token", out var tokenProp))
        {
            _cachedToken = tokenProp.GetString();
        }
        else
        {
            _cachedToken = responseBody.Trim('"'); 
        }
    }
}
