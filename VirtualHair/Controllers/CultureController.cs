using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;

namespace VirtualHair.Controllers
{
    public class CultureController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public CultureController(IWebHostEnvironment env) => _env = env;

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public IActionResult GetTranslations(string culture)
        {
            if (culture != "en" && culture != "bg") culture = "en";
            var filePath = Path.Combine(_env.ContentRootPath, "translations.json");
            if (!System.IO.File.Exists(filePath)) return NotFound();
            
            var json = System.IO.File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

            if (data != null && data.TryGetValue(culture, out var items))
                return Ok(items);
                
            return NotFound();
        }
    }
}
