using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Interfaces
{
    public interface IAddressRepository
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Address> pagination);
        Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id);
        Task<ResponseApi<Address?>> GetByIdAsync(string id);
        Task<ResponseApi<Address?>> GetByParentIdAsync(string parentId, string parent);
        // Task<ResponseApi<dynamic?>> GetByParentIdAggregationAsync(string parentId, string parent);
        Task<int> GetCountDocumentsAsync(PaginationUtil<Address> pagination);
        Task<ResponseApi<Address?>> CreateAsync(Address address);
        Task<ResponseApi<List<Address>?>> CreateManyAsync(List<Address> addresses);
        Task<ResponseApi<Address?>> UpdateAsync(Address address);
        Task<ResponseApi<Address>> DeleteAsync(string id);
    }
}