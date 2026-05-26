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
    public class StoreRepository(AppDbContext context) : IStoreRepository
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Store> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),
                    new("$addFields", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0}, 
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Stores.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Lojas");
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

                BsonDocument? response = await context.Stores.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Lojas não encontrado") : new(result);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Lojas");
            }
        }
        
        public async Task<ResponseApi<Store?>> GetByIdAsync(string id)
        {
            try
            {
                Store? store = await context.Stores.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return new(store);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Lojas");
            }
        }
        public async Task<ResponseApi<Store?>> GetByCompanyIdAsync(string companyId)
        {
            try
            {
                Store? store = await context.Stores.Find(x => x.Company == companyId && !x.Deleted).FirstOrDefaultAsync();
                return new(store);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Lojas");
            }
        }        
        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(PaginationUtil<Store> pagination)
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

                List<BsonDocument> results = await context.Stores.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Lojas");
            }
        }

        public async Task<ResponseApi<List<Store>>> GetTotalCompanies(string planId, string companyId)
        {
            try
            {
                List<Store> stores = await context.Stores.Find(x => x.Plan == planId && x.Company == companyId && !x.Deleted).ToListAsync();
                return new(stores);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Empresa");
            }
        }   

        public async Task<int> GetCountDocumentsAsync(PaginationUtil<Store> pagination)
        {
            List<BsonDocument> pipeline = new()
            {
                new("$match", pagination.PipelineFilter),
                new("$sort", pagination.PipelineSort),
                new("$addFields", new BsonDocument
                {
                    {"id", new BsonDocument("$toString", "$_id")},
                }),
                new("$project", new BsonDocument
                {
                    {"_id", 0},
                }),
                new("$sort", pagination.PipelineSort),
            };

            List<BsonDocument> results = await context.Stores.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<Store?>> CreateAsync(Store store)
        {
            try
            {
                await context.Stores.InsertOneAsync(store);

                return new(store, 201, "Lojas criada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar Lojas");  
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<Store?>> UpdateAsync(Store store)
        {
            try
            {
                await context.Stores.ReplaceOneAsync(x => x.Id == store.Id, store);

                return new(store, 201, "Lojas atualizada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao atualizar Lojas");
            }
        }
        #endregion
        
        #region DELETE
        public async Task<ResponseApi<Store>> DeleteAsync(string id)
        {
            try
            {
                Store? store = await context.Stores.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                if(store is null) return new(null, 404, "Lojas não encontrado");
                store.Deleted = true;
                store.DeletedAt = DateTime.UtcNow;

                await context.Stores.ReplaceOneAsync(x => x.Id == id, store);

                return new(store, 204, "Lojas excluída com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao excluír Lojas");
            }
        }
        #endregion
    }
}