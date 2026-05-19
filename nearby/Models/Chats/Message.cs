using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Models;

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; }
    public string? SenderProfilePicture { get; set; }
    public string ContentType { get; set; }
    public string Content { get; set; }
    public string FileUrl { get; set; }
    public string TranscribedText { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string ChatType {  get; set; }
    public Message Reply {  get; set; }
}
