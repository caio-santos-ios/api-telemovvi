using System.Security.Claims;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_telemovvi.src.Shared.DTOs.Subscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_infor_cell.src.Controllers
{
    [Route("api/subscriptions")]
    [ApiController]
    public class SubscriptionController(ISubscriptionService service) : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionDTO body)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Unauthorized();

            ResponseApi<Subscription?> response = await service.CreateSubscriptionAsync(body, userId);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> SignaturePlan([FromBody] HandlerWebhookDTO request)
        {
            string token = Request.Headers["asaas-access-token"].ToString();
            string tokenWebhook = Environment.GetEnvironmentVariable("TOKEN_WEBHOOK") ?? "";
            if (token != tokenWebhook) return Unauthorized();

            ResponseApi<string> response = await service.HandlerWebhookAsync(request);
            return StatusCode(response.StatusCode, new { response.Result });
        }
        
        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSubscription()
        {
            string? plan = User.FindFirst("plan")?.Value;

            if (plan is null) return Unauthorized();


            ResponseApi<Subscription?> response = await service.GetCurrentSubscriptionAsync(plan);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [Authorize]
        [HttpGet("payments")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            string? plan = User.FindFirst("plan")?.Value;

            if (plan is null) return Unauthorized();

            var response = await service.GetPaymentHistoryAsync(plan);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        /// <summary>Busca a assinatura ativa do usuário logado</summary>
        [Authorize]
        [HttpGet("plan")]
        public async Task<IActionResult> GetPlan()
        {
            string? plan = User.FindFirst("plan")?.Value;
            ResponseApi<Subscription?> response = await service.GetByPlanAsync(plan!);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        /// <summary>Cancela a assinatura do usuário logado</summary>
        [Authorize]
        [HttpDelete("cancel")]
        public async Task<IActionResult> CancelSubscription()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Unauthorized();

            ResponseApi<Subscription?> response = await service.CancelSubscriptionAsync(userId);
            return StatusCode(response.StatusCode, new { response.Result });
        }
    }
}