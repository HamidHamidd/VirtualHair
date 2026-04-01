using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VirtualHair.Data;
using VirtualHair.Models;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MessagesController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Redirect("/Identity/Account/Login");

            var users = await _userManager.Users
                .Where(u => u.Id != currentUser.Id)
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetChat(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Friendship check
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.RequesterId == currentUser.Id && f.AddresseeId == userId) || 
                    (f.RequesterId == userId && f.AddresseeId == currentUser.Id));

            bool isFriend = friendship?.IsAccepted ?? false;
            bool isPending = friendship != null && !friendship.IsAccepted;
            bool amRequester = friendship?.RequesterId == currentUser.Id;

            // Mark unread incoming messages from this user as read
            var unreadMessages = await _context.Messages
                .Where(m => m.ReceiverId == currentUser.Id && m.SenderId == userId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages) msg.IsRead = true;
                await _context.SaveChangesAsync();
            }

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == userId) || 
                            (m.SenderId == userId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .Select(m => new {
                    id = m.Id,
                    content = m.Content,
                    senderId = m.SenderId,
                    sentAt = m.SentAt,
                    isMine = m.SenderId == currentUser.Id
                })
                .ToListAsync();

            var otherUser = await _userManager.FindByIdAsync(userId);
            return Json(new { 
                messages, 
                otherUser = otherUser?.UserName,
                isFriend,
                isPending,
                amRequester,
                otherUserId = userId
            });
        }

        public class SendMessageRequest
        {
            public string ReceiverId { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(req.Content)) return BadRequest();

            // Logic: Can only send if friends or if it's the VERY FIRST message (invitation)
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.RequesterId == currentUser.Id && f.AddresseeId == req.ReceiverId) || 
                    (f.RequesterId == req.ReceiverId && f.AddresseeId == currentUser.Id));

            if (friendship == null)
            {
                // NO previous contact. This is the INVITATION message.
                // Create pending friendship
                _context.Friendships.Add(new Friendship
                {
                    RequesterId = currentUser.Id,
                    AddresseeId = req.ReceiverId,
                    IsAccepted = false
                });
            }
            else if (!friendship.IsAccepted)
            {
                // Contact exists but not accepted.
                // If I am the one who sent the request, I can't send more messages.
                // Exception: if the other person hasn't responded yet, we only allow 1 message.
                var existingMessagesCount = await _context.Messages
                    .CountAsync(m => m.SenderId == currentUser.Id && m.ReceiverId == req.ReceiverId);

                if (existingMessagesCount >= 1) {
                    return Json(new { success = false, message = "You must wait for the other user to accept your invitation before sending more messages." });
                }
            }

            var msg = new Message
            {
                SenderId = currentUser.Id,
                ReceiverId = req.ReceiverId,
                Content = req.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                msg = new {
                    id = msg.Id,
                    content = msg.Content,
                    senderId = msg.SenderId,
                    sentAt = msg.SentAt,
                    isMine = true
                }
            });
        }
    }
}
