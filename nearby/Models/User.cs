using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using nearby.Classes;

namespace nearby.Models
{
    public class User: Clone<User>
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        private string _fullname { get; set; }
        public string FullName 
        { 
            get => _fullname;
            set
            {
                _fullname = value;
                string[]? fio = value?.Split(' ');
                if (fio != null)
                {
                    Surname = fio.Length > 0 ? fio[0] : "";
                    Name = fio.Length > 1 ? fio[1] : "";
                    Patronymic = fio.Length > 2 ? fio[2] : "";
                }
            }
        }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? About { get; set; }
        public string? ProfilePicture { get; set; }
        public decimal Balance { get; set; }
        public string AvailabilityStatus { get; set; } = "available";
        public bool IsAdmin { get; set; }
        public bool IsModerator { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public bool IsOnline { get; set; }
        public double AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? EducationInstitution { get; set; }
        public string? EducationDegree { get; set; }
        public string? EducationField { get; set; }
        public int? EducationStartYear { get; set; }
        public int? EducationEndYear { get; set; }
    }
}
