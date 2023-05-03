using Braintree;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProj_Utility.BrainTree
{
    public class BrainTreeGate : IBrainTreeGate
    {
        public BrainTreeSettings _settings { get; set; }
        private IBraintreeGateway _braintreeGateway { get; set; }

        public BrainTreeGate(IOptions<BrainTreeSettings> settings)
        {
            _settings = settings.Value;
        }
        public IBraintreeGateway CreateGateway()
        {
            return new BraintreeGateway(_settings.Environment, _settings.MerchantId, _settings.PrivateKey, _settings.PublicKey);
        }

        public IBraintreeGateway GetGateway()
        {
            if(_braintreeGateway == null)
            {
                _braintreeGateway = CreateGateway();
            }
            return _braintreeGateway;
        }
    }
}
