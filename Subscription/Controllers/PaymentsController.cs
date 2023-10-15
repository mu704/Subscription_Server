using Stripe;
using Stripe.Checkout;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Subscription.Dtos;
using Microsoft.Extensions.Options;
using Subscription.Configuration;

namespace Subscription.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : Controller
    {
        public readonly IOptions<StripeOptions> options;
        private readonly IStripeClient client;


        public PaymentsController(IOptions<StripeOptions> options)
        {
                this.options = options;
            this.client = new StripeClient(this.options.Value.SecretKey);
        }

        //[HttpGet("config")]
        //public ConfigResponseDto Setup()
        //{
        //    return new ConfigResponseDto()
        //    {
        //        ProPrice = this.options.Value.PropPrice,
        //        BasicPrice = this.options.Value.BasicPrice,
        //        PublishableKey = this.options.Value.PublishableKey,
        //    };

        //}

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutDto checkoutDto)
        {
            StripeConfiguration.ApiKey = "{{SECRET_KEY}}";
            string priceId = checkoutDto.PriceId;

            SessionCreateOptions options = new SessionCreateOptions()
            {
                SuccessUrl = "http://localhost:3000/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "http://localhost:3000/cancel",
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>()
            {
                new SessionLineItemOptions()
                {
                    Price = priceId,
                    Quantity = 1,
                },
            },
            };

            SessionService service = new SessionService();
            Session session = await service.CreateAsync(options);

            SessionUrl sessionUrl = new SessionUrl() { Url = session.Url };

            return Ok(sessionUrl);
        }

        [HttpGet("checkout-session")]
        public async Task<IActionResult> CheckoutSession(string sessionId)
        {
            SessionService service = new SessionService(this.client);
            Session session = await service.GetAsync(sessionId);

            return this.Ok(session);
        }

        [HttpPost("customer-portal")]
        public async Task<IActionResult> CustomerPoral(string sessionId)
        {
            SessionService checkoutService = new SessionService(this.client);
            Session checkoutSession = await checkoutService.GetAsync(sessionId);

            string returnUrl = this.options.Value.Domain;

            Stripe.BillingPortal.SessionCreateOptions options = new Stripe.BillingPortal.SessionCreateOptions()
            {
                Customer = checkoutSession.CustomerId,
                ReturnUrl = returnUrl,
            };

            Stripe.BillingPortal.SessionService service = new Stripe.BillingPortal.SessionService(this.client);
            Stripe.BillingPortal.Session session = await service.CreateAsync(options);
            SessionUrl sessionUrl = new SessionUrl() { Url = session.Url };
            return this.Ok(sessionUrl);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;
            try
            {
                string webhookSecret = "{{STRIPE_WEBHOOK_SECRET}}";
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret);

                Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something failed {e}");
                return BadRequest();
            }

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    // Add customer priviliges to the relevant subscription
                    break;
                case "invoice.paid":
                    // Continue to provision the subscription as payments continue to be made.
                    // Store the status in your database and check when a user accesses your service.
                    // This approach helps you avoid hitting rate limits.
                    break;
                case "invoice.payment_failed":
                    // The payment failed or the customer does not have a valid payment method.
                    // The subscription becomes past_due.Notify your customer and send them to the
                    // customer portal to update their payment information.
                    break;
            }


            return Ok();
        }
    }
}
