using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using nearby.Interfaces;

namespace nearby.Classes;
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;

    public ApiClient(HttpClient httpClient, ITokenService tokenService)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(20);
        //#if __MOBILE__
        //        _httpClient.BaseAddress = new Uri("http://192.168.0.10:8080/api/");
        //#else
        //        _httpClient.BaseAddress = new Uri("http://127.0.0.1:8080/api/");
        //#endif
        _httpClient.BaseAddress = new Uri("http://10.0.2.2:8080/api/");
        //_httpClient.BaseAddress = new Uri("http://192.168.0.10:8080/api/");
        _tokenService = tokenService;
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = content
        };

        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Add("Cookie", $"jwt={token}");
        }

        return request;
    }

    public async Task<HttpResponseMessage?> GetAsync(string url)
    {
        try
        {
            using var request = await CreateRequestAsync(HttpMethod.Get, url);
            return await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ApiClient error: {ex}");
            return null;
        }

    }

    public async Task<HttpResponseMessage?> PostAsync(string url, HttpContent content)
    {
        try
        {
            using var request = await CreateRequestAsync(HttpMethod.Post, url, content);
            return await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ApiClient error: {ex}");
            return null;
        }
    }

    public async Task<HttpResponseMessage?> PutAsync(string url, HttpContent content)
    {
        try
        {
            using var request = await CreateRequestAsync(HttpMethod.Put, url, content);
            return await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ApiClient error: {ex}");
            return null;
        }
    }

    public async Task<HttpResponseMessage?> DeleteAsync(string url)
    {
        try
        {
            using var request = await CreateRequestAsync(HttpMethod.Delete, url);
            return await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ApiClient error: {ex}");
            return null;
        }
    }

    //TODO: Сделат рефреш токен, мб стоит запихнуть это в токен сервис
    //private async Task<HttpResponseMessage> SendWithRefreshAsync(HttpRequestMessage request)
    //{
    //    var response = await _httpClient.SendAsync(request);
    //    if (response.StatusCode == HttpStatusCode.Unauthorized)
    //    {
    //        // Пытаемся обновить токен
    //        var refreshed = await _authService.RefreshTokenAsync();
    //        if (refreshed)
    //        {
    //            // Обновляем заголовок в оригинальном запросе
    //            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenService.GetTokenAsync());
    //            return await _httpClient.SendAsync(request);
    //        }
    //        else
    //        {
    //            // Редирект на логин
    //            await Shell.Current.GoToAsync("//AuthShell");
    //            throw new UnauthorizedAccessException();
    //        }
    //    }
    //    return response;
    //}
}