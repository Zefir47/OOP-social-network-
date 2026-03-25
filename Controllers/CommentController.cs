using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoApp.Data;
using PhotoApp.Models;

namespace PhotoApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int imagePostId, string content, int? parentId)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
                return Redirect(Request.Headers["Referer"].ToString());

            var comment = new Comment
            {
                Content = content,
                ImagePostId = imagePostId,
                AuthorId = _userManager.GetUserId(User),
                ParentId = parentId,
                CreatedAt = DateTime.Now
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // ===== ВІДКРИТИ РЕДАГУВАННЯ =====
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string returnUrl)
        {
            var comment = await _db.Comments.FindAsync(id);

            if (comment == null)
                return NotFound();

            if (comment.AuthorId != _userManager.GetUserId(User))
                return Unauthorized();

            ViewBag.ReturnUrl = returnUrl;

            return View(comment);
        }

        // ===== ЗБЕРЕГТИ РЕДАГУВАННЯ =====
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string content, string returnUrl)
        {
            var comment = await _db.Comments.FindAsync(id);

            if (comment == null)
                return NotFound();

            if (comment.AuthorId != _userManager.GetUserId(User))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
                return Redirect(returnUrl ?? "/");

            comment.Content = content;
            comment.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Redirect(returnUrl ?? "/");
        }

        // ===== ПОВНЕ ВИДАЛЕННЯ =====
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _db.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null || comment.AuthorId != _userManager.GetUserId(User))
                return Unauthorized();

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}