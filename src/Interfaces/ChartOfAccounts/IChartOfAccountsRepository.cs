using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Interfaces
{
    public interface IChartOfAccountsRepository
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<ChartOfAccounts> pagination);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(PaginationUtil<ChartOfAccounts> pagination);
        Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id);
        Task<ResponseApi<ChartOfAccounts?>> GetByIdAsync(string id);
        Task<ResponseApi<long>> GetNextCodeAsync(string plan, string company, string store, string type, string groupDRE);
        Task<int> GetCountDocumentsAsync(PaginationUtil<ChartOfAccounts> pagination);
        Task<ResponseApi<ChartOfAccounts?>> CreateAsync(ChartOfAccounts chartOfAccounts);
        Task<ResponseApi<ChartOfAccounts?>> UpdateAsync(ChartOfAccounts chartOfAccounts);
        Task<ResponseApi<ChartOfAccounts>> DeleteAsync(string id);
        Task<ResponseApi<List<dynamic>>> GetTreeAsync(string planId, string companyId);
    }
}