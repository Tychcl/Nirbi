using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Classes
{
    public class Clone<T>
    {
        public T Copy()
        {
            return (T)MemberwiseClone();
        }
    }
}
