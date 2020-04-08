using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UpDiddyLib.Domain.AzureSearchDocuments;


namespace UpDiddyLib.Domain.AzureSearchDocuments
{
    public class AzureIndexResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusMsg { get; set; } 
        public AzureIndexDOCResults DOCResults { get; set; }
    }
}
