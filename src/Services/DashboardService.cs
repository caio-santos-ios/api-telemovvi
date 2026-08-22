using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models.Base;

namespace api_infor_cell.src.Services
{
    public class DashboardService(IDashboardRepository dashboardRepository) : IDashboardService
    {
        public async Task<ResponseApi<dynamic?>> GetCardsAsync(string plan, string company, string store)
        {
            try
            {
                ResponseApi<dynamic> obj = await dashboardRepository.GetCardsAsync(plan, company, store);
                return new(obj.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> GetMonthlySalesAsync(string plan, string company, string store)
        {
            try
            {
                ResponseApi<dynamic> obj = await dashboardRepository.GetMonthlySalesAsync(plan, company, store);
                return new(obj.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> GetMonthlyTargetAsync(string plan, string company, string store)
        {
            try
            {
                ResponseApi<dynamic> obj = await dashboardRepository.GetMonthlyTargetAsync(plan, company, store);
                return new(obj.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> GetRecentOrdersAsync(string plan, string company, string store)
        {
            try
            {
                ResponseApi<dynamic> obj = await dashboardRepository.GetRecentOrdersAsync(plan, company, store);
                return new(obj.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
    }
}