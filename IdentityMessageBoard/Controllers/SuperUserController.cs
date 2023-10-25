using IdentityMessageBoard.DataAccess;
using IdentityMessageBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace IdentityMessageBoard.Controllers
{
    [Authorize(Roles = "SuperUser")]
    public class SuperUserController : Controller
    {
        private readonly MessageBoardContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperUserController(MessageBoardContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var allUserMessages = new Dictionary<string, List<Message>>()
            {
                { "active" , new List<Message>() },
                { "expired", new List<Message>() }
            };

            foreach (var message in _context.Messages.Include(m => m.Author).Where(m => m.Author.Id == userId))
            {
                if (message.IsActive())
                {
                    allUserMessages["active"].Add(message);
                }
                else
                {
                    Log.Information($"ID : {message.Id} Message Expired");
                    allUserMessages["expired"].Add(message);
                }
            }

            return View(allUserMessages);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSuperUserRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "SuperUser");
                return RedirectToAction("Index", "Home");
            }

            return NotFound();
        }
    }
}
