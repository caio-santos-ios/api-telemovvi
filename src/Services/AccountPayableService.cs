using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class AccountPayableService(IAccountPayableRepository repository, IMapper _mapper) : IAccountPayableService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Models.AccountPayable> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> accountsPayable = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(accountsPayable.Data, count, pagination.PageNumber, pagination.PageSize);
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
                ResponseApi<dynamic?> accountPayable = await repository.GetByIdAggregateAsync(id);
                if (accountPayable.Data is null) return new(null, 404, "Conta a pagar não encontrada");
                return new(accountPayable.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Models.AccountPayable?>> CreateAsync(CreateAccountPayableDTO request)
        {
            try
            {
                Models.AccountPayable accountPayable = _mapper.Map<Models.AccountPayable>(request);

                ResponseApi<long> code = await repository.GetNextCodeAsync(request.Plan, request.Company, request.Store);
                accountPayable.Code = code.Data.ToString().PadLeft(6, '0');
                accountPayable.Status = "open";
                accountPayable.AmountPaid = 0;

                ResponseApi<Models.AccountPayable?> response = await repository.CreateAsync(accountPayable);
                if (response.Data is null) return new(null, 400, "Falha ao criar conta a pagar.");
                return new(response.Data, 201, "Conta a pagar criada com sucesso.");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<Models.AccountPayable?>> UpdateAsync(UpdateAccountPayableDTO request)
        {
            try
            {
                ResponseApi<Models.AccountPayable?> existing = await repository.GetByIdAsync(request.Id);
                if (existing.Data is null) return new(null, 404, "Conta a pagar não encontrada");

                Models.AccountPayable accountPayable = _mapper.Map<Models.AccountPayable>(request);
                accountPayable.UpdatedAt = DateTime.UtcNow;
                accountPayable.UpdatedBy = request.UpdatedBy;
                // Preserva campos gerados/controlados
                accountPayable.Code = existing.Data.Code;
                accountPayable.Status = existing.Data.Status;
                accountPayable.AmountPaid = existing.Data.AmountPaid;
                accountPayable.PaidAt = existing.Data.PaidAt;

                ResponseApi<Models.AccountPayable?> response = await repository.UpdateAsync(accountPayable);
                if (!response.IsSuccess) return new(null, 400, "Falha ao atualizar conta a pagar");
                return new(response.Data, 200, "Conta a pagar atualizada com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<Models.AccountPayable?>> PayAsync(PayAccountPayableDTO request)
        {
            try
            {
                ResponseApi<Models.AccountPayable?> existing = await repository.GetByIdAsync(request.Id);
                if (existing.Data is null) return new(null, 404, "Conta a pagar não encontrada");

                if (existing.Data.Status == "paid")
                    return new(null, 400, "Este título já foi baixado.");

                if (request.AmountPaid <= 0)
                    return new(null, 400, "O valor pago deve ser maior que zero.");

                existing.Data.AmountPaid = request.AmountPaid;
                existing.Data.PaidAt = request.PaidAt;
                existing.Data.UpdatedAt = DateTime.UtcNow;

                // Pagamento parcial automático se valor < total
                existing.Data.Status = (request.AmountPaid < existing.Data.Amount && request.Status != "cancelled")
                    ? "partial"
                    : request.Status;

                ResponseApi<Models.AccountPayable?> response = await repository.PayAsync(existing.Data);
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
        public async Task<ResponseApi<Models.AccountPayable>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<Models.AccountPayable> response = await repository.DeleteAsync(id);
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
