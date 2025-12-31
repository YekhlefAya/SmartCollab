using SmartCollab.Models;
using System;
using System.Collections.Generic;

namespace SmartCollab.Models
{
    public class TaskListViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }          // Pour détails ou édition rapide
        public string ProjectName { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime Deadline { get; set; }
        public Guid ProjectId { get; set; }



        // Liste des membres assignés
        public List<AssignedMemberViewModel> AssignedMembers { get; set; } = new List<AssignedMemberViewModel>();

        public int RemainingAssignedCount { get; set; }  // Pour "+X" affichage

        public bool CanEdit { get; set; }      // Owner ou Manager
        public bool CanAssign { get; set; }    // Owner seulement
        public bool CanDelete { get; set; }    // Owner ou Manager
    }

    public class AssignedMemberViewModel
    {
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public string UserId { get; set; }    // utile pour assignation
    }

    public class ProjectTaskViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ProjectName { get; set; }
        public TaskStatus Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public List<AssignedMemberViewModel> AssignedMembers { get; set; } = new List<AssignedMemberViewModel>();
        public List<string> AssignedMemberNames { get; set; } = new List<string>();
        public bool CanEdit { get; set; }
        public bool CanAssign { get; set; }
        public bool CanDelete { get; set; }
        public Guid ProjectId { get; set; }

    }

    public class ProjectTaskCreateViewModel
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime Deadline { get; set; }

        public List<string> AssignedUserIds { get; set; } = new List<string>();

        public List<AssignedMemberViewModel> ProjectMembers { get; set; } = new List<AssignedMemberViewModel>();
    }


    public class ProjectTaskAssignViewModel
    {
        public Guid TaskId { get; set; }
        public List<AssignedMemberViewModel> ProjectMembers { get; set; } = new List<AssignedMemberViewModel>();
    }
    public class ProjectTaskDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }

        public List<AssignedMemberViewModel> AssignedMembers { get; set; } = new();
        public List<CommentViewModel> Comments { get; set; } = new();
        public List<AttachmentViewModel> Attachments { get; set; } = new();
        public List<TagViewModel> Tags { get; set; } = new();

        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class CommentViewModel
    {
        public string AuthorName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; }
    }

    public class AttachmentViewModel
    {
        public string FileName { get; set; }
        public string FileTypeIcon { get; set; } // ex: "fa-file-pdf"
        public string ColorClass { get; set; } // ex: "text-red-500"
        public string Size { get; set; } // ex: "2.4 MB"
        public string Url { get; set; } // lien pour téléchargement
    }

    public class TagViewModel
    {
        public string Name { get; set; }
        public string BgColorClass { get; set; }
        public string TextColorClass { get; set; }
    }
}
