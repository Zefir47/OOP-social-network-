using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Models
{
    public class Like
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ImagePostId { get; set; }
        public ImagePost ImagePost { get; set; }
    }
}
