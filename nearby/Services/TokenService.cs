using Microsoft.Maui.Storage;
using nearby.Interfaces;

namespace nearby.Services;
public class TokenService : ITokenService
{
    private const string TokenKey = "jwt";

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.GetAsync(TokenKey);
    }

    public async Task SetTokenAsync(string token)
    {
        await SecureStorage.SetAsync(TokenKey, token);
    }

    public Task ClearTokenAsync()
    {
        SecureStorage.Remove(TokenKey);
        return Task.CompletedTask;
    }
}