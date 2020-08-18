using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IRedisService
    {
        string GetString(string key);

        Task<string> GetStringAsync(string key);

        void SetString(string key, string value);

        Task SetStringAsync(string key, string value);

        void RemoveKey(string key);

        Task RemoveKeyAsync(string key);
    }
}
