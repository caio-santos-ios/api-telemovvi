using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;

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

                if(address.Data is not null)
                {
                    zipCode = address.Data.ZipCode;
                    number = address.Data.Number;
                };

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

                Subscription subscription = new()
                {
                    UserId = userId,
                    AsaasCustomerId = customer.Id,
                    AsaasSubscriptionId = asaasSubscription.Id,
                    AsaasPaymentId = paymentId,
                    PlanType = request.PlanType,
                    PlanId = request.Plan,
                    BillingType = billingType,
                    Status = "PENDING",
                    Value = value,
                    NextDueDate = DateTime.TryParse(nextDueDate, out DateTime nd) ? nd : null,
                    StartDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddMonths(1),
                    PaymentUrl = paymentUrl,
                    IdentificationField = identificationField,
                    PixQrCode = pixQrCode,
                    PixQrCodeImage = pixQrCodeImage
                };

                ResponseApi<Subscription?> result = await repository.CreateAsync(subscription);
                return result;
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Subscription?>> GetCurrentSubscriptionAsync(string userId)
        {
            try
            {
                ResponseApi<Subscription?> sub = await repository.GetByUserIdAsync(userId);
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

        public async Task<ResponseApi<List<AsaasPaymentDetailResponse>>> GetPaymentHistoryAsync(string userId)
        {
            try
            {
                ResponseApi<Subscription?> subResp = await repository.GetByUserIdAsync(userId);
                if (subResp.Data is null) return new([], 200, "Sem assinatura ativa");

                List<AsaasPaymentDetailResponse> history = await asaasHandler.GetPaymentHistoryAsync(subResp.Data.AsaasSubscriptionId);
                return new(history, 200, "ok");
            }
            catch
            {
                return new(null, 500, "Erro ao buscar histórico de pagamentos");
            }
        }

        public async Task<ResponseApi<string>> HandleWebhookAsync(AsaasWebhookDTO webhook)
        {
            try
            {
                // Buscar assinatura pelo ID do Asaas
                ResponseApi<Subscription?> subResp = await repository.GetByAsaasSubscriptionIdAsync(webhook.Payment.Subscription);
                if (subResp.Data is null) return new("Assinatura não encontrada", 404, "Assinatura não encontrada");

                Subscription sub = subResp.Data;

                // Atualizar status conforme evento
                sub.Status = webhook.Event switch
                {
                    "PAYMENT_CONFIRMED" or "PAYMENT_RECEIVED" => "ACTIVE",
                    "PAYMENT_OVERDUE" => "OVERDUE",
                    "PAYMENT_DELETED" or "SUBSCRIPTION_DELETED" => "CANCELLED",
                    _ => sub.Status
                };

                if (sub.Status == "ACTIVE")
                {
                    sub.ExpirationDate = DateTime.UtcNow.AddMonths(1);
                }

                await repository.UpdateAsync(sub);

                if(sub.Status == "ACTIVE") {
                    ResponseApi<Plan?> planResp = await planRepository.GetByIdAsync(sub.PlanId);
                    if (planResp.Data is null) return new(null, 404, "Plano não encontrado");
                    
                    planResp.Data.Type = sub.PlanType;

                    await planRepository.UpdateAsync(planResp.Data);
                };
                return new("ok", 200, "Webhook processado");
            }
            catch
            {
                return new(null, 500, "Erro ao processar webhook");
            }
        }
    }
}