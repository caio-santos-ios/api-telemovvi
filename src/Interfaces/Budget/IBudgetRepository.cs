using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Interfaces
{
    public interface IBudgetRepository
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Budget> pagination);
        Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id);
        Task<ResponseApi<Budget?>> GetByIdAsync(string id);
        Task<int> GetCountDocumentsAsync(PaginationUtil<Budget> pagination);
        Task<ResponseApi<long>> GetNextCodeAsync(string planId, string companyId, string storeId);
        Task<ResponseApi<Budget?>> CreateAsync(Budget budget);
        Task<ResponseApi<Budget?>> UpdateAsync(Budget budget);
        Task<ResponseApi<Budget>> DeleteAsync(string id);
    }
}
