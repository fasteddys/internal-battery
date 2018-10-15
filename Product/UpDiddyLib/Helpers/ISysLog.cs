namespace UpDiddyLib.Helpers
{
    public interface ISysLog
    {
        void SysError(string Info);
        void SysInfo(string Info);
    }
}