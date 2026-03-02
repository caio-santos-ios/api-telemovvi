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
    public class AccountPayableRepository(AppDbContext context) : IAccountPayableRepository
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Models.AccountPayable> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),

                    MongoUtil.Lookup("payment_methods", ["$paymentMethodId"], ["$_id"], "_paymentMethod", [["deleted", false]], 1),
                    MongoUtil.Lookup("suppliers", ["$supplierId"], ["$_id"], "_supplier", [["deleted", false]], 1),

                    new("$addFields", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"paymentMethodName", MongoUtil.First("_paymentMethod.name")},
                        {"supplierName", MongoUtil.First("_supplier.corporateName")},
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"_paymentMethod", 0},
                        {"_supplier", 0},
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.AccountsPayable.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
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

                BsonDocument? response = await context.AccountsPayable.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Conta a pagar não encontrada") : new(result);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<Models.AccountPayable?>> GetByIdAsync(string id)
        {
            try
            {
                Models.AccountPayable? accountPayable = await context.AccountsPayable
                    .Find(x => x.Id == id && !x.Deleted)
                    .FirstOrDefaultAsync();
                return new(accountPayable);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<int> GetCountDocumentsAsync(PaginationUtil<Models.AccountPayable> pagination)
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

            List<BsonDocument> results = await context.AccountsPayable.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }

        public async Task<ResponseApi<long>> GetNextCodeAsync(string planId, string companyId, string storeId)
        {
            try
            {
                long code = await context.AccountsPayable
                    .Find(x => x.Plan == planId && x.Company == companyId && x.Store == storeId)
                    .CountDocumentsAsync() + 1;
                return new(code);
            }
            catch
            {
                return new(0, 500, "Falha ao buscar próximo código");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Models.AccountPayable?>> CreateAsync(Models.AccountPayable accountPayable)
        {
            try
            {
                await context.AccountsPayable.InsertOneAsync(accountPayable);
                return new(accountPayable, 201, "Conta a pagar criada com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<Models.AccountPayable?>> UpdateAsync(Models.AccountPayable accountPayable)
        {
            try
            {
                await context.AccountsPayable.ReplaceOneAsync(x => x.Id == accountPayable.Id, accountPayable);
                return new(accountPayable, 200, "Conta a pagar atualizada com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<Models.AccountPayable?>> PayAsync(Models.AccountPayable accountPayable)
        {
            try
            {
                await context.AccountsPayable.ReplaceOneAsync(x => x.Id == accountPayable.Id, accountPayable);
                return new(accountPayable, 200, "Título baixado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<Models.AccountPayable>> DeleteAsync(string id)
        {
            try
            {
                Models.AccountPayable? accountPayable = await context.AccountsPayable
                    .Find(x => x.Id == id && !x.Deleted)
                    .FirstOrDefaultAsync();

                if (accountPayable is null) return new(null, 404, "Conta a pagar não encontrada");

                accountPayable.Deleted = true;
                accountPayable.DeletedAt = DateTime.UtcNow;

                await context.AccountsPayable.ReplaceOneAsync(x => x.Id == id, accountPayable);
                return new(accountPayable, 204, "Conta a pagar excluída com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
    }
}
