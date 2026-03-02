using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Services
{
    public class ChartOfAccountsService(IChartOfAccountsRepository repository) : IChartOfAccountsService
    {
        public async Task<ResponseApi<dynamic?>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<ChartOfAccounts> pagination = new(request.QueryParams);

                ResponseApi<List<dynamic>> list = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);

                dynamic response = new
                {
                    data = list.Data,
                    page = pagination.PageNumber,
                    pageSize = pagination.PageSize,
                    count
                };

                return new(response);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Plano de Contas");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<ChartOfAccounts> pagination = new(request.QueryParams);

                ResponseApi<List<dynamic>> list = await repository.GetSelectAsync(pagination);

                return new(list.Data);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Plano de Contas");
            }
        }

        public async Task<ResponseApi<dynamic?>> GetByIdAsync(string id)
        {
            try
            {
                dynamic? obj = (await repository.GetByIdAggregateAsync(id)).Data;
                return new(obj);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Conta");
            }
        }

        public async Task<ResponseApi<long>> GetNextCodeAsync(string planId, string companyId)
        {
            try
            {
                // long nextCode = (await repository.GetNextCodeAsync(planId, companyId)).Data;
                return new();
            }
            catch
            {
                return new(1, 500, "Falha ao buscar próximo código");
            }
        }

        public async Task<ResponseApi<ChartOfAccounts?>> CreateAsync(ChartOfAccounts chartOfAccounts)
        {
            try
            {
                chartOfAccounts.CreatedAt = DateTime.UtcNow;
                long nextCode = (await repository.GetNextCodeAsync(chartOfAccounts.Plan, chartOfAccounts.Company, chartOfAccounts.Store, chartOfAccounts.Type, chartOfAccounts.GroupDRE)).Data;
                chartOfAccounts.Code = $"{chartOfAccounts.Account}.{nextCode.ToString().PadLeft(3, '0')}";
                ResponseApi<ChartOfAccounts?> response = await repository.CreateAsync(chartOfAccounts);
                return new(response.Data, response.StatusCode, "Conta criada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar Conta");
            }
        }

        public async Task<ResponseApi<ChartOfAccounts?>> UpdateAsync(ChartOfAccounts chartOfAccounts)
        {
            try
            {
                ResponseApi<ChartOfAccounts?> existingAccount = await repository.GetByIdAsync(chartOfAccounts.Id);

                if (existingAccount.Data is null)
                {
                    return new(null, 404, "Conta não encontrada");
                }

                existingAccount.Data.UpdatedAt = DateTime.UtcNow;
                existingAccount.Data.UpdatedBy = chartOfAccounts.UpdatedBy;
                existingAccount.Data.Name = chartOfAccounts.Name;
                existingAccount.Data.Type = chartOfAccounts.Type;
                existingAccount.Data.Account = chartOfAccounts.Account;
                existingAccount.Data.GroupDRE = chartOfAccounts.GroupDRE;

                ResponseApi<ChartOfAccounts?> response = await repository.UpdateAsync(existingAccount.Data);
                return new(response.Data, response.StatusCode, "Conta atualizada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao atualizar Conta");
            }
        }

        public async Task<ResponseApi<ChartOfAccounts?>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<ChartOfAccounts?> existingAccount = await repository.GetByIdAsync(id);

                if (existingAccount.Data is null)
                {
                    return new(null, 404, "Conta não encontrada");
                }

                existingAccount.Data.DeletedAt = DateTime.UtcNow;

                ResponseApi<ChartOfAccounts> response = await repository.DeleteAsync(id);
                return new(response.Data, response.StatusCode, "Conta excluída com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao excluir Conta");
            }
        }

        public async Task<ResponseApi<List<dynamic>>> GetTreeAsync(string planId, string companyId)
        {
            try
            {
                ResponseApi<List<dynamic>> response = await repository.GetTreeAsync(planId, companyId);
                return new(response.Data);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar árvore de contas");
            }
        }
    }
}