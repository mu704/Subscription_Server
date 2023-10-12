using Stripe;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Subscription.Dtos;

namespace Subscription.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {

        [HttpPost]
        [Route("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutDto checkoutDto)
        {
            StripeConfiguration.ApiKey = "SECRET_STRIP_API_KEY";
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

            Response.Headers.Add("Location", session.Url);
            return Ok();
        }
    }
}
