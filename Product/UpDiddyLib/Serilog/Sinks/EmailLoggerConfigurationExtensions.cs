using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using UpDiddyLib.Helpers;

namespace UpDiddyLib.Serilog.Sinks
{
    public static class SendGridLoggerConfigurationExtensions
    {
        public static LoggerConfiguration SendGrid(
            this LoggerSinkConfiguration loggerConfiguration,
            LogEventLevel logLevel,
            string apiKey,
            string toEmail)
        {
            return loggerConfiguration.Sink(new SendGridSink(logLevel, apiKey, toEmail));
        }
    }
}
