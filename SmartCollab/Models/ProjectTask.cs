using System;
using System.Collections.Generic;

namespace SmartCollab.Models
{
    public enum TaskStatus { NotStarted, InProgress, Completed }
    public enum Priority { Low, Medium, High }

    public class ProjectTask
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime Deadline { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public Guid CreatedById { get; set; }
        public ProjectMember CreatedBy { get; set; }


        // Relations
        public ICollection<TaskAssignment> AssignedMembers { get; set; } = new List<TaskAssignment>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>();
    }
}
