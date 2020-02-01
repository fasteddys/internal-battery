using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Domain.AzureSearch;

namespace UpDiddyLib.Domain.AzureSearchDocuments
{
    public class SDOCRequest<TEntity> where TEntity : class
    {
       
        public SDOCRequest()
        {
            value = new List<TEntity>();
        }

        public List<TEntity> value { get; set; }
    }
}
