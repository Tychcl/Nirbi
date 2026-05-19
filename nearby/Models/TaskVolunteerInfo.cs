using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nearby.Classes;

namespace nearby.Models
{
    public class TaskVolunteerInfo : User
    {
        private string _status { get; set; }
        public string Status 
        { 
            get => _status;
            set
            {
                _status = value;
                pending = value == "pending";
                accepted = value == "accepted";
                rejected = value == "rejected";
            }
        } // pending, accepted, rejected
        public bool pending { get; set; } = false;
        public bool accepted { get; set; } = false;
        public bool rejected { get; set; } = false;
    }
}
