using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class BudgetItemService(
        IBudgetItemRepository repository,
        IBudgetRepository budgetRepository,
        IMapper _mapper) : IBudgetItemService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<BudgetItem> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> budgetItems = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(budgetItems.Data, count, pagination.PageNumber, pagination.PageSize);
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
                ResponseApi<dynamic?> budgetItem = await repository.GetByIdAggregateAsync(id);
                if (budgetItem.Data is null) return new(null, 404, "Item do Orçamento não encontrado");
                return new(budgetItem.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<BudgetItem?>> CreateAsync(CreateBudgetItemDTO request)
        {
            try
            {
                BudgetItem budgetItem = _mapper.Map<BudgetItem>(request);

                ResponseApi<BudgetItem?> response = await repository.CreateAsync(budgetItem);
                if (response.Data is null) return new(null, 400, "Falha ao criar Item do Orçamento.");

                // Atualiza totais do orçamento
                ResponseApi<Budget?> budget = await budgetRepository.GetByIdAsync(request.BudgetId);
                if (budget.Data is not null)
                {
                    ResponseApi<List<BudgetItem>> items = await repository.GetByBudgetIdAsync(
                        budget.Data.Id, request.Plan, request.Company, request.Store);

                    if (items.Data is not null)
                    {
                        budget.Data.Total = items.Data.Sum(x => x.Total);
                        budget.Data.SubTotal = items.Data.Sum(x => x.SubTotal);
                        budget.Data.Quantity = items.Data.Sum(x => x.Quantity);
                        budget.Data.UpdatedAt = DateTime.UtcNow;
                        await budgetRepository.UpdateAsync(budget.Data);
                    }
                }

                return new(response.Data, 201, "Item adicionado ao Orçamento com sucesso.");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<BudgetItem?>> UpdateAsync(UpdateBudgetItemDTO request)
        {
            try
            {
                ResponseApi<BudgetItem?> budgetItemResponse = await repository.GetByIdAsync(request.Id);
                if (budgetItemResponse.Data is null) return new(null, 404, "Item do Orçamento não encontrado");

                budgetItemResponse.Data.UpdatedAt = DateTime.UtcNow;
                budgetItemResponse.Data.UpdatedBy = request.UpdatedBy;
                budgetItemResponse.Data.ProductId = request.ProductId;
                budgetItemResponse.Data.VariationId = request.VariationId;
                budgetItemResponse.Data.CodeVariation = request.CodeVariation;
                budgetItemResponse.Data.Total = request.Total;
                budgetItemResponse.Data.SubTotal = request.SubTotal;
                budgetItemResponse.Data.Value = request.Value;
                budgetItemResponse.Data.Quantity = request.Quantity;
                budgetItemResponse.Data.DiscountValue = request.DiscountValue;
                budgetItemResponse.Data.DiscountType = request.DiscountType;
                budgetItemResponse.Data.Serial = request.Serial;

                ResponseApi<BudgetItem?> response = await repository.UpdateAsync(budgetItemResponse.Data);
                if (!response.IsSuccess) return new(null, 400, "Falha ao atualizar Item do Orçamento");

                // Atualiza totais do orçamento
                ResponseApi<Budget?> budget = await budgetRepository.GetByIdAsync(request.BudgetId);
                if (budget.Data is not null)
                {
                    ResponseApi<List<BudgetItem>> items = await repository.GetByBudgetIdAsync(
                        budget.Data.Id, request.Plan, request.Company, request.Store);

                    if (items.Data is not null)
                    {
                        budget.Data.Total = items.Data.Sum(x => x.Total);
                        budget.Data.SubTotal = items.Data.Sum(x => x.SubTotal);
                        budget.Data.Quantity = items.Data.Sum(x => x.Quantity);
                        budget.Data.UpdatedAt = DateTime.UtcNow;
                        await budgetRepository.UpdateAsync(budget.Data);
                    }
                }

                return new(response.Data, 201, "Item do Orçamento atualizado com sucesso.");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<BudgetItem>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<BudgetItem> response = await repository.DeleteAsync(id);
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
