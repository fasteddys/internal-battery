using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class PageResultDto<T>
    {
        public int TotalRecords;
        public List<T> Data;
        public int Pages;
    }
}
