using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Data;
using VirtualHair.Models;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public ProfileController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var hasPhoto = await _db.UserPhotos.AnyAsync(x => x.UserId == user.Id);

            var vm = new ProfileViewModel
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                NewUserName = user.UserName ?? string.Empty,
                HasPhoto = hasPhoto
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserName(ProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var currentUserName = user.UserName ?? string.Empty;
            var email = user.Email ?? string.Empty;

            var newName = (vm.NewUserName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                ModelState.AddModelError(nameof(vm.NewUserName), "Username is required.");
            }
            else if (newName.Length < 3)
            {
                ModelState.AddModelError(nameof(vm.NewUserName), "Username must be at least 3 characters.");
            }

            if (ModelState.IsValid)
            {
                var existing = await _userManager.FindByNameAsync(newName);
                if (existing != null && existing.Id != user.Id)
                {
                    ModelState.AddModelError(nameof(vm.NewUserName), "This username is already taken.");
                }
            }

            if (!ModelState.IsValid)
            {
                vm.UserName = currentUserName;
                vm.Email = email;
                vm.HasPhoto = await _db.UserPhotos.AnyAsync(x => x.UserId == user.Id);
                return View("Index", vm);
            }

            if (string.Equals(currentUserName, newName, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ProfileMsg"] = "No changes.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.SetUserNameAsync(user, newName);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);

                vm.UserName = currentUserName;
                vm.Email = email;
                vm.HasPhoto = await _db.UserPhotos.AnyAsync(x => x.UserId == user.Id);
                return View("Index", vm);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["ProfileMsg"] = "Username updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/UploadPhoto
        // Приема вече изрязаната снимка като dataURL (base64)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(string croppedImageBase64)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(croppedImageBase64))
            {
                TempData["ProfileMsg"] = "Please crop a photo first.";
                return RedirectToAction(nameof(Index));
            }

            // Очакваме: data:image/png;base64,....
            const string prefixPng = "data:image/png;base64,";
            const string prefixJpg = "data:image/jpeg;base64,";
            const string prefixJpg2 = "data:image/jpg;base64,";

            string contentType;
            string base64;

            if (croppedImageBase64.StartsWith(prefixPng, StringComparison.OrdinalIgnoreCase))
            {
                contentType = "image/png";
                base64 = croppedImageBase64.Substring(prefixPng.Length);
            }
            else if (croppedImageBase64.StartsWith(prefixJpg, StringComparison.OrdinalIgnoreCase))
            {
                contentType = "image/jpeg";
                base64 = croppedImageBase64.Substring(prefixJpg.Length);
            }
            else if (croppedImageBase64.StartsWith(prefixJpg2, StringComparison.OrdinalIgnoreCase))
            {
                contentType = "image/jpeg";
                base64 = croppedImageBase64.Substring(prefixJpg2.Length);
            }
            else
            {
                TempData["ProfileMsg"] = "Invalid image data.";
                return RedirectToAction(nameof(Index));
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch
            {
                TempData["ProfileMsg"] = "Invalid image data.";
                return RedirectToAction(nameof(Index));
            }

            // лимит 2MB
            const int maxBytes = 2 * 1024 * 1024;
            if (bytes.Length > maxBytes)
            {
                TempData["ProfileMsg"] = "Cropped photo is too large. Max size is 2 MB.";
                return RedirectToAction(nameof(Index));
            }

            // Upsert + трием старите
            var existing = await _db.UserPhotos
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                existing = new UserPhoto { UserId = user.Id };
                _db.UserPhotos.Add(existing);
            }

            existing.ImageData = bytes;
            existing.ContentType = contentType;
            existing.FileName = "profile.png";
            existing.CreatedAt = DateTime.UtcNow;

            var oldOnes = await _db.UserPhotos
                .Where(x => x.UserId == user.Id && x.Id != existing.Id)
                .ToListAsync();

            if (oldOnes.Count > 0)
                _db.UserPhotos.RemoveRange(oldOnes);

            await _db.SaveChangesAsync();

            TempData["ProfileMsg"] = "Profile photo updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Photo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var photo = await _db.UserPhotos
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (photo == null || photo.ImageData == null || photo.ImageData.Length == 0)
                return NotFound();

            return File(photo.ImageData, photo.ContentType);
        }
    }

    public class ProfileViewModel
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";

        [Required]
        [StringLength(60, MinimumLength = 3)]
        public string NewUserName { get; set; } = "";

        public bool HasPhoto { get; set; }
    }
}
