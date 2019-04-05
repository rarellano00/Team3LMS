using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public uint AId { get; set; }
        public uint UId { get; set; }
        public DateTime Time { get; set; }
        public string Contents { get; set; }
        public uint Score { get; set; }

        public virtual Assignment A { get; set; }
        public virtual Student U { get; set; }
    }
}
