using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public enum ImportBehavior
    {
        Ignored = 0,
        Created = 1,
        Updated = 2
    }

    public class ImportActionDto
    {
        public ImportBehavior ImportBehavior { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }
    }
}
