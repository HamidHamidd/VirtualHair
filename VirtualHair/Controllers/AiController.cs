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
            if (string.IsNullOrEmpty(replicateApiKey)) return Ok(new { success = true, imageUrl = request.OriginalImageBase64 });

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", replicateApiKey);
            
            // Using SDXL Lightning Inpainting (High Speed + Quality)
            var payload = new {
                version = "f2922ef653fb939316d8a39e8e50b6a7ee7b8f9e67ca08cc83b54af3a0b5f1de", 
                input = new {
                    image = request.OriginalImageBase64,
                    mask = request.MaskImageBase64,
                    prompt = request.Prompt,
                    negative_prompt = request.NegativePrompt,
                    num_inference_steps = 10, // Lightning is fast
                    guidance_scale = 1.0,      // Optimized for Lightning
                    width = 1024,
                    height = 1024
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
                if (status == "failed") return BadRequest(new { success = false, message = "AI Failed" });
            }
            return StatusCode(408, new { success = false, message = "Timeout" });
        }
    }
}
