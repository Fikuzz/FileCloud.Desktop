using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.ViewModels.Interfaces
{
    public interface IUiDispatcher
    {
        void Invoke(Action action);
        void BeginInvoke(Action action);
    }
}
