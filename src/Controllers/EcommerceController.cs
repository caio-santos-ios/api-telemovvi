using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_infor_cell.src.Controllers
{
    [Route("api/ecommerce")]
    [ApiController]
    public class EcommerceController(IEcommerceService service) : ControllerBase
    {
        // ─── CONFIG (requer auth do ERP) ─────────────────────────────────────────

        [Authorize]
        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            string? plan    = User.FindFirst("plan")?.Value;
            string? company = User.FindFirst("company")?.Value;
            string? store   = User.FindFirst("store")?.Value;

            var response = await service.GetConfigAsync(plan!, company!, store!);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [Authorize]
        [HttpPost("config")]
        public async Task<IActionResult> SaveConfig([FromBody] SaveEcommerceConfigDTO body)
        {
            var response = await service.SaveConfigAsync(body);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        // ─── PÚBLICO — sem autenticação ───────────────────────────────────────────

        [AllowAnonymous]
        [HttpGet("public/config/{plan}/{company}/{store}")]
        public async Task<IActionResult> GetPublicConfig(string plan, string company, string store)
        {
            var response = await service.GetConfigAsync(plan, company, store);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [AllowAnonymous]
        [HttpGet("public/products/{plan}/{company}/{store}")]
        public async Task<IActionResult> GetPublicProducts(
            string plan, string company, string store,
            [FromQuery] string? search, [FromQuery] string? categoryId)
        {
            var response = await service.GetPublicProductsAsync(plan, company, store, search, categoryId);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [AllowAnonymous]
        [HttpPost("public/register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] EcommerceRegisterDTO body)
        {
            var response = await service.RegisterCustomerAsync(body);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [AllowAnonymous]
        [HttpPost("public/login")]
        public async Task<IActionResult> LoginCustomer([FromBody] EcommerceLoginDTO body)
        {
            var response = await service.LoginCustomerAsync(body);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [AllowAnonymous]
        [HttpPost("public/checkout")]
        public async Task<IActionResult> Checkout([FromBody] EcommerceCheckoutDTO body)
        {
            var response = await service.CheckoutAsync(body);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [AllowAnonymous]
        [HttpGet("public/order/{orderId}/{customerId}")]
        public async Task<IActionResult> GetOrder(string orderId, string customerId)
        {
            var response = await service.GetOrderByIdAsync(orderId, customerId);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [AllowAnonymous]
        [HttpPost("public/webhook")]
        public async Task<IActionResult> Webhook([FromBody] dynamic body)
        {
            try
            {
                string paymentId = body.GetProperty("payment").GetProperty("id").GetString() ?? "";
                string eventType = body.GetProperty("event").GetString() ?? "";
                var response = await service.HandlePaymentWebhookAsync(paymentId, eventType);
                return StatusCode(response.StatusCode, new { response.Result });
            }
            catch { return Ok(); }
        }
    }
}
