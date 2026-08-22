using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class AccountReceivableService(IAccountReceivableRepository repository, IMapper _mapper) : IAccountReceivableService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Models.AccountReceivable> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> accountsReceivable = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(accountsReceivable.Data, count, pagination.PageNumber, pagination.PageSize);
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
                ResponseApi<dynamic?> accountReceivable = await repository.GetByIdAggregateAsync(id);
                if (accountReceivable.Data is null) return new(null, 404, "Conta a receber não encontrada");
                return new(accountReceivable.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Models.AccountReceivable?>> CreateAsync(CreateAccountReceivableDTO request)
        {
            try
            {
                Models.AccountReceivable accountReceivable = _mapper.Map<Models.AccountReceivable>(request);

                ResponseApi<long> code = await repository.GetNextCodeAsync(request.Plan, request.Company, request.Store);
                accountReceivable.Code = code.Data.ToString().PadLeft(6, '0');
                accountReceivable.Status = "open";
                accountReceivable.AmountPaid = 0;

                if(request.IsPaymented)
                {
                    accountReceivable.AmountPaid = request.Amount;
                    accountReceivable.PaidAt = DateTime.UtcNow;
                    accountReceivable.Status = "paid";
                }

                ResponseApi<Models.AccountReceivable?> response = await repository.CreateAsync(accountReceivable);
                if (response.Data is null) return new(null, 400, "Falha ao criar conta a receber.");
                return new(response.Data, 201, "Conta a receber criada com sucesso.");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<Models.AccountReceivable?>> UpdateAsync(UpdateAccountReceivableDTO request)
        {
            try
            {
                ResponseApi<Models.AccountReceivable?> existing = await repository.GetByIdAsync(request.Id);
                if (existing.Data is null) return new(null, 404, "Conta a receber não encontrada");

                Models.AccountReceivable accountReceivable = _mapper.Map<Models.AccountReceivable>(request);
                accountReceivable.UpdatedAt = DateTime.UtcNow;
                accountReceivable.UpdatedBy = request.UpdatedBy;

                accountReceivable.Code = existing.Data.Code;
                accountReceivable.Status = existing.Data.Status;
                accountReceivable.AmountPaid = existing.Data.AmountPaid;
                accountReceivable.PaidAt = existing.Data.PaidAt;

                ResponseApi<Models.AccountReceivable?> response = await repository.UpdateAsync(accountReceivable);
                if (!response.IsSuccess) return new(null, 400, "Falha ao atualizar conta a receber");
                return new(response.Data, 200, "Conta a receber atualizada com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<Models.AccountReceivable?>> PayAsync(PayAccountReceivableDTO request)
        {
            try
            {
                ResponseApi<Models.AccountReceivable?> existing = await repository.GetByIdAsync(request.Id);
                if (existing.Data is null) return new(null, 404, "Conta a receber não encontrada");

                if (existing.Data.Status == "paid")
                    return new(null, 400, "Este título já foi baixado.");

                if (request.AmountPaid <= 0)
                    return new(null, 400, "O valor recebido deve ser maior que zero.");

                existing.Data.AmountPaid = request.AmountPaid;
                existing.Data.PaidAt = request.PaidAt;
                existing.Data.UpdatedAt = DateTime.UtcNow;

                // Pagamento parcial automático se valor < total
                existing.Data.Status = (request.AmountPaid < existing.Data.Amount && request.Status != "cancelled")
                    ? "partial"
                    : request.Status;

                ResponseApi<Models.AccountReceivable?> response = await repository.PayAsync(existing.Data);
                if (!response.IsSuccess) return new(null, 400, "Falha ao baixar título");
                return new(response.Data, 200, "Título baixado com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<Models.AccountReceivable>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<Models.AccountReceivable> response = await repository.DeleteAsync(id);
                if (!response.IsSuccess) return new(null, 400, response.Message);
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
