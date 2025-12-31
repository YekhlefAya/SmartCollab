using System;
using System.Collections.Generic;

namespace SmartCollab.Models
{
    public enum UserRole { Owner, Manager, Collaborator, Viewer }

    public class ProjectMember

    {
        public Guid Id { get; set; }   //  ID technique

        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public string UserId { get; set; }  // <- string, pas Guid
        public User User { get; set; }


        public UserRole Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsFavorite { get; set; }
        public bool IsArchived { get; set; }

        // Relations
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<CommentMention> MentionedInComments { get; set; } = new List<CommentMention>();
        public ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>(); // <-- ajouté
    }
}
