using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SmartCollab.Models
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentMention> CommentMentions { get; set; }
        public DbSet<ProjectFile> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // essentiel pour Identity

            // Enum conversion
            modelBuilder.Entity<Project>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            // Composite keys for many-to-many tables
            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.ProjectId, pm.UserId })
                .IsUnique();

            modelBuilder.Entity<TaskAssignment>()
                .HasKey(ta => new { ta.TaskId, ta.UserId });

            modelBuilder.Entity<CommentMention>()
                .HasKey(cm => new { cm.CommentId, cm.ProjectMemberId });

            // TaskAssignment relations
            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Task)
                .WithMany(t => t.AssignedMembers)
                .HasForeignKey(ta => ta.TaskId);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.User)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(ta => ta.UserId) // <-- doit être string
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectMember relations
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId); // <-- doit être string

            // Task → Project
            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment relations
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ProjectMember)
                .WithMany(pm => pm.Comments)
                .HasForeignKey(c => c.ProjectMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommentMention relations
            modelBuilder.Entity<CommentMention>()
                .HasOne(cm => cm.Comment)
                .WithMany(c => c.Mentions)
                .HasForeignKey(cm => cm.CommentId);

            modelBuilder.Entity<CommentMention>()
                .HasOne(cm => cm.ProjectMember)
                .WithMany(pm => pm.MentionedInComments)
                .HasForeignKey(cm => cm.ProjectMemberId);

            // File relations
            modelBuilder.Entity<ProjectFile>()
                .HasOne(f => f.ProjectMember)
                .WithMany(pm => pm.Files)
                .HasForeignKey(f => f.ProjectMemberId);

            modelBuilder.Entity<ProjectFile>()
                .HasOne(f => f.Task)
                .WithMany(t => t.Files)
                .HasForeignKey(f => f.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById);
        }
    }
}
