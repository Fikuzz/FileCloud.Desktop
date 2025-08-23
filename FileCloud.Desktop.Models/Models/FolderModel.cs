namespace FileCloud.Desktop.Models
{
    public record FolderModel(
        Guid Id,
        string Name,
        Guid ParentId);
}
