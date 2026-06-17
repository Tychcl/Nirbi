using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using nearby.Interfaces;
using nearby.Models.Api;
using nearby.Services;
using Newtonsoft.Json;

namespace nearby.Classes;
public class ApiClient
{
    private readonly HttpClient _httpClient;      
    private readonly ITokenService _tokenService;
    
    public ApiClient(HttpClient httpClient, ITokenService tokenService)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(20); 
        //_httpClient.BaseAddress = new Uri("http://10.0.2.2:8080/api/");
        _httpClient.BaseAddress = new Uri("http://5.129.220.192:3000/api/");
        _tokenService = tokenService;
    }
    
    private async Task<HttpResponseMessage?> SendRequestAsync(HttpMethod method, string url, HttpContent? content = null, bool refresh = true)
    {
        try
        {
            var request = new HttpRequestMessage(method, url) { Content = content };
            //var r = request.RequestUri.ToString();
            string? token = await _tokenService.GetTokenAsync(TokenService.TokenKey.Access);
            if (!string.IsNullOrEmpty(token))
                request.Headers.Add("Authorization", $"Bearer {token}");
            var response = await _httpClient.SendAsync(request);
            if (refresh && response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var auth = await RefreshAsync(await _tokenService.GetTokenAsync(TokenService.TokenKey.Refresh));
                await _tokenService.SetTokenAsync(TokenService.TokenKey.Access, auth.AccessToken);
                await _tokenService.SetTokenAsync(TokenService.TokenKey.Refresh, auth.RefreshToken);
                return await SendRequestAsync(method, url, content, false);
            }
            return response;
        } 
        catch (Exception ex)
        {
            Debug.WriteLine($"ApiClient error: {ex}");
            return null;
        }
    }
    
    public async Task<HttpResponseMessage?> GetAsync(string url)
    {
        return await SendRequestAsync(HttpMethod.Get, url);
    }
    
    public async Task<HttpResponseMessage?> PostAsync(string url, HttpContent content)
    {
        return await SendRequestAsync(HttpMethod.Post, url, content);
    }
    
    public async Task<HttpResponseMessage?> PutAsync(string url, HttpContent content)
    {
        return await SendRequestAsync(HttpMethod.Put, url, content);
    }

    public async Task<HttpResponseMessage?> PatchAsync(string url, HttpContent content)
    {
        return await SendRequestAsync(HttpMethod.Patch, url, content);
    }

    public async Task<HttpResponseMessage?> DeleteAsync(string url)
    {
        return await SendRequestAsync(HttpMethod.Delete, url);
    }

    public async Task<AuthResponse?> RefreshAsync(string? refreshToken)
    {
        if (refreshToken is null) return null;
        var request = new { refreshToken };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await PostAsync("Auth/refresh", content);
        if (response is null) return null;
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        var auth = JsonConvert.DeserializeObject<AuthResponse>(json);
        return auth;
    }
}