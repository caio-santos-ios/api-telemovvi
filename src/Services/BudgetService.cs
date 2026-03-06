using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class BudgetService(
        IBudgetRepository repository,
        IBudgetItemRepository budgetItemRepository,
        ISalesOrderRepository salesOrderRepository,
        ISalesOrderItemRepository salesOrderItemRepository,
        IMapper _mapper) : IBudgetService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Budget> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> budgets = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(budgets.Data, count, pagination.PageNumber, pagination.PageSize);
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
                ResponseApi<dynamic?> budget = await repository.GetByIdAggregateAsync(id);
                if (budget.Data is null) return new(null, 404, "Orçamento não encontrado");
                return new(budget.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Budget?>> CreateAsync(CreateBudgetDTO request)
        {
            try
            {
                if (request.CustomerId.ToLower().Equals("ao consumidor"))
                {
                    request.CustomerId = "";
                }

                ResponseApi<long> code = await repository.GetNextCodeAsync(request.Plan, request.Company, request.Store);

                Budget budget = _mapper.Map<Budget>(request);
                budget.Status = "Em Aberto";
                budget.Code = code.Data.ToString().PadLeft(6, '0');

                ResponseApi<Budget?> response = await repository.CreateAsync(budget);
                if (response.Data is null) return new(null, 400, "Falha ao criar Orçamento.");

                return new(response.Data, 201, "Orçamento criado com sucesso.");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<Budget?>> UpdateAsync(UpdateBudgetDTO request)
        {
            try
            {
                ResponseApi<Budget?> budgetResponse = await repository.GetByIdAsync(request.Id);
                if (budgetResponse.Data is null) return new(null, 404, "Orçamento não encontrado");

                budgetResponse.Data.UpdatedAt = DateTime.UtcNow;
                budgetResponse.Data.UpdatedBy = request.UpdatedBy;
                budgetResponse.Data.SellerId = request.SellerId;
                budgetResponse.Data.CustomerId = request.CustomerId.ToLower().Equals("ao consumidor") ? "" : request.CustomerId;
                budgetResponse.Data.Validity = request.Validity;
                budgetResponse.Data.Notes = request.Notes;

                ResponseApi<Budget?> response = await repository.UpdateAsync(budgetResponse.Data);
                if (!response.IsSuccess) return new(null, 400, "Falha ao atualizar Orçamento");

                return new(response.Data, 201, "Atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region CONVERT TO SALES ORDER
        public async Task<ResponseApi<string>> ConvertToSalesOrderAsync(ConvertBudgetToSalesOrderDTO request)
        {
            try
            {
                // Busca o orçamento
                ResponseApi<Budget?> budgetResponse = await repository.GetByIdAsync(request.BudgetId);
                if (budgetResponse.Data is null) return new(null, 404, "Orçamento não encontrado");

                Budget budget = budgetResponse.Data;

                // Busca os itens do orçamento
                ResponseApi<List<BudgetItem>> itemsResponse = await budgetItemRepository.GetByBudgetIdAsync(
                    budget.Id, budget.Plan, budget.Company, budget.Store);

                if (itemsResponse.Data is null || itemsResponse.Data.Count == 0)
                    return new(null, 400, "Orçamento não possui itens para converter.");

                // Gera código do novo pedido de venda
                ResponseApi<long> code = await salesOrderRepository.GetNextCodeAsync(budget.Plan, budget.Company, budget.Store);

                // Cria o pedido de venda com os dados do orçamento
                SalesOrder salesOrder = new()
                {
                    Plan = budget.Plan,
                    Company = budget.Company,
                    Store = budget.Store,
                    CreatedBy = request.UpdatedBy,
                    CreatedAt = DateTime.UtcNow,
                    SellerId = budget.SellerId,
                    CustomerId = budget.CustomerId,
                    Total = budget.Total,
                    SubTotal = budget.SubTotal,
                    Quantity = budget.Quantity,
                    DiscountValue = budget.DiscountValue,
                    DiscountType = budget.DiscountType,
                    Status = "Em Aberto",
                    Code = code.Data.ToString().PadLeft(6, '0'),
                };

                ResponseApi<SalesOrder?> salesOrderResponse = await salesOrderRepository.CreateAsync(salesOrder);
                if (salesOrderResponse.Data is null) return new(null, 400, "Falha ao criar Pedido de Venda.");

                // Copia os itens do orçamento para o pedido de venda
                foreach (BudgetItem budgetItem in itemsResponse.Data)
                {
                    SalesOrderItem salesOrderItem = new()
                    {
                        Plan = budget.Plan,
                        Company = budget.Company,
                        Store = budget.Store,
                        CreatedBy = request.UpdatedBy,
                        CreatedAt = DateTime.UtcNow,
                        SalesOrderId = salesOrderResponse.Data.Id,
                        ProductId = budgetItem.ProductId,
                        VariationId = budgetItem.VariationId,
                        CodeVariation = budgetItem.CodeVariation,
                        Total = budgetItem.Total,
                        SubTotal = budgetItem.SubTotal,
                        Value = budgetItem.Value,
                        Quantity = budgetItem.Quantity,
                        DiscountValue = budgetItem.DiscountValue,
                        DiscountType = budgetItem.DiscountType,
                        Serial = budgetItem.Serial,
                        Status = string.Empty,
                    };

                    await salesOrderItemRepository.CreateAsync(salesOrderItem);
                }

                // Marca o orçamento como convertido
                budget.Status = "Convertido";
                budget.UpdatedAt = DateTime.UtcNow;
                budget.UpdatedBy = request.UpdatedBy;
                await repository.UpdateAsync(budget);

                return new(salesOrderResponse.Data.Id, 201, "Orçamento convertido em Pedido de Venda com sucesso.");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<Budget>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<Budget> response = await repository.DeleteAsync(id);
                return response;
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
    }
}
