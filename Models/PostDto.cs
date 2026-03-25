namespace PhotoApp.Models
{
    public class PostDto
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int RepliesCount { get; set; }
        public int ForwardsCount { get; set; }
        public bool IsLikedByMe { get; set; }
        public bool IsOwnPost { get; set; }
        public string CreatedAt { get; set; }

        // Reply info (якщо цей пост є reply)
        public bool IsReply { get; set; }
        public int? OriginalPostId { get; set; }
        public string OriginalPostImagePath { get; set; }
        public string OriginalPostDescription { get; set; }
        public string OriginalPostUserId { get; set; }
        public string OriginalPostUserName { get; set; }
    }
}
