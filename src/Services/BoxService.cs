using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class BoxService(IBoxRepository repository, IMapper _mapper) : IBoxService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Box> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> Boxs = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(Boxs.Data, count, pagination.PageNumber, pagination.PageSize);
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
                ResponseApi<dynamic?> Box = await repository.GetByIdAggregateAsync(id);
                if(Box.Data is null) return new(null, 404, "Caixa não encontrada");
                return new(Box.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> GetByCreatedIdAggregateAsync(string createdBy)
        {
            try
            {
                ResponseApi<dynamic?> Box = await repository.GetByCreatedIdAggregateAsync(createdBy);
                return new(Box.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<Box?>> CreateAsync(CreateBoxDTO request)
        {
            try
            {
                Box box = _mapper.Map<Box>(request);
                box.Status = "opened";
                box.OpendBy = request.CreatedBy;

                ResponseApi<Box?> response = await repository.CreateAsync(box);

                if(response.Data is null) return new(null, 400, "Falha ao abrir Caixa.");
                return new(response.Data, 201, "Caixa aberto com sucesso.");
            }
            catch
            { 
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<Box?>> UpdateAsync(UpdateBoxDTO request)
        {
            try
            {
                ResponseApi<Box?> boxResponse = await repository.GetByIdAsync(request.Id);
                if(boxResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                Box box = _mapper.Map<Box>(request);
                box.UpdatedAt = DateTime.UtcNow;
                box.CreatedAt = boxResponse.Data.CreatedAt;
                
                ResponseApi<Box?> response = await repository.UpdateAsync(box);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

                return new(response.Data, 201, "Atualizada com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<Box?>> UpdateCloseAsync(UpdateBoxDTO request)
        {
            try
            {
                ResponseApi<Box?> boxResponse = await repository.GetByIdAsync(request.Id);
                if(boxResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                Box box = _mapper.Map<Box>(request);
                box.UpdatedAt = DateTime.UtcNow;
                box.CreatedAt = boxResponse.Data.CreatedAt;
                box.ClosedBy = request.UpdatedBy;
                box.Status = "closed";
                
                ResponseApi<Box?> response = await repository.UpdateAsync(box);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

                box.Id = "";
                box.OpeningValue = 0;
                box.ClosingValue = 0;
                box.ClosedBy = "";
                box.OpendBy = request.UpdatedBy;
                box.Status = "opened";
                box.CreatedAt = DateTime.Now;
                
                ResponseApi<Box?> responseOpen = await repository.CreateAsync(box);
                if(responseOpen.Data is null) return new(null, 400, "Falha ao abrir Caixa.");

                return new(response.Data, 201, "Caixa fechado e aberto com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<Box?>> UpdateSangriaAsync(UpdateBoxDTO request)
        {
            try
            {
                ResponseApi<Box?> boxResponse = await repository.GetByIdAsync(request.Id);
                if(boxResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                boxResponse.Data.UpdatedAt = DateTime.UtcNow;
                boxResponse.Data.UpdatedBy = request.UpdatedBy;
                boxResponse.Data.Value -= request.ValueSettings;
                boxResponse.Data.Sangria.Add(request.ValueSettings);

                ResponseApi<Box?> response = await repository.UpdateAsync(boxResponse.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

                return new(response.Data, 201, "Sanfria feita com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<Box?>> UpdateReinforceAsync(UpdateBoxDTO request)
        {
            try
            {
                ResponseApi<Box?> boxResponse = await repository.GetByIdAsync(request.Id);
                if(boxResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                boxResponse.Data.UpdatedAt = DateTime.UtcNow;
                boxResponse.Data.UpdatedBy = request.UpdatedBy;
                boxResponse.Data.Value += request.ValueSettings;
                boxResponse.Data.Reinforce.Add(request.ValueSettings);

                ResponseApi<Box?> response = await repository.UpdateAsync(boxResponse.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

                return new(response.Data, 201, "Reforço feito com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<Box?>> UpdateClosingAsync(UpdateBoxDTO request)
        {
            try
            {
                ResponseApi<Box?> boxResponse = await repository.GetByIdAsync(request.Id);
                if(boxResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                boxResponse.Data.UpdatedAt = DateTime.UtcNow;
                boxResponse.Data.ClosedBy = request.UpdatedBy;
                boxResponse.Data.Status = "closed";
                boxResponse.Data.ClosingValue = boxResponse.Data.Value;

                ResponseApi<Box?> response = await repository.UpdateAsync(boxResponse.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

                return new(response.Data, 201, "Caixa fechado com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        #endregion
        
        #region DELETE
        public async Task<ResponseApi<Box>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<Box> Box = await repository.DeleteAsync(id);
                if(!Box.IsSuccess) return new(null, 400, Box.Message);
                return new(null, 204, "Excluída com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion 
    }
}