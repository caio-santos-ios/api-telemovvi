using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace api_infor_cell.src.Handlers
{
    /// <summary>
    /// Handler para integração com a API do Asaas.
    /// Configure a chave em appsettings.json: "Asaas:ApiKey" e "Asaas:BaseUrl"
    /// Sandbox: https://sandbox.asaas.com/api/v3
    /// Produção: https://api.asaas.com/v3
    /// </summary>
    public class AsaasHandler()
    {
        private readonly string _apiKey = Environment.GetEnvironmentVariable("KEY") ?? "";
        private readonly string _baseUrl = Environment.GetEnvironmentVariable("URI_ASAAS") ?? "";

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("access_token", _apiKey);
            client.DefaultRequestHeaders.Add("User-Agent", "ERP-SaaS/1.0");
            return client;
        }

        // ─── CUSTOMERS ────────────────────────────────────────────────────────────

        /// <summary>Busca ou cria um cliente no Asaas pelo CPF/CNPJ</summary>
        public async Task<AsaasCustomerResponse?> GetOrCreateCustomerAsync(string name, string cpfCnpj, string email, string phone)
        {
            using var client = CreateClient();
            
            // Buscar cliente existente pelo CPF/CNPJ
            var searchResp = await client.GetAsync($"{_baseUrl}/customers?cpfCnpj={cpfCnpj}");
            if (searchResp.IsSuccessStatusCode)
            {
                var json = await searchResp.Content.ReadAsStringAsync();
                var list = JsonSerializer.Deserialize<AsaasListResponse<AsaasCustomerResponse>>(json, JsonOpts);
                if (list?.Data?.Count > 0) return list.Data[0];
            }

            // Criar novo cliente
            var body = new
            {
                name,
                cpfCnpj,
                email,
                phone = phone.Replace("+55", "").Replace(" ", "").Replace("-", "")
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync($"{_baseUrl}/customers", content);
            var respJson = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AsaasCustomerResponse>(respJson, JsonOpts);
        }

        // ─── SUBSCRIPTIONS ────────────────────────────────────────────────────────

        /// <summary>Cria uma assinatura recorrente mensal no Asaas</summary>
        public async Task<AsaasSubscriptionResponse?> CreateSubscriptionAsync(
            string customerId,
            decimal value,
            string billingType,
            string nextDueDate,
            AsaasCardData? card = null)
        {
            using var client = CreateClient();

            object body;

            if (billingType is "CREDIT_CARD" or "DEBIT_CARD" && card is not null)
            {
                body = new
                {
                    customer = customerId,
                    billingType,
                    value,
                    nextDueDate,
                    cycle = "MONTHLY",
                    description = "Assinatura ERP SaaS",
                    creditCard = new
                    {
                        holderName = card.HolderName,
                        number = card.Number,
                        expiryMonth = card.ExpiryMonth,
                        expiryYear = card.ExpiryYear,
                        ccv = card.Cvv
                    },
                    creditCardHolderInfo = new
                    {
                        name = card.HolderName,
                        email = card.HolderEmail,
                        cpfCnpj = card.HolderCpfCnpj,
                        postalCode = card.HolderPostalCode,
                        addressNumber = card.HolderAddressNumber,
                        phone = card.HolderPhone
                    }
                };
            }
            else
            {
                body = new
                {
                    customer = customerId,
                    billingType,
                    value,
                    nextDueDate,
                    cycle = "MONTHLY",
                    description = "Assinatura ERP SaaS"
                };
            }

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync($"{_baseUrl}/subscriptions", content);
            var respJson = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AsaasSubscriptionResponse>(respJson, JsonOpts);
        }

        /// <summary>Cancela uma assinatura no Asaas</summary>
        public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
        {
            using var client = CreateClient();
            var resp = await client.DeleteAsync($"{_baseUrl}/subscriptions/{subscriptionId}");
            return resp.IsSuccessStatusCode;
        }

        // ─── PAYMENTS (para obter QR Code PIX / Boleto) ───────────────────────────

        /// <summary>Busca histórico de pagamentos de uma assinatura (últimas 12 cobranças)</summary>
        public async Task<List<AsaasPaymentDetailResponse>> GetPaymentHistoryAsync(string subscriptionId)
        {
            using var client = CreateClient();
            var resp = await client.GetAsync($"{_baseUrl}/subscriptions/{subscriptionId}/payments?limit=12&offset=0");
            if (!resp.IsSuccessStatusCode) return [];
            var json = await resp.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<AsaasListResponse<AsaasPaymentDetailResponse>>(json, JsonOpts);
            return list?.Data ?? [];
        }

        /// <summary>Busca o último pagamento pendente de uma assinatura</summary>
        public async Task<AsaasPaymentDetailResponse?> GetLastPaymentFromSubscriptionAsync(string subscriptionId)
        {
            using var client = CreateClient();
            var resp = await client.GetAsync($"{_baseUrl}/subscriptions/{subscriptionId}/payments?status=PENDING&limit=1");

            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<AsaasListResponse<AsaasPaymentDetailResponse>>(json, JsonOpts);
            return list?.Data?.FirstOrDefault();
        }

        /// <summary>Busca detalhes de pagamento PIX (QR Code)</summary>
        public async Task<AsaasPixResponse?> GetPixQrCodeAsync(string paymentId)
        {
            using var client = CreateClient();
            var resp = await client.GetAsync($"{_baseUrl}/payments/{paymentId}/pixQrCode");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AsaasPixResponse>(json, JsonOpts);
        }

        /// <summary>Busca linha digitável do boleto</summary>
        public async Task<AsaasBoletoResponse?> GetBoletoIdentificationFieldAsync(string paymentId)
        {
            using var client = CreateClient();
            var resp = await client.GetAsync($"{_baseUrl}/payments/{paymentId}/identificationField");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AsaasBoletoResponse>(json, JsonOpts);
        }

        // ─── JSON Options ─────────────────────────────────────────────────────────
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    // ─── Response Models ──────────────────────────────────────────────────────────

    public class AsaasListResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; } = [];
    }

    public class AsaasCustomerResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("cpfCnpj")]
        public string CpfCnpj { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class AsaasSubscriptionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public decimal Value { get; set; }
        [JsonPropertyName("billingType")]
        public string BillingType { get; set; } = string.Empty;
        [JsonPropertyName("nextDueDate")]
        public string NextDueDate { get; set; } = string.Empty;
        [JsonPropertyName("errors")]
        public List<AsaasError>? Errors { get; set; }
    }

    public class AsaasPaymentDetailResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("invoiceUrl")]
        public string InvoiceUrl { get; set; } = string.Empty;
        [JsonPropertyName("bankSlipUrl")]
        public string BankSlipUrl { get; set; } = string.Empty;
        [JsonPropertyName("dueDate")]
        public string DueDate { get; set; } = string.Empty;
        [JsonPropertyName("paymentDate")]
        public string? PaymentDate { get; set; }
        [JsonPropertyName("billingType")]
        public string BillingType { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }

    public class AsaasPixResponse
    {
        [JsonPropertyName("encodedImage")]
        public string EncodedImage { get; set; } = string.Empty;
        [JsonPropertyName("payload")]
        public string Payload { get; set; } = string.Empty;
        [JsonPropertyName("expirationDate")]
        public string ExpirationDate { get; set; } = string.Empty;
    }

    public class AsaasBoletoResponse
    {
        [JsonPropertyName("identificationField")]
        public string IdentificationField { get; set; } = string.Empty;
        [JsonPropertyName("nossoNumero")]
        public string NossoNumero { get; set; } = string.Empty;
        [JsonPropertyName("barCode")]
        public string BarCode { get; set; } = string.Empty;
    }

    public class AsaasError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class AsaasCardData
    {
        public string HolderName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string ExpiryMonth { get; set; } = string.Empty;
        public string ExpiryYear { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
        public string HolderEmail { get; set; } = string.Empty;
        public string HolderCpfCnpj { get; set; } = string.Empty;
        public string HolderPostalCode { get; set; } = string.Empty;
        public string HolderAddressNumber { get; set; } = string.Empty;
        public string HolderPhone { get; set; } = string.Empty;
    }
}