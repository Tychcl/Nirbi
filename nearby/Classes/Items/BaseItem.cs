using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Classes
{
    public class BaseItem
    {
        public string Icon { get; set; }
        public string Text { get; set; }

        public BaseItem() { }

        public BaseItem(string icon, string text)
        {
            Icon = icon;
            Text = text;
        }
    }
}
