using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Interfaces
{
    public interface IStoreRepository
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Store> pagination);
        Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id);
        Task<ResponseApi<Store?>> GetByIdAsync(string id);
        Task<ResponseApi<Store?>> GetByCompanyIdAsync(string companyId);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(PaginationUtil<Store> pagination);
        Task<ResponseApi<List<Store>>> GetTotalCompanies(string planId, string companyId);
        Task<int> GetCountDocumentsAsync(PaginationUtil<Store> pagination);
        Task<ResponseApi<Store?>> CreateAsync(Store address);
        Task<ResponseApi<Store?>> UpdateAsync(Store address);
        Task<ResponseApi<Store>> DeleteAsync(string id);
    }
}
