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

        public AiController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public class AiGenerateRequest
        {
            public string OriginalImageBase64 { get; set; } = string.Empty;
            public string MaskImageBase64 { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
            public string Type { get; set; } = "hair"; // hair or beard
        }

        [HttpPost("hair-edit")]
        public async Task<IActionResult> GenerateHairEdit([FromBody] AiGenerateRequest request)
        {
            if (string.IsNullOrEmpty(request.OriginalImageBase64) || string.IsNullOrEmpty(request.MaskImageBase64))
            {
                return BadRequest(new { message = "Image and mask are required." });
            }

            try
            {
                // This is where you would call an external API like Replicate (Stable Diffusion Inpainting)
                // Example architecture for Replicate:
                
                var replicateApiKey = _configuration["ReplicateApiKey"];
                if (!string.IsNullOrEmpty(replicateApiKey))
                {
                    /* REAL IMPLEMENTATION COMMENTED OUT UNTIL API KEY IS ADDED
                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", replicateApiKey);
                    
                    var payload = new
                    {
                        version = "stability-ai/stable-diffusion-inpainting", // Example Replicate model
                        input = new
                        {
                            image = request.OriginalImageBase64,
                            mask = request.MaskImageBase64,
                            prompt = request.Prompt + ", ultra realistic, high detail, 8k resolution, photorealistic",
                            negative_prompt = "cartoon, fake, drawing, illustration, deformed, bad anatomy",
                            num_inference_steps = 30,
                            guidance_scale = 7.5
                        }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://api.replicate.com/v1/predictions", content);
                    
                    // Polling logic for prediction result would go here
                    // return Ok(new { imageUrl = finalImageUrl });
                    */
                }

                // --- MOCK RESPONSE FOR LOCAL TESTING ---
                // Simulating AI processing time
                await Task.Delay(3500);

                // Note: For now, we return the original image back as a successful mock, 
                // but in production, this would be the URL of the newly generated AI image.
                return Ok(new { 
                    success = true, 
                    imageUrl = request.OriginalImageBase64,
                    message = "AI generation simulated successfully (Add Replicate API key for real generation)."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "AI Generation failed: " + ex.Message });
            }
        }
    }
}
