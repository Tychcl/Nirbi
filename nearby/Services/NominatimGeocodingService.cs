using System.Text.Json;
using nearby.Interfaces;
using nearby.Models.Task;
using Newtonsoft.Json;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;

    public NominatimGeocodingService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("NearbyApp/1.0");
    }

    public async Task<string?> GetAddressAsync(double lat, double lon)
    {
        var url = $"reverse?lat={lat.ToString().Replace(',','.')}&lon={lon.ToString().Replace(',', '.')}&format=jsonv2";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<address_data>(json);
        if (data.address is null) return null;
        return $"{data.address.City}, {data.address.Road}, {data.address.House}";
    }

    public async Task<List<Position>> SearchAddressAsync(string query)
    {
        var url = $"search?q={Uri.EscapeDataString(query)}&format=jsonv2&limit=5";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new List<Position>();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Position>>(json) ?? new List<Position>();
    }
}