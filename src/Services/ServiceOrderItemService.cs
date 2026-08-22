using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class ServiceOrderItemService(IServiceOrderItemRepository repository) : IServiceOrderItemService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<ServiceOrderItem> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> ServiceOrderItems = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(ServiceOrderItems.Data, count, pagination.PageNumber, pagination.PageSize);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        
        public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> ServiceOrderItem = await repository.GetByIdAggregateAsync(id);
                if(ServiceOrderItem.Data is null) return new(null, 404, "Loja não encontrada");
                return new(ServiceOrderItem.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<ServiceOrderItem?>> CreateAsync(CreateServiceOrderItemDTO request)
        {
            try
            {
                ServiceOrderItem item = new()
                {
                    ServiceOrderId = request.ServiceOrderId,
                    ItemType = request.ItemType,
                    Description = request.Description,
                    ProductId = request.ProductId,
                    IsManual = request.IsManual,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    Cost = request.Cost,
                    Total = request.Quantity * request.Price,
                    SupplierId = request.SupplierId,
                    SupplierName = request.SupplierName,
                    TechnicianId = request.TechnicianId,
                    TechnicianName = request.TechnicianName,
                    Commission = request.Commission,
                    CommissionType = request.CommissionType,
                    Company = request.Company,
                    Store = request.Store,
                    Plan = request.Plan,
                    CreatedBy = request.CreatedBy,
                };
                ResponseApi<ServiceOrderItem?> response = await repository.CreateAsync(item);
                if (response.Data is null) return new(null, 400, "Falha ao criar item.");
                return new(response.Data, 201, "Item adicionado com sucesso.");
            }
            catch
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<ServiceOrderItem?>> UpdateAsync(UpdateServiceOrderItemDTO request)
        {
            try
            {
                ResponseApi<ServiceOrderItem?> existing = await repository.GetByIdAsync(request.Id);
                if (existing.Data is null) return new(null, 404, "Item não encontrado");

                ServiceOrderItem item = existing.Data;
                item.ItemType = request.ItemType;
                item.Description = request.Description;
                item.ProductId = request.ProductId;
                item.IsManual = request.IsManual;
                item.Quantity = request.Quantity;
                item.Price = request.Price;
                item.Cost = request.Cost;
                item.Total = request.Quantity * request.Price;
                item.SupplierId = request.SupplierId;
                item.SupplierName = request.SupplierName;
                item.TechnicianId = request.TechnicianId;
                item.TechnicianName = request.TechnicianName;
                item.Commission = request.Commission;
                item.CommissionType = request.CommissionType;
                item.UpdatedAt = DateTime.UtcNow;
                item.UpdatedBy = request.UpdatedBy;

                ResponseApi<ServiceOrderItem?> response = await repository.UpdateAsync(item);
                if (!response.IsSuccess) return new(null, 400, "Falha ao atualizar item");
                return new(response.Data, 200, "Item atualizado com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion
        #region DELETE
        public async Task<ResponseApi<ServiceOrderItem>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<ServiceOrderItem> ServiceOrderItem = await repository.DeleteAsync(id);
                if(!ServiceOrderItem.IsSuccess) return new(null, 400, ServiceOrderItem.Message);
                return new(null, 204, "Excluída com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion 
    }
}