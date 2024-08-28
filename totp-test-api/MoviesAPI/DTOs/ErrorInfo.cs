using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.DTOs
{
    public class ErrorInfo
    {
        public ErrorInfo(string message)
        {
            Message = message;
        }
        public string Message { get; set; }
    }
}
