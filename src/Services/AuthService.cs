using api_infor_cell.src.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Responses;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Handlers;
using api_infor_cell.src.Shared.Templates;
using api_infor_cell.src.Shared.Validators;
using api_infor_cell.src.Shared.Utils;
using System.Text.Json;
using MongoDB.Driver.Linq;

namespace api_infor_cell.src.Services
{
    public class AuthService(IUserRepository repository, IEmployeeRepository employeeRepository, IPlanRepository planRepository, ICompanyRepository companyRepository, IStoreRepository storeRepository, MailHandler mailHandler) : IAuthService
    {
        public async Task<ResponseApi<AuthResponse>> LoginAsync(LoginDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email)) return new(null, 400, "E-mail é obrigatório");
                if (string.IsNullOrEmpty(request.Password)) return new(null, 400, "Senha é obrigatória");
                
                ResponseApi<User?> res = await GetUserToken(request.Email);

                if(res.Data is null) return new(null, 400, res.Message);
                
                User user = res.Data;

                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                if(!isValid) return new(null, 400, "Dados incorretos");

                ResponseApi<Company?> company = await companyRepository.GetByIdAsync(user.Company);

                ResponseApi<Store?> store = await storeRepository.GetByIdAsync(user.Store);

                ResponseApi<Plan?> plan = await planRepository.GetByIdAsync(user.Plan);

                AuthResponse response = new ()
                {
                    Token = GenerateJwtToken(user, plan.Data!.ExpirationDate, plan.Data.Type), 
                    RefreshToken = GenerateJwtToken(user, plan.Data.ExpirationDate, plan.Data.Type, true), 
                    Name = user.Name, 
                    Id = user.Id, 
                    Admin = user.Admin, 
                    Modules = user.Modules, 
                    Photo = user.Photo, 
                    Email = user.Email,
                    Plan = user.Plan,
                    LogoCompany = company.Data is not null ? company.Data.Photo : "",
                    NameCompany = company.Data is not null ? company.Data.TradeName : "",
                    NameStore = store.Data is not null ? store.Data.TradeName : "",
                    TypePlan = plan.Data is not null ? plan.Data.Type : "",
                    SubscriberPlan =  user!.SubscriberPlan,
                    ExpirationDate = plan.Data!.ExpirationDate,
                    Master = user.Master
                };

                return new(response);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");            
            }
        }
        public async Task<ResponseApi<dynamic>> RegisterAsync(RegisterDTO request)
        {
            try
            {
                if (!request.PrivacyPolicy) return new(null, 400, "Aceitar os Termos e Condições e nossa Política de Privacidade é obrigatório");
                if (string.IsNullOrEmpty(request.CompanyName)) return new(null, 400, "Nome da empresa é obrigatório");
                if (string.IsNullOrEmpty(request.Name)) return new(null, 400, "Nome completo é obrigatório");
                if (string.IsNullOrEmpty(request.Document)) return new(null, 400, "CPF/CNPJ é obrigatório");
                if (string.IsNullOrEmpty(request.Email)) return new(null, 400, "E-mail é obrigatório");
                if (string.IsNullOrEmpty(request.Password)) return new(null, 400, "Senha é obrigatória");
                
                ResponseApi<User?> isEmail = await repository.GetByEmailAsync(request.Email);
                if(isEmail.Data is not null || !Validator.IsEmail(request.Email)) return new(null, 400, "E-mail inválido.");

                if(Validator.IsReliable(request.Password).Equals("Ruim")) return new(null, 400, $"Senha é muito fraca");

                dynamic access = Util.GenerateCodeAccess(5);

                User user = new()
                {
                    UserName = $"usuário{access.CodeAccess}",
                    Email = request.Email,
                    Phone = request.Phone,
                    Name = request.Name,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CodeAccess = access.CodeAccess,
                    CodeAccessExpiration = access.CodeAccessExpiration,
                    ValidatedAccess = false,
                    Modules = [],
                    Admin = true,
                    Master = false,
                    Blocked = false,
                    Whatsapp = request.Whatsapp,
                    Role = Enums.User.RoleEnum.Admin,
                    SubscriberPlan = true
                };

                ResponseApi<User?> response = await repository.CreateAsync(user);
                
                if(response.Data is null) return new(null, 400, "Falha ao criar conta.");

                DateTime date = DateTime.UtcNow;

                ResponseApi<Plan?> responsePlan = await planRepository.CreateAsync(new ()
                {
                    StartDate = date,
                    ExpirationDate = date.AddDays(10),
                    Type = "free",
                    CreatedBy = user.Id
                });

                if(responsePlan.Data is null) return new(null, 400, "Falha ao criar conta.");

                ResponseApi<Company?> responseCompany = await companyRepository.CreateAsync(new ()
                {
                    CorporateName = request.CompanyName,
                    TradeName = request.CompanyName,
                    Phone = request.Phone,
                    Plan = responsePlan.Data.Id,
                    Document = request.Document,
                    Email = request.Email,
                });

                if(responseCompany.Data is null) return new(null, 400, "Falha ao criar conta.");
                
                ResponseApi<Store?> responseStore = await storeRepository.CreateAsync(new ()
                {
                    CorporateName = request.CompanyName,
                    TradeName = "Matriz",
                    Phone = request.Phone,
                    Plan = responsePlan.Data.Id,
                    Company = responseCompany.Data.Id
                });

                if(responseStore.Data is null) return new(null, 400, "Falha ao criar conta.");

                response.Data.Companies.Add(responseCompany.Data.Id);
                response.Data.Company = responseCompany.Data.Id;
                response.Data.Plan = responsePlan.Data.Id;
                response.Data.Stores.Add(responseStore.Data.Id);
                response.Data.Store = responseStore.Data.Id;
                
                await repository.UpdateAsync(response.Data);

                                
                await mailHandler.SendMailAsync(request.Email, "Código de Confirmação", MailTemplate.ConfirmAccount(request.Name, access.CodeAccess));

                return new(null, 201, "Conta criada com sucesso, foi enviado o e-mail de confirmação.");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");            
            }
        }
        public async Task<ResponseApi<dynamic>> ConfirmAccountAsync(ConfirmAccountDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Code)) return new(null, 400, "Código de confirmação é obrigatório");
                
                ResponseApi<User?> user = await repository.GetByCodeAccessAsync(request.Code);
                if(user.Data is null) return new(null, 400, "Código inválido.");

                if(user.Data.CodeAccessExpiration < DateTime.UtcNow) return new(null, 400, "Código expirou, solicite um novo código.");

                user.Data.CodeAccess = "";
                user.Data.CodeAccessExpiration = null;
                user.Data.ValidatedAccess = true;

                ResponseApi<User?> response = await repository.UpdateAsync(user.Data);
                if(response.Data is null) return new(null, 400, "Falha ao solicitar novo código.");
                                
                return new(null, 200, "Conta verificada com sucesso.");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");            
            }
        }
        public async Task<ResponseApi<dynamic>> NewCodeConfirmAsync(RegisterDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email)) return new(null, 400, "E-mail é obrigatório");
                
                ResponseApi<User?> user = await repository.GetByEmailAsync(request.Email);
                if(user.Data is null || !Validator.IsEmail(request.Email)) return new(null, 400, "E-mail inválido.");

                dynamic access = Util.GenerateCodeAccess(5);

                user.Data.CodeAccess = access.CodeAccess;
                user.Data.CodeAccessExpiration = access.CodeAccessExpiration;

                ResponseApi<User?> response = await repository.UpdateAsync(user.Data);
                if(response.Data is null) return new(null, 400, "Falha ao solicitar novo código.");
                                
                await mailHandler.SendMailAsync(request.Email, "Novo Código de Verificação", MailTemplate.NewCodeConfirmAccount(request.Name, access.CodeAccess));

                return new(null, 200, "Novo código foi enviado.");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");            
            }
        }
        public async Task<ResponseApi<AuthResponse>> RefreshTokenAsync(string token, string planId)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                SecurityToken? validatedToken;

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("ISSUER"),
                    ValidAudience = Environment.GetEnvironmentVariable("AUDIENCE"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET_KEY") ?? "")),
                    ValidateLifetime = false 
                };

                var principal = handler.ValidateToken(token, validationParameters, out validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken == null) return new(null, 401, "Token inválido.");

                string? tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
                if (tokenType != "refresh") return new(null, 401, "O token fornecido não é um refresh token.");

                var userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return new(null, 401, "Usuário não encontrado no token.");
                
                var email = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email)) return new(null, 401, "Usuário não encontrado no token.");
                ResponseApi<User?> user = await GetUserToken(email);

                if (user.Data is null) return new(null, 401, "Usuário não encontrado.");

                ResponseApi<Plan?> plan = await planRepository.GetByIdAsync(planId);
                if (plan.Data is null) return new(null, 401, "Usuário não encontrado.");

                string accessToken = GenerateJwtToken(user.Data, plan.Data.ExpirationDate, plan.Data.Type);
                string refreshToken = GenerateJwtToken(user.Data, plan.Data.ExpirationDate, plan.Data.Type, true);

                return new(new AuthResponse
                {
                    Token = accessToken,
                    RefreshToken = refreshToken
                });
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");            
            }
        }
        public async Task<ResponseApi<User>> ResetPasswordAsync(ResetPasswordDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password)) return new(null, 400, "Senha é obrigatória");
                if (string.IsNullOrEmpty(request.Id)) return new(null, 400, "Falha ao alterar senha");
                
                if(Validator.IsReliable(request.Password).Equals("Ruim")) return new(null, 400, $"Senha é muito fraca");

                ResponseApi<User?> user = await repository.GetByIdAsync(request.Id);
                if(!user.IsSuccess || user.Data is null) return new(null, 400, "Falha ao alterar senha");
                
                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Data.Password);
                if(!isValid) return new(null, 400, "Senha antiga incorreta");

                user.Data.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                ResponseApi<User?> response = await repository.UpdateAsync(user.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao alterar senha");

                return new(null, 200, "Senha alterada com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");            
            }
        }
        public async Task<ResponseApi<User>> RequestForgotPasswordAsync(ForgotPasswordDTO request)
        {
            try
            {
                ResponseApi<User?> responseUser = await repository.GetByEmailAsync(request.Email);

                if(responseUser.Data is null) return new(null, 400, "Dados incorretos");

                dynamic access = Util.GenerateCodeAccess();

                responseUser.Data.CodeAccess = access.CodeAccess;
                responseUser.Data.CodeAccessExpiration = access.CodeAccessExpiration;
                responseUser.Data.ValidatedAccess = false;

                string template = MailTemplate.ForgotPasswordWeb(responseUser.Data.Name, responseUser.Data.CodeAccess);

                await mailHandler.SendMailAsync(request.Email, "Redefinição de Senha", template);

                if(responseUser.Data is not null) 
                {
                    ResponseApi<User?> response = await repository.UpdateAsync(responseUser.Data);
                    if(!response.IsSuccess) return new(null, 400, "Falha ao redefinir senha");
                };

                return new(null, 200, "Foi enviado um e-mail para redefinir sua senha");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");            
            }
        }
        public async Task<ResponseApi<User>> ResetPassordForgotAsync(ResetPasswordDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password)) return new(null, 400, "Senha é obrigatória");
                if (string.IsNullOrEmpty(request.NewPassword)) return new(null, 400, "Confirmação da senha é obrigatória");
                if (request.Password != request.NewPassword) return new(null, 400, "As senhas não podem ser diferentes");

                ResponseApi<User?> responseUser = await repository.GetByCodeAccessAsync(request.CodeAccess);

                if(responseUser.Data is null) return new(null, 400, "Falha ao alterar senha");

                if(responseUser.Data.CodeAccessExpiration < DateTime.UtcNow) return new(null, 400, "Código expirou, solicite um novo e-mail.");
                
                if(Validator.IsReliable(request.Password).Equals("Ruim")) return new(null, 400, $"Senha é muito fraca");

                responseUser.Data.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                responseUser.Data.ValidatedAccess = true;
                responseUser.Data.CodeAccess = "";
                responseUser.Data.CodeAccessExpiration = null;

                ResponseApi<User?> response = await repository.UpdateAsync(responseUser.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao redefinir senha");
                
                return new(null, 200, "Senha alterada com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");            
            }
        }
        private static string GenerateJwtToken(User user, DateTime expirationDate, string typePlan, bool refresh = false)
        {
            string? SecretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ?? "";
            string? Issuer = Environment.GetEnvironmentVariable("ISSUER") ?? "";
            string? Audience = Environment.GetEnvironmentVariable("AUDIENCE") ?? "";

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(SecretKey));

            var companiesJson = JsonSerializer.Serialize(user.Companies);
            var storesJson = JsonSerializer.Serialize(user.Stores);

            Claim[] claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("companies", companiesJson),
                new Claim("stores", storesJson),
                new Claim("plan", user.Plan),
                new Claim("store", user.Store),
                new Claim("company", user.Company),
                new Claim("typePlan", typePlan),
                new Claim("planExpirationDate", expirationDate.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                new Claim(JwtRegisteredClaimNames.Nickname, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("type", refresh ? "refresh" : "access")
            ];

            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: refresh ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<ResponseApi<User?>> GetUserToken (string email)
        {
            ResponseApi<User?> responseUser = await repository.GetByEmailAsync(email);
            if(responseUser.Data is null) 
            {
                return new(null, 400, "Dados incorretos");
            };

            if(responseUser.Data.Role == Enums.User.RoleEnum.Employee) {

                ResponseApi<Employee?> responseEmployee = await employeeRepository.GetByUserIdAsync(responseUser.Data.Id);
                if(responseEmployee.Data is null) return new(null, 400, "Dados incorretos");
                
                DayOfWeek today = DateTime.Now.DayOfWeek;
                TimeSpan now = DateTime.Now.TimeOfDay;
                Calendar calendar = responseEmployee.Data.Calendar;
                List<string> hoursString = new();
                switch (today)
                {
                    case DayOfWeek.Monday:    hoursString = calendar.Monday; break;
                    case DayOfWeek.Tuesday:   hoursString = calendar.Tuesday; break;
                    case DayOfWeek.Wednesday: hoursString = calendar.Wednesday; break;
                    case DayOfWeek.Thursday:  hoursString = calendar.Thursday; break;
                    case DayOfWeek.Friday:    hoursString = calendar.Friday; break;
                    case DayOfWeek.Saturday:  hoursString = calendar.Saturday; break;
                    case DayOfWeek.Sunday:    hoursString = calendar.Sunday; break;
                }

                var times = hoursString?.Select(h => TimeSpan.Parse(h)).ToList() ?? new List<TimeSpan>();

                if(times.Count == 0) return new(null, 400, "Fora do horário permitido");
                bool isBetween = now >= times.Min() && now <= times.Max();
                if(!isBetween) return new(null, 400, "Fora do horário permitido");
                if(responseUser.Data.Stores.Count == 0) return new(null, 400, "O colaborador não possui nenhuma loja vinculada ao seu perfil.");
            };

            return new(responseUser.Data);
        }
    }
}