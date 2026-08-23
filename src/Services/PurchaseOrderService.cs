using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class PurchaseOrderService(IPurchaseOrderRepository repository, IPurchaseOrderItemRepository purchaseOrderItemRepository, IStockService stockService, IMapper _mapper) : IPurchaseOrderService
{
    #region READ
    public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
    {
        try
        {
            PaginationUtil<PurchaseOrder> pagination = new(request.QueryParams);
            ResponseApi<List<dynamic>> PurchaseOrders = await repository.GetAllAsync(pagination);
            int count = await repository.GetCountDocumentsAsync(pagination);
            return new(PurchaseOrders.Data, count, pagination.PageNumber, pagination.PageSize);
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
            ResponseApi<dynamic?> PurchaseOrder = await repository.GetByIdAggregateAsync(id);
            if(PurchaseOrder.Data is null) return new(null, 404, "Pedido de Compra não encontrado");
            return new(PurchaseOrder.Data);
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    #endregion
    
    #region CREATE
    public async Task<ResponseApi<PurchaseOrder?>> CreateAsync(CreatePurchaseOrderDTO request)
    {
        try
        {
            PurchaseOrder product = _mapper.Map<PurchaseOrder>(request);
            ResponseApi<long> code = await repository.GetNextCodeAsync(request.Company, request.Store);
            product.Code = code.Data.ToString().PadLeft(6, '0');
            product.Status = "Rascunho";
            ResponseApi<PurchaseOrder?> response = await repository.CreateAsync(product);

            if(response.Data is null) return new(null, 400, "Falha ao criar Pedido de Compra.");
            return new(response.Data, 201, "Pedido de Compra criado com sucesso.");
        }
        catch
        { 
            return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
        }
    }
    #endregion
    
    #region UPDATE
    public async Task<ResponseApi<PurchaseOrder?>> UpdateAsync(UpdatePurchaseOrderDTO request)
    {
        try
        {
            ResponseApi<PurchaseOrder?> PurchaseOrderResponse = await repository.GetByIdAsync(request.Id);
            if(PurchaseOrderResponse.Data is null) return new(null, 404, "Falha ao atualizar");
            
            PurchaseOrder PurchaseOrder = _mapper.Map<PurchaseOrder>(request);
            PurchaseOrder.UpdatedAt = DateTime.UtcNow;
            PurchaseOrder.Code = PurchaseOrderResponse.Data.Code;
            PurchaseOrder.Status = PurchaseOrderResponse.Data.Status;

            ResponseApi<PurchaseOrder?> response = await repository.UpdateAsync(PurchaseOrder);
            if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
            return new(response.Data, 201, "Atualizado com sucesso");
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    public async Task<ResponseApi<PurchaseOrder?>> UpdateApprovalAsync(UpdatePurchaseOrderDTO request)
    {
        try
        {
            ResponseApi<PurchaseOrder?> purchaseOrderResponse = await repository.GetByIdAsync(request.Id);
            if(purchaseOrderResponse.Data is null) return new(null, 404, "Falha ao atualizar");
            
            purchaseOrderResponse.Data.UpdatedAt = DateTime.UtcNow;
            purchaseOrderResponse.Data.UpdatedBy = request.UpdatedBy;
            purchaseOrderResponse.Data.ApprovalBy = request.UpdatedBy;
            purchaseOrderResponse.Data.Status = "Finalizado";

            ResponseApi<List<PurchaseOrderItem>> items = await purchaseOrderItemRepository.GetByPurchaseOrderIdAsync(request.Id);
            if(items.Data is not null)
            {
                foreach(PurchaseOrderItem item in items.Data)
                {
                    if(item.MoveStock == "yes")
                    {
                        CreateStockDTO stock = new ()
                        {
                            PurchaseOrderItemId = item.Id,
                            Cost = item.Cost,
                            CostDiscount = item.CostDiscount,
                            CreatedBy = request.CreatedBy,
                            Price = item.Price,
                            PriceDiscount = item.PriceDiscount,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            SupplierId = item.SupplierId,
                            Variations = item.Variations,
                            VariationsCode = item.VariationsCode,
                            Company = purchaseOrderResponse.Data.Company,
                            Store = purchaseOrderResponse.Data.Store,
                            Plan = purchaseOrderResponse.Data.Plan,
                            OriginDescription = $"Pedido de Compra - Nº {purchaseOrderResponse.Data.Code}",
                            Origin = "purchase-order-item",
                            ForSale = "yes",
                            OriginId = item.Id,
                            HasProductSerial = item.Variations.Any(v => v.Serials != null && v.Serials.Count > 0) ? "yes" : "no",
                            HasProductVariations = item.Variations.Count > 0 ? "yes" : "no",
                        };

                        await stockService.CreateAsync(stock);
                    }
                };
            };

            ResponseApi<PurchaseOrder?> response = await repository.UpdateAsync(purchaseOrderResponse.Data);
            if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

            return new(null, 201, "Finalizado com sucesso");
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    #endregion
    
    #region DELETE
    public async Task<ResponseApi<PurchaseOrder>> DeleteAsync(string id)
    {
        try
        {
            ResponseApi<PurchaseOrder> PurchaseOrder = await repository.DeleteAsync(id);
            if(!PurchaseOrder.IsSuccess) return new(null, 400, PurchaseOrder.Message);
            return new(null, 204, "Excluído com sucesso");
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    #endregion 

    #region FUCTIONS
    // public (string Key, int Quantidade) ObterKeyMaisRepetida(List<Variation> variacoes)
    // {
    //     if (variacoes == null || !variacoes.Any()) return (string.Empty, 0);

    //     var resultado = variacoes
    //         .GroupBy(v => v.Key) 
    //         .Select(g => new 
    //         { 
    //             g.Key, 
    //             Contagem = g.Count() 
    //         }) 
    //         .OrderByDescending(x => x.Contagem) 
    //         .FirstOrDefault(); 

    //     return (resultado!.Key, resultado.Contagem);
    // }
    #endregion
}
}