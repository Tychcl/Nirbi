using nearby.Models;

namespace nearby.Interfaces
{
    public interface IConfirmationService
    {
        Task<Guid> CreateConfirmationAsync(CreateConfirmationRequest request);
        Task<Confirmation> GetConfirmationAsync(Guid id);
        Task<List<Confirmation>> GetConfirmationsByReviewerAsync();
        Task<List<Confirmation>> GetConfirmationsByInitiatorAsync();
        Task RespondToConfirmationAsync(Guid confirmationId, RespondToConfirmationRequest request);
        Task RevokeConfirmationAsync(Guid id, Guid initiatorId);
        Task<string?> GetTaskConfirmationStatus(Guid taskId);
        Task<List<Candidate>> GetTaskCandidates(Guid taskId);
    }
}