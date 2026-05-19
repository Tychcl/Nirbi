using System.Text;
using nearby.Interfaces;
using nearby.Classes;
using nearby.Models;
using Newtonsoft.Json;
using System.Diagnostics;

//signin, signup, signout
public class AuthService : IAuthService
{
    private readonly ApiClient _apiClient;
    private readonly ITokenService _tokenService;

    public AuthService(ApiClient apiClient, ITokenService tokenService)
    {
        _apiClient = apiClient;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<User>?> LoginAsync(string login, string password)
    {
        var request = new { login, password };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync("users/signin", content);
        if (response is null)
        {
            throw new Exception("Неудается подключиться к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Неверный логин или пароль");
        }
        if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
        {
            var jwtCookie = cookieValues.FirstOrDefault(c => c.StartsWith("jwt="));
            if (jwtCookie != null)
            {
                var token = jwtCookie.Substring("jwt=".Length).Split(';').FirstOrDefault();
                await _tokenService.SetTokenAsync(token);
                var user = JsonConvert.DeserializeObject<User>(json);
                return new ApiResponse<User>("", user);
            }
        }
        throw new Exception("Успешный ответ, но кука jwt отсутствует");
    }

    public async Task<ApiResponse<bool?>> RegisterAsync(string fullName, string phone, string email, string password)
    {
        var request = new { full_name = fullName, phone, email, password, confirm = password };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync("users/register", content);
        if (response is null)
        {
            throw new Exception("Неудается подключиться к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<bool?>(json, null);
    }

    public async Task LogoutAsync()
    {
        await _tokenService.ClearTokenAsync();
    }

    public async Task<string?> GetTokenAsync() => await _tokenService.GetTokenAsync();
}