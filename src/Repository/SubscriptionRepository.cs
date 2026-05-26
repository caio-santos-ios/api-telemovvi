using api_infor_cell.src.Configuration;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using MongoDB.Driver;

namespace api_infor_cell.src.Repository
{
    public class SubscriptionRepository(AppDbContext context) : ISubscriptionRepository
    {
        public async Task<ResponseApi<List<Subscription>>> GetByPlanIdAllAsync(string plan)
        {
            try
            {
                List<Subscription> sub = await context.Subscriptions
                    .Find(x => x.Plan == plan && !x.Deleted)
                    .SortByDescending(x => x.Date)
                    .ToListAsync();
                return new(sub);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> GetByUserIdAsync(string plan)
        {
            try
            {
                Subscription? sub = await context.Subscriptions
                    .Find(x => x.Plan == plan && !x.Deleted)
                    .SortByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();
                return new(sub);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> GetByPlanIdAsync(string planId)
        {
            try
            {
                Subscription? sub = await context.Subscriptions
                    .Find(x => x.Plan == planId && !x.Deleted)
                    .SortByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();
                return new(sub);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> GetByAsaasSubscriptionIdAsync(string asaasSubscriptionId)
        {
            try
            {
                Subscription? sub = await context.Subscriptions
                    .Find(x => x.AsaasSubscriptionId == asaasSubscriptionId && !x.Deleted)
                    .FirstOrDefaultAsync();
                return new(sub);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> GetByAsaasCustomerIdAsync(string asaasCustomerId)
        {
            try
            {
                Subscription? sub = await context.Subscriptions
                    .Find(x => x.AsaasCustomerId == asaasCustomerId && !x.Deleted)
                    .FirstOrDefaultAsync();
                return new(sub);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> CreateAsync(Subscription subscription)
        {
            try
            {
                await context.Subscriptions.InsertOneAsync(subscription);
                return new(subscription, 201, "Assinatura criada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar assinatura");
            }
        }
        public async Task<ResponseApi<SubscriptionInvoice?>> CreateInvoiceAsync(SubscriptionInvoice subscription)
        {
            try
            {
                await context.SubscriptionInvoices.InsertOneAsync(subscription);
                return new(subscription, 201, "Assinatura criada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar assinatura");
            }
        }
        public async Task<ResponseApi<SubscriptionInvoice?>> UpdateInvoiceAsync(SubscriptionInvoice subscription)
        {
            try
            {
                await context.SubscriptionInvoices.ReplaceOneAsync(x => x.Id.Equals(subscription.Id), subscription);
                return new(subscription, 200, "Assinatura atualizada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> GetInvoiceDateMonthAsync(string plan, int month)
        {
            try
            {
                Subscription invoice = await context.Subscriptions.Find(x => !x.Deleted && x.Plan == plan && x.Date.Month == month).FirstOrDefaultAsync();
                return new(invoice);
            }
            catch
            {
                return new(null, 500, "Falha ao criar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> UpdateAsync(Subscription subscription)
        {
            try
            {
                await context.Subscriptions.ReplaceOneAsync(x => x.Id == subscription.Id, subscription);
                return new(subscription, 200, "Assinatura atualizada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao atualizar assinatura");
            }
        }
    }
}