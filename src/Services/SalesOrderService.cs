using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class SalesOrderService(ISalesOrderRepository repository, ISalesOrderItemRepository salesOrderItemRepository, IStockRepository stockRepository, IProductRepository productRepository, IExchangeService exchangeService, IBoxRepository boxRepository, IMapper _mapper) : ISalesOrderService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<SalesOrder> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> SalesOrders = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(SalesOrders.Data, count, pagination.PageNumber, pagination.PageSize);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        
        public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> SalesOrder = await repository.GetByIdAggregateAsync(id);
                if(SalesOrder.Data is null) return new(null, 404, "Pedido de Venda não encontrada");
                return new(SalesOrder.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<dynamic?>> GetReceiptByIdAggregateAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> SalesOrder = await repository.GetReceiptByIdAggregateAsync(id);
                if(SalesOrder.Data is null) return new(null, 404, "Pedido de Venda não encontrada");
                return new(SalesOrder.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<SalesOrder?>> CreateAsync(CreateSalesOrderDTO request)
        {
            try
            {
                if(request.CustomerId.ToLower().Equals("ao consumidor"))
                {
                    request.CustomerId = "";
                };

                ResponseApi<long> code = await repository.GetNextCodeAsync(request.Plan, request.Company, request.Store);
                
                SalesOrder salesOrder = _mapper.Map<SalesOrder>(request);
                salesOrder.Status = "Em Aberto";
                salesOrder.Code = code.Data.ToString().PadLeft(6, '0');

                ResponseApi<SalesOrder?> response = await repository.CreateAsync(salesOrder);

                if(response.Data is null) return new(null, 400, "Falha ao criar Pedido de Venda.");
                
                // if(request.CreateItem)
                // {
                //     await salesOrderItemRepository.CreateAsync(new ()
                //     {
                //         Plan = request.Plan,
                //         Company = request.Company,
                //         Store = request.Store,
                //         DiscountType = request.DiscountType,
                //         DiscountValue = request.DiscountValue,
                //         ProductId = request.ProductId,
                //         Quantity = request.Quantity,
                //         Value = request.Value,
                //         Total = request.Total,
                //         CreatedBy = request.CreatedBy,
                //         CreatedAt = DateTime.UtcNow,
                //         SalesOrderId = response.Data.Id,
                //         VariationId = request.VariationId,
                //         CodeVariation = request.CodeVariation,
                //         Serial = request.Serial
                //     });
                // };
                
                return new(response.Data, 201, "Pedido de Venda criado com sucesso.");
            }
            catch
            { 
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<SalesOrder?>> UpdateAsync(UpdateSalesOrderDTO request)
        {
            try
            {
                ResponseApi<SalesOrder?> salesOrderResponse = await repository.GetByIdAsync(request.Id);
                if(salesOrderResponse.Data is null) return new(null, 404, "Falha ao atualizar 1");
                
                salesOrderResponse.Data.UpdatedAt = DateTime.UtcNow;
                salesOrderResponse.Data.UpdatedBy = request.UpdatedBy;
                salesOrderResponse.Data.SellerId = request.SellerId;
                salesOrderResponse.Data.CustomerId = request.CustomerId.ToLower().Equals("ao consumidor") ? "" : request.CustomerId; 

                ResponseApi<SalesOrder?> response = await repository.UpdateAsync(salesOrderResponse.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
                return new(response.Data, 201, "Atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<SalesOrder?>> FinishAsync(FinishSalesOrderDTO request)
        {
            try
            {
                ResponseApi<SalesOrder?> salesOrderResponse = await repository.GetByIdAsync(request.Id);
                if(salesOrderResponse.Data is null) return new(null, 404, "Falha ao finalizar Pedido de Venda");

                ResponseApi<List<SalesOrderItem>> items = await salesOrderItemRepository.GetBySalesOrderIdAsync(request.Id, salesOrderResponse.Data.Plan, salesOrderResponse.Data.Company, salesOrderResponse.Data.Store);
                if(items.Data is not null)
                {
                    foreach (SalesOrderItem salesOrderItem in items.Data)
                    {
                        ResponseApi<Product?> product = await productRepository.GetByIdAsync(salesOrderItem.ProductId);
                        if(product.Data is null) return new(null, 404, "Algum dos Produtos não tem estoque disponível");
                        
                        if(product.Data.HasVariations == "yes")
                        {
                            ResponseApi<Stock?> stock = await stockRepository.GetVerifyStock(salesOrderItem.ProductId, salesOrderItem.Plan, salesOrderItem.Company, salesOrderItem.Store);
                            
                            if(stock.Data is null) return new(null, 404, $"O Produto [{product.Data.Code} - {product.Data.Name}] não tem estoque disponível");
                            if(product.Data.HasSerial == "yes")
                            {
                                bool hasStockAvailable = false;
                                foreach (var variation in stock.Data.Variations)
                                {
                                    VariationItemSerial? serial = variation.Serials.Where(s => s.Code == salesOrderItem.Serial && s.HasAvailable).FirstOrDefault();
                                    if(serial is not null) 
                                    {
                                        serial.HasAvailable = false;
                                        hasStockAvailable = true;
                                    };
                                };

                                if(!hasStockAvailable) return new(null, 404, $"O Produto [{product.Data.Code} - {product.Data.Name} | Serial: {salesOrderItem.Serial}] não tem estoque disponível");

                                stock.Data.UpdatedAt = DateTime.UtcNow;
                                stock.Data.UpdatedBy = request.UpdatedBy;
                                stock.Data.Quantity -= 1;

                                await stockRepository.UpdateAsync(stock.Data);
                            }
                            else
                            {
                                VariationProduct? variation = stock.Data.Variations.Where(v => v.Code.ToString() == salesOrderItem.CodeVariation).FirstOrDefault();
                                if(variation is null) return new(null, 404, $"O Produto [{product.Data.Code} - {product.Data.Name}] não tem estoque disponível");
                                
                                stock.Data.UpdatedAt = DateTime.UtcNow;
                                stock.Data.UpdatedBy = request.UpdatedBy;
                                stock.Data.Quantity -= 1;
                                variation.Stock -= salesOrderItem.Quantity;

                                await stockRepository.UpdateAsync(stock.Data);
                            };
                        } 
                        else
                        {
                            ResponseApi<List<Stock>> stocks = await stockRepository.GetVerifyStockAll(salesOrderItem.ProductId, salesOrderItem.Plan, salesOrderItem.Company, salesOrderItem.Store);
                            if(stocks.Data is null) return new(null, 404, $"O Produto [{product.Data.Code} - {product.Data.Name}] não tem estoque disponível");

                            decimal totalStock = stocks.Data.Sum(x => x.QuantityAvailable);
                            if(totalStock < salesOrderItem.Quantity) return new(null, 404, $"O Produto [{product.Data.Code} - {product.Data.Name}] não tem estoque disponível");

                            decimal remaining = salesOrderItem.Quantity;

                            foreach (Stock stock in stocks.Data)
                            {
                                if (stock.QuantityAvailable == 0) continue;
                                if (remaining <= 0) break;

                                decimal toDeduct = Math.Min(stock.QuantityAvailable, remaining);

                                stock.Quantity -= toDeduct;
                                stock.QuantityAvailable -= toDeduct;
                                stock.UpdatedAt = DateTime.UtcNow;
                                stock.UpdatedBy = request.UpdatedBy;

                                await stockRepository.UpdateAsync(stock);

                                salesOrderItem.StockIds.Add(stock.Id);
                                remaining -= toDeduct;
                            }
                        }
                        
                        await salesOrderItemRepository.UpdateAsync(salesOrderItem);
                    }
                };

                salesOrderResponse.Data.UpdatedAt = DateTime.UtcNow;
                salesOrderResponse.Data.UpdatedBy = request.UpdatedBy;
                salesOrderResponse.Data.Status = "Finalizado";
                salesOrderResponse.Data.Payment = new () 
                {
                    Currier = request.Currier,
                    DiscountType = request.DiscountType,
                    DiscountValue = request.DiscountValue,
                    Freight = request.Freight,
                    NumberOfInstallments = request.NumberOfInstallments,
                    PaymentMethodId = request.PaymentMethodId,
                    Tax = request.Tax
                };
                salesOrderResponse.Data.SubTotal = request.SubTotal;
                salesOrderResponse.Data.Total = request.Total;

                ResponseApi<SalesOrder?> response = await repository.UpdateAsync(salesOrderResponse.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao finalizar Pedido de Venda");
                
                await exchangeService.CreateReleasedStockAsync(new CreateExchangeDTO() { Plan = request.Plan, Company = request.Company, Store = request.Store, UpdatedBy = request.UpdatedBy });

                ResponseApi<Box?> box = await boxRepository.GetByCreatedIdAsync(request.UpdatedBy);
                if(box.Data is not null)
                {
                    box.Data.UpdatedAt = DateTime.Now;
                    box.Data.UpdatedBy = request.UpdatedBy;
                    box.Data.Value += salesOrderResponse.Data.Total;

                    await boxRepository.UpdateAsync(box.Data);
                };

                return new(response.Data, 200, "Pedido de Venda Finalizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        #endregion
        
        #region DELETE
        public async Task<ResponseApi<SalesOrder>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<SalesOrder> SalesOrder = await repository.DeleteAsync(id);
                if(!SalesOrder.IsSuccess) return new(null, 400, SalesOrder.Message);
                return new(null, 204, "Excluída com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion 
    }
}