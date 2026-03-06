using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;

namespace api_infor_cell.src.Interfaces
{
    public interface IBudgetItemService
    {
        Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request);
        Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id);
        Task<ResponseApi<BudgetItem?>> CreateAsync(CreateBudgetItemDTO request);
        Task<ResponseApi<BudgetItem?>> UpdateAsync(UpdateBudgetItemDTO request);
        Task<ResponseApi<BudgetItem>> DeleteAsync(string id);
    }
}
