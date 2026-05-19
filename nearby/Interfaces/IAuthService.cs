using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nearby.Classes;
using nearby.Models;

namespace nearby.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<User>> LoginAsync(string login, string password);
        Task<ApiResponse<bool?>> RegisterAsync(string fullName, string phone, string email, string password);
        Task LogoutAsync();
        Task<string?> GetTokenAsync();
    }
}
