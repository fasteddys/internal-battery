using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using UpDiddyLib.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace UpDiddyLib.Serilog.Sinks
{
    public class SendGridSink : ILogEventSink
    {
        private string _apiKey { get; }
        private string _toEmail { get; }
        private LogEventLevel _minimumLogLevel { get; }

        public SendGridSink(LogEventLevel minimumLogLevel, string apiKey, string toEmail)
        {
            _apiKey = apiKey;
            _toEmail = toEmail;
            _minimumLogLevel = minimumLogLevel;
        }

        public void Emit(LogEvent logEvent)
        {
            // check log level
            if (logEvent.Level.CompareTo(_minimumLogLevel) < 0)
                return;

            string subject = string.Format("{0} - CareerCircle - {1}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), logEvent.Level);
            var client = new SendGridClient(_apiKey);
            EmailAddress from = new EmailAddress("support@careercircle.com", "CareerCircle Support");
            EmailAddress to = new EmailAddress(_toEmail);
            string htmlContent = $@"
                <div>
                    <p>Timestamp: {logEvent.Timestamp}</p>
                    <p>Log Message: {logEvent.RenderMessage()}</p>
                </div>
            ";

            if (logEvent.Exception != null)
            {
                htmlContent += $@"
                    <div>
                        <pre>
{logEvent.Exception}
{logEvent.Exception.StackTrace}
                        </pre>
                    </div>
";
            }

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            var response = AsyncHelper.RunSync<Response>(() => client.SendEmailAsync(msg));
        }
    }
}