using Newtonsoft.Json;

namespace Subscription.Dtos
{
    public class ConfigResponseDto
    {
        public string PublishableKey { get; set; }

        public string ProPrice { get; set; }

        public string BasicPrice { get; set; }
    }
}
