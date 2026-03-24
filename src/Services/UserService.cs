using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Templates;
using api_infor_cell.src.Shared.Utils;
using api_infor_cell.src.Shared.Validators;
using CloudinaryDotNet;

namespace api_infor_cell.src.Services
{
    public class UserService(IUserRepository userRepository, IProfilePermissionRepository profilePermissionRepository, IEmployeeRepository employeeRepository, ICompanyRepository companyRepository, SmsHandler smsHandler, MailHandler mailHandler, CloudinaryHandler cloudinaryHandler) : IUserService
    {
        #region CREATE
        public async Task<ResponseApi<User?>> CreateAsync(CreateUserDTO request)
        {
            try
            {
                ResponseApi<User?> isEmail = await userRepository.GetByEmailAsync(request.Email);
                if(isEmail.Data is not null || !Validator.IsEmail(request.Email)) return new(null, 400, "E-mail inválido.");

                if(Validator.IsReliable(request.Password).Equals("Ruim")) return new(null, 400, $"Senha é muito fraca");

                dynamic access = Util.GenerateCodeAccess();

                List<api_infor_cell.src.Models.Module> modules = [];
                foreach (var module in request.Modules)
                {
                    List<api_infor_cell.src.Models.Routine> routines = [];

                    foreach (var routine in module.Routines)
                    {
                        routines.Add(new () 
                        {
                            Code = routine.Code,
                            Description = routine.Description,
                            Permissions = new ()
                            {
                                Create = routine.Permissions.Create,
                                Read = routine.Permissions.Read,
                                Update = routine.Permissions.Update,
                                Delete = routine.Permissions.Delete
                            }
                        });
                    }
                    
                    modules.Add(new () 
                    {
                        Code = module.Code,
                        Description = module.Description,
                        Routines = routines
                    });
                };

                User user = new()
                {
                    UserName = $"usuário{access.CodeAccess}",
                    Email = request.Email,
                    Phone = request.Phone,
                    Name = request.Name,
                    Password = BCrypt.Net.BCrypt.HashPassword(access.CodeAccess),
                    CodeAccess = "",
                    CodeAccessExpiration = null,
                    ValidatedAccess = true,
                    Modules = modules,
                    Admin = request.Admin,
                    Blocked = request.Blocked
                };

                ResponseApi<User?> response = await userRepository.CreateAsync(user);
                if(response.Data is null) return new(null, 400, "Falha ao criar conta.");
                
                string messageCode = $"Seu código de verificação é: {access.CodeAccess}";
                
                // await smsHandler.SendMessageAsync(request.Phone, messageCode);
                
                return new(null, 201, "Conta criada com sucesso, foi enviado o código de verificação para seu celular e e-mail.");
            }
            catch
            {                
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        public async Task<ResponseApi<User?>> CreateEmployeeAsync(CreateUserEmployeeDTO request)
        {
            try
            {
                ResponseApi<User?> isEmail = await userRepository.GetByEmailAsync(request.Email);
                if(isEmail.Data is not null || !Validator.IsEmail(request.Email)) return new(null, 400, "E-mail inválido.");

                dynamic access = Util.GenerateCodeAccess();

                ResponseApi<ProfilePermission?> profile = await profilePermissionRepository.GetByIdAsync(request.Type);

                User user = new()
                {
                    UserName = $"usuário{access.CodeAccess}",
                    Email = request.Email,
                    Phone = request.Phone,
                    Name = request.Name,
                    Password = BCrypt.Net.BCrypt.HashPassword(access.CodeAccess),
                    CodeAccess = "",
                    CodeAccessExpiration = null,
                    ValidatedAccess = true,
                    Modules = profile.Data is null ? new() : profile.Data.Modules,
                    Admin = request.Admin,
                    Blocked = false,
                    Companies = [request.Company],
                    Stores = request.Stores,
                    Role = Enums.User.RoleEnum.Employee,
                    Company = request.Company,
                    Store = request.Store,
                    Plan = request.Plan
                };

                ResponseApi<User?> response = await userRepository.CreateAsync(user);
                if(response.Data is null) return new(null, 400, "Falha ao cadastrar Profissional.");
                
                string messageCode = $"Seu código de verificação é: {access.CodeAccess}";

                ResponseApi<Company?> company = await companyRepository.GetByIdAsync(request.Company);
                if(company.Data is null) return new(null, 400, "Falha ao cadastrar Profissional.");
                
                await mailHandler.SendMailAsync(request.Email, "Bem-vindo à equipe", MailTemplate.NewEmployee(request.Name, company.Data.CorporateName, profile.Data!.Name, user.Email, access.CodeAccess));

                await employeeRepository.CreateAsync(new ()
                {
                    Active = true,
                    Calendar = new(),
                    Cpf = request.CPF,
                    CreatedBy = request.CreatedBy,
                    DateOfBirth = request.DateOfBirth,
                    Phone = request.Phone,
                    Rg = request.Rg,
                    Type = request.Type,
                    Whatsapp = request.Whatsapp,
                    UserId = user.Id
                });
                
                return new(user, 201, "Profissional criado com sucesso, primeiro acesso ao Telemovvi foi enviado ao e-mail do profissional.");
            }
            catch
            {                
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion
        
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request, string userId)
        {
            try
            {
                PaginationUtil<User> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> users = await userRepository.GetAllAsync(pagination);
                int count = await userRepository.GetCountDocumentsAsync(pagination);
                return new(users.Data, count, pagination.PageNumber, pagination.PageSize);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<PaginationApi<List<dynamic>>> GetEmployeeAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<User> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> users = await userRepository.GetEmployeeAllAsync(pagination);
                int count = await userRepository.GetCountDocumentsAsync(pagination);
                return new(users.Data, count, pagination.PageNumber, pagination.PageSize);
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
                ResponseApi<dynamic?> user = await userRepository.GetByIdAggregateAsync(id);
                if(user.Data is null) return new(null, 404, "Usuário não encontrado");
                return new(user.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<dynamic?>> GetEmployeeByIdAggregateAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> user = await userRepository.GetEmployeeByIdAggregateAsync(id);
                if(user.Data is null) return new(null, 404, "Usuário não encontrado");
                return new(user.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<dynamic?>> GetLoggedAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> user = await userRepository.GetLoggedAsync(id);
                if(user.Data is null) return new(null, 404, "Usuário não encontrado");
                return new(user.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetSelectBarberAsync(GetAllDTO request)
        {
            try
            {
                ResponseApi<List<dynamic>> users = await userRepository.GetSelectBarberAsync(new(request.QueryParams));
                return new(users.Data);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<User?>> ValidatedAccessAsync(string codeAccess)
        {
            try
            {
                ResponseApi<User?> user = await userRepository.ValidatedAccessAsync(codeAccess);
                if(!user.IsSuccess) return new(null, 400, "Código inválido");
                return new(user.Data, 201, "Código de acesso confirmado");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User?>> UpdateAsync(UpdateUserDTO request)
        {
            try
            {
                ResponseApi<User?> user = await userRepository.GetByIdAsync(request.Id);
                Util.ConsoleLog(Validator.IsEmail(request.Email));
                if(user.Data is null || !Validator.IsEmail(request.Email)) return new(null, 404, "Falha ao atualizar");
                
                user.Data.UpdatedAt = DateTime.UtcNow;
                user.Data.UserName = request.UserName;
                user.Data.Email = request.Email;
                user.Data.Phone = request.Phone;
                user.Data.Name = request.Name;
                user.Data.Whatsapp = request.Whatsapp;

                ResponseApi<User?> response = await userRepository.UpdateAsync(user.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
                return new(response.Data, 201, "Atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User?>> UpdateModuleAsync(UpdateUserModuleDTO request)
        {
            try
            {
                ResponseApi<User?> user = await userRepository.GetByIdAsync(request.Id);
                if(user.Data is null) return new(null, 404, "Falha ao atualizar");
                
                user.Data.UpdatedAt = DateTime.UtcNow;
                List<api_infor_cell.src.Models.Module> modules = [];
                foreach (var module in request.Modules)
                {
                    List<api_infor_cell.src.Models.Routine> routines = [];

                    foreach (var routine in module.Routines)
                    {
                        routines.Add(new () 
                        {
                            Code = routine.Code,
                            Description = routine.Description,
                            Permissions = new ()
                            {
                                Create = routine.Permissions.Create,
                                Read = routine.Permissions.Read,
                                Update = routine.Permissions.Update,
                                Delete = routine.Permissions.Delete
                            }
                        });
                    }
                    
                    modules.Add(new () 
                    {
                        Code = module.Code,
                        Description = module.Description,
                        Routines = routines
                    });
                };

                user.Data.Modules = modules;
                user.Data.Name = request.Name;
                user.Data.Email = request.Email;
                user.Data.Blocked = request.Blocked;
                user.Data.Admin = request.Admin;

                ResponseApi<User?> response = await userRepository.UpdateAsync(user.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
                return new(response.Data, 201, "Atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User?>> UpdateStoreAsync(UpdateUserDTO request)
        {
            try
            {
                ResponseApi<User?> user = await userRepository.GetByIdAsync(request.Id);
                if(user.Data is null) return new(null, 404, "Falha ao atualizar");
                
                user.Data.UpdatedAt = DateTime.UtcNow;
                user.Data.Store = request.Store;

                ResponseApi<User?> response = await userRepository.UpdateAsync(user.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
                return new(response.Data, 201, "Atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User?>> ResendCodeAccessAsync(UpdateUserDTO request)
        {
            try
            {
                if(string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Phone)) return new(null, 400, "E-mail ou celular é obrigatório.");                    

                User? user = new();

                dynamic access = Util.GenerateCodeAccess();
                string messageCode = $"Seu código de verificação é: {access.CodeAccess}";
                
                if (!string.IsNullOrEmpty(request.Email))
                {
                    ResponseApi<User?> isEmail = await userRepository.GetByEmailAsync(request.Email);
                    if(isEmail.Data is null && !Validator.IsEmail(request.Email)) return new(null, 400, "E-mail inválido.");                    
                    user = isEmail.Data;
                    await mailHandler.SendMailAsync(request.Email, "Código de verificação", messageCode);
                };

                if (!string.IsNullOrEmpty(request.Phone))
                {
                    ResponseApi<User?> isPhone = await userRepository.GetByPhoneAsync(request.Phone);
                    if(isPhone.Data is null) return new(null, 400, "Celular inválido.");
                    user = isPhone.Data;
                    await smsHandler.SendMessageAsync(request.Phone, messageCode);
                };
                                             
                if(user is null) return new(null, 400, "Falha ao reenviar código de acesso");

                user.UpdatedAt = DateTime.UtcNow;
                user.CodeAccess = access.CodeAccess;
                user.CodeAccessExpiration = access.CodeAccessExpiration;
                user.ValidatedAccess = false;

                ResponseApi<User?> response = await userRepository.UpdateAsync(user);
                if(!response.IsSuccess) return new(null, 400, "Falha ao reenviar código de acesso");
                return new(response.Data, 201, "Novo código de acesso enviado");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User?>> SavePhotoProfileAsync(SaveUserPhotoDTO request)
        {
            try
            {
                if (request.Photo == null || request.Photo.Length == 0) return new(null, 400, "Falha ao salvar foto de perfil");

                ResponseApi<User?> user = await userRepository.GetByIdAsync(request.Id);
                if(user.Data is null) return new(null, 404, "Falha ao salvar foto de perfil");

                var tempPath = Path.GetTempFileName();

                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    request.Photo.CopyTo(stream);
                }

                string uriPhoto = await cloudinaryHandler.UploadAttachment("user", request.Photo);
                user.Data.UpdatedAt = DateTime.UtcNow;
                user.Data.Photo = uriPhoto;

                ResponseApi<User?> response = await userRepository.UpdateAsync(user.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao salvar foto de perfil");
                return new(new () { Photo = response.Data!.Photo }, 201, "Foto de perfil salva com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User?>> RemovePhotoProfileAsync(string id)
        {
            try
            {
                ResponseApi<User?> user = await userRepository.GetByIdAsync(id);
                if(user.Data is null) return new(null, 404, "Falha ao remover foto de perfil");
                string photo = user.Data.Photo.Split("/").Last();
                string publicId = photo.Split(".")[0];

                bool isRemoved = await cloudinaryHandler.Delete(publicId, "api-barber", "users");
                if(!isRemoved) return new(null, 400, "Falha ao remover foto de perfil");
                user.Data.UpdatedAt = DateTime.UtcNow;
                user.Data.Photo = "";

                ResponseApi<User?> response = await userRepository.UpdateAsync(user.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao remover foto de perfil");
                return new(response.Data, 201, "Foto de perfil removida com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
        
        #region DELETE
        public async Task<ResponseApi<User>> DeleteAsync(string userId)
        {
            try
            {
                ResponseApi<User> user = await userRepository.DeleteAsync(userId);
                if(!user.IsSuccess) return new(null, 400, user.Message);
                return user;
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion        
    }
}