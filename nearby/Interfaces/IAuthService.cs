using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nearby.Classes;
using nearby.Models;
using nearby.Models.Api;

namespace nearby.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(string login, string password);
        Task<AuthResponse?> RegisterAsync(string fName, string sName, string lName, string phone, string email, string password);
        Task<bool?> LogoutAsync(string? refreshToken);
    }
}
