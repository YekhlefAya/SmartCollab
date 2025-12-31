using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace SmartCollab.Models
{
    public class User : IdentityUser  // <--- hérité
    {
        // Champs personnalisés
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicturePath { get; set; }

        // EF Core ne supporte pas List<string> directement → on stocke en CSV
        public string? Skills { get; set; } // ex: "C#,SQL,React"

        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
        public ICollection<TaskAssignment> AssignedTasks { get; set; } = new List<TaskAssignment>();
        public ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>();
    }
}
