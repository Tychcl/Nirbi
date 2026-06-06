using System.Text;
using Microsoft.Maui.Storage;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models.Api;
using Newtonsoft.Json;

namespace nearby.Services;
public class TokenService : ITokenService
{
    public enum TokenKey { Access, Refresh };

    public async Task<string?> GetTokenAsync(TokenKey key)
    {
        return await SecureStorage.GetAsync(key.ToString());
    }
    
    public async Task SetTokenAsync(TokenKey key, string token)
    {
        await SecureStorage.SetAsync(key.ToString(), token);
    }
    
    public Task ClearTokenAsync()
    {
        SecureStorage.Remove(TokenKey.Access.ToString());
        SecureStorage.Remove(TokenKey.Refresh.ToString());
        return Task.CompletedTask;
    }
}