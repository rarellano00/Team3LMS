using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrolled
    {
        public uint UId { get; set; }
        public uint ClassId { get; set; }
        public string Grade { get; set; }

        public virtual Class Class { get; set; }
        public virtual Student U { get; set; }
    }
}
