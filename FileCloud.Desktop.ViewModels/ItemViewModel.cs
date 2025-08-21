using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.ViewModels
{
    public class ItemViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? PreviewPath { get; set; }
    }
}
