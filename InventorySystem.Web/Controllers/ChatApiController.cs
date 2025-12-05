using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Web.Data;
using InventorySystem.Web.Data.Entities;
using InventorySystem.Web.Services;

namespace InventorySystem.Web.Controllers
{
    [ApiController]
    [Route("api/chatbot")]
    public class ChatApiController : ControllerBase
    {
        private readonly GeminiService _gemini;
        private readonly InventoryContext _db;

        public ChatApiController(GeminiService gemini, InventoryContext db)
        {
            _gemini = gemini;
            _db = db;
        }

        // Lo que envía tu JS: { sessionId, text }
        public record ChatRequest(string? SessionId, string Text);

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { error = "Mensaje vacío" });

            var userText = request.Text.Trim();

            // Intentar detectar si mencionan un equipo
            var asset = await FindAssetInMessage(userText);

            if (asset != null)
            {
                // Creamos contexto con datos del equipo
                var context = BuildAssetContext(asset);

                // Le pasamos contexto + pregunta a Gemini
                var reply = await _gemini.GenerateResponseAsync(userText, context);

                return Ok(new { answer = reply });
            }

            // Si no hay equipo, sólo preguntamos a Gemini normal
            var normalReply = await _gemini.GenerateResponseAsync(userText);

            return Ok(new { answer = normalReply });
        }

        // Busca en el mensaje algún AssetTag o SerialNumber que exista en la BD.
        private async Task<Asset?> FindAssetInMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var tokens = message
                .ToUpper()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var token in tokens)
            {
                var asset = await _db.Assets
                    .Where(a =>
                        a.AssetTag.ToUpper() == token ||
                        (a.SerialNumber != null && a.SerialNumber.ToUpper() == token))
                    .FirstOrDefaultAsync();

                if (asset != null)
                    return asset;
            }

            // Caso extra: si sólo escribieron una palabra, ya lo revisamos arriba,
            // así que no hace falta otra búsqueda especial.
            return null;
        }

        /// Construye el texto con los datos del equipo para dárselo a Gemini como contexto.
        private static string BuildAssetContext(Asset a)
        {
            var tipo = a.AssetType?.Name ?? "sin tipo";
            var marca = a.Brand?.Name ?? "sin marca";
            var modelo = !string.IsNullOrWhiteSpace(a.ModelText) ? a.ModelText : "sin modelo";

            var serie = string.IsNullOrWhiteSpace(a.SerialNumber)
                ? "sin número de serie"
                : a.SerialNumber;

            var ubicacion = a.Location?.Name ?? "sin ubicación";
            var responsable = string.IsNullOrWhiteSpace(a.Assignee)
                ? "sin responsable"
                : a.Assignee;

            var win11 = a.Win11Status?.Name ?? "sin registro";
            var estado = a.EqStatus?.Name ?? "sin estado";

            return
                $"Información del equipo desde la base de datos:\n" +
                $"- AssetTag: {a.AssetTag}\n" +
                $"- Tipo: {tipo}\n" +
                $"- Marca: {marca}\n" +
                $"- Modelo: {modelo}\n" +
                $"- Serie: {serie}\n" +
                $"- Ubicación: {ubicacion}\n" +
                $"- Responsable: {responsable}\n" +
                $"- Estado: {estado}\n" +
                $"- Windows 11: {win11}\n\n" +
                "Con esta información, explícale al empleado en qué estado está su equipo " +
                "y dale recomendaciones específicas según lo que te esté preguntando.";
        }
    }
}
