using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VirtualHair.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiController> _logger;

        public AiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AiController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public class AiGenerateRequest
        {
            public string OriginalImageBase64 { get; set; } = string.Empty;
            public string MaskImageBase64 { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
            public string NegativePrompt { get; set; } = string.Empty;
            public string Type { get; set; } = "hair";
        }

        [HttpPost("hair-edit")]
        public async Task<IActionResult> GenerateHairEdit([FromBody] AiGenerateRequest request)
        {
            if (string.IsNullOrEmpty(request.OriginalImageBase64) || string.IsNullOrEmpty(request.MaskImageBase64))
            {
                return BadRequest(new { success = false, message = "Image/Mask missing" });
            }

            try { return await ExecuteAIWorkflowWithRetry(request, 0); }
            catch (Exception ex) { return StatusCode(500, new { success = false, message = ex.Message }); }
        }

        private async Task<IActionResult> ExecuteAIWorkflowWithRetry(AiGenerateRequest request, int attempt)
        {
            var replicateApiKey = _configuration["ReplicateApiKey"];
            if (string.IsNullOrEmpty(replicateApiKey) || replicateApiKey == "YOUR_REPLICATE_API_KEY_HERE") return Ok(new { success = true, imageUrl = request.OriginalImageBase64 });

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", replicateApiKey);
            
            // Using verified SDXL Inpainting model version hash
            var payload = new {
                version = "aca001c8b137114d5e594c68f7084ae6d82f364758aab8d997b233e8ef3c4d93",
                input = new {
                    image = request.OriginalImageBase64,
                    mask = request.MaskImageBase64,
                    prompt = request.Prompt,
                    negative_prompt = request.NegativePrompt,
                    num_inference_steps = 35,
                    guidance_scale = 15.0, // Extreme guidance to force the AI to respect color and style
                    prompt_strength = 1.0 // 1.0 means 100% replacement of the old hair
                }
            };

            var createResponse = await client.PostAsync("https://api.replicate.com/v1/predictions", 
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            if (!createResponse.IsSuccessStatusCode) {
                var err = await createResponse.Content.ReadAsStringAsync();
                return StatusCode((int)createResponse.StatusCode, new { success = false, message = "API Error: " + err });
            }

            var prediction = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            string pollUrl = prediction.GetProperty("urls").GetProperty("get").GetString()!;
            
            for (int i = 0; i < 50; i++) {
                await Task.Delay(2000);
                var pollRes = await client.GetAsync(pollUrl);
                var statusData = await pollRes.Content.ReadFromJsonAsync<JsonElement>();
                var status = statusData.GetProperty("status").GetString();

                if (status == "succeeded") {
                    var output = statusData.GetProperty("output");
                    string? finalUrl = null;
                    if (output.ValueKind == JsonValueKind.Array && output.GetArrayLength() > 0) finalUrl = output[0].GetString();
                    else if (output.ValueKind == JsonValueKind.String) finalUrl = output.GetString();
                    return Ok(new { success = true, imageUrl = finalUrl });
                }
                if (status == "failed") {
                    string errorMsg = "AI Generation Failed";
                    if (statusData.TryGetProperty("error", out var errorProp) && errorProp.ValueKind == JsonValueKind.String) {
                        errorMsg = errorProp.GetString() ?? errorMsg;
                    }
                    return BadRequest(new { success = false, message = errorMsg });
                }
            }
            return StatusCode(408, new { success = false, message = "Timeout" });
        }
    }
}
