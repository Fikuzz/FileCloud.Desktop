using FileCloud.Desktop.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileCloud.Desktop.View.Helpers
{
    public class FileDialogService : IDialogService
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

        public string? OpenFolder()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        public void SendMessage(string message, string caption)
        {
            MessageBox.Show(message, caption);
        }

        public bool SendOkCancelMessage(string message, string caption)
        {
            return MessageBox.Show(message, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK? true : false;
        }
    }
}
