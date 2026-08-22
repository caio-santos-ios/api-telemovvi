using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class ServiceOrderService(IServiceOrderRepository repository, ISituationRepository situationRepository, IAccountReceivableService accountReceivableService, IMapper _mapper) : IServiceOrderService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<ServiceOrder> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> serviceOrders = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(serviceOrders.Data, count, pagination.PageNumber, pagination.PageSize);
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
                ResponseApi<dynamic?> serviceOrder = await repository.GetByIdAggregateAsync(id);
                if (serviceOrder.Data is null) return new(null, 404, "Ordem de Serviço não encontrada");
                return new(serviceOrder.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> CheckWarrantyAsync(string? customerId, string? serialImei)
        {
            try
            {
                ResponseApi<dynamic?> result = await repository.CheckWarrantyAsync(customerId, serialImei);
                return new(result.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<ServiceOrder?>> CreateAsync(CreateServiceOrderDTO request)
        {
            try
            {
                ResponseApi<long> code = await repository.GetNextCodeAsync(request.Plan, request.Company, request.Store);

                ResponseApi<Situation?> situation = await situationRepository.GetByMomentAsync(request.Plan, request.Company, request.Store, "start");
                
                ServiceOrder serviceOrder = _mapper.Map<ServiceOrder>(request);
                serviceOrder.Status = situation.Data is not null ? situation.Data.Id : "";
                serviceOrder.OpenedAt = DateTime.UtcNow;
                serviceOrder.OpenedByUserId = request.CreatedBy;
                serviceOrder.Code = code.Data.ToString().PadLeft(6, '0');
                serviceOrder.Device = new()
                {
                    Type = request.DeviceType,
                    BrandId = request.BrandId,
                    ModelName = request.ModelName,
                    Color = request.Color,
                    SerialImei = request.SerialImei,
                    CustomerReportedIssue = request.CustomerReportedIssue,
                    UnlockPassword = request.UnlockPassword,
                    Accessories = request.Accessories,
                    PhysicalCondition = request.PhysicalCondition,
                };

                if (!string.IsNullOrEmpty(request.SerialImei) || !string.IsNullOrEmpty(request.CustomerId))
                {
                    ResponseApi<dynamic?> warrantyCheck = await repository.CheckWarrantyAsync(request.CustomerId, request.SerialImei);
                    if (warrantyCheck.Data != null)
                    {
                        serviceOrder.IsWarrantyInternal = true;
                    }
                }

                ResponseApi<ServiceOrder?> response = await repository.CreateAsync(serviceOrder);
                if (response.Data is null) return new(null, 400, "Falha ao criar Ordem de Serviço.");
                return new(response.Data, 201, "Ordem de Serviço criada com sucesso.");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<ServiceOrder?>> UpdateAsync(UpdateServiceOrderDTO request)
        {
            try
            {
                ResponseApi<ServiceOrder?> existing = await repository.GetByIdAsync(request.Id);
                if (existing.Data is null) return new(null, 404, "Ordem de Serviço não encontrada");

                existing.Data.OpenedAt = DateTime.UtcNow;
                existing.Data.OpenedByUserId = request.CreatedBy;
                existing.Data.Code = existing.Data.Code;
                existing.Data.Device = new()
                {
                    Type = request.DeviceType,
                    BrandId = request.BrandId,
                    ModelName = request.ModelName,
                    Color = request.Color,
                    SerialImei = request.SerialImei,
                    CustomerReportedIssue = request.CustomerReportedIssue,
                    UnlockPassword = request.UnlockPassword,
                    Accessories = request.Accessories,
                    PhysicalCondition = request.PhysicalCondition,
                };

                existing.Data.CustomerId = request.CustomerId;
                existing.Data.Status = string.IsNullOrEmpty(request.Status) ? existing.Data.Status : request.Status;
                existing.Data.Notes = request.Notes;
                existing.Data.CancelReason = request.CancelReason;
                existing.Data.DiscountValue = request.DiscountValue;
                existing.Data.DiscountType = request.DiscountType;
                existing.Data.UpdatedAt = DateTime.UtcNow;
                existing.Data.UpdatedBy = request.UpdatedBy;

                existing.Data.Laudo.TechnicalReport = request.TechnicalReport;
                existing.Data.Laudo.TestsPerformed = request.TestsPerformed;
                existing.Data.Laudo.RepairStatus = request.RepairStatus;

                ResponseApi<ServiceOrder?> response = await repository.UpdateAsync(existing.Data);
                if (!response.IsSuccess) return new(null, 400, "Falha ao atualizar Ordem de Serviço.");
                return new(null, 200, "Ordem de Serviço atualizada com sucesso.");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<ServiceOrder?>> CloseAsync(CloseServiceOrderDTO request)
        {
            try
            {
                ResponseApi<ServiceOrder?> existing = await repository.GetByIdAsync(request.Id);
                if (existing.Data is null) return new(null, 404, "Ordem de Serviço não encontrada");

                ServiceOrder serviceOrder = existing.Data;
                serviceOrder.IsClosed = true;
                serviceOrder.ClosedAt = DateTime.UtcNow;
                serviceOrder.ClosedByUserId = request.ClosedByUserId;
                serviceOrder.WarrantyDays = request.WarrantyDays;
                serviceOrder.WarrantyUntil = request.WarrantyUntil ?? DateTime.UtcNow.AddDays(request.WarrantyDays);
                serviceOrder.UpdatedAt = DateTime.UtcNow;
                serviceOrder.UpdatedBy = request.UpdatedBy;

                if (!serviceOrder.IsWarrantyInternal && !string.IsNullOrEmpty(request.PaymentMethodId))
                {
                    serviceOrder.Payment = new ServiceOrderPayment
                    {
                        PaymentMethodId = request.PaymentMethodId,
                        NumberOfInstallments = request.NumberOfInstallments,
                        PaidAt = DateTime.UtcNow,
                        PaidByUserId = request.ClosedByUserId
                    };
                }

                ResponseApi<ServiceOrder?> response = await repository.UpdateAsync(serviceOrder);
                if (!response.IsSuccess) return new(null, 400, "Falha ao encerrar Ordem de Serviço.");

                ResponseApi<Situation?> situation = await situationRepository.GetByIdAsync(serviceOrder.Status);
                if(situation.Data is not null)
                {
                    if(situation.Data.GenerateFinancial && !serviceOrder.IsWarrantyInternal && !string.IsNullOrEmpty(request.PaymentMethodId))
                    {
                        DateTime issueDate = DateTime.UtcNow;

                        for (int i = 0; i < request.NumberOfInstallments; i++)
                        {
                            DateTime currentIssue = issueDate.AddMonths(i);
                            DateTime dueDate = currentIssue.AddDays(3);

                            await accountReceivableService.CreateAsync(new CreateAccountReceivableDTO()
                            {
                                Plan = serviceOrder.Plan,
                                Company = serviceOrder.Company,
                                Store = serviceOrder.Store,
                                CreatedBy = request.UpdatedBy,
                                CustomerId = serviceOrder.CustomerId,
                                Description = $"O.S. nº {serviceOrder.Code}",
                                DueDate = dueDate,
                                InstallmentNumber = i + 1,
                                OriginId = serviceOrder.Id,
                                OriginType = "service-order",
                                TotalInstallments = request.NumberOfInstallments,
                                PaymentMethodId = request.PaymentMethodId,
                                Amount = Math.Truncate(request.Value * 100) / 100,
                                IssueDate = issueDate,
                                IsPaymented = true
                            });
                        }
                    }
                }

                return new(response.Data, 200, "Ordem de Serviço encerrada com sucesso.");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<ServiceOrder>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<ServiceOrder> serviceOrder = await repository.DeleteAsync(id);
                if (!serviceOrder.IsSuccess) return new(null, 400, serviceOrder.Message);
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