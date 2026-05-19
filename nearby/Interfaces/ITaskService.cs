using nearby.Models;
using nearby.Classes;

namespace nearby.Interfaces;

public interface ITaskService
{
    event EventHandler<TaskItem> TaskUpdated;
    Task<ApiResponse<List<TaskItem>>> GetTasksAsync(int page = 1, int limit = 10, string status = null, string priority = null, string city = null);
    Task<ApiResponse<List<TaskItem>>> GetUserTasksAsync(int userId, string status, int page = 1, int pageSize = 10);
    Task<ApiResponse<TaskItem>> GetTaskAsync(int id);
    Task<ApiResponse<TaskItem>> CreateTaskAsync(TaskItem task);
    Task<ApiResponse<TaskItem>> UpdateTaskAsync(int id, TaskItem task);
    Task<ApiResponse<TaskItem>> DeleteTaskAsync(int id);
    Task<ApiResponse<TaskItem>> VolunteerForTaskAsync(int taskId);
    Task<ApiResponse<List<TaskVolunteerInfo>>> GetTaskVolunteersAsync(int taskId);
    Task<ApiResponse<TaskItem>> AcceptVolunteerAsync(int taskId, int volunteerUserId);
    Task<ApiResponse<TaskItem>> RejectVolunteerAsync(int taskId, int volunteerUserId);
    Task<ApiResponse<TaskItem>> StartTaskAsync(int taskId);
    Task<ApiResponse<TaskItem>> CompleteTaskAsync(int taskId);
    Task<ApiResponse<string>> GetMyVolunteerStatusAsync(int taskId);
}