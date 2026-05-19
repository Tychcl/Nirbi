using System.ComponentModel;
using nearby.Models;
using nearby.Classes;

namespace nearby.Services;

public interface IUserService : INotifyPropertyChanged
{
    User? CurrentUser { get; set; }
    Task<ApiResponse<User>> LoadUserByIdAsync(int id = -1);
    Task<ApiResponse<User>> UpdateUserByIdAsync(object updatedData, int id = -1);
    Task<ApiResponse<List<User>>> SearchUsersAsync(string query, int limit = 10, int offset = 0);
    //Task<ApiResponse<User>?> ChangePassword(string old, string _new, string confirm);
    //Task<ApiResponse<User>?> DeleteUserById(int id = -1);
}