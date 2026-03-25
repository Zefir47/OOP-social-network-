using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhotoApp.Models
{
    public class ImagePost
    {
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; }

        public string Description { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Interactions where this post is the original (people replied/forwarded it)
        public ICollection<PostInteraction> ReceivedInteractions { get; set; } = new List<PostInteraction>();

        // Interaction record if this post itself IS a reply post
        public PostInteraction ReplyInteraction { get; set; }
    }
}