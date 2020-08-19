using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.ApplicationCore.Services.Redis
{
    public class RedisService : IRedisService
    {
        private readonly RedisOptions _options;
        private readonly IDatabase _database;
        private readonly ILogger _logger;

        public RedisService(IOptions<RedisOptions> optionsAccessor, ILogger logger)
        {
            _options = optionsAccessor.Value;

            var muxer = ConnectionMultiplexer.Connect(_options.Host);
            _database = muxer.GetDatabase();
            _logger = logger;
        }

        public string GetString(string key)
            => _database.StringGet(key);

        public async Task<string> GetStringAsync(string key)
        {
            try
            {
                if (await _database.KeyExistsAsync(key))
                {
                    _logger.LogInformation("Found {key} in the database", key);
                }
                else
                {
                    throw new NotFoundException($"Unable to find a value for key \"{key}\" in Redis");
                };
                return await _database.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching {key} from redis", key);
                throw;
            }
        }

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
