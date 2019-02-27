using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public enum ImportBehavior
    {
        Nothing,
        Insert,
        Update,
        Error
    }

    public class ImportActionDto
    {
        public ImportBehavior ImportBehavior { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }
    }
}
