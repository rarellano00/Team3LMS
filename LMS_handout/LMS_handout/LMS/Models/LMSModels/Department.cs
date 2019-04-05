using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Department
    {
        public Department()
        {
            Course = new HashSet<Course>();
            Professor = new HashSet<Professor>();
            Student = new HashSet<Student>();
        }

        public uint DepartmentId { get; set; }
        public string Name { get; set; }
        public string Abbrv { get; set; }

        public virtual ICollection<Course> Course { get; set; }
        public virtual ICollection<Professor> Professor { get; set; }
        public virtual ICollection<Student> Student { get; set; }
    }
}
