using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VirtualHair.Data;
using VirtualHair.Models;
using System.Collections.Generic;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public FriendsController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var friendships = await _context.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .Where(f => f.RequesterId == currentUser.Id || f.AddresseeId == currentUser.Id)
                .ToListAsync();

            var allUsers = await _userManager.Users
                .Where(u => u.Id != currentUser.Id)
                .ToListAsync();

            // Extract IDs
            var myFriendsIds = friendships
                .Where(f => f.IsAccepted)
                .Select(f => f.RequesterId == currentUser.Id ? f.AddresseeId : f.RequesterId)
                .ToList();

            var sentRequestIds = friendships
                .Where(f => !f.IsAccepted && f.RequesterId == currentUser.Id)
                .Select(f => f.AddresseeId)
                .ToList();

            var receivedRequestIds = friendships
                .Where(f => !f.IsAccepted && f.AddresseeId == currentUser.Id)
                .Select(f => f.RequesterId)
                .ToList();

            ViewBag.Friends = allUsers.Where(u => myFriendsIds.Contains(u.Id)).ToList();
            ViewBag.SentRequests = allUsers.Where(u => sentRequestIds.Contains(u.Id)).ToList();
            ViewBag.ReceivedRequests = allUsers.Where(u => receivedRequestIds.Contains(u.Id)).ToList();
            
            var involvedIds = myFriendsIds.Concat(sentRequestIds).Concat(receivedRequestIds).ToList();
            ViewBag.Suggestions = allUsers.Where(u => !involvedIds.Contains(u.Id)).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(string targetId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id == targetId) return BadRequest();

            var existing = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.RequesterId == currentUser.Id && f.AddresseeId == targetId) ||
                    (f.RequesterId == targetId && f.AddresseeId == currentUser.Id));

            if (existing != null)
            {
                // If target already requested, accept it
                if (!existing.IsAccepted && existing.AddresseeId == currentUser.Id)
                {
                    existing.IsAccepted = true;
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }

            var friendship = new Friendship
            {
                RequesterId = currentUser.Id,
                AddresseeId = targetId,
                IsAccepted = false
            };
            
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AcceptFriend(string targetId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return BadRequest();

            var request = await _context.Friendships
                .FirstOrDefaultAsync(f => f.RequesterId == targetId && f.AddresseeId == currentUser.Id && !f.IsAccepted);

            if (request != null)
            {
                request.IsAccepted = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string targetId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return BadRequest();

            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.RequesterId == currentUser.Id && f.AddresseeId == targetId) ||
                    (f.RequesterId == targetId && f.AddresseeId == currentUser.Id));

            if (friendship != null)
            {
                _context.Friendships.Remove(friendship);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
