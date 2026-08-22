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
    public class LogApiRepository(AppDbContext context) : ILogApiRepository
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<LogApi> pagination)
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

                    new("$project", new BsonDocument
                    {
                        {"_id", 0}, 
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"type", 1},
                        {"createdAt", 1},
                        {"quantity", 1},
                        {"productName", MongoUtil.First("_product.name")},
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.ApiLogs.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Log");
            }
        }
        #endregion
        #region CREATE
        public async Task<ResponseApi<LogApi?>> CreateAsync(LogApi logApi)
        {
            try
            {
                await context.ApiLogs.InsertOneAsync(logApi);

                return new(logApi, 201, "Log criada com sucesso");
            }
            catch
            {
                return new(null, 500, "Falha ao criar Log");  
            }
        }
        #endregion
    }
}