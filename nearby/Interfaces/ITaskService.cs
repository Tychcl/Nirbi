using nearby.Models;
using nearby.Services;

namespace nearby.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItem> CreateTaskAsync(CreateMinorTaskRequest request);
        Task<List<TaskItem>> GetTasksAsync(int offset = 0, int limit = 20, string? search = null, string? status = null, string? sort = null);
        Task<TaskItem> GetTaskAsync(Guid minorTaskId);
        Task<TaskItem> UpdateTaskAsync(Guid minorTaskId, UpdateMinorTaskRequest request);
        Task<TaskItem> UpdateTaskStatusAsync(Guid minorTaskId, Guid statusId);
        Task DeleteTaskAsync(Guid minorTaskId);
        Task<List<string>> GetTaskNamesByIdsAsync(List<Guid> ids);
        Task<List<nearby.Services.TaskStatus>> GetStatusesAsync();
        Task<List<Guid>> GetTaskParticipantsAsync(Guid minorTaskId);
        Task RemoveParticipantAsync(Guid minorTaskId, Guid participantId);
        Task<List<string>> GetTaskCollectionsByIdsAsync(List<Guid> ids);
    }
}