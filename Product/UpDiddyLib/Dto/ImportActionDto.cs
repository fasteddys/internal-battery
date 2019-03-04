using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public enum ImportBehavior
    {
        Pending,
        Ignored,
        Inserted,
        Updated,
        Error
    }

    public class ImportActionDto
    {
        public ImportBehavior ImportBehavior { get; set; }
        public string Reason { get; set; }
        public int Count { get; set; }
    }
}
