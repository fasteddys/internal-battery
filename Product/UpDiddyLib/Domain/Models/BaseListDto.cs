using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    /* TODO: Come up with a way to use generics to eliminate duplicative code for list dtos
     * http://docs.automapper.org/en/stable/Open-Generics.html
     */
    public class BaseListDto<T>
    {
        public List<T> Entities { get; set; } = new List<T>();
        public int TotalEntities { get; set; }
    }
}
