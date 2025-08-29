using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Services.Configurations
{
    public interface IFileSaveService
    {
        Task SaveFileAsync(Guid id, string fileName, byte[] content, string? path = null);
    }
}
