using System;
using System.Collections.Generic;

namespace SmartCollab.Models
{
    public class DashboardViewModel
    {
        public string UserFullName { get; set; } = string.Empty;
        public int ActiveProjectsCount { get; set; }
        public int OngoingTasksCount { get; set; }
        public int CompletedTasksCount { get; set; }
        public int TeamMembersCount { get; set; }

        public List<ProjectTask> TodayTasks { get; set; } = new List<ProjectTask>();
        public List<ActivityViewModel> RecentActivities { get; set; } = new List<ActivityViewModel>();
        public List<Project> RecentProjects { get; set; } = new List<Project>();
        public List<ProjectTask> UpcomingTasks { get; set; } = new List<ProjectTask>();
    }

    public class ActivityViewModel
    {
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Target { get; set; } // tâche, projet, fichier…
        public DateTime Date { get; set; }
        public string AvatarUrl { get; set; }
    }
}
