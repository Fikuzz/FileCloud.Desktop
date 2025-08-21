using FileCloud.Desktop.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.View.Helpers
{
    public class FileDialogService : IFileDialogService
    {
        public string[]? OpenFiles(string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                Multiselect = true
            };

            return dialog.ShowDialog() == true ? dialog.FileNames : null;
        }
    }
}
