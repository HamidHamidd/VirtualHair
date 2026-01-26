using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = new ProfileViewModel
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? ""
            };

            return View(vm);
        }
    }

    public class ProfileViewModel
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
