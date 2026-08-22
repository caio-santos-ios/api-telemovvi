using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class PlanService(IPlanRepository repository, IMapper _mapper) : IPlanService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Plan> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> companys = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(companys.Data, count, pagination.PageNumber, pagination.PageSize);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
       
        public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> company = await repository.GetByIdAggregateAsync(id);
                if(company.Data is null) return new(null, 404, "Plano não encontrada");
                return new(company.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<Plan?>> CreateAsync(CreatePlanDTO request)
        {
            try
            {
                Plan company = _mapper.Map<Plan>(request);
                ResponseApi<Plan?> response = await repository.CreateAsync(company);

                if(response.Data is null) return new(null, 400, "Falha ao criar Plano.");
                return new(response.Data, 201, "Plano criada com sucesso.");
            }
            catch
            { 
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<Plan?>> UpdateAsync(UpdatePlanDTO request)
        {
            try
            {
                ResponseApi<Plan?> companyResponse = await repository.GetByIdAsync(request.Id);
                if(companyResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                Plan company = _mapper.Map<Plan>(request);
                company.UpdatedAt = DateTime.UtcNow;

                ResponseApi<Plan?> response = await repository.UpdateAsync(company);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
                return new(response.Data, 201, "Atualizada com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion
        
        #region DELETE
        public async Task<ResponseApi<Plan>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<Plan> company = await repository.DeleteAsync(id);
                if(!company.IsSuccess) return new(null, 400, company.Message);

                return new(null, 204, "Excluído com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion 
    }
}