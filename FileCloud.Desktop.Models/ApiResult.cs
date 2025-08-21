using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Models
{
    public class ApiResult<T>
    {
        public T? Response { get; set; }
        public string? Error { get; set; }
    }
}
