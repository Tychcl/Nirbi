using System.ComponentModel;
using nearby.Models;

namespace nearby.Interfaces
{
    public interface IUserService : INotifyPropertyChanged
    {
        User? CurrentUser { get; set; }
        Guid? CurrentUserId { get; set; }

        Task<User> LoadUserByIdAsync(Guid? id = null, List<string>? fields = null);
        Task<User> UpdateUserByIdAsync(object updatedData, Guid? id = null);
        Task<List<User>> SearchUsersAsync(string username);
        Task<List<User>> GetUsersAsync(int offset = 0, int limit = 20,
            string? search = null, List<string>? fields = null);
        Task<List<string>> GetAvailableFieldsAsync();
        Task UpdateContactsAsync(Guid? id = null);
        Task<List<FullNames>> GetUserFullNamesAsync(List<Guid> ids);
    }
}