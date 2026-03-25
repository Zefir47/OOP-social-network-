using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoApp.Data;
using PhotoApp.Models;

namespace PhotoApp.Controllers
{
    [Authorize]
    public class PostInteractionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PostInteractionController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // ─── REPLY ────────────────────────────────────────────────────────────

     
        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            var original = await _db.ImagePosts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (original == null) return NotFound();

            ViewBag.OriginalPost = original;
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Reply(int id, string description)
        {
            var currentUserId = _userManager.GetUserId(User);

            var original = await _db.ImagePosts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (original == null) return NotFound();

            // Create new post that references the original image
            var replyPost = new ImagePost
            {
                ImagePath = original.ImagePath,
                Description = description ?? "",
                UserId = currentUserId,
                CreatedAt = DateTime.Now
            };

            _db.ImagePosts.Add(replyPost);
            await _db.SaveChangesAsync();

            var interaction = new PostInteraction
            {
                Type = InteractionType.Reply,
                OriginalPostId = original.Id,
                OriginalAuthorId = original.UserId,
                ActorId = currentUserId,
                ReplyPostId = replyPost.Id,
                CreatedAt = DateTime.Now
            };

            _db.PostInteractions.Add(interaction);
            await _db.SaveChangesAsync();

            return RedirectToAction("MyPosts", "Image");
        }

        // ─── FORWARD ──────────────────────────────────────────────────────────

       
        [HttpGet]
        public async Task<IActionResult> Forward(int id)
        {
            var original = await _db.ImagePosts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (original == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            // Cannot forward own post
            if (original.UserId == currentUserId)
            {
                TempData["Error"] = "Не можна пересилати власну публікацію.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Cannot forward duplicate
            bool alreadyForwarded = await _db.PostInteractions.AnyAsync(pi =>
                pi.Type == InteractionType.Forward &&
                pi.OriginalPostId == id &&
                pi.ActorId == currentUserId);

            if (alreadyForwarded)
            {
                TempData["Error"] = "Ви вже пересилали цю публікацію.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            ViewBag.OriginalPost = original;
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Forward(int id, string comment)
        {
            var currentUserId = _userManager.GetUserId(User);

            var original = await _db.ImagePosts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (original == null) return NotFound();

            if (original.UserId == currentUserId)
            {
                TempData["Error"] = "Не можна пересилати власну публікацію.";
                return RedirectToAction("MyPosts", "Image");
            }

            bool alreadyForwarded = await _db.PostInteractions.AnyAsync(pi =>
                pi.Type == InteractionType.Forward &&
                pi.OriginalPostId == id &&
                pi.ActorId == currentUserId);

            if (alreadyForwarded)
            {
                TempData["Error"] = "Ви вже пересилали цю публікацію.";
                return RedirectToAction("MyPosts", "Image");
            }

            var interaction = new PostInteraction
            {
                Type = InteractionType.Forward,
                OriginalPostId = original.Id,
                OriginalAuthorId = original.UserId,
                ActorId = currentUserId,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _db.PostInteractions.Add(interaction);
            await _db.SaveChangesAsync();

            return RedirectToAction("MyPosts", "Image");
        }
    }
}
