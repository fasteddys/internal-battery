using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels.Components.Layout
{
    public class MenuRootViewModel<T>
    {
        public IList<T> menu_item { get; set; }
    }
}
