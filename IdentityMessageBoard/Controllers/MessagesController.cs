using IdentityMessageBoard.DataAccess;
using IdentityMessageBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace IdentityMessageBoard.Controllers
{
    public class MessagesController : Controller
    {
        private readonly MessageBoardContext _context;

        public MessagesController(MessageBoardContext context)
        {
            _context = context;
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
                var user = _context.ApplicationUsers.Find(User);

                var message = new Message
                {
                    Author = user,
                    Content = model.Content,
                    ExpirationDate = model.GetExpirationDate()
                };

                _context.Messages.Add(message);

                Log.Information($"Message Created with contents of: {model.Content} by user: {user}");

                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}
