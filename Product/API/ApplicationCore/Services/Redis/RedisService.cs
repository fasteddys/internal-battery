using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.ApplicationCore.Services.Redis
{
    public class RedisService : IRedisService
    {
        private readonly RedisOptions _options;
        private readonly IDatabase _database;

        public RedisService(IOptions<RedisOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;

            var muxer = ConnectionMultiplexer.Connect(_options.Host);
            _database = muxer.GetDatabase();
        }

        public string GetString(string key)
            => _database.StringGet(key);

        public async Task<string> GetStringAsync(string key)
            => await _database.StringGetAsync(key);

        public void SetString(string key, string value)
            => _database.StringSet(key, value);

        public async Task SetStringAsync(string key, string value)
            => await _database.StringSetAsync(key, value);

        public void RemoveKey(string key)
            => _database.KeyDelete(key);

        public async Task RemoveKeyAsync(string key)
            => await _database.KeyDeleteAsync(key);
    }
}
