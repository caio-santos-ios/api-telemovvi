using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;

namespace api_infor_cell.src.Interfaces
{
    public interface IBudgetService
    {
        Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request);
        Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id);
        Task<ResponseApi<Budget?>> CreateAsync(CreateBudgetDTO request);
        Task<ResponseApi<Budget?>> UpdateAsync(UpdateBudgetDTO request);
        Task<ResponseApi<string>> ConvertToSalesOrderAsync(ConvertBudgetToSalesOrderDTO request);
        Task<ResponseApi<Budget>> DeleteAsync(string id);
    }
}
