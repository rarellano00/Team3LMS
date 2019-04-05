using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCategory
    {
        public AssignmentCategory()
        {
            Assignment = new HashSet<Assignment>();
        }

        public uint AcId { get; set; }
        public string Name { get; set; }
        public sbyte Weight { get; set; }
        public uint ClassId { get; set; }

        public virtual Class Class { get; set; }
        public virtual ICollection<Assignment> Assignment { get; set; }
    }
}
