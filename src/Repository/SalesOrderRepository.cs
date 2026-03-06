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
    public class SalesOrderRepository(AppDbContext context) : ISalesOrderRepository
{
    #region READ
    public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<SalesOrder> pagination)
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

                new("$addFields", new BsonDocument
                {
                    {"id", new BsonDocument("$toString", "$_id")},
                    {"customerName", MongoUtil.First("_customer.tradeName")},
                    {"userName", MongoUtil.First("_user.name")},
                }),
                new("$project", new BsonDocument
                {
                    {"_id", 0}, 
                    {"_customer", 0}, 
                    {"_user", 0}, 
                }),
                new("$sort", pagination.PipelineSort),
            };

            List<BsonDocument> results = await context.SalesOrders.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return new(list);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Pedidos de Venda");
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

            BsonDocument? response = await context.SalesOrders.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
            return result is null ? new(null, 404, "Pedidos de Venda não encontrado") : new(result);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Pedidos de Venda");
        }
    }
    public async Task<ResponseApi<dynamic?>> GetReceiptByIdAggregateAsync(string id)
    {
        try
        {
            BsonDocument[] pipeline = [
                new("$match", new BsonDocument{
                    {"_id", new ObjectId(id)},
                    {"deleted", false}
                }),
                
                MongoUtil.Lookup("stores", ["$store"], ["$_id"], "_store", [["deleted", false]], 1),
                MongoUtil.Lookup("customers", ["$customerId"], ["$_id"], "_customer", [["deleted", false]], 1),
                MongoUtil.Lookup("sales_order_items", ["$_id"], ["$salesOrderId"], "_items", [["deleted", false]]),

                new("$unwind", "$_items"),

                MongoUtil.Lookup("products", ["$_items.productId"], ["$_id"], "_items._product", [["deleted", false]], 1),

                new("$addFields", new BsonDocument {
                    {"storeName", MongoUtil.First("_store.corporateName")},
                    {"storePhone", MongoUtil.First("_store.phone")},
                    {"storeDocument", MongoUtil.First("_store.document")},
                    {"customerName", MongoUtil.First("_customer.tradeName")},
                }),
                new("$group", new BsonDocument
                {
                    {"_id", "$_id"},
                    {"storeName", MongoUtil.First("storeName")},
                    {"storePhone", MongoUtil.First("storePhone")},
                    {"storeDocument", MongoUtil.First("storeDocument")},
                    {"customerName", MongoUtil.First("customerName")},
                    {"createdAt", MongoUtil.First("createdAt")},
                    {"payment", MongoUtil.First("payment")},
                    {"total", MongoUtil.First("total")},
                    {"items", new BsonDocument("$push", new BsonDocument 
                        {
                            {"id", new BsonDocument("$toString", "$_items._id")},
                            {"productName", MongoUtil.First("_items._product.name")}, 
                            {"quantity", "$_items.quantity"},
                            {"value", "$_items.value"},
                            {"total", "$_items.total"}
                        }
                    )},
                }),

                new("$project", new BsonDocument
                {
                    {"_id", 0},
                    {"id", new BsonDocument("$toString", "$_id")},
                    {"storeName", 1},
                    {"storePhone", 1},
                    {"storeDocument", 1},
                    {"customerName", 1},
                    {"createdAt", 1},
                    {"total", 1},
                    {"items", 1},
                    {"payment", 1}
                })
            ];

            BsonDocument? response = await context.SalesOrders.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
            return result is null ? new(null, 404, "Pedidos de Venda não encontrado") : new(result);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Pedidos de Venda");
        }
    }
    
    public async Task<ResponseApi<SalesOrder?>> GetByIdAsync(string id)
    {
        try
        {
            SalesOrder? salesOrder = await context.SalesOrders.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
            return new(salesOrder);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Pedidos de Venda");
        }
    }
    
    public async Task<ResponseApi<long>> GetNextCodeAsync(string planId, string companyId, string storeId)
    {
        try
        {
            long code = await context.SalesOrders.Find(x => x.Plan == planId && x.Company == companyId && x.Store == storeId).CountDocumentsAsync() + 1;
            return new(code);
        }
        catch
        {
            return new(0, 500, "Falha ao buscar Próximo Código");
        }
    }
    public async Task<int> GetCountDocumentsAsync(PaginationUtil<SalesOrder> pagination)
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

        List<BsonDocument> results = await context.SalesOrders.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
    }
    #endregion
    
    #region CREATE
    public async Task<ResponseApi<SalesOrder?>> CreateAsync(SalesOrder salesOrder)
    {
        try
        {
            await context.SalesOrders.InsertOneAsync(salesOrder);

            return new(salesOrder, 201, "Pedidos de Venda criada com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao criar Pedidos de Venda");  
        }
    }
    #endregion
    
    #region UPDATE
    public async Task<ResponseApi<SalesOrder?>> UpdateAsync(SalesOrder salesOrder)
    {
        try
        {
            await context.SalesOrders.ReplaceOneAsync(x => x.Id == salesOrder.Id, salesOrder);

            return new(salesOrder, 201, "Pedidos de Venda atualizada com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao atualizar Pedidos de Venda");
        }
    }
    #endregion
    
    #region DELETE
    public async Task<ResponseApi<SalesOrder>> DeleteAsync(string id)
    {
        try
        {
            SalesOrder? salesOrder = await context.SalesOrders.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
            if(salesOrder is null) return new(null, 404, "Pedidos de Venda não encontrado");
            salesOrder.Deleted = true;
            salesOrder.DeletedAt = DateTime.UtcNow;

            await context.SalesOrders.ReplaceOneAsync(x => x.Id == id, salesOrder);

            return new(salesOrder, 204, "Pedidos de Venda excluída com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao excluír Pedidos de Venda");
        }
    }
    #endregion
}
}