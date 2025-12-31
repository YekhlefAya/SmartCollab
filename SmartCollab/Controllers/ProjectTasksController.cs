using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCollab.Models;

namespace SmartCollab.Controllers
{
    [Authorize]
    public class ProjectTasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProjectTasksController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===================== Helpers =====================

        private string GetUserId() => _userManager.GetUserId(User) ?? string.Empty;

        private async Task<ProjectMember?> GetUserMembership(Guid projectId)
        {
            var userId = GetUserId();
            return await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        // ===================== List Tasks =====================

        public async Task<IActionResult> Index(Guid? projectId, string statusFilter = null, string priorityFilter = null)
        {
            var userId = GetUserId();

            var tasksQuery = _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedMembers)
                    .ThenInclude(a => a.User)
                .Include(t => t.Project.Members)
                .Where(t => t.AssignedMembers.Any(a => a.UserId == userId))
                .AsQueryable();

            if (projectId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.ProjectId == projectId.Value);

            if (!string.IsNullOrEmpty(statusFilter))
                tasksQuery = tasksQuery.Where(t => t.Status.ToString() == statusFilter);

            if (!string.IsNullOrEmpty(priorityFilter))
                tasksQuery = tasksQuery.Where(t => t.Priority.ToString() == priorityFilter);

            var tasks = await tasksQuery
                .OrderBy(t => t.Deadline)
                .ToListAsync();

            var taskViewModels = tasks.Select(t =>
            {
                var membership = t.Project.Members.FirstOrDefault(m => m.UserId == userId);
                bool isOwnerOrManager = membership != null && (membership.Role == UserRole.Owner || membership.Role == UserRole.Manager);

                var assignedMembers = t.AssignedMembers
                    .OrderBy(a => a.User.FirstName)
                    .Take(2)
                    .Select(a => new AssignedMemberViewModel
                    {
                        FullName = $"{a.User.FirstName} {a.User.LastName}",
                        AvatarUrl = $"https://ui-avatars.com/api/?name={a.User.FirstName[0]}+{a.User.LastName[0]}&size=32&background=6366f1&color=fff"
                    })
                    .ToList();

                int remainingCount = t.AssignedMembers.Count - assignedMembers.Count;

                return new TaskListViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectName = t.Project.Name,
                    ProjectId = t.ProjectId,
                    Priority = t.Priority.ToString(),
                    Status = t.Status.ToString(),
                    Deadline = t.Deadline,
                    AssignedMembers = assignedMembers,
                    RemainingAssignedCount = remainingCount > 0 ? remainingCount : 0,
                    CanEdit = isOwnerOrManager,
                    CanDelete = isOwnerOrManager,
                    CanAssign = membership != null && membership.Role == UserRole.Owner
                };
            }).ToList();

            return View(taskViewModels ?? new List<TaskListViewModel>());

        }

        // ===================== Details =====================

        

        public async Task<IActionResult> Details(Guid id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedMembers)
                    .ThenInclude(a => a.User)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            var membership = await GetUserMembership(task.ProjectId);
            if (membership == null) return Forbid();

            var vm = new ProjectTaskDetailsViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Deadline = task.Deadline,
                ProjectId = task.ProjectId,
                ProjectName = task.Project.Name,
                CanEdit = membership.Role == UserRole.Owner || membership.Role == UserRole.Manager,
                CanDelete = membership.Role == UserRole.Owner || membership.Role == UserRole.Manager,


                // Membres assignés
                AssignedMembers = task.AssignedMembers.Select(a => new AssignedMemberViewModel
                {
                    FullName = $"{a.User.FirstName} {a.User.LastName}",
                    AvatarUrl = $"https://ui-avatars.com/api/?name={a.User.FirstName}+{a.User.LastName}&size=32&background=6366f1&color=fff"
                }).ToList(),

                // Commentaires
                Comments = task.Comments.Select(c => new CommentViewModel
                {
                    AuthorName = $"{c.ProjectMember.User.FirstName} {c.ProjectMember.User.LastName}",
                    AvatarUrl = $"https://ui-avatars.com/api/?name={c.ProjectMember.User.FirstName}+{c.ProjectMember.User.LastName}&size=40&background=6366f1&color=fff",
                    CreatedAt = c.CreatedAt,
                    Content = c.Content
                }).ToList(),

                // Comme Attachments et Tags n’existent plus dans ton modèle, on laisse vide
                Attachments = new List<AttachmentViewModel>(),
                Tags = new List<TagViewModel>()
            };

            return View(vm);
        }


        // ===================== Create =====================

        public async Task<IActionResult> Create(Guid projectId)
        {
            var membership = await GetUserMembership(projectId);
            if (membership == null)
                return Forbid();

            // Récupère les membres du projet
            var projectMembers = await _context.ProjectMembers
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId)
                .Select(pm => new AssignedMemberViewModel
                {
                    UserId = pm.UserId,
                    FullName = pm.User.FirstName + " " + pm.User.LastName
                }).ToListAsync();

            var vm = new ProjectTaskCreateViewModel
            {
                ProjectId = projectId,
                ProjectMembers = projectMembers,
                Priority = Priority.Low,   // valeur par défaut
                Status = SmartCollab.Models.TaskStatus.NotStarted // valeur par défaut
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectTaskCreateViewModel model)
        {
            var membership = await GetUserMembership(model.ProjectId);
            if (membership == null)
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            var task = new ProjectTask
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                Status = model.Status,
                Priority = model.Priority,
                Deadline = model.Deadline,
                ProjectId = model.ProjectId,
                CreatedById = membership.Id
            };

            _context.Tasks.Add(task);

            foreach (var userId in model.AssignedUserIds)
            {
                _context.TaskAssignments.Add(new TaskAssignment
                {
                    TaskId = task.Id,
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = model.ProjectId });
        }


        // ===================== Edit =====================

        public async Task<IActionResult> Edit(Guid id)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedMembers)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                        .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            var membership = await GetUserMembership(task.ProjectId);
            if (membership == null ||
                (membership.Role != UserRole.Owner && membership.Role != UserRole.Manager))
                return Forbid();

            var vm = new ProjectTaskCreateViewModel
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Priority = task.Priority,
                Status = task.Status,

                // 👉 utilisateurs déjà assignés
                AssignedUserIds = task.AssignedMembers
                    .Select(a => a.UserId)
                    .ToList(),

                // 👉 membres du projet
                ProjectMembers = task.Project.Members.Select(pm => new AssignedMemberViewModel
                {
                    UserId = pm.UserId,
                    FullName = pm.User.FirstName + " " + pm.User.LastName
                }).ToList()
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(ProjectTaskCreateViewModel model)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedMembers)
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (task == null)
                return NotFound();

            var membership = await GetUserMembership(task.ProjectId);
            if (membership == null ||
                (membership.Role != UserRole.Owner && membership.Role != UserRole.Manager))
                return Forbid();

            if (!ModelState.IsValid)
            {
                // Recharger les membres du projet si erreur
                model.ProjectMembers = await _context.ProjectMembers
                    .Include(pm => pm.User)
                    .Where(pm => pm.ProjectId == model.ProjectId)
                    .Select(pm => new AssignedMemberViewModel
                    {
                        UserId = pm.UserId,
                        FullName = pm.User.FirstName + " " + pm.User.LastName
                    }).ToListAsync();

                return View(model);
            }

            // 👉 Mise à jour des champs
            task.Title = model.Title;
            task.Description = model.Description;
            task.Deadline = model.Deadline;
            task.Priority = model.Priority;
            task.Status = model.Status;
            task.UpdatedAt = DateTime.UtcNow;

            // 👉 Reset assignations
            _context.TaskAssignments.RemoveRange(task.AssignedMembers);

            foreach (var userId in model.AssignedUserIds)
            {
                _context.TaskAssignments.Add(new TaskAssignment
                {
                    TaskId = task.Id,
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = task.Id });
        }


        // ===================== Delete =====================

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            var membership = await GetUserMembership(task.ProjectId);
            if (membership == null || (membership.Role != UserRole.Owner && membership.Role != UserRole.Manager))
                return Forbid();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ===================== Assign Members =====================

        public async Task<IActionResult> Assign(Guid id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                        .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            var membership = await GetUserMembership(task.ProjectId);
            if (membership == null || membership.Role != UserRole.Owner)
                return Forbid();

            var vm = new ProjectTaskAssignViewModel
            {
                TaskId = task.Id,
                ProjectMembers = task.Project.Members.Select(m => new AssignedMemberViewModel
                {
                    FullName = $"{m.User.FirstName} {m.User.LastName}",
                    UserId = m.UserId,
                    AvatarUrl = $"https://ui-avatars.com/api/?name={m.User.FirstName[0]}+{m.User.LastName[0]}&size=32&background=6366f1&color=fff"
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(ProjectTaskAssignViewModel model, List<string> selectedUserIds)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedMembers)
                .FirstOrDefaultAsync(t => t.Id == model.TaskId);

            if (task == null)
                return NotFound();

            var membership = await GetUserMembership(task.ProjectId);
            if (membership == null || membership.Role != UserRole.Owner)
                return Forbid();

            // Supprime les assignations existantes
            _context.TaskAssignments.RemoveRange(task.AssignedMembers);

            // Ajoute les nouvelles assignations
            foreach (var userId in selectedUserIds)
            {
                _context.TaskAssignments.Add(new TaskAssignment
                {
                    TaskId = task.Id,
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
