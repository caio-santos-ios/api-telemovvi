using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Services
{
    public class DreService(IDreRepository repository) : IDreService
    {
        public async Task<ResponseApi<dynamic?>> GenerateAsync(
            string planId,
            string companyId,
            string storeId,
            DateTime startDate,
            DateTime endDate,
            string regime)
        {
            try
            {
                if (startDate > endDate)
                {
                    return new(null, 400, "Data inicial não pode ser maior que data final");
                }

                if (regime != "caixa" && regime != "competencia")
                {
                    return new(null, 400, "Regime deve ser 'caixa' ou 'competencia'");
                }

                ResponseApi<dynamic> response = await repository.GenerateAsync(
                    planId, companyId, storeId, startDate, endDate, regime
                );
                
                return new(response.Data);
            }
            catch
            {
                return new(null, 500, "Erro ao gerar DRE");
            }
        }
    }
}