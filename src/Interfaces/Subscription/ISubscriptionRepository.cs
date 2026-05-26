using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;

namespace api_infor_cell.src.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<ResponseApi<List<Subscription>>> GetByPlanIdAllAsync(string plan);
        Task<ResponseApi<Subscription?>> GetByUserIdAsync(string userId);
        Task<ResponseApi<Subscription?>> GetByPlanIdAsync(string planId);
        Task<ResponseApi<Subscription?>> GetByAsaasSubscriptionIdAsync(string asaasSubscriptionId);
        Task<ResponseApi<Subscription?>> GetByAsaasCustomerIdAsync(string asaasCustomerId);
        Task<ResponseApi<Subscription?>> CreateAsync(Subscription subscription);
        Task<ResponseApi<SubscriptionInvoice?>> CreateInvoiceAsync(SubscriptionInvoice subscription);
        Task<ResponseApi<SubscriptionInvoice?>> UpdateInvoiceAsync(SubscriptionInvoice subscription);
        Task<ResponseApi<Subscription?>> GetInvoiceDateMonthAsync(string plan, int month);
        Task<ResponseApi<Subscription?>> UpdateAsync(Subscription subscription);
    }
}