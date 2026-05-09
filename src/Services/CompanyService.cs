using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using api_infor_cell.src.Shared.Validators;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class CompanyService(
        ICompanyRepository repository, 
        IStoreRepository storeRepository,
        IUserRepository userRepository, 
        CloudinaryHandler cloudinaryHandler, 
        IMapper _mapper
    ) : ICompanyService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Company> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> companies = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(companies.Data, count, pagination.PageNumber, pagination.PageSize);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Company> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> companies = await repository.GetSelectAsync(pagination);
                return new(companies.Data);
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
                ResponseApi<dynamic?> company = await repository.GetByIdAggregateAsync(id);
                if(company.Data is null) return new(null, 404, "Empresa não encontrada");
                return new(company.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<Company?>> CreateAsync(CreateCompanyDTO request)
        {
            try
            {
                Company company = _mapper.Map<Company>(request);

                ResponseApi<Company?> companyExisted = await repository.GetByPlanDocumentAsync(request.Plan, request.Document, "");
                if(companyExisted.Data is not null) return new(null, 400, "CNPJ já cadastrado.");

                ResponseApi<Company?> response = await repository.CreateAsync(company);

                if(response.Data is null) return new(null, 400, "Falha ao criar Empresa.");

                ResponseApi<Store?> responseStore = await storeRepository.CreateAsync(new ()
                {
                    Plan = response.Data.Plan,
                    Company = response.Data.Id,
                    CorporateName = response.Data.CorporateName,
                    TradeName = response.Data.TradeName,
                    Document = response.Data.Document,
                    Email = response.Data.Email,
                    Phone = response.Data.Phone,
                    Whatsapp = response.Data.Whatsapp,
                    StateRegistration = response.Data.StateRegistration,
                    MunicipalRegistration = response.Data.MunicipalRegistration,
                    Website = response.Data.Website,
                    Photo = response.Data.Photo,
                    CreatedBy = request.CreatedBy
                });

                ResponseApi<User?> user = await userRepository.GetBySubscribedAsync(request.Plan);

                if(user.Data is not null && responseStore.Data is not null)
                {
                    user.Data.Companies.Add(company.Id);
                    user.Data.Stores.Add(responseStore.Data.Id);

                    await userRepository.UpdateAsync(user.Data);
                }

                return new(response.Data, 201, "Empresa criada com sucesso.");
            }
            catch
            { 
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<Company?>> UpdateAsync(UpdateCompanyDTO request)
        {
            try
            {
                if(string.IsNullOrEmpty(request.CorporateName)) return  new(null, 400, "Razão social é obrigatório.");
                if(string.IsNullOrEmpty(request.TradeName)) return  new(null, 400, "Nome fantasia é obrigatório.");
                if(string.IsNullOrEmpty(request.Document)) return  new(null, 400, "CNPJ é obrigatório.");
                if(string.IsNullOrEmpty(request.Email)) return  new(null, 400, "CNPJ é obrigatório.");
                if(!Validator.IsEmail(request.Email)) return new(null, 400, "E-mail inválido.");
                if(string.IsNullOrEmpty(request.Phone)) return  new(null, 400, "Telefone obrigatório.");
                if(string.IsNullOrEmpty(request.StateRegistration)) return  new(null, 400, "Incrição estadual obrigatório.");
                if(string.IsNullOrEmpty(request.MunicipalRegistration)) return  new(null, 400, "Incrição municipal obrigatório.");


                ResponseApi<Company?> companyResponse = await repository.GetByIdAsync(request.Id);
                if(companyResponse.Data is null) return new(null, 404, "Falha ao atualizar");

                ResponseApi<Company?> companyExisted = await repository.GetByPlanDocumentAsync(request.Plan, request.Document, request.Id);
                if(companyExisted.Data is not null) return new(null, 400, "CNPJ já cadastrado.");
                
                Company company = _mapper.Map<Company>(request);
                company.UpdatedAt = DateTime.UtcNow;
                company.CreatedAt = companyResponse.Data.CreatedAt;

                ResponseApi<Company?> response = await repository.UpdateAsync(company);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
                return new(response.Data, 201, "Empresa atualizada com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Company?>> SavePhotoProfileAsync(SaveCompanyPhotoDTO request)
        {
            try
            {
                if (request.Photo == null || request.Photo.Length == 0) return new(null, 400, "Falha ao salvar foto de perfil");

                ResponseApi<Company?> user = await repository.GetByIdAsync(request.Id);
                if(user.Data is null) return new(null, 404, "Falha ao salvar foto de perfil");

                var tempPath = Path.GetTempFileName();

                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    request.Photo.CopyTo(stream);
                }

                string uriPhoto = await cloudinaryHandler.UploadAttachment("company", request.Photo);
                user.Data.UpdatedAt = DateTime.UtcNow;
                user.Data.Photo = uriPhoto;

                ResponseApi<Company?> response = await repository.UpdateAsync(user.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao salvar logo");
                return new(new () { Photo = response.Data!.Photo }, 201, "Logo salvo com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
        
        #region DELETE
        public async Task<ResponseApi<Company>> DeleteAsync(string id, string plan)
        {
            try
            {
                ResponseApi<List<Company>> companies = await repository.GetTotalCompanies(plan);
                if(companies.Data is null) return new(null, 400, "O sistema deve possuir ao menos uma empresa cadastrada.");
                if(companies.Data.Count == 1) return new(null, 400, "O sistema deve possuir ao menos uma empresa cadastrada.");

                ResponseApi<Company> company = await repository.DeleteAsync(id);
                if(!company.IsSuccess) return new(null, 400, company.Message);

                ResponseApi<User?> user = await userRepository.GetByCompanyIdAsync(id);
                if(user.Data is not null) return new(null, 400, "Não é possível excluir a empresa, pois existem usuários vinculados a ela.");

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