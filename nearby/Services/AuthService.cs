using System.Diagnostics;
using System.Text;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;
using nearby.Models.Api;
using nearby.Services;
using Newtonsoft.Json;

public class AuthService : IAuthService
{
    private readonly ApiClient _apiClient;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<AuthResponse?> LoginAsync(string username, string password)
    {
        var request = new { username, password };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync("Auth/login", content);
        if (response is null)
            throw new Exception("Неудается подключиться к серверу");
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception("Неверный логин или пароль");
        var auth = JsonConvert.DeserializeObject<AuthResponse>(json);
        if (auth is null) throw new Exception("Неизвестная ошибка");
        return auth;
    }

    public async Task<AuthResponse?> RegisterAsync(string fName, string sName, string lName, string phone, string email, string password)
    {
        var request = new { fName, sName, lName, phone, email, password, confirm = password };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync("Auth/register", content);
        if (response is null)
            throw new Exception("Неудается подключиться к серверу");
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(json);
        var auth = JsonConvert.DeserializeObject<AuthResponse>(json);
        if (auth is null) throw new Exception("Неизвестная ошибка");
        return auth;
    }

    public async Task<bool?> LogoutAsync(string? refreshToken)
    {
        if (refreshToken is null) return null;
        var request = new { refreshToken };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync("Auth/logout", content);
        if (response is null) throw new Exception("Неудается подключиться к серверу");
        if (!response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            throw new Exception(json);
        }
        return true;
    }
}