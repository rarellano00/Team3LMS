using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Class = new HashSet<Class>();
        }

        public uint CourseId { get; set; }
        public short Number { get; set; }
        public string Name { get; set; }
        public uint DepartmentId { get; set; }

        public virtual Department Department { get; set; }
        public virtual ICollection<Class> Class { get; set; }
    }
}
