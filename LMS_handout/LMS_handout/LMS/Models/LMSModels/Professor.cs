using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Professor
    {
        public Professor()
        {
            Class = new HashSet<Class>();
        }

        public uint UId { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public DateTime Dob { get; set; }
        public uint DepartmentId { get; set; }

        public virtual Department Department { get; set; }
        public virtual ICollection<Class> Class { get; set; }
    }
}
