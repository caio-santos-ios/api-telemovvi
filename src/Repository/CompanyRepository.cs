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
    public class CompanyRepository(AppDbContext context) : ICompanyRepository
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Company> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),
                    new("$addFields", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                    }),
                    new("$match", pagination.PipelineFilter),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0}, 
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Companies.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Empresas");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(PaginationUtil<Company> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$sort", pagination.PipelineSort),
                    new("$addFields", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                    }),
                    new("$match", pagination.PipelineFilter),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", 1}, 
                        {"tradeName", 1}, 
                        {"photo", 1} 
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Companies.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Empresas");
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
                   
                    new("$addFields", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                    }),

                    MongoUtil.Lookup("addresses", ["$id"], ["$parentId"], "_address", [["deleted", false]], 1),

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
                        {"_address", 0},
                    }),
                ];

                BsonDocument? response = await context.Companies.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Empresa não encontrado") : new(result);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Empresa");
            }
        }       
        public async Task<ResponseApi<Company?>> GetByIdAsync(string id)
        {
            try
            {
                Company? company = await context.Companies.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return new(company);
            }
            catch(Exception ex)
            {
                return new(null, 500, $"Falha ao buscar Empresa - ERRO: {ex.Message}");
            }
        }         
        public async Task<ResponseApi<List<Company>>> GetTotalCompanies(string planId)
        {
            try
            {
                List<Company> companies = await context.Companies.Find(x => x.Plan == planId && !x.Deleted).ToListAsync();
                return new(companies);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Empresa");
            }
        }       
        public async Task<ResponseApi<Company?>> GetByPlanDocumentAsync(string plan, string document, string id)
        {
            try
            {
                Company? company = new();
                if(string.IsNullOrEmpty(id))
                {
                    company = await context.Companies.Find(x => x.Plan == plan && x.Document == document && !x.Deleted).FirstOrDefaultAsync();
                }
                else 
                {
                    company = await context.Companies.Find(x => x.Plan == plan && x.Document == document && x.Id != id && !x.Deleted).FirstOrDefaultAsync();
                };

                return new(company);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Empresa");
            }
        }     
        public async Task<int> GetCountDocumentsAsync(PaginationUtil<Company> pagination)
        {
            List<BsonDocument> pipeline = new()
            {
                new("$sort", pagination.PipelineSort),
                new("$addFields", new BsonDocument
                {
                    {"id", new BsonDocument("$toString", "$_id")},
                }),
                new("$match", pagination.PipelineFilter),
                new("$project", new BsonDocument
                {
                    {"_id", 0},
                }),
                new("$sort", pagination.PipelineSort),
            };

            List<BsonDocument> results = await context.Companies.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<Company?>> CreateAsync(Company address)
        {
            try
            {
                await context.Companies.InsertOneAsync(address);

                return new(address, 201, "Empresa criada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar Empresa");  
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<Company?>> UpdateAsync(Company address)
        {
            try
            {
                await context.Companies.ReplaceOneAsync(x => x.Id == address.Id, address);

                return new(address, 201, "Empresa atualizada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao atualizar Empresa");
            }
        }
        #endregion
        
        #region DELETE
        public async Task<ResponseApi<Company>> DeleteAsync(string id)
        {
            try
            {
                Company? address = await context.Companies.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                if(address is null) return new(null, 404, "Empresa não encontrado");
                address.Deleted = true;
                address.DeletedAt = DateTime.UtcNow;

                await context.Companies.ReplaceOneAsync(x => x.Id == id, address);

                return new(address, 204, "Empresa excluída com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao excluír Empresa");
            }
        }
        #endregion
    }
}