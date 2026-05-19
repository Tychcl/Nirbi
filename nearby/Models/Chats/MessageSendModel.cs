using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Models
{
    public class MessageSendModel
    {
        public string content_type { get; set; } = "text";
        public string? content { get; set; }
        public string? file_url { get; set; }
        public string? transcribed_text { get; set; }
        public int? reply { get; set; }
    }
}
