using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Classes
{
    public class ThemeDescription
    {
        public string Name;
        public string Mode;
        public string Color;
        public ThemeDescription(string name = "Такой", bool light = false, string color = "Нет")
        {
            Name = name;
            Mode = light ? "Светлая" : "Темная";
            Color = color;
        }
    }
}
