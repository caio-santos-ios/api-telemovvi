using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class CustomerService(ICustomerRepository repository, IStockRepository stockRepository, IMapper _mapper) : ICustomerService
{
    #region READ
    public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
    {
        try
        {
            PaginationUtil<Customer> pagination = new(request.QueryParams);
            ResponseApi<List<dynamic>> Customers = await repository.GetAllAsync(pagination);
            int count = await repository.GetCountDocumentsAsync(pagination);
            return new(Customers.Data, count, pagination.PageNumber, pagination.PageSize);
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    public async Task<ResponseApi<List<dynamic>>> GetMovementAsync(GetAllDTO request)
    {
        try
        {
            PaginationUtil<Customer> pagination = new(request.QueryParams);
            ResponseApi<List<dynamic>> Customers = await repository.GetMovementAsync(pagination);
            return new(Customers.Data);
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    
    public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
    {
        try
        {
            ResponseApi<dynamic?> Customer = await repository.GetByIdAggregateAsync(id);
            if(Customer.Data is null) return new(null, 404, "Cliente não encontrada");
            return new(Customer.Data);
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    #endregion
    
    #region CREATE
    public async Task<ResponseApi<Customer?>> CreateAsync(CreateCustomerDTO request)
    {
        try
        {
            string messageName = request.Type == "F" ? "O Nome é obrigatório" : "A Razão Social é obrigatória";
            string messageDocument = request.Type == "F" ? "O CPF é obrigatório" : "O CNPJ é obrigatório";

            if(string.IsNullOrEmpty(request.CorporateName)) return new(null, 400, messageName);
            if(string.IsNullOrEmpty(request.Document)) return new(null, 400, messageDocument);
            if(string.IsNullOrEmpty(request.Email)) return new(null, 400, "O E-mail é obrigatório");

            ResponseApi<Customer?> existedDocument = await repository.GetByDocumentAsync(request.Document, "");
            string messageExited = request.Type == "F" ? "Este CPF já está sendo utilizado por outro Cliente" : "Este CNPJ já está sendo utilizado por outro Cliente";
            if(existedDocument.Data is not null) return new(null, 400, messageExited);

            ResponseApi<Customer?> existedEmail = await repository.GetByEmailAsync(request.Email, "");
            if(existedEmail.Data is not null) return new(null, 400, "Este e-mail já está sendo utilizado por outro Cliente");
            
            Customer Customer = _mapper.Map<Customer>(request);
            if(request.Type == "F")
            {
                Customer.TradeName = request.CorporateName;
            };
            
            ResponseApi<Customer?> response = await repository.CreateAsync(Customer);

            if(response.Data is null) return new(null, 400, "Falha ao criar Cliente.");
            return new(response.Data, 201, "Cliente criado com sucesso.");
        }
        catch
        { 
            return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
        }
    }
    public async Task<ResponseApi<Customer?>> CreateMinimalAsync(CreateCustomerMinimalDTO request)
    {
        try
        {
            if(string.IsNullOrEmpty(request.CorporateName)) return new(null, 400, request.Type == "F" ? "O Nome é obrigatório" : "A Razão Social é obrigatória");            

            if(!string.IsNullOrEmpty(request.Document))
            {
                ResponseApi<Customer?> existedDocument = await repository.GetByDocumentAsync(request.Document, "");
                string messageExited = request.Type == "F" ? "Este CPF já está sendo utilizado por outro Cliente" : "Este CNPJ já está sendo utilizado por outro Cliente";
                if(existedDocument.Data is not null) return new(null, 400, messageExited);
            }

            if(!string.IsNullOrEmpty(request.Email))
            {
                ResponseApi<Customer?> existedEmail = await repository.GetByEmailAsync(request.Email, "");
                if(existedEmail.Data is not null) return new(null, 400, "Este e-mail já está sendo utilizado por outro Cliente");
            }

            ResponseApi<Customer?> response = await repository.CreateAsync(new()
            {
                Plan = request.Plan,
                Company = request.Company,
                Store = request.Store,
                CreatedBy = request.CreatedBy,
                CorporateName = request.CorporateName,
                TradeName = request.Type == "F" ? request.CorporateName : request.TradeName,
                Type = request.Type,
                Document = request.Document,
                Email = request.Email,
                Phone = request.Phone
            });

            if(response.Data is null) return new(null, 400, "Falha ao criar Cliente.");
            return new(response.Data, 201, "Cliente criado com sucesso.");
        }
        catch
        { 
            return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
        }
    }
    #endregion
    
    #region UPDATE
    public async Task<ResponseApi<Customer?>> UpdateAsync(UpdateCustomerDTO request)
    {
        try
        {
            ResponseApi<Customer?> CustomerResponse = await repository.GetByIdAsync(request.Id);
            if(CustomerResponse.Data is null) return new(null, 404, "Falha ao atualizar");

            string messageName = request.Type == "F" ? "O Nome é obrigatório" : "A Razão Social é obrigatória";
            string messageDocument = request.Type == "F" ? "O CPF é obrigatório" : "O CNPJ é obrigatório";

            if(string.IsNullOrEmpty(request.CorporateName)) return new(null, 400, messageName);
            if(string.IsNullOrEmpty(request.Document)) return new(null, 400, messageDocument);
            if(string.IsNullOrEmpty(request.Email)) return new(null, 400, "O E-mail é obrigatório");

            ResponseApi<Customer?> existedDocument = await repository.GetByDocumentAsync(request.Document, request.Id);
            string messageExited = request.Type == "F" ? "Este CPF já está sendo utilizado por outro Cliente" : "Este CNPJ já está sendo utilizado por outro Cliente";
            if(existedDocument.Data is not null) return new(null, 400, messageExited);

            ResponseApi<Customer?> existedEmail = await repository.GetByEmailAsync(request.Email, request.Id);
            if(existedEmail.Data is not null) return new(null, 400, "Este e-mail já está sendo utilizado por outro Cliente");
            
            Customer customer = _mapper.Map<Customer>(request);
            customer.UpdatedAt = DateTime.UtcNow;
            customer.CreatedAt = CustomerResponse.Data.CreatedAt;

            if(request.Type == "F")
            {
                customer.TradeName = request.CorporateName;
            };

            ResponseApi<Customer?> response = await repository.UpdateAsync(customer);
            if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
            return new(response.Data, 201, "Atualizada com sucesso");
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    public async Task<ResponseApi<Customer?>> UpdateMinimalAsync(CreateCustomerMinimalDTO request)
    {
        try
        {
            if(string.IsNullOrEmpty(request.CorporateName)) return new(null, 400, request.Type == "F" ? "O Nome é obrigatório" : "A Razão Social é obrigatória");            

            ResponseApi<Customer?> existedDocument = await repository.GetByDocumentAsync(request.Document, "");
            string messageExited = request.Type == "F" ? "Este CPF já está sendo utilizado por outro Cliente" : "Este CNPJ já está sendo utilizado por outro Cliente";
            if(existedDocument.Data is not null) return new(null, 400, messageExited);

            ResponseApi<Customer?> existedEmail = await repository.GetByEmailAsync(request.Email, "");
            if(existedEmail.Data is not null) return new(null, 400, "Este e-mail já está sendo utilizado por outro Cliente");
            
            ResponseApi<Customer?> response = await repository.CreateAsync(new()
            {
                Plan = request.Plan,
                Company = request.Company,
                Store = request.Store,
                CreatedBy = request.CreatedBy,
                CorporateName = request.CorporateName,
                TradeName = request.Type == "F" ? request.CorporateName : request.TradeName,
                Type = request.Type,
                Document = request.Document,
                Email = request.Email,
                Phone = request.Phone
            });

            if(response.Data is null) return new(null, 400, "Falha ao criar Cliente.");
            return new(response.Data, 201, "Cliente criado com sucesso.");
        }
        catch
        { 
            return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
        }
    }
    public async Task<ResponseApi<Customer?>> UpdateCashbackAsync(UpdateCustomerCashbackDTO request)
    {
        try
        {
            ResponseApi<Customer?> customer = await repository.GetByIdAsync(request.Id);
            if(customer.Data is null) return new(null, 400, "Cliente não foi encontrado");

            customer.Data.UpdatedAt = DateTime.UtcNow;
            customer.Data.UpdatedBy = request.UpdatedBy;
            customer.Data.Cashbacks = request.Cashbacks;
            customer.Data.TotalCashback = request.Cashbacks.Sum(x => x.Value);
            customer.Data.TotalCurrentCashback = request.Cashbacks.Sum(x => x.CurrentValue);

            ResponseApi<Customer?> response = await repository.UpdateAsync(customer.Data);

            if(response.Data is null) return new(null, 400, "Falha ao criar Cliente.");

            if(!string.IsNullOrEmpty(request.ProductId))
            {
                ResponseApi<Stock?> stock = await stockRepository.GetVerifyStock(request.ProductId, response.Data.Plan, response.Data.Company, response.Data.Store);
                if(stock.Data is null) return new(null, 400, "Sem estoque do produto 1"); 

                stock.Data.UpdatedAt = DateTime.UtcNow;
                stock.Data.UpdatedBy = request.UpdatedBy;
                stock.Data.QuantityAvailable -= 1;

                if(stock.Data.HasProductVariations == "yes")
                {
                    if(stock.Data.HasProductSerial == "yes") 
                    {
                        bool hasAvailable = false;
                        foreach (VariationProduct variationProduct in stock.Data.Variations)
                        {
                            if(variationProduct.Stock == 0 || hasAvailable) continue;
                            
                            VariationItemSerial? serial = variationProduct.Serials.Where(x => x.HasAvailable).FirstOrDefault();
                            
                            if(serial is not null)
                            {
                                serial.HasAvailable = false;
                                hasAvailable = true;
                            }
                        };

                        if(!hasAvailable) return new(null, 400, "Sem estoque do produto"); 
                    }
                    else
                    {
                        VariationProduct? variationProduct = stock.Data.Variations.Where(x => x.Stock > 0).FirstOrDefault();
                        if(variationProduct is null) return new(null, 400, "Sem estoque do produto"); 

                        variationProduct.Stock -= 1;
                    }
                }

                stock.Data.CustomerIdReserved.Add(response.Data.Id);
                stock.Data.IsReserved = true;
                
                await stockRepository.UpdateAsync(stock.Data);
            }

            return new(response.Data, 200, "Cashback adicionado com sucesso.");
        }
        catch
        { 
            return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
        }
    }
    #endregion
    
    #region DELETE
    public async Task<ResponseApi<Customer>> DeleteAsync(string id)
    {
        try
        {
            ResponseApi<Customer> Customer = await repository.DeleteAsync(id);
            if(!Customer.IsSuccess) return new(null, 400, Customer.Message);
            return new(null, 204, "Excluída com sucesso");
        }
        catch
        {
            return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
    #endregion 
}
}