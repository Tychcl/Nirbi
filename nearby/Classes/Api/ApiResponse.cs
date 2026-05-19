using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Classes
{
    public class ApiResponse<T>: Clone<ApiResponse<T>>
    {
        public string? message { get; set; }
        public T? Data { get; set; }

        public ApiResponse() { }

        public ApiResponse(string? message, T? _object)
        {
            this.message = message;
            Data = _object;
        }

        public int total;
        public int page;
        public int limit;
        public int pages;
    }
}
