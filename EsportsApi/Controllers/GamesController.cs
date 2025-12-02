using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EsportsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        // ¡PEGÁ TU KEY DE RAWG AQUÍ!
        private const string ApiKey = "fac179779ac74dfda500578a6eea8b66"; 
        private const string BaseUrl = "https://api.rawg.io/api/games";

        public GamesController()
        {
            _httpClient = new HttpClient();
        }

        // GET: api/Games/search?query=mario
        [HttpGet("search")]
        public async Task<IActionResult> SearchGame([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query)) return BadRequest("Escribe algo para buscar.");

            try
            {
                // 1. Tu Backend llama a la API Externa (RAWG)
                var response = await _httpClient.GetAsync($"{BaseUrl}?key={ApiKey}&search={query}&page_size=5");
                
                if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode, "Error en RAWG API");

                // 2. Leemos el JSON que nos devuelve RAWG
                var content = await response.Content.ReadAsStringAsync();
                
                // 3. (Opcional) Podríamos procesarlo, pero para cumplir, se lo pasamos al Front
                // Esto demuestra que tu backend es el intermediario.
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}