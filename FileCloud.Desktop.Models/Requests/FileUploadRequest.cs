namespace FileCloud.Desktop.Models.Requests
{
    public class FileUploadRequest
    {
        public Guid FolderId { get; set; }
        public Stream Stream { get; set; }
        public required string Name { get; set; }
    }
}
