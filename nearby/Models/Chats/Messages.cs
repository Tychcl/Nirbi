using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Models;

public class Messages
{
    public List<Message>? Data {  get; set; }
    public int? total { get; set; }
    public int? page { get; set; }
    public int? limit { get; set; }
    public int? pages { get; set; }
}
