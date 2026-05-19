using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;
using Newtonsoft.Json;

namespace nearby.Services;

public partial class UserService : ObservableObject, IUserService
{
    private readonly ApiClient _apiClient;
    private readonly ITokenService _tokenService;

    [ObservableProperty]
    private User? _currentUser;

    public UserService(ApiClient apiClient, ITokenService tokenService)
    {
        _apiClient = apiClient;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<User>> LoadUserByIdAsync(int id = -1)
    {
        var token = await _tokenService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            CurrentUser = null;
            throw new Exception("Неавторизован");
        }
        var response = await _apiClient.GetAsync($"users/{id}");
        if (response is null)
        {
            throw new Exception("Неудается подключиться к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var user = JsonConvert.DeserializeObject<User>(json);
        if (id == -1 || CurrentUser?.Id == id)
        {
            CurrentUser = user;
        }
        return new ApiResponse<User>("Данные получены", user);
    }

    public async Task<ApiResponse<User>> UpdateUserByIdAsync(object updatedData, int id = -1)
    {
        var json = JsonConvert.SerializeObject(updatedData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _apiClient.PutAsync($"users/{id}", content);
        if (response is null)
        {
            throw new Exception("Неудается подключиться к серверу");
        }
        json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var user = await LoadUserByIdAsync(_currentUser.Id);
        //this.OnPropertyChanged(nameof(IUserService.CurrentUser));
        return new ApiResponse<User>("Данные обновленны", user.Data);
    }

    public async Task<ApiResponse<List<User>>> SearchUsersAsync(string query, int limit = 10, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new ApiResponse<List<User>>("Пустой поисковый запрос", new List<User>());

        var url = $"users/search?q={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}";
        var response = await _apiClient.GetAsync(url);

        if (response is null)
            throw new Exception("Не удалось подключиться к серверу");

        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(json);

        var users = JsonConvert.DeserializeObject<List<User>>(json);
        return new ApiResponse<List<User>>("Результаты поиска", users);
    }
}