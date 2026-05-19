using System.ComponentModel;
using System.Globalization;

public static class ResourceManager
{
    public static ICollection<ResourceDictionary> MergedDictionaries =>
        Application.Current.Resources.MergedDictionaries;

    public static object? Get(string key)
        => Application.Current.Resources.TryGetValue(key, out var value) ? value : null;

    public static async Task<T?> Load<T>(string key)
    {
        var storedString = await SecureStorage.GetAsync(key);
        if (storedString is null) return default;

        try
        {
            T? value = ConvertFromString<T>(storedString);
            if (value is not null)
                Set(key, value);
            return value;
        }
        catch
        {
            return default;
        }
    }

    public static async Task SetSave<T>(string key, T value)
    {
        Set(key, value);
        await Save(key, value);
    }

    public static void Set<T>(string key, T value)
    {
        Application.Current.Resources[key] = value;
    }

    public static async Task Save<T>(string key, T value)
    {
        string data = value?.ToString() ?? string.Empty;
        await SecureStorage.SetAsync(key, data);
    }

    private static T? ConvertFromString<T>(string input)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)input;
        Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        object? converted = Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
        return (T?)converted;
    }
}