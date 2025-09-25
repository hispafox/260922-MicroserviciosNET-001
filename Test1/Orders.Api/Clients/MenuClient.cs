using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Orders.Api.Clients
{
    public class MenuClient : IMenuClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        private readonly string _baseUrl;

        public MenuClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _baseUrl = _configuration["MenuApi:BaseUrl"] ?? "";
        }

        public async Task<MenuItemDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/api/v1.0/menu/{id}";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var apiResponse = JsonSerializer.Deserialize<MenuItemDtoApiResponse>(json, _jsonOptions);
            return apiResponse?.Data;
        }

        public async Task<List<MenuItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/api/v1.0/menu";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var apiResponse = JsonSerializer.Deserialize<MenuItemDtoIEnumerableApiResponse>(json, _jsonOptions);
            return apiResponse?.Data ?? new List<MenuItemDto>();
        }

        public async Task<int> CreateAsync(CreateMenuItemDto dto, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/api/v1.0/menu";
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(dto, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var apiResponse = JsonSerializer.Deserialize<ObjectApiResponse>(json, _jsonOptions);
            // Suponiendo que el id se retorna en Data
            return apiResponse?.Data is int id ? id : 0;
        }

        public async Task UpdateAsync(int id, UpdateMenuItemDto dto, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/api/v1.0/menu/{id}";
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(dto, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/api/v1.0/menu/{id}";
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync(url, ct);
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
