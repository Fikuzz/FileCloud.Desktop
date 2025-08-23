using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileCloud.Desktop.ViewModels.Interfaces
{
    public interface IEditableItem
    {
        bool IsEditing { get; set; }
        string Name { get; set; }
        ICommand CommitEditCommand { get; }
        ICommand CancelEditCommand { get; }
    }
}
