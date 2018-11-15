using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Design;
using UpDiddy.Helpers;

namespace UpDiddyLib.Helpers
{
    public class SysLog : ISysLog
    {
        private readonly ISysEmail _sysEmail;
        private readonly IConfiguration _configuration;
        private readonly string _errorEmailAddress;
        private readonly string _infoEmailAddress;
        private readonly ILogger _log;

        public SysLog(IConfiguration configuration, ISysEmail sysEmail, IServiceProvider serviceProvider)
        {
            _sysEmail = sysEmail;
            _configuration = configuration;
            _errorEmailAddress = _configuration["SysEmail:SystemErrorEmailAddress"];
            _infoEmailAddress = _configuration["SysEmail:SystemInfoEmailAddress"];
  
            _log = new LoggerFactory()
                .AddConsole(true)
                .AddApplicationInsights(serviceProvider)
                .CreateLogger<SysLog>();  
        }

        public void Log(LogLevel level, string Info, bool sendEmail = false)
        {
            string LogTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string LogEntry = $"{LogTimeStamp} {Info}";
            string LogInformation = _configuration["SysLog:LogInformation"];

            if (sendEmail)            
                _sysEmail.SendEmail(_infoEmailAddress, LogEmailTitle(level), LogEntry);
                           
            if (level == LogLevel.Information && LogInformation == Constants.SysLogLogInformationTrue)
                _log.LogWarning(LogEntry);
            else if (level == LogLevel.Error )
                _log.LogError(LogEntry);
        }

        private string LogEmailTitle(LogLevel level)
        {
            if (level == LogLevel.Error)
                return "System Error";
            else if (level == LogLevel.Information)
                return "System Info";
            else
                return "SysLog " + level.ToString();
        }
 


    }
}
