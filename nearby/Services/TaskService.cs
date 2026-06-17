using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;
using Newtonsoft.Json;

namespace nearby.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApiClient _apiClient;

        public TaskService(ApiClient apiClient) => _apiClient = apiClient;

        public async Task<TaskItem> CreateTaskAsync(CreateMinorTaskRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync("tasks", content);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(responseBody);
            return JsonConvert.DeserializeObject<TaskItem>(responseBody)!;
        }

        public async Task<List<TaskItem>> GetTasksAsync(int offset = 0, int limit = 20, string? search = null, string? status = null, string? sort = null)
        {
            var queryParams = new List<string> { $"offset={offset}", $"limit={limit}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrWhiteSpace(sort)) queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

            var url = $"tasks?{string.Join("&", queryParams)}";
            var response = await _apiClient.GetAsync(url);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            var r = JsonConvert.DeserializeObject<GetTasksResponse> (json) ?? null;
            return r is null ? new List<TaskItem>() { } : r.items ;
        }

        public async Task<TaskItem> GetTaskAsync(Guid minorTaskId)
        {
            var response = await _apiClient.GetAsync($"tasks/{minorTaskId}");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<TaskItem>(json)!;
        }

        public async Task<TaskItem> UpdateTaskAsync(Guid minorTaskId, UpdateMinorTaskRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PatchAsync($"tasks/{minorTaskId}", content);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<TaskItem>(json)!;
        }

        public async Task<TaskItem> UpdateTaskStatusAsync(Guid minorTaskId, Guid statusId)
        {
            var body = new UpdateMinorTaskStatusRequest { statusId = statusId };
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PutAsync($"tasks/{minorTaskId}", content);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<TaskItem>(json)!;
        }

        public async Task DeleteTaskAsync(Guid minorTaskId)
        {
            var response = await _apiClient.DeleteAsync($"tasks/{minorTaskId}");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }

        public async Task<List<string>> GetTaskNamesByIdsAsync(List<Guid> ids)
        {
            var body = new GetTaskNamesByIdsRequest { ids = ids };
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync("tasks/names", content);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }

        public async Task<List<TaskStatus>> GetStatusesAsync()
        {
            var response = await _apiClient.GetAsync("statuses");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<TaskStatus>>(json) ?? new List<TaskStatus>();
        }

        public async Task<List<Guid>> GetTaskParticipantsAsync(Guid minorTaskId)
        {
            var response = await _apiClient.GetAsync($"tasks/{minorTaskId}/participants");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<Guid>>(json) ?? new List<Guid>();
        }

        public async Task RemoveParticipantAsync(Guid minorTaskId, Guid participantId)
        {
            var response = await _apiClient.DeleteAsync($"tasks/{minorTaskId}/participants/{participantId}");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }

        public async Task<List<string>> GetTaskCollectionsByIdsAsync(List<Guid> ids)
        {
            var body = new GetTaskCollectionsByIdsRequest { ids = ids };
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync("tasks/collections", content);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
    }

    public class GetTasksResponse
    {
        public int total {  get; set; }
        public List<TaskItem> items { get; set; }
    }

    public class CreateMinorTaskRequest
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int numberVolunteers { get; set; }
        public double encouragement { get; set; }
    }

    public class UpdateMinorTaskRequest
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int numberVolunteers { get; set; }
        public double encouragement { get; set; }
    }

    public class UpdateMinorTaskStatusRequest
    {
        public Guid statusId { get; set; }
    }

    public class GetTaskNamesByIdsRequest
    {
        public List<Guid>? ids { get; set; }
    }

    public class GetTaskCollectionsByIdsRequest
    {
        public List<Guid>? ids { get; set; }
    }

    public class TaskStatus
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}