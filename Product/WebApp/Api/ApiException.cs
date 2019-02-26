using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.Api
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public BasicResponseDto ResponseDto { get; set; }

        public ApiException(HttpResponseMessage response, BasicResponseDto basicResponseDto)
        {
            StatusCode = response.StatusCode;
            ResponseDto = basicResponseDto;
        }
    }
}
