using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Braintree;
using Microsoft.Extensions.Configuration;
using UpDiddy.Helpers.Braintree;

namespace UpDiddy.Helpers
{
    public class BraintreeConfiguration : IBraintreeConfiguration
    {
        public string Environment { get; set; }
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        private IBraintreeGateway BraintreeGateway { get; set; }
        private IConfiguration _configuration;

        public BraintreeConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IBraintreeGateway CreateGateway()
        {
            
            Environment = GetConfigurationSetting("BraintreeEnvironment");
            MerchantId = GetConfigurationSetting("BraintreeMerchantID");
            PublicKey = GetConfigurationSetting("BraintreePublicKey");
            PrivateKey = GetConfigurationSetting("BraintreePrivateKey");

            return new BraintreeGateway(Environment, MerchantId, PublicKey, PrivateKey);
        }

        public string GetConfigurationSetting(string setting)
        {
            return _configuration[setting];
        }

        public IBraintreeGateway GetGateway()
        {
            if (BraintreeGateway == null)
            {
                BraintreeGateway = CreateGateway();
            }

            return BraintreeGateway;
        }
    }
}
