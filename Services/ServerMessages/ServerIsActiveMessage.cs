using FileCloud.Desktop.Models;

namespace FileCloud.Desktop.Services.ServerMessages
{
    public record ServerIsActiveMessage(ServerStatus Status, string? Message);
}
