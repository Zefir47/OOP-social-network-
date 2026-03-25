using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoApp.Data;
using PhotoApp.Models;
using System.Threading.Tasks;

namespace PhotoApp.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Follow(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == id)
                return BadRequest();

            bool alreadyExists = await _db.Follows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

            if (!alreadyExists)
            {
                var follow = new Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = id
                };

                _db.Follows.Add(follow);
                await _db.SaveChangesAsync();
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Unfollow(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var follow = await _db.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

            if (follow != null)
            {
                _db.Follows.Remove(follow);
                await _db.SaveChangesAsync();
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}