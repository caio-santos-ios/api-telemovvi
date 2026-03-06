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
    public class BudgetRepository(AppDbContext context) : IBudgetRepository
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Budget> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),

                    MongoUtil.Lookup("customers", ["$customerId"], ["$_id"], "_customer", [["deleted", false]], 1),
                    MongoUtil.Lookup("users", ["$sellerId"], ["$_id"], "_user", [["deleted", false]], 1),
                    MongoUtil.Lookup("employees", ["$sellerId"], ["$_id"], "_seller", [["deleted", false]], 1),

                    new("$addFields", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"customerName", MongoUtil.First("_customer.tradeName")},
                        {"userName", MongoUtil.First("_user.name")},
                        {"employeeName", MongoUtil.First("_seller.name")},
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"_customer", 0},
                        {"_user", 0},
                        {"_seller", 0},
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Budgets.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Orçamentos");
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

                    MongoUtil.Lookup("customers", ["$customerId"], ["$_id"], "_customer", [["deleted", false]], 1),
                    MongoUtil.Lookup("employees", ["$sellerId"], ["$_id"], "_seller", [["deleted", false]], 1),
                    MongoUtil.Lookup("users", ["$sellerId"], ["$_id"], "_user", [["deleted", false]], 1),

                    new("$addFields", new BsonDocument {
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"customerName", MongoUtil.First("_customer.tradeName")},
                        {"userName", MongoUtil.First("_user.name")},
                        {"employeeName", MongoUtil.First("_seller.name")},
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"_customer", 0},
                        {"_user", 0},
                        {"_seller", 0},
                    }),
                ];

                BsonDocument? response = await context.Budgets.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Orçamento não encontrado") : new(result);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Orçamento");
            }
        }

        public async Task<ResponseApi<Budget?>> GetByIdAsync(string id)
        {
            try
            {
                Budget? budget = await context.Budgets.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return new(budget);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Orçamento");
            }
        }

        public async Task<ResponseApi<long>> GetNextCodeAsync(string planId, string companyId, string storeId)
        {
            try
            {
                long code = await context.Budgets.Find(x => x.Plan == planId && x.Company == companyId && x.Store == storeId).CountDocumentsAsync() + 1;
                return new(code);
            }
            catch
            {
                return new(0, 500, "Falha ao buscar Próximo Código");
            }
        }

        public async Task<int> GetCountDocumentsAsync(PaginationUtil<Budget> pagination)
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

            List<BsonDocument> results = await context.Budgets.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Budget?>> CreateAsync(Budget budget)
        {
            try
            {
                await context.Budgets.InsertOneAsync(budget);
                return new(budget, 201, "Orçamento criado com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar Orçamento");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<Budget?>> UpdateAsync(Budget budget)
        {
            try
            {
                await context.Budgets.ReplaceOneAsync(x => x.Id == budget.Id, budget);
                return new(budget, 201, "Orçamento atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao atualizar Orçamento");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<Budget>> DeleteAsync(string id)
        {
            try
            {
                Budget? budget = await context.Budgets.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                if (budget is null) return new(null, 404, "Orçamento não encontrado");
                budget.Deleted = true;
                budget.DeletedAt = DateTime.UtcNow;

                await context.Budgets.ReplaceOneAsync(x => x.Id == id, budget);
                return new(budget, 204, "Orçamento excluído com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao excluir Orçamento");
            }
        }
        #endregion
    }
}
