using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Interfaces
{
    public interface IBudgetItemRepository
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<BudgetItem> pagination);
        Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id);
        Task<ResponseApi<BudgetItem?>> GetByIdAsync(string id);
        Task<ResponseApi<List<BudgetItem>>> GetByBudgetIdAsync(string budgetId, string plan, string company, string store);
        Task<int> GetCountDocumentsAsync(PaginationUtil<BudgetItem> pagination);
        Task<ResponseApi<BudgetItem?>> CreateAsync(BudgetItem budgetItem);
        Task<ResponseApi<BudgetItem?>> UpdateAsync(BudgetItem budgetItem);
        Task<ResponseApi<BudgetItem>> DeleteAsync(string id);
    }
}
