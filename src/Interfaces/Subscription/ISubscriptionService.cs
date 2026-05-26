using api_infor_cell.src.Handlers;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_telemovvi.src.Shared.DTOs.Subscription;

namespace api_infor_cell.src.Interfaces
{
    public interface ISubscriptionService
    {
        Task<ResponseApi<Subscription?>> CreateSubscriptionAsync(CreateSubscriptionDTO request, string userId);
        Task<ResponseApi<Subscription?>> GetCurrentSubscriptionAsync(string userId);
        Task<ResponseApi<Subscription?>> GetByPlanAsync(string plan);
        Task<ResponseApi<Subscription?>> CancelSubscriptionAsync(string userId);
        Task<ResponseApi<string>> HandlerWebhookAsync(HandlerWebhookDTO request);
        Task<ResponseApi<string>> HandleSignatureWebhookAsync(SignaturePlansDTO webhook);
        Task<ResponseApi<string>> HandlePaymentWebhookAsync(HandlerWebhookDTO webhook);
        Task<ResponseApi<List<Subscription>>> GetPaymentHistoryAsync(string plan);
    }
}