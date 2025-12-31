using System;

namespace SmartCollab.Models
{
    public class CommentMention
    {
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; }

        public Guid ProjectMemberId { get; set; }
        public ProjectMember ProjectMember { get; set; }
    }
}
