namespace UpDiddyLib.Helpers
{

    public enum LogLevel { Error = 0, Information }
    public interface ISysLog
    {
        void Log(LogLevel level, string Info, bool SendEmail = false);        
    }
}