
using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategory = new HashSet<AssignmentCategory>();
            Enrolled = new HashSet<Enrolled>();
        }

        public uint ClassId { get; set; }
        public string Season { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Location { get; set; }
        public uint ProfessorId { get; set; }
        public uint CourseId { get; set; }
        public uint Year { get; set; }

        public virtual Course Course { get; set; }
        public virtual Professor Professor { get; set; }
        public virtual ICollection<AssignmentCategory> AssignmentCategory { get; set; }
        public virtual ICollection<Enrolled> Enrolled { get; set; }
    }
}
