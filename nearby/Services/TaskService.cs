using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;
using Newtonsoft.Json;

namespace nearby.Services;

public partial class TaskService : ObservableObject, ITaskService
{
    public event EventHandler<TaskItem> TaskUpdated;
    private void OnTaskUpdated(TaskItem task)
    {
        TaskUpdated?.Invoke(this, task);
    }
    private readonly ApiClient _apiClient;

    public TaskService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ApiResponse<List<TaskItem>>> GetUserTasksAsync(int userId, string status, int page = 1, int pageSize = 10)
    {
        var response = await _apiClient.GetAsync($"tasks/user/{userId}?status={status}&page={page}&limit={pageSize}");
        if(response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        ApiResponse<List<TaskItem>> r = JsonConvert.DeserializeObject<ApiResponse<List<TaskItem>>>(json) ?? new();
        return r;
    }

    public async Task<ApiResponse<List<TaskItem>>> GetTasksAsync(int page = 1, int limit = 10, string status = null, string priority = null, string city = null)
    {
        var settings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd HH:mm:ss"
        };
        var queryParams = new List<string>();
        queryParams.Add($"page={page}");
        queryParams.Add($"limit={limit}");
        if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
        if (!string.IsNullOrEmpty(priority)) queryParams.Add($"priority={priority}");
        if (!string.IsNullOrEmpty(city)) queryParams.Add($"city={Uri.EscapeDataString(city)}");

        var url = $"tasks?{string.Join("&", queryParams)}";
        var response = await _apiClient.GetAsync(url);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var result = JsonConvert.DeserializeObject<ApiResponse<List<TaskItem>>>(json, settings);
        return result;
    }

    public async Task<ApiResponse<TaskItem>> GetTaskAsync(int id)
    {
        var response = await _apiClient.GetAsync($"tasks/{id}");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var r = JsonConvert.DeserializeObject<TaskItem>(json);
        return new ApiResponse<TaskItem>("", r);
    }

    public async Task<ApiResponse<TaskItem>> CreateTaskAsync(TaskItem task)
    {
        var data = new
        {
            title = task.Title,
            description = task.Description,
            needed_volunteers = task.NeededVolunteers,
            priority = task.Priority,
            location = task.Location,
            reward = task.Reward,
            deadline = task.Deadline.ToString("yyyy-MM-dd HH:mm:ss"),
        };
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync("tasks", content);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<TaskItem>(json, null);
    }

    public async Task<ApiResponse<TaskItem>> UpdateTaskAsync(int id, TaskItem task)
    {
        var data = new
        {
            title = task.Title,
            description = task.Description,
            needed_volunteers = task.NeededVolunteers,
            priority = task.Priority,
            location = task.Location,
            reward = task.Reward,
            deadline = task.Deadline.ToString("yyyy-MM-dd HH:mm:ss"),
        };
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _apiClient.PutAsync($"tasks/{id}", content);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        OnTaskUpdated(task);
        return new ApiResponse<TaskItem>(json, null);
    }

    public async Task<ApiResponse<TaskItem>> DeleteTaskAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"tasks/{id}");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<TaskItem>(json, null);
    }

    public async Task<ApiResponse<TaskItem>> VolunteerForTaskAsync(int taskId)
    {
        var response = await _apiClient.PostAsync($"tasks/{taskId}/volunteer", null);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<TaskItem>(json, null);
    }

    public async Task<ApiResponse<List<TaskVolunteerInfo>>> GetTaskVolunteersAsync(int taskId)
    {
        var response = await _apiClient.GetAsync($"tasks/{taskId}/volunteers");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<List<TaskVolunteerInfo>>("", JsonConvert.DeserializeObject<List<TaskVolunteerInfo>>(json));
    }

    public async Task<ApiResponse<TaskItem>> AcceptVolunteerAsync(int taskId, int volunteerUserId)
    {
        var response = await _apiClient.PutAsync($"tasks/{taskId}/volunteers/{volunteerUserId}/accept", null);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<TaskItem>(json, null);
    }

    public async Task<ApiResponse<TaskItem>> RejectVolunteerAsync(int taskId, int volunteerUserId)
    {
        var response = await _apiClient.PutAsync($"tasks/{taskId}/volunteers/{volunteerUserId}/reject", null);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<TaskItem>(json, null);
    }

    public async Task<ApiResponse<TaskItem>> StartTaskAsync(int taskId)
    {
        var response = await _apiClient.PutAsync($"tasks/{taskId}/start", null);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<TaskItem>(json, null);
    }

    public async Task<ApiResponse<TaskItem>> CompleteTaskAsync(int taskId)
    {
        var response = await _apiClient.PutAsync($"tasks/{taskId}/complete", null);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<TaskItem>(json, null);
    }

    public async Task<ApiResponse<string>> GetMyVolunteerStatusAsync(int taskId)
    {
        var response = await _apiClient.GetAsync($"tasks/{taskId}/my-status");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        return new ApiResponse<string>("", data?.GetValueOrDefault("status"));
    }

}