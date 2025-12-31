namespace SmartCollab.Models
{
    public class ProjectFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
        public string StoragePath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Lien vers le ProjectMember qui a uploadé le fichier
        public Guid? ProjectMemberId { get; set; }
        public ProjectMember ProjectMember { get; set; }

        // Lien vers la tâche associée
        public Guid? TaskId { get; set; }
        public ProjectTask Task { get; set; }
    }
}
