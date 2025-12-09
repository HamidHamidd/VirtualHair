using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
