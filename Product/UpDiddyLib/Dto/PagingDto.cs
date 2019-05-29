using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class PagingDto<T>
    {
        public int Page { get; set;}
        public int PageSize { get; set; }
        public int Count { get; set; }
        public List<T> Results { get; set; }
    }
}