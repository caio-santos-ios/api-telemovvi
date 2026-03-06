using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class ExchangeService(
        IExchangeRepository repository,
        IStockService stockService,
        IStockRepository stockRepository,
        ISalesOrderItemRepository salesOrderItemRepository,
        ISalesOrderRepository salesOrderRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        IAccountPayableService accountPayableService,
        IAccountReceivableService accountReceivableService,
        IMapper _mapper) : IExchangeService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Exchange> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> Exchanges = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(Exchanges.Data, count, pagination.PageNumber, pagination.PageSize);
            }
            catch { return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."); }
        }

        public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> Exchange = await repository.GetByIdAggregateAsync(id);
                if (Exchange.Data is null) return new(null, 404, "Registro não encontrado");
                return new(Exchange.Data);
            }
            catch { return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."); }
        }

        public async Task<ResponseApi<List<dynamic>>> GetBySalesOrderItemIdAggregateAsync(string salesOrderItemId)
        {
            try
            {
                ResponseApi<List<dynamic>> Exchange = await repository.GetBySalesOrderItemIdAggregateAsync(salesOrderItemId);
                if (Exchange.Data is null) return new(null, 404, "Registro não encontrado");
                return new(Exchange.Data);
            }
            catch { return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."); }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Exchange?>> CreateAsync(CreateExchangeDTO request)
        {
            try
            {
                return request.Type switch
                {
                    "return"   => await ProcessReturnAsync(request),
                    "exchange" => await ProcessExchangeAsync(request),
                    _          => new(null, 400, "Tipo inválido. Use 'return' ou 'exchange'.")
                };
            }
            catch { return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."); }
        }

        // ─── DEVOLUÇÃO ────────────────────────────────────────────────────────────
        private async Task<ResponseApi<Exchange?>> ProcessReturnAsync(CreateExchangeDTO request)
        {
            // 1. Busca item do PDV
            ResponseApi<SalesOrderItem?> itemResp = await salesOrderItemRepository.GetByIdAsync(request.SalesOrderItemId);
            if (itemResp.Data is null) return new(null, 404, "Item do PDV não encontrado.");
            SalesOrderItem item = itemResp.Data;

            // 2. Busca pedido
            ResponseApi<SalesOrder?> orderResp = await salesOrderRepository.GetByIdAsync(item.SalesOrderId);
            if (orderResp.Data is null) return new(null, 404, "PDV não encontrado.");
            SalesOrder order = orderResp.Data;

            string productName = (await productRepository.GetByIdAsync(item.ProductId)).Data?.Name ?? "";

            // 3. Devolve estoque (todos os stocks do item)
            foreach (string stockId in item.StockIds)
            {
                ResponseApi<Stock?> stockResp = await stockRepository.GetByIdAsync(stockId);
                if (stockResp.Data is null) continue;
                stockResp.Data.Quantity          += item.Quantity;
                stockResp.Data.QuantityAvailable += item.Quantity;
                stockResp.Data.UpdatedAt          = DateTime.UtcNow;
                stockResp.Data.UpdatedBy          = request.CreatedBy;
                await stockRepository.UpdateAsync(stockResp.Data);
            }

            // 4. Atualiza status do item
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = request.CreatedBy;
            await salesOrderItemRepository.UpdateAsync(item);

            ResponseApi<List<SalesOrderItem>> allItems = await salesOrderItemRepository.GetBySalesOrderIdAsync(item.SalesOrderId, request.Plan, request.Company, request.Store);

            bool allReturned = allItems.Data is null || allItems.Data.Count == 0;
            if(allItems.Data is not null)
            {
                if(allItems.Data.Count == 1)
                {
                    allReturned = true;
                }
            }

            if (allReturned)
            {
                order.Status     = "Cancelado - Produto Devolvido";
                order.UpdatedAt  = DateTime.UtcNow;
                order.UpdatedBy  = request.CreatedBy;
                await salesOrderRepository.UpdateAsync(order);
            }

            // 6. Financeiro: cashback ou conta a pagar (reembolso)
            if (request.GenerateCashback)
                await GenerateCashbackAsync(order, item.Total, $"devolução do produto {productName}", item.Id, request.CreatedBy);
            else
                await accountPayableService.CreateAsync(new()
                {
                    Plan = request.Plan, Company = request.Company, Store = request.Store,
                    CreatedBy = request.CreatedBy,
                    Amount = Math.Truncate(item.Value * 100) / 100,
                    InstallmentNumber = 1, TotalInstallments = 1,
                    Description = $"Reembolso — devolução do produto {productName} do PDV nº {order.Code}",
                    DueDate = DateTime.UtcNow, IssueDate = DateTime.UtcNow,
                    OriginId = item.Id, OriginType = "exchange",
                });

            // 7. Persiste o registro de devolução
            Exchange exchange = _mapper.Map<Exchange>(request);
            exchange.UpdatedAt     = DateTime.UtcNow;
            exchange.ReleasedStock = true;
            ResponseApi<Exchange?> response = await repository.CreateAsync(exchange);
            return new(response.Data, 201, "Devolução registrada com sucesso!");
        }

        // ─── TROCA ────────────────────────────────────────────────────────────────
        private async Task<ResponseApi<Exchange?>> ProcessExchangeAsync(CreateExchangeDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.ProductId))
                return new(null, 422, "Informe o produto novo para a troca.");

            // 1. Busca item original do PDV
            ResponseApi<SalesOrderItem?> itemResp = await salesOrderItemRepository.GetByIdAsync(request.SalesOrderItemId);
            if (itemResp.Data is null) return new(null, 404, "Item do PDV não encontrado.");
            SalesOrderItem item = itemResp.Data;

            ResponseApi<SalesOrder?> orderResp = await salesOrderRepository.GetByIdAsync(item.SalesOrderId);
            if (orderResp.Data is null) return new(null, 404, "PDV não encontrado.");
            SalesOrder order = orderResp.Data;

            // 2. Valida produto novo
            Product? newProduct = (await productRepository.GetByIdAsync(request.ProductId)).Data;
            if (newProduct is null) return new(null, 404, "Produto novo não encontrado.");

            // 3. Verifica estoque do produto NOVO
            ResponseApi<List<Stock>> stocksResp = await stockRepository.GetVerifyStockAll(request.ProductId, request.Plan, request.Company, request.Store);
            if (stocksResp.Data is null || stocksResp.Data.Count == 0)
                return new(null, 422, $"Produto [{newProduct.Code} - {newProduct.Name}] sem estoque disponível.");

            Stock? newStock = stocksResp.Data.FirstOrDefault(x => x.QuantityAvailable >= item.Quantity) ?? stocksResp.Data.FirstOrDefault(x => x.QuantityAvailable > 0);

            if (newStock is null || newStock.QuantityAvailable < item.Quantity)
                return new(null, 422, $"Estoque insuficiente para [{newProduct.Code} - {newProduct.Name}]. Disponível: {newStock?.QuantityAvailable ?? 0}");

            string oldProductName = (await productRepository.GetByIdAsync(item.ProductId)).Data?.Name ?? "";

            // 4. Devolve estoque do produto ORIGINAL
            foreach (string stockId in item.StockIds)
            {
                ResponseApi<Stock?> oldStockResp = await stockRepository.GetByIdAsync(stockId);
                if (oldStockResp.Data is null) continue;
                oldStockResp.Data.Quantity          += item.Quantity;
                oldStockResp.Data.QuantityAvailable += item.Quantity;
                oldStockResp.Data.UpdatedAt          = DateTime.UtcNow;
                oldStockResp.Data.UpdatedBy          = request.CreatedBy;
                await stockRepository.UpdateAsync(oldStockResp.Data);
            }

            // 5. Baixa estoque do produto NOVO
            newStock.Quantity          -= item.Quantity;
            newStock.QuantityAvailable -= item.Quantity;
            newStock.UpdatedAt          = DateTime.UtcNow;
            newStock.UpdatedBy          = request.CreatedBy;
            await stockRepository.UpdateAsync(newStock);

            // 6. Atualiza o SalesOrderItem com o produto novo
            item.ProductId = request.ProductId;
            item.StockIds = [newStock.Id];
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = request.CreatedBy;
            await salesOrderItemRepository.UpdateAsync(item);

            // 7. Trata saldo (diferença de valor)
            if (request.Balance != 0)
            {
                // balance > 0 = cliente paga a diferença → conta a receber
                // balance < 0 = cliente recebe de volta → cashback ou conta a pagar
                if (request.Balance > 0 && request.TypeBalance == "charge")
                {
                    await accountReceivableService.CreateAsync(new()
                    {
                        Plan = request.Plan, Company = request.Company, Store = request.Store,
                        CreatedBy = request.CreatedBy,
                        CustomerId = order.CustomerId,
                        Description = $"Diferença a cobrar — troca: {oldProductName} → {newProduct.Name} | PDV nº {order.Code}",
                        Amount = Math.Truncate(request.Balance * 100) / 100,
                        InstallmentNumber = 1, TotalInstallments = 1,
                        DueDate = DateTime.UtcNow.AddDays(3), IssueDate = DateTime.UtcNow,
                        OriginId = item.Id, OriginType = "exchange",
                    });
                }
                else if (request.Balance < 0)
                {
                    decimal valueToReturn = Math.Abs(request.Balance);
                    string desc = $"troca: {oldProductName} → {newProduct.Name} | PDV nº {order.Code}";

                    if (request.TypeBalance == "cashback")
                        await GenerateCashbackAsync(order, valueToReturn, desc, item.Id, request.CreatedBy);
                    else
                        await accountPayableService.CreateAsync(new()
                        {
                            Plan = request.Plan, Company = request.Company, Store = request.Store,
                            CreatedBy = request.CreatedBy,
                            Amount = Math.Truncate(valueToReturn * 100) / 100,
                            InstallmentNumber = 1, TotalInstallments = 1,
                            Description = $"Reembolso de diferença — {desc}",
                            DueDate = DateTime.UtcNow, IssueDate = DateTime.UtcNow,
                            OriginId = item.Id, OriginType = "exchange",
                        });
                }
            }

            // 8. Persiste o registro de troca
            Exchange exchange = _mapper.Map<Exchange>(request);
            exchange.UpdatedAt          = DateTime.UtcNow;
            exchange.ReleasedStock      = true;
            exchange.OriginDescription  = $"Troca: {oldProductName} → {newProduct.Name} | PDV nº {order.Code}";
            ResponseApi<Exchange?> response = await repository.CreateAsync(exchange);
            return new(response.Data, 201, "Troca realizada com sucesso!");
        }

        // ─── HELPER: Cashback ─────────────────────────────────────────────────────
        private async Task GenerateCashbackAsync(SalesOrder order, decimal value, string description, string originId, string userId)
        {
            ResponseApi<Customer?> customerResp = await customerRepository.GetByIdAsync(order.CustomerId);
            if (customerResp.Data is null) return;

            string userName = (await userRepository.GetByIdAsync(userId)).Data?.Name ?? "";

            customerResp.Data.UpdatedAt = DateTime.UtcNow;
            customerResp.Data.UpdatedBy = userId;
            customerResp.Data.Cashbacks.Add(new()
            {
                Available         = true,
                Value             = Math.Truncate(value * 100) / 100,
                CurrentValue      = Math.Truncate(value * 100) / 100,
                Date              = DateTime.UtcNow,
                Description       = $"Cashback gerado pela {description}",
                Origin            = "exchange",
                OriginDescription = $"{description} - PDV nº {order.Code}",
                OriginId          = originId,
                Responsible       = userName,
            });
            await customerRepository.UpdateAsync(customerResp.Data);
        }

        public async Task<ResponseApi<Exchange?>> CreateReleasedStockAsync(CreateExchangeDTO request)
        {
            try
            {
                ResponseApi<List<Exchange>> exchanges = await repository.GetReleasedStockAsync(request.Plan, request.Company, request.Store);
                if (!exchanges.IsSuccess || exchanges.Data is null) return new(null, 400, "Falha ao processar");

                foreach (Exchange exchange in exchanges.Data)
                {
                    exchange.UpdatedAt     = DateTime.UtcNow;
                    exchange.UpdatedBy     = request.UpdatedBy;
                    exchange.ReleasedStock = true;

                    await stockService.CreateAsync(new()
                    {
                        ProductId      = exchange.ProductId,
                        Quantity       = 1,
                        Origin         = exchange.Origin,
                        OriginId       = exchange.SalesOrderItemId,
                        ForSale        = exchange.ForSale,
                        Plan           = exchange.Plan,
                        Company        = exchange.Company,
                        Store          = exchange.Store
                    });
                }
                return new(null, 201, "Estoque liberado com sucesso!");
            }
            catch { return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."); }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<Exchange?>> UpdateAsync(UpdateExchangeDTO request)
        {
            try
            {
                ResponseApi<Exchange?> exchangeResponse = await repository.GetByIdAsync(request.Id);
                if (exchangeResponse.Data is null) return new(null, 404, "Registro não encontrado");

                Exchange exchange = _mapper.Map<Exchange>(request);
                exchange.UpdatedAt     = DateTime.UtcNow;
                exchange.ReleasedStock = false;

                ResponseApi<Exchange?> response = await repository.UpdateAsync(exchange);
                if (!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

                return new(response.Data, 200, "Atualizado com sucesso");
            }
            catch { return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."); }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<Exchange>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<Exchange> exchange = await repository.DeleteAsync(id);
                if (!exchange.IsSuccess || exchange.Data is null) return new(null, 400, exchange.Message);
                return new(null, 204, "Excluído com sucesso");
            }
            catch { return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."); }
        }
        #endregion
    }
}