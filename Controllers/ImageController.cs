using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoApp.Data;
using PhotoApp.Models;

namespace PhotoApp.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ImageController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        public IActionResult MyPosts()
        {
            var userId = _userManager.GetUserId(User);

            var posts = _db.ImagePosts
                .Include(p => p.Likes)
                .Where(p => p.UserId == userId)
                .ToList();

            return View(posts);
        }

        public IActionResult Users()
        {
            var currentUserId = _userManager.GetUserId(User);

            var users = _db.Users
                .Where(u => u.Id != currentUserId)
                .ToList();

            return View(users);
        }

        public IActionResult UserPosts(string id)
        {
            var posts = _db.ImagePosts
                .Include(p => p.Likes)
                .Where(p => p.UserId == id)
                .ToList();

            return View("MyPosts", posts);
        }

        public IActionResult Upload() => View();

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image, string description)
        {
            if (image == null || image.Length == 0) return View();

            string folder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            string path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await image.CopyToAsync(stream);

            var post = new ImagePost
            {
                ImagePath = "/uploads/" + fileName,
                Description = description,
                UserId = _userManager.GetUserId(User)
            };

            _db.ImagePosts.Add(post);
            _db.SaveChanges();

            return RedirectToAction(nameof(MyPosts));
        }

        [HttpPost]
        public IActionResult Like(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (!_db.Likes.Any(l => l.ImagePostId == id && l.UserId == userId))
            {
                _db.Likes.Add(new Like
                {
                    ImagePostId = id,
                    UserId = userId
                });
                _db.SaveChanges();
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public IActionResult Unlike(int id)
        {
            var userId = _userManager.GetUserId(User);

            var like = _db.Likes.FirstOrDefault(l =>
                l.ImagePostId == id && l.UserId == userId);

            if (like != null)
            {
                _db.Likes.Remove(like);
                _db.SaveChanges();
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }


        public IActionResult Delete(int id)
        {
            var post = _db.ImagePosts.Find(id);
            if (post == null) return NotFound();

            if (post.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            _db.ImagePosts.Remove(post);
            _db.SaveChanges();

            return RedirectToAction(nameof(MyPosts));
        }

        public IActionResult Feed()
        {
            var currentUserId = _userManager.GetUserId(User);

            var followingIds = _db.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowingId)
                .ToList();

            if (!followingIds.Any())
            {
                ViewBag.Message = "Ви ще не підписані ні на кого";
                return View(new List<ImagePost>());
            }

            var posts = _db.ImagePosts
                .Include(p => p.Likes)
                .Where(p => followingIds.Contains(p.UserId))
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(posts);
        }

        public async Task<IActionResult> Profile(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var posts = await _db.ImagePosts
    .Where(p => p.UserId == id)
    .Include(p => p.Likes)
    .Include(p => p.Comments)
    .ToListAsync();

            var followersCount = await _db.Follows
                .CountAsync(f => f.FollowingId == id);

            var followingCount = await _db.Follows
                .CountAsync(f => f.FollowerId == id);

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = user.Id;
            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;

            return View(posts);
        }

        public IActionResult Followers(string id)
        {
            var followers = _db.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => f.Follower)
                .ToList();

            ViewBag.Title = "Підписники";
            return View("FollowList", followers);
        }

        public IActionResult Following(string id)
        {
            var following = _db.Follows
                .Where(f => f.FollowerId == id)
                .Select(f => f.Following)
                .ToList();

            ViewBag.Title = "Підписки";
            return View("FollowList", following);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeedPosts(int page = 1, int pageSize = 12)
        {
            var currentUserId = _userManager.GetUserId(User);

            var followingIds = await _db.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var query = _db.ImagePosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => followingIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = await MapToDto(posts, currentUserId);

            return Json(new PaginatedResult<PostDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                HasMore = (page * pageSize) < totalCount
            });
        }

        // ── Приватний хелпер — конвертація в DTO ────────────────────
        private async Task<List<PostDto>> MapToDto(List<ImagePost> posts, string currentUserId)
        {
            var postIds = posts.Select(p => p.Id).ToList();

            var replyInteractions = await _db.PostInteractions
                .Include(pi => pi.OriginalPost).ThenInclude(op => op.User)
                .Where(pi => pi.ReplyPostId != null && postIds.Contains(pi.ReplyPostId.Value)
                             && pi.Type == InteractionType.Reply)
                .ToListAsync();

            var replyCounts = await _db.PostInteractions
                .Where(pi => postIds.Contains(pi.OriginalPostId) && pi.Type == InteractionType.Reply)
                .GroupBy(pi => pi.OriginalPostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PostId, x => x.Count);

            var forwardCounts = await _db.PostInteractions
                .Where(pi => postIds.Contains(pi.OriginalPostId) && pi.Type == InteractionType.Forward)
                .GroupBy(pi => pi.OriginalPostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PostId, x => x.Count);

            return posts.Select(p =>
            {
                var replyInfo = replyInteractions.FirstOrDefault(pi => pi.ReplyPostId == p.Id);

                return new PostDto
                {
                    Id = p.Id,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    UserId = p.UserId,
                    UserName = p.User?.UserName ?? "",
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    RepliesCount = replyCounts.GetValueOrDefault(p.Id),
                    ForwardsCount = forwardCounts.GetValueOrDefault(p.Id),
                    IsLikedByMe = p.Likes.Any(l => l.UserId == currentUserId),
                    IsOwnPost = p.UserId == currentUserId,
                    CreatedAt = p.CreatedAt.ToString("dd.MM.yyyy"),
                    IsReply = replyInfo != null,
                    OriginalPostId = replyInfo?.OriginalPostId,
                    OriginalPostImagePath = replyInfo?.OriginalPost?.ImagePath,
                    OriginalPostDescription = replyInfo?.OriginalPost?.Description,
                    OriginalPostUserId = replyInfo?.OriginalPost?.UserId,
                    OriginalPostUserName = replyInfo?.OriginalPost?.User?.UserName
                };
            }).ToList();
        }

    }
}
