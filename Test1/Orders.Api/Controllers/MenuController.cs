using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Orders.Api.Controllers
{
    [ApiController]
    [Route("api/v{version}/menu")]
    public class MenuController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public MenuController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<MenuItemDtoIEnumerableApiResponse>> GetAll([FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            var baseUrl = _configuration["MenuApi:BaseUrl"]; // Asegúrate de configurar esto en appsettings.json
            var version = "1.0"; // Puedes obtenerlo dinámicamente si lo necesitas
            var url = $"{baseUrl}/api/v{version}/menu?page={page}&size={size}"; // Ajusta según los parámetros que soporte tu API
            var client = _httpClientFactory.CreateClient(); // Usa un cliente nombrado si es necesario
            var response = await client.GetAsync(url); // Realiza la llamada a la API externa
            if (!response.IsSuccessStatusCode) // Maneja errores adecuadamente
                return StatusCode((int)response.StatusCode, new MenuItemDtoIEnumerableApiResponse { Success = false, Message = "Error al obtener los menús.", Data = null }); // Ajusta el mensaje según sea necesario
            var json = await response.Content.ReadAsStringAsync(); // Lee el contenido de la respuesta
            var result = JsonSerializer.Deserialize<MenuItemDtoIEnumerableApiResponse>(json, _jsonOptions); // Deserializa el JSON a tu DTO
            return Ok(result); // Devuelve el resultado
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemDtoApiResponse>> GetById(int id)
        {
            var baseUrl = _configuration["MenuApi:BaseUrl"];
            var version = "1.0";
            var url = $"{baseUrl}/api/v{version}/menu/{id}";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, new MenuItemDtoApiResponse { Success = false, Message = "Error al obtener el menú.", Data = null });
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MenuItemDtoApiResponse>(json, _jsonOptions);
            return Ok(result);
        }
    }

    // DTOs según el OpenAPI
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
    }

    public class MenuItemDtoApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public MenuItemDto? Data { get; set; }
    }

    public class MenuItemDtoIEnumerableApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<MenuItemDto>? Data { get; set; }
    }
}
