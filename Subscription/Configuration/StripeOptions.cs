namespace Subscription.Configuration
{
    public class StripeOptions
    {
        public string PublishableKey { get; set; } = "{{PUBLISHABLE_KEY}";

        public string SecretKey { get; set; } = "{{SECRET_KEY}}";

        public string WebhookSecret { get; set; }

        public string BasicPrice { get; set; } = "{{PRICE_BASIC}}";

        public string PropPrice { get; set; } = "{{PRICE_PREMIUM}}";

        public string Domain { get; set; } = "http://localhost:3000";

    }
}
