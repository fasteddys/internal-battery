namespace UpDiddyLib.Helpers
{
    public interface ISysLog
    {
        void SysError(string Info, bool SendEmail = false);
        void SysInfo(string Info, bool SendEmail = false );
    }
}