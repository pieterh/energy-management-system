using System;
using System.IO.Compression;
using EMS.BlazorWasm.Client.Services;
using System.Text;
using Microsoft.JSInterop;

namespace EMS.BlazorWasm.Services
{
	public class LocalStorageService : ILocalStorage
	{
        private readonly IJSRuntime jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
		{
            this.jsRuntime = jsRuntime;
        }

        public async Task RemoveAsync(string key)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key).ConfigureAwait(false);
        }

        public async Task SaveStringAsync(string key, string value)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value).ConfigureAwait(false);
        }

        public async Task SaveStringCompressedAsync(string key, string value)
        {
            var compressedBytes = await GZip.CompressBytesAsync(Encoding.UTF8.GetBytes(value));
            var b64String = Convert.ToBase64String(compressedBytes);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, b64String).ConfigureAwait(false);
        }

        public async Task<string> GetStringAsync(string key)
        {
            var value = await jsRuntime.InvokeAsync<string>("localStorage.getItem", key).ConfigureAwait(false);
            return value;
        }

        public async Task<string> GetStringCompressedAsync(string key)
        {
            var b64String = await jsRuntime.InvokeAsync<string>("localStorage.getItem", key).ConfigureAwait(false);
            var bytes = await GZip.DecompressBytesAsync(Convert.FromBase64String(b64String));
            var value = Encoding.UTF8.GetString(bytes);
            return value;
        }

        /* TODO: optimize by not re-using the SaveStringCompressedAsync and GetStringCompressedAsync methods */
        public async Task SaveObjectAsync(string key, object value)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(value);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
            string b64String = System.Convert.ToBase64String(data);
            await SaveStringCompressedAsync(key, b64String);
        }

        public async Task<T?> GetObjectAsync<T>(string key)
        {
            string b64String = await GetStringCompressedAsync(key);
            if (string.IsNullOrWhiteSpace(b64String))
                return default(T);
            byte[] data = System.Convert.FromBase64String(b64String);
            string jsonString = System.Text.Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<T>(jsonString);
        }
    }

    internal class GZip
    {
        public static async Task<byte[]> CompressBytesAsync(byte[] bytes)
        {
            using var outputStream = new MemoryStream();
            using var compressionStream = new GZipStream(outputStream, CompressionLevel.Optimal);                
            await compressionStream.WriteAsync(bytes);                
            return outputStream.ToArray();            
        }

        public static async Task<byte[]> DecompressBytesAsync(byte[] bytes)
        {
            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            using var compressionStream = new GZipStream(inputStream, CompressionMode.Decompress);
            await compressionStream.CopyToAsync(outputStream);                    
            return outputStream.ToArray();
        }
    }
}
