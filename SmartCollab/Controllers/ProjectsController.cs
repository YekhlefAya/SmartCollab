using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCollab.Models;


namespace SmartCollab.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProjectsController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===================== Helpers =====================

        private string GetUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }

        private async Task<ProjectMember?> GetUserMembership(Guid projectId)
        {
            var userId = GetUserId();
            return await _context.ProjectMembers
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == projectId &&
                    pm.UserId == userId);
        }

        // ===================== Index =====================

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var projects = await _context.ProjectMembers
                .Include(pm => pm.Project)
                .Where(pm => pm.UserId == userId && !pm.IsArchived)
                .Select(pm => pm.Project)
                .ToListAsync();

            return View("Index", projects);
        }

        // ===================== Create =====================

        [Authorize]
        public IActionResult Create() => View("Create");

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            project.CreatedById = GetUserId();

            ModelState.Remove(nameof(Project.CreatedById));
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                Console.WriteLine("Erreurs ModelState : " + string.Join(", ", errors));
                return View("Create", project);
            }

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Auth");
            }

            project.Id = Guid.NewGuid();
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var projectMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                UserId = GetUserId(),
                Role = UserRole.Owner,
                IsArchived = false
            };
            _context.ProjectMembers.Add(projectMember);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ===================== Details =====================

        public async Task<IActionResult> Details(Guid id)
        {
            var userId = GetUserId();

            var membership = await _context.ProjectMembers
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Tasks)
                        .ThenInclude(t => t.AssignedMembers)
                            .ThenInclude(ta => ta.User)
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Members)
                        .ThenInclude(m => m.User)
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Tasks)
                        .ThenInclude(t => t.Files)
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == id &&
                    pm.UserId == userId);

            if (membership == null)
                return Forbid();

            var project = membership.Project;

            var vm = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Color = project.Color,
                Status = project.Status.ToString(),
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CompletedTasksCount = project.Tasks.Count(t => t.Status == SmartCollab.Models.TaskStatus.Completed),
                TotalTasksCount = project.Tasks.Count,
                OverdueTasksCount = project.Tasks.Count(t => t.Deadline < DateTime.UtcNow && t.Status != SmartCollab.Models.TaskStatus.Completed),
                RecentTasks = project.Tasks
                .OrderByDescending(t => t.CreatedAt)
                .Take(3)
                .Select(t => new ProjectTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    Deadline = t.Deadline,
                    AssignedMemberNames = t.AssignedMembers
                        .Select(a => a.User.FirstName + " " + a.User.LastName)
                        .ToList()
                })
    .ToList(),
                TeamMembers = project.Members
                    .Select(m => new ProjectMemberViewModel
                    {
                        FullName = $"{m.User.FirstName} {m.User.LastName}",
                        Role = m.Role.ToString()
                    })
                    .ToList(),
                RecentFiles = project.Tasks
                                .SelectMany(t => t.Files)
                                .OrderByDescending(f => f.UploadedAt)
                                .Take(4)
                                .ToList(),
                IsFavorite = membership.IsFavorite,
                IsArchived = membership.IsArchived,
                UserRole = membership.Role.ToString()
            };

            return View("Details", vm);
        }

        // ===================== Edit =====================

        public async Task<IActionResult> Edit(Guid id)
        {
            var membership = await GetUserMembership(id);

            if (membership == null || (membership.Role != UserRole.Owner && membership.Role != UserRole.Manager))
                return Forbid();

            var project = await _context.Projects.FindAsync(id);
            return View("Edit", project);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Project model, string color)
        {
            var membership = await GetUserMembership(model.Id);

            if (membership == null || (membership.Role != UserRole.Owner && membership.Role != UserRole.Manager))
                return Forbid();

            var project = await _context.Projects.FindAsync(model.Id);
            if (project == null)
                return NotFound();

            project.Name = model.Name;
            project.Description = model.Description;
            project.Status = model.Status;
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;
            project.Color = color;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = project.Id });
        }

        // ===================== Delete =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = _userManager.GetUserId(User);

            var project = await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedMembers)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            
            var membership = project.Members
                .FirstOrDefault(m => m.UserId == userId);

            if (membership == null || membership.Role != UserRole.Owner)
                return Forbid();

            
            foreach (var task in project.Tasks)
            {
                _context.TaskAssignments.RemoveRange(task.AssignedMembers);
                _context.Comments.RemoveRange(task.Comments);
            }

            _context.Tasks.RemoveRange(project.Tasks);
            _context.ProjectMembers.RemoveRange(project.Members);
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Projects");
        }


        // ===================== Archive / Restore =====================

        [HttpPost]
        public async Task<IActionResult> Archive(Guid id)
        {
            var membership = await GetUserMembership(id);
            if (membership == null) return Forbid();

            membership.IsArchived = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Restore(Guid id)
        {
            var membership = await GetUserMembership(id);
            if (membership == null) return NotFound();

            membership.IsArchived = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Archive()
        {
            var userId = GetUserId();

            var archivedProjects = await _context.ProjectMembers
                .Include(pm => pm.Project)
                .Where(pm => pm.UserId == userId && pm.IsArchived)
                .Select(pm => new ArchivedProjectViewModel
                {
                    Id = pm.Project.Id,
                    Name = pm.Project.Name,
                    Description = pm.Project.Description,
                    CompletedTasksCount = pm.Project.Tasks.Count(t => t.Status == SmartCollab.Models.TaskStatus.Completed),
                    ArchivedAt = pm.Project.UpdatedAt
                })
                .ToListAsync();

            return View("Archive", archivedProjects);
        }


        // ===================== Invite Member =====================

        public async Task<IActionResult> InviteMember(Guid projectId)
        {
            var membership = await GetUserMembership(projectId);
            if (membership == null || membership.Role != UserRole.Owner)
                return Forbid();

            return View("InviteMember",
                new InviteMemberViewModel { ProjectId = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> InviteMember(InviteMemberViewModel model)
        {
            var membership = await GetUserMembership(model.ProjectId);
            if (membership == null || membership.Role != UserRole.Owner)
                return Forbid();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                model.ErrorMessage = "Utilisateur non trouvé.";
                return View("InviteMember", model);
            }

            if (await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == model.ProjectId && pm.UserId == user.Id))
            {
                model.ErrorMessage = "L'utilisateur est déjà membre de ce projet.";
                return View("InviteMember", model);
            }

            if (!Enum.TryParse<UserRole>(model.Role, true, out var role))
            {
                model.ErrorMessage = "Rôle invalide.";
                return View("InviteMember", model);
            }

            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = model.ProjectId,
                UserId = user.Id,
                Role = role,
                IsArchived = false
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = model.ProjectId });
        }

        // ===================== Leave Project =====================

        [HttpPost]
        public async Task<IActionResult> LeaveProject(Guid projectId)
        {
            var membership = await GetUserMembership(projectId);
            if (membership == null) return NotFound();

            if (membership.Role == UserRole.Owner)
                return BadRequest("Le propriétaire ne peut pas quitter le projet.");

            _context.ProjectMembers.Remove(membership);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ===================== Favorite / Unfavorite =====================

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(Guid projectId)
        {
            var membership = await GetUserMembership(projectId);
            if (membership == null) return NotFound();

            membership.IsFavorite = !membership.IsFavorite;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }

}
