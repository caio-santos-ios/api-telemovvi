using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Interfaces
{
    public interface ISerialNumberRepository
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<SerialNumber> pagination);
        Task<ResponseApi<SerialNumber?>> GetByIdAsync(string id);
        Task<bool> ExistsCodeAsync(string plan, string company, string code);
        Task<ResponseApi<SerialNumber?>> CreateAsync(SerialNumber serialNumber);
    }
}
