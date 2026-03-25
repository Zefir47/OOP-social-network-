using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoApp.Models;

namespace PhotoApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ImagePost> ImagePosts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostInteraction> PostInteractions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Comment>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // PostInteraction → OriginalPost
            builder.Entity<PostInteraction>()
                .HasOne(pi => pi.OriginalPost)
                .WithMany(p => p.ReceivedInteractions)
                .HasForeignKey(pi => pi.OriginalPostId)
                .OnDelete(DeleteBehavior.Cascade);

            // PostInteraction → ReplyPost (the newly created post for Reply type)
            builder.Entity<PostInteraction>()
                .HasOne(pi => pi.ReplyPost)
                .WithOne(p => p.ReplyInteraction)
                .HasForeignKey<PostInteraction>(pi => pi.ReplyPostId)
                .OnDelete(DeleteBehavior.Restrict);

            // PostInteraction → OriginalAuthor
            builder.Entity<PostInteraction>()
                .HasOne(pi => pi.OriginalAuthor)
                .WithMany()
                .HasForeignKey(pi => pi.OriginalAuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // PostInteraction → Actor
            builder.Entity<PostInteraction>()
                .HasOne(pi => pi.Actor)
                .WithMany()
                .HasForeignKey(pi => pi.ActorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
