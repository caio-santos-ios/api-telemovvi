using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Interfaces
{
    public interface IChartOfAccountsService
    {
        Task<ResponseApi<dynamic?>> GetAllAsync(GetAllDTO request);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllDTO request);
        Task<ResponseApi<dynamic?>> GetByIdAsync(string id);
        Task<ResponseApi<long>> GetNextCodeAsync(string planId, string companyId);
        Task<ResponseApi<ChartOfAccounts?>> CreateAsync(ChartOfAccounts chartOfAccounts);
        Task<ResponseApi<ChartOfAccounts?>> UpdateAsync(ChartOfAccounts chartOfAccounts);
        Task<ResponseApi<ChartOfAccounts?>> DeleteAsync(string id);
        Task<ResponseApi<List<dynamic>>> GetTreeAsync(string planId, string companyId);
    }
}