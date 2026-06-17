using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Models
{
    public class Candidate : FullNames
    {
        public Guid ConfirmationId { get; set; }
        public string Status { get; set; }
        public bool IsPending  { get; set; } 
        public bool IsRejected { get; set; } 
        public bool IsAccepted { get; set; }
        public Candidate() { }

        public Candidate(FullNames fullName, Guid confirmationId, string status)
        {
            Id = fullName.Id;
            FirstName = fullName.FirstName;
            SecondName = fullName.SecondName;
            LastName = fullName.LastName;
            ConfirmationId = confirmationId;
            Status = status;
            IsPending = Status == "Created";
            IsRejected = Status == "Rejected";
            IsAccepted = Status == "Accepted";
        }               
    }
}
                        