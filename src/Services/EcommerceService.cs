using api_infor_cell.src.Configuration;
using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using MongoDB.Driver;

namespace api_infor_cell.src.Services
{
    public class EcommerceService(
        AppDbContext context,
        AsaasHandler asaasHandler
        // IStockRepository stockRepository,
        // IProductRepository productRepository
    ) : IEcommerceService
    {
        // ─── CONFIG ──────────────────────────────────────────────────────────────

        public async Task<ResponseApi<EcommerceConfig?>> GetConfigAsync(string plan, string company, string store)
        {
            try
            {
                var config = await context.EcommerceConfigs
                    .Find(x => !x.Deleted && x.Plan == plan && x.Company == company && x.Store == store)
                    .FirstOrDefaultAsync();

                return new(config);
            }
            catch { return new(null, 500, "Erro ao buscar configurações da loja."); }
        }

        public async Task<ResponseApi<EcommerceConfig?>> SaveConfigAsync(SaveEcommerceConfigDTO request)
        {
            try
            {
                var existing = await context.EcommerceConfigs
                    .Find(x => !x.Deleted && x.Plan == request.Plan && x.Company == request.Company && x.Store == request.Store)
                    .FirstOrDefaultAsync();

                if (existing is null)
                {
                    var config = new EcommerceConfig
                    {
                        Plan = request.Plan,
                        Company = request.Company,
                        Store = request.Store,
                        CreatedBy = request.CreatedBy,
                        StoreName = request.StoreName,
                        StoreDescription = request.StoreDescription,
                        LogoUrl = request.LogoUrl,
                        BannerUrl = request.BannerUrl,
                        Enabled = request.Enabled,
                        PrimaryColor = request.PrimaryColor,
                        ShippingEnabled = request.ShippingEnabled,
                        ShippingFixedPrice = request.ShippingFixedPrice,
                        ShippingFreeAbove = request.ShippingFreeAbove,
                        ShippingDescription = request.ShippingDescription,
                    };
                    await context.EcommerceConfigs.InsertOneAsync(config);
                    return new(config, 201, "Configurações salvas com sucesso.");
                }

                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = request.CreatedBy;
                existing.StoreName = request.StoreName;
                existing.StoreDescription = request.StoreDescription;
                existing.LogoUrl = request.LogoUrl;
                existing.BannerUrl = request.BannerUrl;
                existing.Enabled = request.Enabled;
                existing.PrimaryColor = request.PrimaryColor;
                existing.ShippingEnabled = request.ShippingEnabled;
                existing.ShippingFixedPrice = request.ShippingFixedPrice;
                existing.ShippingFreeAbove = request.ShippingFreeAbove;
                existing.ShippingDescription = request.ShippingDescription;

                await context.EcommerceConfigs.ReplaceOneAsync(x => x.Id == existing.Id, existing);
                return new(existing, 200, "Configurações atualizadas com sucesso.");
            }
            catch { return new(null, 500, "Erro ao salvar configurações da loja."); }
        }

        // ─── PRODUTOS PÚBLICOS ────────────────────────────────────────────────────

        public async Task<ResponseApi<List<dynamic>>> GetPublicProductsAsync(string plan, string company, string store, string? search, string? categoryId)
        {
            try
            {
                // busca estoques disponíveis para venda
                var stockFilter = Builders<Stock>.Filter.And(
                    Builders<Stock>.Filter.Eq(x => x.Deleted, false),
                    Builders<Stock>.Filter.Eq(x => x.Plan, plan),
                    Builders<Stock>.Filter.Eq(x => x.Company, company),
                    Builders<Stock>.Filter.Eq(x => x.Store, store),
                    Builders<Stock>.Filter.Eq(x => x.ForSale, "yes"),
                    Builders<Stock>.Filter.Gt(x => x.QuantityAvailable, 0m)
                );

                var stocks = await context.Stocks.Find(stockFilter).ToListAsync();
                var productIds = stocks.Select(s => s.ProductId).Distinct().ToList();

                var productFilter = Builders<Product>.Filter.And(
                    Builders<Product>.Filter.Eq(x => x.Deleted, false),
                    Builders<Product>.Filter.In(x => x.Id, productIds)
                );

                if (!string.IsNullOrEmpty(search))
                    productFilter = Builders<Product>.Filter.And(productFilter,
                        Builders<Product>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(search, "i")));

                if (!string.IsNullOrEmpty(categoryId))
                    productFilter = Builders<Product>.Filter.And(productFilter,
                        Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId));

                var products = await context.Products.Find(productFilter).ToListAsync();

                var result = products.Select(p =>
                {
                    var productStocks = stocks.Where(s => s.ProductId == p.Id).ToList();
                    var totalQty = productStocks.Sum(s => s.QuantityAvailable);
                    var minPrice = productStocks.Min(s => s.Price > 0 ? s.Price : s.PriceDiscount);

                    return (dynamic)new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.DescriptionComplet,
                        p.Code,
                        p.CategoryId,
                        p.BrandId,
                        p.HasVariations,
                        Price = minPrice,
                        Quantity = totalQty,
                        Stocks = productStocks.Select(s => new
                        {
                            s.Id,
                            s.Price,
                            s.PriceDiscount,
                            s.QuantityAvailable,
                            s.Variations,
                            s.HasProductVariations,
                            s.HasProductSerial
                        })
                    };
                }).ToList();

                return new(result);
            }
            catch { return new(null, 500, "Erro ao buscar produtos."); }
        }

        // ─── CLIENTES DA LOJA ─────────────────────────────────────────────────────

        public async Task<ResponseApi<dynamic>> RegisterCustomerAsync(EcommerceRegisterDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name)) return new(null, 400, "Nome é obrigatório");
                if (string.IsNullOrEmpty(request.Email)) return new(null, 400, "E-mail é obrigatório");
                if (string.IsNullOrEmpty(request.Password)) return new(null, 400, "Senha é obrigatória");
                if (string.IsNullOrEmpty(request.Document)) return new(null, 400, "CPF/CNPJ é obrigatório");

                var existing = await context.EcommerceCustomers
                    .Find(x => !x.Deleted && x.Plan == request.Plan && x.Email == request.Email)
                    .FirstOrDefaultAsync();

                if (existing is not null) return new(null, 400, "E-mail já cadastrado.");

                // criar cliente no Asaas
                var asaasCustomer = await asaasHandler.GetOrCreateCustomerAsync(
                    request.Name, request.Document, request.Email, request.Phone);

                var customer = new EcommerceCustomer
                {
                    Plan = request.Plan,
                    Company = request.Company,
                    Store = request.Store,
                    Name = request.Name,
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Phone = request.Phone,
                    Document = request.Document,
                    AsaasCustomerId = asaasCustomer?.Id ?? string.Empty
                };

                await context.EcommerceCustomers.InsertOneAsync(customer);

                var token = GenerateToken(customer);
                return new((dynamic)new { token, customer.Id, customer.Name, customer.Email }, 201, "Conta criada com sucesso.");
            }
            catch { return new(null, 500, "Erro ao criar conta."); }
        }

        public async Task<ResponseApi<dynamic>> LoginCustomerAsync(EcommerceLoginDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email)) return new(null, 400, "E-mail é obrigatório");
                if (string.IsNullOrEmpty(request.Password)) return new(null, 400, "Senha é obrigatória");

                var customer = await context.EcommerceCustomers
                    .Find(x => !x.Deleted && x.Plan == request.Plan && x.Company == request.Company && x.Email == request.Email)
                    .FirstOrDefaultAsync();

                if (customer is null) return new(null, 400, "Dados incorretos.");

                bool valid = BCrypt.Net.BCrypt.Verify(request.Password, customer.Password);
                if (!valid) return new(null, 400, "Dados incorretos.");

                var token = GenerateToken(customer);
                return new((dynamic)new { token, customer.Id, customer.Name, customer.Email }, 200, "Login realizado com sucesso.");
            }
            catch { return new(null, 500, "Erro ao realizar login."); }
        }

        // ─── CHECKOUT ────────────────────────────────────────────────────────────

        public async Task<ResponseApi<EcommerceOrder?>> CheckoutAsync(EcommerceCheckoutDTO request)
        {
            try
            {
                if (!new[] { "PIX", "BOLETO", "CREDIT_CARD" }.Contains(request.BillingType.ToUpper()))
                    return new(null, 400, "Forma de pagamento inválida.");

                if (request.Items.Count == 0)
                    return new(null, 400, "Carrinho vazio.");

                // buscar cliente
                var customer = await context.EcommerceCustomers
                    .Find(x => x.Id == request.CustomerId && !x.Deleted)
                    .FirstOrDefaultAsync();

                if (customer is null) return new(null, 400, "Cliente não encontrado.");

                // buscar config da loja para calcular frete
                var config = await context.EcommerceConfigs
                    .Find(x => x.Plan == request.Plan && x.Company == request.Company && x.Store == request.Store && !x.Deleted)
                    .FirstOrDefaultAsync();

                decimal subtotal = request.Items.Sum(i => i.Price * i.Quantity);
                decimal shipping = 0;

                if (config?.ShippingEnabled == true)
                {
                    if (config.ShippingFreeAbove > 0 && subtotal >= config.ShippingFreeAbove)
                        shipping = 0;
                    else
                        shipping = config.ShippingFixedPrice;
                }

                decimal total = subtotal + shipping;

                // gerar código do pedido
                long count = await context.EcommerceOrders
                    .Find(x => x.Plan == request.Plan && x.Company == request.Company && x.Store == request.Store)
                    .CountDocumentsAsync();
                string code = (count + 1).ToString().PadLeft(6, '0');

                // criar/buscar cliente no Asaas
                string asaasCustomerId = customer.AsaasCustomerId;
                if (string.IsNullOrEmpty(asaasCustomerId))
                {
                    var asaasCustomer = await asaasHandler.GetOrCreateCustomerAsync(
                        customer.Name, customer.Document, customer.Email, customer.Phone);
                    asaasCustomerId = asaasCustomer?.Id ?? string.Empty;
                    if (!string.IsNullOrEmpty(asaasCustomerId))
                    {
                        await context.EcommerceCustomers.UpdateOneAsync(
                            x => x.Id == customer.Id,
                            Builders<EcommerceCustomer>.Update.Set(x => x.AsaasCustomerId, asaasCustomerId));
                    }
                }

                // criar pagamento avulso no Asaas
                string paymentId = string.Empty;
                string paymentUrl = string.Empty;
                string pixQrCode = string.Empty;
                string pixQrCodeImage = string.Empty;
                string identificationField = string.Empty;

                AsaasCardData? cardData = null;
                if (request.BillingType.ToUpper() == "CREDIT_CARD" && !string.IsNullOrEmpty(request.CardNumber))
                {
                    cardData = new AsaasCardData
                    {
                        HolderName = request.CardHolderName ?? customer.Name,
                        Number = request.CardNumber,
                        ExpiryMonth = request.CardExpiryMonth ?? "",
                        ExpiryYear = request.CardExpiryYear ?? "",
                        Cvv = request.CardCvv ?? "",
                        HolderEmail = customer.Email,
                        HolderCpfCnpj = customer.Document,
                        HolderPostalCode = request.ShippingAddress.ZipCode,
                        HolderAddressNumber = request.ShippingAddress.Number,
                        HolderPhone = customer.Phone
                    };
                }

                var payment = await asaasHandler.CreateSinglePaymentAsync(
                    customerId: asaasCustomerId,
                    value: total,
                    billingType: request.BillingType.ToUpper(),
                    description: $"Pedido #{code} - Loja Virtual",
                    dueDate: DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd"),
                    card: cardData
                );

                if (payment is not null)
                {
                    paymentId = payment.Id;
                    paymentUrl = payment.InvoiceUrl ?? payment.BankSlipUrl ?? "";

                    if (request.BillingType.ToUpper() == "PIX")
                    {
                        var pix = await asaasHandler.GetPixQrCodeAsync(payment.Id);
                        if (pix is not null)
                        {
                            pixQrCode = pix.Payload;
                            pixQrCodeImage = pix.EncodedImage;
                        }
                    }
                    else if (request.BillingType.ToUpper() == "BOLETO")
                    {
                        var boleto = await asaasHandler.GetBoletoIdentificationFieldAsync(payment.Id);
                        if (boleto is not null)
                            identificationField = boleto.IdentificationField;
                    }
                }

                var order = new EcommerceOrder
                {
                    Plan = request.Plan,
                    Company = request.Company,
                    Store = request.Store,
                    Code = code,
                    CustomerId = request.CustomerId,
                    CustomerName = customer.Name,
                    Items = request.Items.Select(i => new EcommerceOrderItem
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        StockId = i.StockId,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        Total = i.Price * i.Quantity
                    }).ToList(),
                    Subtotal = subtotal,
                    Shipping = shipping,
                    Total = total,
                    Status = "PENDING",
                    BillingType = request.BillingType.ToUpper(),
                    AsaasPaymentId = paymentId,
                    PaymentUrl = paymentUrl,
                    PixQrCode = pixQrCode,
                    PixQrCodeImage = pixQrCodeImage,
                    IdentificationField = identificationField,
                    ShippingAddress = new EcommerceAddress
                    {
                        ZipCode = request.ShippingAddress.ZipCode,
                        Street = request.ShippingAddress.Street,
                        Number = request.ShippingAddress.Number,
                        Complement = request.ShippingAddress.Complement,
                        Neighborhood = request.ShippingAddress.Neighborhood,
                        City = request.ShippingAddress.City,
                        State = request.ShippingAddress.State
                    }
                };

                await context.EcommerceOrders.InsertOneAsync(order);
                return new(order, 201, "Pedido criado com sucesso.");
            }
            catch { return new(null, 500, "Erro ao processar pedido."); }
        }

        public async Task<ResponseApi<dynamic>> GetOrderByIdAsync(string orderId, string customerId)
        {
            try
            {
                var order = await context.EcommerceOrders
                    .Find(x => x.Id == orderId && x.CustomerId == customerId && !x.Deleted)
                    .FirstOrDefaultAsync();

                if (order is null) return new(null, 404, "Pedido não encontrado.");
                return new((dynamic)order);
            }
            catch { return new(null, 500, "Erro ao buscar pedido."); }
        }

        public async Task<ResponseApi<string>> HandlePaymentWebhookAsync(string paymentId, string status)
        {
            try
            {
                var order = await context.EcommerceOrders
                    .Find(x => x.AsaasPaymentId == paymentId && !x.Deleted)
                    .FirstOrDefaultAsync();

                if (order is null) return new("Pedido não encontrado", 404, "Pedido não encontrado.");

                order.Status = status switch
                {
                    "PAYMENT_CONFIRMED" or "PAYMENT_RECEIVED" => "PAID",
                    "PAYMENT_OVERDUE" => "OVERDUE",
                    "PAYMENT_DELETED" => "CANCELLED",
                    _ => order.Status
                };

                order.UpdatedAt = DateTime.UtcNow;
                await context.EcommerceOrders.ReplaceOneAsync(x => x.Id == order.Id, order);

                return new("ok", 200, "Webhook processado.");
            }
            catch { return new(null, 500, "Erro ao processar webhook."); }
        }

        private static string GenerateToken(EcommerceCustomer customer)
        {
            // token simples JWT para autenticação da loja
            var key = new System.Text.StringBuilder();
            key.Append(customer.Id);
            key.Append('|');
            key.Append(customer.Plan);
            key.Append('|');
            key.Append(customer.Company);
            key.Append('|');
            key.Append(customer.Store);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key.ToString()));
        }
    }
}
