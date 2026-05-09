using api_infor_cell.src.Configuration;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace api_infor_cell.src.Repository
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        #region CREATE
        public async Task<ResponseApi<User?>> CreateAsync(User user)
        {
            try
            {
                await context.Users.InsertOneAsync(user);
                return new(user, 201, "Usuário criado com sucesso");
            }
            catch(Exception e)
            {
                return new(null, 500, e.Message);   
            }
        }
        #endregion
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<User> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),
                    
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"email", 1},
                        {"admin", 1},
                        {"blocked", 1},
                        {"photo", 1}
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Users.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch(Exception e)
            {
                return new(null, 500, e.Message); ;
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetEmployeeAllAsync(PaginationUtil<User> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),
                    
                    MongoUtil.Lookup("employees", ["$_id"], ["$userId"], "_employee", [["deleted", false]], 1),
                    new("$addFields", new BsonDocument {
                        {"type", MongoUtil.First("_employee.type")}
                    }),
                    MongoUtil.Lookup("profile_permissions", ["$type"], ["$_id"], "_permissions", [["deleted", false]], 1),

                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"email", 1},
                        {"admin", 1},
                        {"blocked", 1},
                        {"photo", 1},
                        {"createdAt", 1},
                        {"dateOfBirth", MongoUtil.First("_employee.dateOfBirth")},
                        {"cpf", MongoUtil.First("_employee.cpf")},
                        {"rg", MongoUtil.First("_employee.rg")},
                        {"typeName", MongoUtil.First("_permissions.name")},
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Users.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch(Exception e)
            {
                return new(null, 500, e.Message); ;
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetSelectBarberAsync(PaginationUtil<User> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),
                    new("$project", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"email", 1},
                        {"modules", 1},
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Users.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch(Exception e)
            {
                return new(null, 500, e.Message); ;
            }
        }
        public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
        {
            try
            {
                BsonDocument[] pipeline = [
                    new("$match", new BsonDocument{
                        {"_id", new ObjectId(id)},
                        {"deleted", false}
                    }),
                    new("$addFields", new BsonDocument {
                        {"id", new BsonDocument("$toString", "$_id")},
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                    }),
                ];

                BsonDocument? response = await context.Users.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Usuário não encontrado") : new(result);
            }
            catch(Exception e)
            {
                return new(null, 500, e.Message); ;
            }
        }
        public async Task<ResponseApi<dynamic?>> GetEmployeeByIdAggregateAsync(string id)
        {
            try
            {
                BsonDocument[] pipeline = [
                    new("$match", new BsonDocument{
                        {"_id", new ObjectId(id)},
                        {"deleted", false}
                    }),
                    
                    MongoUtil.Lookup("employees", ["$_id"], ["$userId"], "_employee", [["deleted", false]], 1),

                    new("$addFields", new BsonDocument {
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"dateOfBirth", MongoUtil.First("_employee.dateOfBirth")},
                        {"cpf", MongoUtil.First("_employee.cpf")},
                        {"rg", MongoUtil.First("_employee.rg")},
                        {"type", MongoUtil.First("_employee.type")},
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                    }),
                ];

                BsonDocument? response = await context.Users.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Usuário não encontrado") : new(result);
            }
            catch(Exception e)
            {
                return new(null, 500, e.Message); ;
            }
        }
        public async Task<ResponseApi<dynamic?>> GetLoggedAsync(string id)
        {
            try
            {
                BsonDocument[] pipeline = [
                    new("$match", new BsonDocument{
                        {"_id", new ObjectId(id)},
                        {"deleted", false}
                    }),

                    new("$addFields", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                    }),

                    MongoUtil.Lookup("companies", ["$company"], ["$_id"], "_company", [["deleted", false]], 1),

                    MongoUtil.Lookup("companies", ["$plan"], ["$plan"], "_companyAll", [["deleted", false]]),
                    
                    MongoUtil.Lookup("stores", ["$store"], ["$_id"], "_store", [["deleted", false]], 1),
                    
                    MongoUtil.Lookup("stores", ["$plan"], ["$plan"], "_storeAll", [["deleted", false]]),

                    MongoUtil.Lookup("addresses", ["$id"], ["$parentId"], "_address", [["deleted", false]], 1),

                    MongoUtil.Lookup("plans", ["$plan"], ["$_id"], "_plan", [["deleted", false]], 1),
                    
                    MongoUtil.Lookup("subscriptions", ["$plan"], ["$planId"], "_subscriptions", [["deleted", false]], 1),

                    new("$addFields", new BsonDocument
                    {
                        {"addressId", MongoUtil.First("_address._id")},
                        {"street",  MongoUtil.First("_address.street")},
                        {"number", MongoUtil.First("_address.number") },
                        {"complement", MongoUtil.First("_address.complement") },
                        {"neighborhood", MongoUtil.First("_address.neighborhood") },
                        {"city", MongoUtil.First("_address.city") },
                        {"state", MongoUtil.First("_address.state") },
                        {"zipCode", MongoUtil.First("_address.zipCode") },
                        {"parent", MongoUtil.First("_address.parent") },
                        {"parentId", MongoUtil.First("_address.parentId") },
                        {"logoCompany", MongoUtil.First("_company.photo") },
                        {"nameCompany", MongoUtil.First("_company.tradeName") },
                        {"nameStore", MongoUtil.First("_store.tradeName") },
                        {"namePlan", MongoUtil.First("_plan.type") },
                        {"subscriberPlan", MongoUtil.First("_subscriptions.active") },
                    }),

                    new("$addFields", new BsonDocument
                    {
                        {"address", new BsonDocument
                            {
                                {"id", MongoUtil.ToString("$addressId")},
                                {"street",  MongoUtil.ValidateNull("street", "")},
                                {"number", MongoUtil.ValidateNull("number", "") },
                                {"complement", MongoUtil.ValidateNull("complement", "") },
                                {"neighborhood", MongoUtil.ValidateNull("neighborhood", "") },
                                {"city", MongoUtil.ValidateNull("city", "") },
                                {"state", MongoUtil.ValidateNull("state", "") },
                                {"zipCode", MongoUtil.ValidateNull("zipCode", "") },
                                {"parent", MongoUtil.ValidateNull("parent", "") },
                                {"parentId", MongoUtil.ValidateNull("parentId", "") },
                            }
                        }
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"email", 1},
                        {"modules", 1},
                        {"admin", 1},
                        {"blocked", 1},
                        {"photo", 1},
                        {"phone", 1},
                        {"whatsapp", 1},
                        {"stores", 1},
                        {"companies", 1},
                        {"createdAt", 1},
                        {"logoCompany", MongoUtil.ValidateNull("logoCompany", "")},
                        {"nameCompany", MongoUtil.ValidateNull("nameCompany", "")},
                        {"namePlan", MongoUtil.ValidateNull("namePlan", "")},
                        {"subscriberPlan", MongoUtil.ValidateNull("subscriberPlan", "")},
                        {"nameStore", MongoUtil.ValidateNull("nameStore", "")},
                        {"address", 1},
                        {"storesAll", "$_storeAll"},
                        {"companiesAll", "$_companyAll"},
                    }),
                ];

                BsonDocument? response = await context.Users.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Usuário não encontrado") : new(result);
            }
            catch(Exception e)
            {
                return new(null, 500, e.Message); ;
            }
        }
        public async Task<ResponseApi<User?>> GetByIdAsync(string id)
        {
            try
            {
                User? user = await context.Users.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar usuário");
            }
        }
        public async Task<ResponseApi<User?>> GetBySubscribedAsync(string plan)
        {
            try
            {
                User? user = await context.Users.Find(x => x.SubscriberPlan && x.Plan == plan && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar usuário");
            }
        }
        public async Task<ResponseApi<User?>> GetByUserNameAsync(string userName)
        {
            try
            {
                User? user = await context.Users.Find(x => x.UserName == userName && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar usuário");
            }
        }
        public async Task<ResponseApi<User?>> GetByEmailAsync(string email)
        {
            try
            {
                User? user = await context.Users.Find(x => x.Email == email && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar usuário");
            }
        }
        public async Task<ResponseApi<User?>> GetByPhoneAsync(string phone)
        {
            try
            {
                User? user = await context.Users.Find(x => x.Phone == phone && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar usuário");
            }
        }
        public async Task<ResponseApi<User?>> GetByCodeAccessAsync(string codeAccess)
        {
            try
            {
                User? user = await context.Users.Find(x => x.CodeAccess == codeAccess && !x.ValidatedAccess && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar usuário");
            }
        }
        public async Task<ResponseApi<User?>> GetByCompanyIdAsync(string companyId)
        {
            try
            {
                User? user = await context.Users.Find(x => x.Companies.Contains(companyId) && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar usuário");
            }
        }
        public async Task<bool> GetAccessValitedAsync(string id)
        {
            try
            {
                User? user = await context.Users.Find(x => x.Id == id && !x.Deleted && !x.Blocked && x.ValidatedAccess).FirstOrDefaultAsync();
                if(user is null) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<int> GetCountDocumentsAsync(PaginationUtil<User> pagination)
        {
            List<BsonDocument> pipeline = new()
            {
                new("$match", pagination.PipelineFilter),
                new("$sort", pagination.PipelineSort),
                // new("$skip", pagination.Skip),
                // new("$limit", pagination.Limit),
                new("$addFields", new BsonDocument
                {
                    {"id", new BsonDocument("$toString", "$_id")},
                }),
                new("$project", new BsonDocument
                {
                    {"_id", 0},
                    {"password", 0},
                    {"role", 0},
                    {"blocked", 0},
                    {"codeAccess", 0},
                    {"validatedAccess", 0}
                }),
                new("$sort", pagination.PipelineSort),
            };

            List<BsonDocument> results = await context.Users.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }
        #endregion
        #region UPDATE
        public async Task<ResponseApi<User?>> UpdateCodeAccessAsync(string userId, string codeAccess)
        {
            try
            {
                User? user = await context.Users.Find(x => x.Id == userId && !x.Deleted && !x.Blocked && !x.ValidatedAccess).FirstOrDefaultAsync();
                if(user is null) return new(null, 404, "Usuário não encontrado");
                user.UpdatedAt = DateTime.UtcNow;
                user.CodeAccess = codeAccess;
                user.ValidatedAccess = false;

                await context.Users.ReplaceOneAsync(x => x.Id == userId, user);

                return new(user, 204, "Código de acesso atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao atualizar código de acesso");
            }
        }
        public async Task<ResponseApi<User?>> UpdateAsync(User user)
        {
            try
            {
                await context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
                user.Password = "";
                return new(user, 201, "Usuário atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao atualizar usuário");
            }
        }
        public async Task<ResponseApi<User?>> ValidatedAccessAsync(string codeAccess)
        {
            try
            {
                User? user = await context.Users.Find(x => x.CodeAccess == codeAccess && !x.Deleted && !x.Blocked && !x.ValidatedAccess).FirstOrDefaultAsync();
                if(user is null) return new(null, 404, "Usuário não encontrado");
                user.UpdatedAt = DateTime.UtcNow;
                user.CodeAccess = "";
                user.ValidatedAccess = true;
                user.CodeAccessExpiration = null;
                
                await context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);

                return new(user, 204, "Código de acesso confirmado com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao confirmar código de acesso");
            }
        }
        #endregion
        #region DELETE
        public async Task<ResponseApi<User>> DeleteAsync(User user)
        {
            try
            {
                await context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);

                return new(user, 204, "Usuário excluído com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao excluír usuário");
            }
        }
        #endregion
    }
}