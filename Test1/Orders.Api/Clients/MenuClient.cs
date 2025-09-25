using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Orders.Api.Clients
{
    public class MenuClient : IMenuClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        private readonly string _baseUrl;

        public MenuClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Puedes configurar la base URL aquí si es necesario
            _baseUrl = httpClient.BaseAddress?.ToString() ?? "";
        }

        public async Task<MenuItemDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/api/v1.0/menu/{id}";
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var apiResponse = JsonSerializer.Deserialize<MenuItemDtoApiResponse>(json, _jsonOptions);
            return apiResponse?.Data;
        }

        public async Task<List<MenuItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var url = $"{_baseUrl}api/v1.0/menu";
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var apiResponse = JsonSerializer.Deserialize<MenuItemDtoIEnumerableApiResponse>(json, _jsonOptions);
            return apiResponse?.Data ?? new List<MenuItemDto>();
        }

        public async Task<int> CreateAsync(CreateMenuItemDto dto, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}api/v1.0/menu";
            var content = new StringContent(JsonSerializer.Serialize(dto, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var apiResponse = JsonSerializer.Deserialize<ObjectApiResponse>(json, _jsonOptions);
            return apiResponse?.Data is int id ? id : 0;
        }

        public async Task UpdateAsync(int id, UpdateMenuItemDto dto, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}api/v1.0/menu/{id}";
            var content = new StringContent(JsonSerializer.Serialize(dto, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}api/v1.0/menu/{id}";
            var response = await _httpClient.DeleteAsync(url, ct);
            response.EnsureSuccessStatusCode();
        }

        // DTOs de respuesta para deserialización interna
        private class MenuItemDtoApiResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public MenuItemDto? Data { get; set; }
        }
        private class MenuItemDtoIEnumerableApiResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public List<MenuItemDto>? Data { get; set; }
        }
        private class ObjectApiResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public object? Data { get; set; }
        }
    }
}
