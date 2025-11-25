using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TesseractOcrTest.Services;

public class AiCleanupService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaEndpoint;
    private readonly string _model;

    public AiCleanupService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(2); // Increase timeout
        _ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        _model = configuration["Ollama:Model"] ?? "llama3.2:latest";
    }

    public async Task<string> CleanupOcrTextAsync(string messyText)
    {
        try
        {
            var prompt = $@"You are an expert at organizing messy OCR text into clear, structured markdown.

The text below was extracted from a document using OCR and is poorly formatted.
Your task: Organize this into clean, well-structured markdown that clearly shows:
- What the main categories are
- What subcategories exist  
- What data belongs to each category
- The relationships between different pieces of information

Use markdown headings (# ## ###), lists, and simple formatting to make the structure crystal clear.
If there's a table, you can use markdown table syntax, but focus on CLARITY and CORRECT DATA PLACEMENT over perfect formatting.

OCR Output:
{messyText}

Respond ONLY with the cleaned, structured markdown. No explanations.";

            var request = new OllamaRequest
            {
                Model = _model,
                Prompt = prompt,
                Stream = false,
                Options = new OllamaOptions
                {
                    Temperature = 0.1f
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_ollamaEndpoint}/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Error calling Ollama API: {response.StatusCode} - {error}";
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<OllamaResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Response != null)
            {
                return result.Response;
            }

            return $"Unexpected response format:\n{responseBody}";
        }
        catch (Exception ex)
        {
            return $"Exception during AI cleanup: {ex.Message}\n\nStack trace: {ex.StackTrace}";
        }
    }

    public class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("options")]
        public OllamaOptions? Options { get; set; }
    }

    public class OllamaOptions
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }
    }

    public class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}