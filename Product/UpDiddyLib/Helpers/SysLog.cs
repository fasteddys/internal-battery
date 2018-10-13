using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace UpDiddyLib.Helpers
{
    static public class SysLog
    {
         
        static public void SysInfo(string Info)
        {

            string LogTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            // TODO call SysEmail to Info Email address 
            // TODO integrate logger 
        }

        static public void SysError(string Info)
        {

            string LogTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            // TODO call SysEmail to Info Email address 
            // TODO integrate logger 

        }


    }
}
