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
    public class MessagesController : Controller
    {
        private readonly MessageBoardContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(MessageBoardContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var messages = _context.Messages
                .Include(m => m.Author)
                .OrderBy(m => m.ExpirationDate)
                .ToList()
                .Where(m => m.IsActive()); // LINQ Where(), not EF Where()

            return View(messages);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AllMessages()
        {
            var allMessages = new Dictionary<string, List<Message>>()
            {
                { "active" , new List<Message>() },
                { "expired", new List<Message>() }
            };

            foreach (var message in _context.Messages)
            {
                if (message.IsActive())
                {
                    allMessages["active"].Add(message);
                }
                else
                {
                    Log.Information($"I : {message.Id} Message Expired");
                    allMessages["expired"].Add(message);
                }
            }


            return View(allMessages);
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(MessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var user = _context.ApplicationUsers.Find(userId);

                bool isSuperUser = User.IsInRole("SuperUser");

                var message = new Message
                {
                    Author = user,
                    Content = model.Content,
                    ExpirationDate = model.GetExpirationDate()
                };

                _context.Messages.Add(message);

                Log.Information($"Message Created with contents of: {model.Content} by user: {user}");

                _context.SaveChanges();

                if(isSuperUser)
                {
                    return RedirectToAction("Index", "SuperUser");
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [Authorize(Roles = "SuperUser")]
        public IActionResult Edit(int id)
        {
            var message = _context.Messages.Find(id);

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        public IActionResult Edit(int id, Message editedMessage)
        {
            if (ModelState.IsValid)
            {
                var messageToUpdate = _context.Messages.Find(id);
                if (messageToUpdate == null)
                {
                    return NotFound();
                }

                messageToUpdate.Content = editedMessage.Content;
                messageToUpdate.ExpirationDate = editedMessage.ExpirationDate;

                _context.Update(messageToUpdate);
                _context.SaveChanges();

                return RedirectToAction("Index", "SuperUser");
            }

            return View(editedMessage);
        }

        [Authorize(Roles = "SuperUser")]
        public IActionResult Delete(int id)
        {
            var message = _context.Messages.Find(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            _context.SaveChanges();

            return RedirectToAction("Index", "SuperUser");
        }
    }
}
