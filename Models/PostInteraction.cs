using System;

namespace PhotoApp.Models
{
    public enum InteractionType { Reply, Forward }

    public class PostInteraction
    {
        public int Id { get; set; }
        public InteractionType Type { get; set; }

        public int OriginalPostId { get; set; }
        public ImagePost OriginalPost { get; set; }

        public string OriginalAuthorId { get; set; }
        public ApplicationUser OriginalAuthor { get; set; }

        public string ActorId { get; set; }
        public ApplicationUser Actor { get; set; }

        public string? Comment { get; set; }

        public int? ReplyPostId { get; set; }
        public ImagePost ReplyPost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
