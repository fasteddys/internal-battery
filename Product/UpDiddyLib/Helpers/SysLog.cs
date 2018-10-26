using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace UpDiddyLib.Helpers
{
     public class SysLog : ISysLog
    {
        private readonly ISysEmail _sysEmail;
        private readonly IConfiguration _configuration;
        private readonly string _errorEmailAddress;
        private readonly string _infoEmailAddress;
        public SysLog(IConfiguration configuration, ISysEmail sysEmail)
        {
            _sysEmail = sysEmail;
            _configuration = configuration;
            _errorEmailAddress = _configuration["SysEmail:SystemErrorEmailAddress"];
            _infoEmailAddress = _configuration["SysEmail:SystemInfoEmailAddress"];
  
        }
         public void SysInfo(string Info, bool SendEmail = false )
        {
           
            string LogTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            // TODO call SysEmail to Info Email address 
            if ( SendEmail )
                _sysEmail.SendEmail(_infoEmailAddress, "System Info", $"{LogTimeStamp} {Info}");
            // TODO integrate logger 
        }

         public void SysError(string Info, bool SendEmail = false)
        {

            string LogTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            if ( SendEmail)
                _sysEmail.SendEmail(_errorEmailAddress, "System Error", $"{LogTimeStamp} {Info}");
            // TODO integrate logger 

        }


    }
}
