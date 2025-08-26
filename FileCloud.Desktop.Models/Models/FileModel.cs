namespace FileCloud.Desktop.Models.Models
{
    public record FileModel(
        Guid Id,
        string Name,
        long? Size,
        Guid FolderId);
}
