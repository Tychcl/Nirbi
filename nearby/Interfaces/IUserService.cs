using System.ComponentModel;
using nearby.Models;
using nearby.Models.Api;

namespace nearby.Interfaces
{
    public interface IUserService : INotifyPropertyChanged
    {
        User? CurrentUser { get; set; }
        Guid? CurrentUserId { get; set; }

        Task<User> LoadUserByIdAsync(Guid? id = null, List<string>? fields = null);
        Task<User> UpdateUserByIdAsync(Guid id, UpdateUserRequest data);
        Task<List<User>> SearchUsersAsync(string username);
        Task<List<User>> GetUsersAsync(int offset = 0, int limit = 20,
            string? search = null, List<string>? fields = null);
        Task<List<string>> GetAvailableFieldsAsync();
        Task UpdateContactsAsync(Guid? id = null);
        Task<List<FullNames>> GetUserFullNamesAsync(List<Guid> ids);
    }
}