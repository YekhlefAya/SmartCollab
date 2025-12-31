using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCollab.Models;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace SmartCollab.Controllers
{
    
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Challenge();

            var today = DateTime.UtcNow.Date;

            // 1️⃣ Projets actifs
            var activeProjects = await _context.ProjectMembers
                .Include(pm => pm.Project)
                .Where(pm => pm.UserId == userId && !pm.IsArchived)
                .Select(pm => pm.Project)
                .ToListAsync();

            // 2️⃣ Tâches assignées
            var userTasks = await _context.TaskAssignments
                .Include(ta => ta.Task)
                .ThenInclude(t => t.Project)
                .Where(ta => ta.UserId == userId)
                .Select(ta => ta.Task)
                .ToListAsync();

            var ongoingTasks = userTasks.Where(t => t.Status != SmartCollab.Models.TaskStatus.Completed).ToList();
            var completedTasks = userTasks.Where(t => t.Status == SmartCollab.Models.TaskStatus.Completed).ToList();
            var todayTasks = ongoingTasks.Where(t => t.Deadline.Date == today).ToList();
            var upcomingTasks = ongoingTasks
                .Where(t => t.Deadline.Date > today)
                .OrderBy(t => t.Deadline)
                .Take(3)
                .ToList();

            // 3️⃣ Membres de l'équipe (distincts)
            var teamMembersCount = await _context.ProjectMembers
                .Where(pm => activeProjects.Select(p => p.Id).Contains(pm.ProjectId))
                .Select(pm => pm.UserId)
                .Distinct()
                .CountAsync();

            // 4️⃣ Activité récente (5 dernières)
            var recentActivities = await _context.Comments
                .Include(c => c.ProjectMember)
                .ThenInclude(pm => pm.User)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new ActivityViewModel
                {
                    UserName = $"{c.ProjectMember.User.FirstName} {c.ProjectMember.User.LastName}",
                    Action = "a commenté sur",
                    Target = c.Task.Title,
                    Date = c.CreatedAt,
                    AvatarUrl = $"https://ui-avatars.com/api/?name={c.ProjectMember.User.FirstName}+{c.ProjectMember.User.LastName}&size=40"
                })
                .ToListAsync();

            // 5️⃣ Projets récents : tri par UpdatedAt ou CreatedAt si pas modifié
            var recentProjects = activeProjects
                .OrderByDescending(p => p.UpdatedAt != default ? p.UpdatedAt : p.CreatedAt)
                .Take(3)
                .ToList();

            // 6️⃣ ViewModel
            var vm = new DashboardViewModel
            {
                UserFullName = $"{user.FirstName} {user.LastName}",
                ActiveProjectsCount = activeProjects.Count,
                OngoingTasksCount = ongoingTasks.Count,
                CompletedTasksCount = completedTasks.Count,
                TeamMembersCount = teamMembersCount,
                TodayTasks = todayTasks,
                RecentActivities = recentActivities,
                RecentProjects = recentProjects,
                UpcomingTasks = upcomingTasks
            };

            return View(vm);
        }

    }
}
