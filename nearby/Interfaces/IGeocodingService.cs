using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nearby.Models.Task;

namespace nearby.Interfaces
{
    public interface IGeocodingService
    {
        Task<string?> GetAddressAsync(double lat, double lon);
        Task<List<Position>> SearchAddressAsync(string query);
    }
}
