using api_infor_cell.src.Configuration;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;


namespace api_infor_cell.src.Repository
{
    public class StockRepository(AppDbContext context) : IStockRepository
{
    #region READ
    public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Stock> pagination)
    {
        try
        {
            List<BsonDocument> pipeline = new()
            {
                new("$match", pagination.PipelineFilter),

                new("$group", new BsonDocument
                {
                    { "_id", new BsonDocument 
                        { 
                            { "productId", "$productId" }, 
                        } 
                    },

                    { "quantity", new BsonDocument("$sum", new BsonDocument("$toDouble", "$quantity")) },
                    { "quantityAvailable", new BsonDocument("$sum", new BsonDocument("$min", new BsonArray 
                        { 
                            new BsonDocument("$toDouble", "$quantity"), 
                            new BsonDocument("$toDouble", "$quantityAvailable") 
                        })) 
                    },
                    { "cost", new BsonDocument("$sum", new BsonDocument("$toDouble", "$cost")) },
                    { "createdAt", MongoUtil.First("createdAt") },
                    { "store", MongoUtil.First("store") },
                    { "originDescription", MongoUtil.First("originDescription") },
                    { "variations", MongoUtil.First("variations") },
                    { "isReserved", MongoUtil.First("isReserved") },
                    { "customerIdReserved", new BsonDocument("$push", "$customerIdReserved") },
                }),


                new("$addFields", new BsonDocument
                {
                    { "productId", "$_id.productId" },

                    { "id", new BsonDocument("$concat", new BsonArray 
                        { 
                            "$_id.productId", 
                        }) 
                    },
                    {"productName", MongoUtil.First("_product.name")},
                    {"productHasSerial", MongoUtil.First("_product.hasSerial")},
                    {"productHasVariations", MongoUtil.First("_product.hasVariations")},
                    {"productMinimumStock", MongoUtil.First("_product.minimumStock")},
                }),

                MongoUtil.Lookup("products", ["$productId"], ["$_id"], "_product", [["deleted", false]], 1),
                MongoUtil.Lookup("suppliers", ["$supplierId"], ["$_id"], "_supplier", [["deleted", false]], 1),
                MongoUtil.Lookup("purchase_order_items", ["$purchaseOrderItemId"], ["$_id"], "_purchaseOrderItem", [["deleted", false]], 1),
                
                new("$addFields", new BsonDocument
                {
                    {"purchaseOrderId", MongoUtil.First("_purchaseOrderItem.purchaseOrderId")},
                }),

                MongoUtil.Lookup("purchase_orders", ["$purchaseOrderId"], ["$_id"], "_purchaseOrder", [["deleted", false]], 1),

                new("$addFields", new BsonDocument
                {
                    {"productName", MongoUtil.First("_product.name")},
                    {"supplierName", MongoUtil.First("_supplier.tradeName")},
                    {"productHasSerial", MongoUtil.First("_product.hasSerial")},
                    {"productHasVariations", MongoUtil.First("_product.hasVariations")},
                    {"productMinimumStock", MongoUtil.First("_product.minimumStock")},
                    {"purchaseOrderDate", MongoUtil.First("_purchaseOrder.date")},
                }),

                new("$sort", pagination.PipelineSort),
                new("$skip", pagination.Skip),
                new("$limit", pagination.Limit),

                new("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "_product", 0 },
                    { "_supplier", 0 },
                    { "_purchaseOrderItem", 0 },
                    { "_purchaseOrder", 0 },
                })
            };

            List<BsonDocument> results = await context.Stocks.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return new(list);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<List<dynamic>>> GetByProductIdAggregationAsync(string plan, string company, string productId)
    {
        try
        {
            List<BsonDocument> pipeline = new()
            {
                new("$match", new BsonDocument 
                {
                    {"deleted", false},
                    {"plan", plan},
                    {"company", company},
                    {"productId", productId}
                }),

                new("$group", new BsonDocument
                {
                    { "_id", new BsonDocument 
                        { 
                            { "productId", "$productId" }, 
                            { "store", "$store" }, 
                        } 
                    },

                    { "quantity", new BsonDocument("$sum", new BsonDocument("$toDouble", "$quantity")) },
                    { "quantityAvailable", new BsonDocument("$sum", new BsonDocument("$toDouble", "$quantityAvailable")) },
                    { "cost", new BsonDocument("$sum", new BsonDocument("$toDouble", "$cost")) },
                    { "createdAt", MongoUtil.First("createdAt") },
                    { "store", MongoUtil.First("store") },
                    { "originDescription", MongoUtil.First("originDescription") },
                    { "variations", new BsonDocument("$push", "$variations") },
                }),


                new("$addFields", new BsonDocument
                {
                    { "productId", "$_id.productId" },
                    { "store", "$_id.store" },

                    { "id", new BsonDocument("$concat", new BsonArray 
                        { 
                            "$_id.productId", 
                            "_", 
                            "$_id.store", 
                        }) 
                    },
                    {"productName", MongoUtil.First("_product.name")},
                    {"productHasSerial", MongoUtil.First("_product.hasSerial")},
                }),

                MongoUtil.Lookup("stores", ["$store"], ["$_id"], "_store", [["deleted", false]], 1),
                MongoUtil.Lookup("products", ["$productId"], ["$_id"], "_product", [["deleted", false]], 1),
                MongoUtil.Lookup("suppliers", ["$supplierId"], ["$_id"], "_supplier", [["deleted", false]], 1),
                MongoUtil.Lookup("purchase_order_items", ["$purchaseOrderItemId"], ["$_id"], "_purchaseOrderItem", [["deleted", false]], 1),
                
                new("$addFields", new BsonDocument
                {
                    {"purchaseOrderId", MongoUtil.First("_purchaseOrderItem.purchaseOrderId")},
                }),

                MongoUtil.Lookup("purchase_orders", ["$purchaseOrderId"], ["$_id"], "_purchaseOrder", [["deleted", false]], 1),

                new("$addFields", new BsonDocument
                {
                    {"productName", MongoUtil.First("_product.name")},
                    {"supplierName", MongoUtil.First("_supplier.tradeName")},
                    {"productHasSerial", MongoUtil.First("_product.hasSerial")},
                    {"productHasVariations", MongoUtil.First("_product.hasVariations")},
                    {"purchaseOrderDate", MongoUtil.First("_purchaseOrder.date")},
                    {"storeName", MongoUtil.First("_store.tradeName")},
                }),


                new("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "_product", 0 },
                    { "_supplier", 0 },
                    { "_purchaseOrderItem", 0 },
                    { "_purchaseOrder", 0 },
                })
            };

            List<BsonDocument> results = await context.Stocks.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return new(list);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
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

            BsonDocument? response = await context.Stocks.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
            return result is null ? new(null, 404, "Estoque não encontrado") : new(result);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<Stock?>> GetByIdAsync(string id)
    {
        try
        {
            Stock? stock = await context.Stocks.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
            return new(stock);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<Stock?>> GetByOriginIdAsync(string originId)
    {
        try
        {
            Stock? stock = await context.Stocks.Find(x => x.OriginId == originId && !x.Deleted).FirstOrDefaultAsync();
            return new(stock);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<List<Stock>>> GetByOriginIdAllAsync(string originId, string origin)
    {
        try
        {
            List<Stock> stocks = await context.Stocks.Find(x => x.OriginId == originId && x.Origin == origin && !x.Deleted).ToListAsync();
            return new(stocks);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<List<Stock>>> GetStockTransfer(string productId, string barcode, string hasSerial, string serial, string planId, string companyId, string storeId)
    {
        try
        {
            List<Stock> stocks = await context.Stocks.Find(x => x.ProductId == productId && x.Plan == planId && x.Company == companyId && x.Store == storeId && !x.Deleted).ToListAsync();
            return new(stocks);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<Stock?>> GetVerifyStock(string productId, string planId, string companyId, string storeId)
    {
        try
        {
            Stock stock = await context.Stocks.Find(x => x.ProductId == productId && x.ForSale == "yes" && x.Quantity > 0 && x.Plan == planId && x.Company == companyId && x.Store == storeId && !x.Deleted).FirstOrDefaultAsync();
            return new(stock);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<List<Stock>>> GetVerifyStockAll(string productId, string planId, string companyId, string storeId)
    {
        try
        {
            List<Stock> stocks = await context.Stocks.Find(x => x.ProductId == productId && x.ForSale == "yes" && x.Quantity > 0 && x.Plan == planId && x.Company == companyId && x.Store == storeId && !x.Deleted).ToListAsync();
            return new(stocks);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<List<Stock>>> GetByProductId(string productId, string planId, string companyId, string storeId)
    {
        try
        {
            List<Stock> stocks = await context.Stocks.Find(x => x.ProductId == productId && x.Plan == planId && x.Company == companyId && x.Store == storeId && !x.Deleted).ToListAsync();
            return new(stocks);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Estoque");
        }
    }
    public async Task<ResponseApi<long>> GetNextCodeAsync(string planId, string companyId, string storeId)
    {
        try
        {
            long code = await context.Stocks.Find(x => x.Plan == x.Plan && x.Company == companyId && x.Store == storeId).CountDocumentsAsync() + 1;
            return new(code);
        }
        catch
        {
            return new(0, 500, "Falha ao buscar Pedido de Compras");
        }
    }    
    public async Task<int> GetCountDocumentsAsync(PaginationUtil<Stock> pagination)
    {
        List<BsonDocument> pipeline = new()
        {
            new("$match", pagination.PipelineFilter),

            new("$group", new BsonDocument
            {
                { "_id", new BsonDocument 
                    { 
                        { "productId", "$productId" }, 
                        // { "supplierId", "$supplierId" }, 
                        // { "purchaseOrderItemId", "$purchaseOrderItemId" } 
                    } 
                },

                { "quantity", new BsonDocument("$sum", new BsonDocument("$toDouble", "$quantity")) },
                { "cost", new BsonDocument("$sum", new BsonDocument("$toDouble", "$cost")) },
                { "createdAt", MongoUtil.First("createdAt") },
                { "store", MongoUtil.First("store") },
                { "originDescription", MongoUtil.First("originDescription") },
                { "variations", MongoUtil.First("variations") },
            }),


            new("$addFields", new BsonDocument
            {
                { "productId", "$_id.productId" },
                // { "supplierId", "$_id.supplierId" },
                // { "purchaseOrderItemId", "$_id.purchaseOrderItemId" },

                { "id", new BsonDocument("$concat", new BsonArray 
                    { 
                        "$_id.productId", 
                        // "_", 
                        // "$_id.supplierId", 
                        // "_", 
                        // "$_id.purchaseOrderItemId" 
                    }) 
                },
                {"productName", MongoUtil.First("_product.name")},
                {"productHasSerial", MongoUtil.First("_product.hasSerial")},
                // {"variations", MongoUtil.First("_product.variations")},
            }),

            MongoUtil.Lookup("products", ["$productId"], ["$_id"], "_product", [["deleted", false]], 1),
            MongoUtil.Lookup("suppliers", ["$supplierId"], ["$_id"], "_supplier", [["deleted", false]], 1),
            MongoUtil.Lookup("purchase_order_items", ["$purchaseOrderItemId"], ["$_id"], "_purchaseOrderItem", [["deleted", false]], 1),
            
            new("$addFields", new BsonDocument
            {
                {"purchaseOrderId", MongoUtil.First("_purchaseOrderItem.purchaseOrderId")},
            }),

            MongoUtil.Lookup("purchase_orders", ["$purchaseOrderId"], ["$_id"], "_purchaseOrder", [["deleted", false]], 1),

            new("$addFields", new BsonDocument
            {
                {"productName", MongoUtil.First("_product.name")},
                {"supplierName", MongoUtil.First("_supplier.tradeName")},
                // {"variations", MongoUtil.First("_product.variations")},
                // {"productHasSerial", MongoUtil.First("_product.variations")},
                {"productHasSerial", MongoUtil.First("_product.hasSerial")},
                {"purchaseOrderDate", MongoUtil.First("_purchaseOrder.date")},
            }),

            new("$sort", pagination.PipelineSort),
            new("$skip", pagination.Skip),
            new("$limit", pagination.Limit),

            new("$project", new BsonDocument
            {
                { "_id", 0 },
                { "_product", 0 },
                { "_supplier", 0 },
                { "_purchaseOrderItem", 0 },
                { "_purchaseOrder", 0 },
            })
        };

        List<BsonDocument> results = await context.Stocks.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
    }
    #endregion
    
    #region CREATE
    public async Task<ResponseApi<Stock?>> CreateAsync(Stock stock)
    {
        try
        {
            await context.Stocks.InsertOneAsync(stock);

            return new(stock, 201, "Estoque criada com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao criar Estoque");  
        }
    }
    #endregion
    
    #region UPDATE
    public async Task<ResponseApi<Stock?>> UpdateAsync(Stock stock)
    {
        try
        {
            await context.Stocks.ReplaceOneAsync(x => x.Id == stock.Id, stock);

            return new(stock, 201, "Estoque atualizada com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao atualizar Estoque");
        }
    }
    #endregion
    
    #region DELETE
    public async Task<ResponseApi<Stock>> DeleteAllByProductAsync(DeleteDTO request)
    {
        try
        {
            List<Stock> stocks = await context.Stocks.Find(x => x.Plan == request.Plan && x.Company == request.Company && x.Store == request.Store && x.ProductId == request.Id && !x.Deleted).ToListAsync();
            if(stocks is null) return new(null, 404, "Estoque não encontrado");

            foreach (Stock stock in stocks)
            {
                stock.Deleted = true;
                stock.DeletedAt = DateTime.UtcNow;
                stock.DeletedBy = request.DeletedBy;

                await context.Stocks.ReplaceOneAsync(x => x.Id == stock.Id, stock);
            }

            return new(null, 204, "Estoque excluída com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao excluír Estoque");
        }
    }
    public async Task<ResponseApi<Stock>> DeleteAsync(string id)
    {
        try
        {
            Stock? stock = await context.Stocks.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
            if(stock is null) return new(null, 404, "Estoque não encontrado");
            stock.Deleted = true;
            stock.DeletedAt = DateTime.UtcNow;

            await context.Stocks.ReplaceOneAsync(x => x.Id == id, stock);

            return new(stock, 204, "Estoque excluída com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao excluír Estoque");
        }
    }
    #endregion
}
}