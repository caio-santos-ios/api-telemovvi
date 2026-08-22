using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using api_telemovvi.src.Shared.DTOs.Subscription;

namespace api_infor_cell.src.Services
{
    public class SubscriptionService
    (
        ISubscriptionRepository repository,
        IPlanRepository planRepository,
        ICompanyRepository companyRepository,
        IAddressRepository addressRepository,
        AsaasHandler asaasHandler
    ) : ISubscriptionService
    {
        private static readonly Dictionary<string, decimal> PlanPrices = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Bronze",  119m },
            { "Prata",   199m },
            { "Ouro",    289m },
            { "Platina", 379m }
        };
        public async Task<ResponseApi<Subscription?>> CreateSubscriptionAsync(CreateSubscriptionDTO request, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PlanType)) return new(null, 400, "Tipo de plano é obrigatório");

                if (!PlanPrices.TryGetValue(request.PlanType, out decimal value)) return new(null, 400, $"Plano '{request.PlanType}' inválido. Opções: Bronze, Prata, Ouro, Platina");

                string billingType = request.BillingType.ToUpper();

                if (!new List<string> { "PIX", "BOLETO", "CREDIT_CARD", "DEBIT_CARD" }.Contains(billingType)) return new(null, 400, "Forma de pagamento inválida. Opções: PIX, BOLETO e CARTÃO DE CRÉDITO");

                ResponseApi<Company?> companyResp = await companyRepository.GetByIdAsync(request.Company);

                if (companyResp.Data is null) return new(null, 404, "Usuário não encontrado");

                Company company = companyResp.Data;

                AsaasCustomerResponse? customer = await asaasHandler.GetOrCreateCustomerAsync(
                    name: company.CorporateName,
                    cpfCnpj: company.Document,
                    email: company.Email,
                    phone: company.Phone
                );

                if (customer is null) return new(null, 500, "Erro ao criar cliente no Asaas");

                string zipCode = "";
                string number = "";

                ResponseApi<Address?> address = await addressRepository.GetByParentIdAsync(company.Id, "company");

                if (address.Data is not null)
                {
                    zipCode = address.Data.ZipCode;
                    number = address.Data.Number;
                }
                ;

                AsaasCardData? cardData = null;
                if (billingType is "CREDIT_CARD" or "DEBIT_CARD")
                {
                    if (string.IsNullOrWhiteSpace(request.CardNumber)) return new(null, 400, "Dados do cartão são obrigatórios para pagamento com cartão");
                    if (address.Data is null) return new(null, 400, "Empresa precisa ter um endereço cadastrado");

                    cardData = new AsaasCardData
                    {
                        HolderName = request.CardHolderName ?? company.CorporateName,
                        Number = request.CardNumber ?? string.Empty,
                        ExpiryMonth = request.CardExpiryMonth ?? string.Empty,
                        ExpiryYear = request.CardExpiryYear ?? string.Empty,
                        Cvv = request.CardCvv ?? string.Empty,
                        HolderEmail = company.Email,
                        HolderCpfCnpj = company.Document,
                        HolderPostalCode = zipCode,
                        HolderAddressNumber = number,
                        HolderPhone = company.Phone
                    };
                }

                string nextDueDate = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd");

                AsaasSubscriptionResponse? asaasSubscription = await asaasHandler.CreateSubscriptionAsync(
                    customerId: customer.Id,
                    value: value,
                    billingType: billingType,
                    nextDueDate: nextDueDate,
                    card: cardData
                );

                if (asaasSubscription is null || (asaasSubscription.Errors?.Count > 0))
                {
                    string errorMsg = asaasSubscription?.Errors?.FirstOrDefault()?.Description ?? "Erro ao criar assinatura no Asaas";
                    return new(null, 400, errorMsg);
                }

                string paymentUrl = "";
                string identificationField = "";
                string pixQrCode = "";
                string pixQrCodeImage = "";
                string paymentId = "";

                AsaasPaymentDetailResponse? payment = await asaasHandler.GetLastPaymentFromSubscriptionAsync(asaasSubscription.Id);
                if (payment is not null)
                {
                    paymentId = payment.Id;
                    paymentUrl = payment.InvoiceUrl ?? payment.BankSlipUrl ?? "";

                    if (billingType == "PIX")
                    {
                        AsaasPixResponse? pix = await asaasHandler.GetPixQrCodeAsync(payment.Id);
                        if (pix is not null)
                        {
                            pixQrCode = pix.Payload;
                            pixQrCodeImage = pix.EncodedImage;
                        }
                    }
                    else if (billingType == "BOLETO")
                    {
                        AsaasBoletoResponse? boleto = await asaasHandler.GetBoletoIdentificationFieldAsync(payment.Id);
                        if (boleto is not null)
                            identificationField = boleto.IdentificationField;
                    }
                }

                DateTime today = DateTime.UtcNow;

                Subscription subscription = new()
                {
                    Company = request.Company,
                    Store = request.Store,
                    Plan = request.Plan,
                    CreatedBy = request.CreatedBy,
                    AsaasCustomerId = customer.Id,
                    AsaasSubscriptionId = asaasSubscription.Id,
                    AsaasPaymentId = paymentId,
                    PlanType = request.PlanType,
                    BillingType = billingType,
                    Status = "Pendente Pagamento",
                    Value = value,
                    NextDueDate = DateTime.TryParse(nextDueDate, out DateTime nd) ? nd : null,
                    StartDate = today,
                    ExpirationDate = today.AddMonths(1),
                    DueDate = today.AddDays(3),
                    Date = today,
                    PaymentUrl = paymentUrl,
                    IdentificationField = identificationField,
                    PixQrCode = pixQrCode,
                    PixQrCodeImage = pixQrCodeImage
                };

                ResponseApi<Subscription?> result = await repository.CreateAsync(subscription);

                return result;
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<Subscription?>> GetCurrentSubscriptionAsync(string plan)
        {
            try
            {
                ResponseApi<Subscription?> sub = await repository.GetByUserIdAsync(plan);
                return sub;
            }
            catch
            {
                return new(null, 500, "Erro ao buscar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> GetByPlanAsync(string plan)
        {
            try
            {
                ResponseApi<Subscription?> sub = await repository.GetByPlanIdAsync(plan);
                return sub;
            }
            catch
            {
                return new(null, 500, "Erro ao buscar assinatura");
            }
        }
        public async Task<ResponseApi<Subscription?>> CancelSubscriptionAsync(string userId)
        {
            try
            {
                ResponseApi<Subscription?> subResp = await repository.GetByUserIdAsync(userId);
                if (subResp.Data is null) return new(null, 404, "Assinatura não encontrada");

                Subscription sub = subResp.Data;
                bool cancelled = await asaasHandler.CancelSubscriptionAsync(sub.AsaasSubscriptionId);
                if (!cancelled) return new(null, 500, "Erro ao cancelar no Asaas");

                sub.Status = "CANCELLED";
                sub.DeletedAt = DateTime.UtcNow;
                sub.Deleted = true;
                await repository.UpdateAsync(sub);

                return new(sub, 200, "Assinatura cancelada com sucesso");
            }
            catch
            {
                return new(null, 500, "Erro ao cancelar assinatura");
            }
        }
        public async Task<ResponseApi<List<Subscription>>> GetPaymentHistoryAsync(string plan)
        {
            try
            {
                ResponseApi<List<Subscription>> invoices = await repository.GetByPlanIdAllAsync(plan);
                return new(invoices.Data);
            }
            catch
            {
                return new(null, 500, "Erro ao buscar histórico de pagamentos");
            }
        }
        public async Task<ResponseApi<string>> HandlerWebhookAsync(HandlerWebhookDTO request)
        {
            try
            {
                // plano criado -                SUBSCRIPTION_CREATED
                if (request.Event.Equals("SUBSCRIPTION_CREATED")) { }

                // criação de pagamento mensal - PAYMENT_CREATED
                if (request.Event.Equals("PAYMENT_CREATED")) await HandleCreateMonthlyBillingWebhookAsync(request);

                // pagamento mensal            - PAYMENT_RECEIVED
                if (request.Event.Equals("PAYMENT_RECEIVED")) await HandlePaymentMonthlyBillingWebhookAsync(request);

                // pagamento vencido
                if (request.Event.Equals("PAYMENT_OVERDUE")) await HandleDueDateMonthlyBillingWebhookAsync(request);
                
                // cancelamento de plano
                
                // alteração de plano

                return new(null, 200, "Webhook processado");
            }
            catch
            {
                return new(null, 500, "Erro ao processar webhook");
            }
        }
        public async Task<ResponseApi<string>> HandleSignatureWebhookAsync(SignaturePlansDTO webhook)
        {
            try
            {
                ResponseApi<Subscription?> subResp = await repository.GetByAsaasSubscriptionIdAsync(webhook.Subscription.Id);
                if (subResp.Data is null) return new(null, 404, "Assinatura não encontrada");

                Subscription sub = subResp.Data;

                string status = "";

                switch (webhook.Event)
                {
                    case "PAYMENT_CONFIRMED" or "PAYMENT_RECEIVED":
                        status = "active";
                        break;

                    case "PAYMENT_DELETED" or "SUBSCRIPTION_DELETED":
                        status = "canceled";
                        break;

                    case "PAYMENT_OVERDUE":
                        status = "overdue";
                        break;
                }

                if (status == "active" && sub.Status != "active")
                {
                    // ResponseApi<Plan?> planResp = await planRepository.GetByIdAsync(sub.PlanId);
                    // if (planResp.Data is null) return new(null, 404, "Plano não encontrado");

                    // planResp.Data.Type = sub.PlanType;

                    // await planRepository.UpdateAsync(planResp.Data);

                    // sub.ExpirationDate = DateTime.UtcNow.AddMonths(1);
                }

                if (status == "active")
                {
                    // DateTime today = DateTime.Now;
                    // ResponseApi<SubscriptionInvoice?> invoice = await repository.GetInvoiceDateMonthAsync(sub.PlanId, today.Month);

                    // if (invoice.Data is not null)
                    // {
                    //     invoice.Data.ExpirationDate = DateTime.UtcNow.AddMonths(1);
                    //     invoice.Data.Status = "Pago";
                    //     invoice.Data.PaymentDate = DateTime.Now;

                    //     await repository.UpdateInvoiceAsync(invoice.Data);
                    // }
                }

                sub.Status = status;
                await repository.UpdateAsync(sub);

                return new(null, 200, "Webhook processado");
            }
            catch
            {
                return new(null, 500, "Erro ao processar webhook");
            }
        }
        private async Task<ResponseApi<string>> HandleCreateMonthlyBillingWebhookAsync(HandlerWebhookDTO webhook)
        {
            try
            {
                ResponseApi<Subscription?> subscription = await repository.GetByAsaasCustomerIdAsync(webhook.Payment.Customer);

                if (subscription.Data is null) return new(null, 404, "Assinatura não encontrada Pagamento");

                ResponseApi<Plan?> plan = await planRepository.GetByIdAsync(subscription.Data.Plan);
                if (plan.Data is null) return new(null, 404, "Assinatura não encontrada Pagamento");

                DateTime today = DateTime.Now;

                ResponseApi<Subscription?> invoice = await repository.GetInvoiceDateMonthAsync(subscription.Data.Plan, today.Month);

                if (invoice.Data is null)
                {
                    Subscription newInvoice = new()
                    {   
                        Company = subscription.Data.Company,
                        Store = subscription.Data.Store,
                        Plan = subscription.Data.Plan,
                        Active = true,
                        Deleted = false,
                        AsaasCustomerId = subscription.Data.AsaasCustomerId,
                        AsaasPaymentId = subscription.Data.AsaasPaymentId,
                        AsaasSubscriptionId = subscription.Data.AsaasSubscriptionId,
                        BillingType = "",
                        DueDate = today.AddDays(3),
                        ExpirationDate = today.AddMonths(1),
                        PaymentDate = null,
                        PaymentUrl = "",
                        PixQrCode = "",
                        PixQrCodeImage = "",
                        Status = "Pendente Pagamento",
                        Value = webhook.Payment.Value,
                        PlanType = plan.Data.Type,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "",
                        Date = DateTime.Now,
                        InvoiceUrl = webhook.Payment.InvoiceUrl,
                        InvoiceNumber = webhook.Payment.InvoiceNumber
                    };

                    await repository.CreateAsync(newInvoice);
                }

                return new(null, 200, "Webhook processado");
            }
            catch
            {
                return new(null, 500, "Erro ao processar webhook");
            }
        }
        private async Task<ResponseApi<string>> HandlePaymentMonthlyBillingWebhookAsync(HandlerWebhookDTO webhook)
        {
            try
            {
                ResponseApi<Subscription?> subResp = await repository.GetByAsaasSubscriptionIdAsync(webhook.Payment.Subscription);
                if (subResp.Data is null) return new(null, 404, "Assinatura não encontrada Pagamento");

                Subscription sub = subResp.Data;
                string planId = sub.Plan;

                List<string> createdSplited = webhook.Payment.DateCreated.Split("-").Select(x => x).ToList();
                if(createdSplited.Count == 3)
                {
                    int month = Convert.ToInt32(createdSplited[1]);
                    ResponseApi<Subscription?> invoice = await repository.GetInvoiceDateMonthAsync(planId, month);

                    if (invoice.Data is not null)
                    {
                        DateTime today = DateTime.UtcNow;

                        invoice.Data.Status = "Pago";
                        invoice.Data.DueDate = today.AddDays(3);
                        invoice.Data.PaymentDate = today;

                        await repository.UpdateAsync(invoice.Data);
                    }

                    ResponseApi<Plan?> plan = await planRepository.GetByIdAsync(planId);
                    if (plan.Data is null) return new(null, 404, "Plano não encontrado");

                    plan.Data.Type = sub.PlanType;
                    plan.Data.Status = "Ativo";

                    await planRepository.UpdateAsync(plan.Data);
                }

                return new(null, 200, "Webhook processado");
            }
            catch
            {
                return new(null, 500, "Erro ao processar webhook");
            }
        }
        private async Task<ResponseApi<string>> HandleDueDateMonthlyBillingWebhookAsync(HandlerWebhookDTO webhook)
        {
            try
            {
                ResponseApi<Subscription?> subscription = await repository.GetByAsaasCustomerIdAsync(webhook.Payment.Customer);

                if (subscription.Data is null) return new(null, 404, "Assinatura não encontrada Pagamento");

                List<string> dueDateSplited = webhook.Payment.DueDate.Split("-").Select(x => x).ToList();
                if(dueDateSplited.Count == 3)
                {
                    int month = Convert.ToInt32(dueDateSplited[1]);
                    ResponseApi<Subscription?> invoice = await repository.GetInvoiceDateMonthAsync(subscription.Data.Plan, month);

                    if (invoice.Data is not null)
                    {
                        invoice.Data.Status = "Vencido";
                        invoice.Data.DueDate = DateTime.Now.AddDays(3);

                        await repository.UpdateAsync(invoice.Data);

                        ResponseApi<Plan?> plan = await planRepository.GetByIdAsync(subscription.Data.Plan);
                        if(plan.Data is not null)
                        {
                            plan.Data.Active = false;
                            plan.Data.Status = "Bloqueado";

                            await planRepository.UpdateAsync(plan.Data);
                        } 
                    }
                }

                return new(null, 200, "Webhook processado");
            }
            catch
            {
                return new(null, 500, "Erro ao processar webhook");
            }
        }
        public async Task<ResponseApi<string>> HandlePaymentWebhookAsync(HandlerWebhookDTO webhook)
        {
            try
            {
                // ResponseApi<Subscription?> subResp = await repository.GetByAsaasSubscriptionIdAsync(webhook.Payment.Subscription);
                // if (subResp.Data is null) return new(null, 404, "Assinatura não encontrada Pagamento");

                // Subscription sub = subResp.Data;

                // string status = "";

                // switch (webhook.Event)
                // {
                //     case "PAYMENT_CONFIRMED" or "PAYMENT_RECEIVED":
                //         status = "Ativo";
                //         break;

                //     case "PAYMENT_DELETED" or "SUBSCRIPTION_DELETED":
                //         status = "Cancelado";
                //         break;

                //     case "PAYMENT_OVERDUE":
                //         status = "Vencido";
                //         break;
                // }

                // if (status == "Ativo" && sub.Status != "Ativo")
                // {
                //     ResponseApi<Plan?> planResp = await planRepository.GetByIdAsync(sub.PlanId);
                //     if (planResp.Data is null) return new(null, 404, "Plano não encontrado");

                //     planResp.Data.Type = sub.PlanType;

                //     await planRepository.UpdateAsync(planResp.Data);

                //     sub.ExpirationDate = DateTime.UtcNow.AddMonths(1);
                // }

                // if (status == "Ativo")
                // {
                //     DateTime today = DateTime.Now;
                //     ResponseApi<SubscriptionInvoice?> invoice = await repository.GetInvoiceDateMonthAsync(sub.PlanId, today.Month);

                //     if (invoice.Data is not null)
                //     {
                //         invoice.Data.ExpirationDate = DateTime.UtcNow.AddMonths(1);
                //         invoice.Data.Status = "Pago";
                //         invoice.Data.PaymentDate = DateTime.Now;

                //         await repository.UpdateInvoiceAsync(invoice.Data);
                //     }
                // }

                // sub.Status = status;
                // await repository.UpdateAsync(sub);

                return new(null, 200, "Webhook processado");
            }
            catch
            {
                return new(null, 500, "Erro ao processar webhook");
            }
        }
    }
}