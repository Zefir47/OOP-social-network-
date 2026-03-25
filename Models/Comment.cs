using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhotoApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; }

        public int ImagePostId { get; set; }
        public ImagePost ImagePost { get; set; }

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        public int? ParentId { get; set; }
        public Comment Parent { get; set; }

        public ICollection<Comment> Replies { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}