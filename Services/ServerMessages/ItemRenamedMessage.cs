using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Services.ServerMessages
{
    public record ItemRenamedMessage(
        Guid Id,
        string NewName);
}
