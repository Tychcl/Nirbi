using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;
using nearby.Models.Api;
using Newtonsoft.Json;

namespace nearby.Services
{
    public partial class UserService : ObservableObject, IUserService
    {
        private readonly ApiClient _apiClient;
        private readonly ITokenService _tokenService;

        [ObservableProperty] private User? _currentUser;
        [ObservableProperty] private Guid? _currentUserId;

        public UserService(ApiClient apiClient, ITokenService tokenService)
        {
            _apiClient = apiClient;
            _tokenService = tokenService;
        }

        public async Task<User> LoadUserByIdAsync(Guid? id = null, List<string>? fields = null)
        {
            var token = await _tokenService.GetTokenAsync(TokenService.TokenKey.Refresh);
            if (string.IsNullOrEmpty(token))
            {
                CurrentUser = null;
                throw new Exception("Неавторизован");
            }
            id ??= _currentUserId;

            var url = $"Users/{id}";
            if (fields?.Count > 0)
                url += "?fields=" + string.Join(",", fields);

            var response = await _apiClient.GetAsync(url);
            if (response == null) throw new HttpRequestException("Не удалось подключиться к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(json);
            var user = JsonConvert.DeserializeObject<User>(json);
            if (id == _currentUserId || CurrentUser?.Id == id)
                CurrentUser = user;
            return user;
        }

        public async Task<User> UpdateUserByIdAsync(object updatedData, Guid? id = null)
        {
            id = id ?? _currentUserId;
            var json = JsonConvert.SerializeObject(updatedData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PutAsync($"Users/{id}", content);
            if (response is null)
                throw new Exception("Неудается подключиться к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception(json);
            var user = await LoadUserByIdAsync(_currentUser.Id);
            return user;
        }

        public async Task<List<User>> SearchUsersAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return new List<User>();
            var url = $"Users/search?username={Uri.EscapeDataString(username)}";
            var response = await _apiClient.GetAsync(url);
            if (response == null) throw new HttpRequestException("Не удалось подключиться к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(json);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        public async Task<List<User>> GetUsersAsync(int offset = 0, int limit = 20,
            string? search = null, List<string>? fields = null)
        {
            var queryParams = new List<string> { $"offset={offset}", $"limit={limit}" };
            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (fields?.Count > 0)
                queryParams.Add($"fields={string.Join("&fields=", fields)}");

            var url = $"Users?{string.Join("&", queryParams)}";
            var response = await _apiClient.GetAsync(url);
            if (response == null) throw new HttpRequestException("Не удалось подключиться к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(json);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        public async Task<List<string>> GetAvailableFieldsAsync()
        {
            var response = await _apiClient.GetAsync("Users/fields");
            if (response == null) throw new HttpRequestException("Не удалось подключиться к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(json);
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }

        public async Task UpdateContactsAsync(Guid? id = null)
        {
            id ??= _currentUserId;
            var response = await _apiClient.PutAsync($"Users/{id}/contacts", null);
            if (response == null) throw new HttpRequestException("Не удалось подключиться к серверу");
            if (!response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                throw new Exception(json);
            }
        }

        public async Task<List<FullNames>> GetUserFullNamesAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return new List<FullNames>();
            var query = string.Join("&", ids.Select(id => $"ids={id}"));
            var url = $"Users/fullnames?{query}";
            var response = await _apiClient.GetAsync(url);
            if (response == null) throw new HttpRequestException("Не удалось подключиться к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(json);
            return JsonConvert.DeserializeObject<List<FullNames>>(json) ?? new List<FullNames>();
        }
    }
}