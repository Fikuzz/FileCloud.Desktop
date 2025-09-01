using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Services.Configurations
{
    public interface IDialogService
    {
        string[]? OpenFiles(string filter);
        string? OpenFolder();
        void SendMessage(string message, string caption);
        bool SendOkCancelMessage(string message, string caption);
    }
}
