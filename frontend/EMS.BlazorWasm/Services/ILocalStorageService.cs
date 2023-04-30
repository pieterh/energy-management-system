namespace EMS.BlazorWasm.Services;

public interface ILocalStorage
{
    Task SaveStringAsync(string key, string value);
    Task SaveStringCompressedAsync(string key, string value);
    Task<string> GetStringAsync(string key);
    Task<string> GetStringCompressedAsync(string key);
    Task SaveObjectAsync(string key, object value);
    Task<T?> GetObjectAsync<T>(string key);
    Task RemoveAsync(string key);
}