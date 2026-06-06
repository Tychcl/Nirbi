using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nearby.Services;

namespace nearby.Interfaces
{
    public interface ITokenService
    {
        Task<string?> GetTokenAsync(TokenService.TokenKey key);
        Task SetTokenAsync(TokenService.TokenKey key, string token);
        Task ClearTokenAsync();
    }
}
