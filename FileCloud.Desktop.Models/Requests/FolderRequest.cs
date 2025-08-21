namespace FileCloud.Desktop.Models.Requests
{
    public class FolderRequest
    {
        public required string Name { get; set; }
        public Guid ParentId { get; set; }
    }
}
