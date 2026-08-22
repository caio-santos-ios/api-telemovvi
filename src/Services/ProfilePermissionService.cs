using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class ProfilePermissionService(IProfilePermissionRepository repository, IMapper _mapper) : IProfilePermissionService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<ProfilePermission> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> ProfilePermissions = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(ProfilePermissions.Data, count, pagination.PageNumber, pagination.PageSize);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<dynamic?>> GetLoggedAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> user = await repository.GetLoggedAsync(id);
                if(user.Data is null) return new(null, 404, "Usuário não encontrado");
                return new(user.Data);
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
                ResponseApi<dynamic?> ProfilePermission = await repository.GetByIdAggregateAsync(id);
                if(ProfilePermission.Data is null) return new(null, 404, "Perfil de Usuário não encontrada");
                return new(ProfilePermission.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<ProfilePermission> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> profilePermissions = await repository.GetSelectAsync(pagination);
                return new(profilePermissions.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        } 
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<ProfilePermission?>> CreateAsync(CreateProfilePermissionDTO request)
        {
            try
            {
                ProfilePermission profilePermission = _mapper.Map<ProfilePermission>(request);
                ResponseApi<long> code = await repository.GetNextCodeAsync(request.Plan, request.Company, request.Store);

                profilePermission.Code = code.Data.ToString().PadLeft(6, '0');
                ResponseApi<ProfilePermission?> response = await repository.CreateAsync(profilePermission);

                if(response.Data is null) return new(null, 400, "Falha ao criar Perfil de Usuário.");
                return new(response.Data, 201, "Perfil de Usuário criada com sucesso.");
            }
            catch
            { 
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }

        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<ProfilePermission?>> UpdateAsync(UpdateProfilePermissionDTO request)
        {
            try
            {
                ResponseApi<ProfilePermission?> profilePermissionResponse = await repository.GetByIdAsync(request.Id);
                if(profilePermissionResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                ProfilePermission profilePermission = _mapper.Map<ProfilePermission>(request);
                profilePermission.UpdatedAt = DateTime.UtcNow;
                profilePermission.CreatedAt = profilePermissionResponse.Data.CreatedAt;
                profilePermission.Code = profilePermissionResponse.Data.Code;

                ResponseApi<ProfilePermission?> response = await repository.UpdateAsync(profilePermission);
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
        public async Task<ResponseApi<ProfilePermission>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<ProfilePermission> ProfilePermission = await repository.DeleteAsync(id);
                if(!ProfilePermission.IsSuccess) return new(null, 400, ProfilePermission.Message);
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