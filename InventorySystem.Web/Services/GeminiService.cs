using Google.GenAI;
using Google.GenAI.Types;

namespace InventorySystem.Web.Services
{
    public class GeminiService
    {
        private readonly Client _client;

        public GeminiService(IConfiguration configuration)
        {
            var apiKey = configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Gemini:ApiKey no está configurada.");

            _client = new Client(apiKey: apiKey);
        }

        // Llama a Gemini usando un prompt de usuario y un contexto opcional.
        public async Task<string> GenerateResponseAsync(string userMessage, string? extraContext = null)
        {
            try
            {
                const string model = "gemini-2.0-flash";

                // Mensaje del sistema: reglas del chatbot
                var systemInstruction = new Content
                {
                    Parts = new List<Part>
                    {
                        new Part
                        {
                            Text =
@"Eres ""Chat IT – Real Alloy"", un asistente virtual de soporte técnico interno.
Tu función es asistir a los empleados de Real Alloy:

- Respondiendo preguntas sobre equipos asignados por la empresa (laptops, desktops, impresoras, etc.).
- Brindando pasos de diagnóstico y solución (1, 2, 3…) de forma clara.
- NO debes inventar información ni generar datos ficticios.
- No debes dar respuestas financieras, administrativas ni de otra área.
- Usa SIEMPRE la información real proveniente de la base de datos cuando esté disponible.
- Si el usuario menciona un equipo (tag, serie), debes usar esos datos como contexto.

Si el usuario no menciona equipo, responde como guía de soporte técnico general. No menciones Mac. Solo son equipos Lenovo o DELL.

FORMATO ESTRICTO DE RESPUESTA (MUY IMPORTANTE):
- No uses negritas, asteriscos ni markdown.
- No uses emojis.
- Separa la información en párrafos claros.
- NUNCA regreses todo en un solo párrafo.

CUANDO TENGAS DATOS DEL EQUIPO, RESPONDE SIEMPRE ASÍ:

Datos del equipo:
Tipo: ...
Marca: ...
Modelo: ...
Serie: ...
Ubicación: ...
Responsable: ...
Estado: ...
Windows 11: ...

Recomendaciones:
1. ...
2. ...
3. ... (solo si es necesario)

REGLAS GENERALES:
- Respeta exactamente ese formato de líneas (cada campo en su propia línea).
- No pongas varios campos en la misma línea.
- Si falta algún dato en la base de datos, escribe por ejemplo ""Tipo: sin tipo"", ""Marca: sin marca"", etc.
- Después de la sección ""Recomendaciones:"" puedes agregar más explicación en uno o más párrafos, siempre separados por saltos de línea."
                        }
                    }
                };

                var fullPrompt = string.IsNullOrWhiteSpace(extraContext)
                    ? userMessage
                    : $"{extraContext}\n\nPregunta del empleado: {userMessage}";

                var config = new GenerateContentConfig
                {
                    SystemInstruction = systemInstruction,
                    Temperature = 0.4,
                    MaxOutputTokens = 512
                };

                var response = await _client.Models.GenerateContentAsync(
                    model: model,
                    contents: fullPrompt,
                    config: config
                );

                var text = response?
                    .Candidates?
                    .FirstOrDefault()?
                    .Content?
                    .Parts?
                    .FirstOrDefault()?
                    .Text;

                return text ?? "Lo siento, no pude generar una respuesta en este momento.";
            }
            catch (Exception ex)
            {
                return $"Error al llamar a Gemini: {ex.Message}";
            }
        }
    }
}
