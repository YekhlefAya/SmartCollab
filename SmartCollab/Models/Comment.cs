using System;
using System.Collections.Generic;

namespace SmartCollab.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid TaskId { get; set; }
        public ProjectTask Task { get; set; }

        public Guid ProjectMemberId { get; set; }
        public ProjectMember ProjectMember { get; set; }

        public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
    }
}
