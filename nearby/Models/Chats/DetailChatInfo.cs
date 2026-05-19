using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Models;

public class DetailChatInfo
{
    public Chat Chat { get; set; }
    public List<User> Participants { get; set; }
    public Messages Messages { get; set; }
}
