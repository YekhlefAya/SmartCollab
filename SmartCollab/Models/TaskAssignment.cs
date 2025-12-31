using System;

namespace SmartCollab.Models
{
    public class TaskAssignment
    {
        public Guid TaskId { get; set; }
        public ProjectTask Task { get; set; }

        public string UserId { get; set; }  // <- string, pas Guid
        public User User { get; set; }

    }
}
