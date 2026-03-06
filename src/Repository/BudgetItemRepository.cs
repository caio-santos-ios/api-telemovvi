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
    public class BudgetItemRepository(AppDbContext context) : IBudgetItemRepository
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<BudgetItem> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),

                    MongoUtil.Lookup("products", ["$productId"], ["$_id"], "_product", [["deleted", false]], 1),
                    MongoUtil.Lookup("attachments", ["$productId"], ["$parentId"], "_images", [["deleted", false]], 1),
                    MongoUtil.Lookup("stock", ["$productId"], ["$productId"], "_stock", [["deleted", false]], 1),

                    new("$addFields", new BsonDocument
                    {
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"productName", MongoUtil.First("_product.name")},
                        {"productHasSerial", MongoUtil.First("_product.hasSerial")},
                        {"productHasVariations", MongoUtil.First("_product.hasVariations")},
                        {"productVariations", MongoUtil.First("_product.variations")},
                        {"averageCost", MongoUtil.First("_product.averageCost")},
                        {"image", MongoUtil.First("_images.uri")},
                        {"stockVariations", MongoUtil.First("_stock")},
                    }),

                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"_product", 0},
                        {"_images", 0},
                        {"_stock", 0},
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.BudgetItems.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Itens do Orçamento");
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

                BsonDocument? response = await context.BudgetItems.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Item do Orçamento não encontrado") : new(result);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Item do Orçamento");
            }
        }

        public async Task<ResponseApi<BudgetItem?>> GetByIdAsync(string id)
        {
            try
            {
                BudgetItem? budgetItem = await context.BudgetItems.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return new(budgetItem);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Item do Orçamento");
            }
        }

        public async Task<ResponseApi<List<BudgetItem>>> GetByBudgetIdAsync(string budgetId, string plan, string company, string store)
        {
            try
            {
                List<BudgetItem> items = await context.BudgetItems
                    .Find(x => x.BudgetId == budgetId && x.Plan == plan && x.Company == company && x.Store == store && !x.Deleted)
                    .ToListAsync();
                return new(items);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Itens do Orçamento");
            }
        }

        public async Task<int> GetCountDocumentsAsync(PaginationUtil<BudgetItem> pagination)
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

            List<BsonDocument> results = await context.BudgetItems.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<BudgetItem?>> CreateAsync(BudgetItem budgetItem)
        {
            try
            {
                await context.BudgetItems.InsertOneAsync(budgetItem);
                return new(budgetItem, 201, "Item do Orçamento criado com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar Item do Orçamento");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<BudgetItem?>> UpdateAsync(BudgetItem budgetItem)
        {
            try
            {
                await context.BudgetItems.ReplaceOneAsync(x => x.Id == budgetItem.Id, budgetItem);
                return new(budgetItem, 201, "Item do Orçamento atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao atualizar Item do Orçamento");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<BudgetItem>> DeleteAsync(string id)
        {
            try
            {
                BudgetItem? budgetItem = await context.BudgetItems.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                if (budgetItem is null) return new(null, 404, "Item do Orçamento não encontrado");
                budgetItem.Deleted = true;
                budgetItem.DeletedAt = DateTime.UtcNow;

                await context.BudgetItems.ReplaceOneAsync(x => x.Id == id, budgetItem);
                return new(budgetItem, 204, "Item do Orçamento excluído com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao excluir Item do Orçamento");
            }
        }
        #endregion
    }
}
