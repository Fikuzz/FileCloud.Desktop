namespace FileCloud.Desktop.Models.Models
{
    public class FileModel
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public long? Size { get; set; }
    }
}
