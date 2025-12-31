using SmartCollab.Models;


public class ProjectViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public string Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    //public List<ProjectTask> RecentTasks { get; set; } = new();
    public List<ProjectTaskViewModel> RecentTasks { get; set; } = new List<ProjectTaskViewModel>();
    public int CompletedTasksCount { get; set; }
    public int TotalTasksCount { get; set; }
    public int OverdueTasksCount { get; set; }
    public int DaysRemaining => (EndDate - DateTime.UtcNow).Days;

    // <-- Ici : ProjectMember au lieu de User
    public List<ProjectMemberViewModel> TeamMembers { get; set; } = new();

    public List<ProjectFile> RecentFiles { get; set; } = new();

    public bool IsFavorite { get; set; }
    public bool IsArchived { get; set; }
    public string UserRole { get; set; } // Owner / Manager / Collaborator / Viewer


}
public class InviteMemberViewModel
{
    public Guid ProjectId { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ProjectMemberViewModel
{
    public string FullName { get; set; }
    public string Role { get; set; }
    
}
public class ArchivedProjectViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CompletedTasksCount { get; set; }
    public DateTime ArchivedAt { get; set; }
}
