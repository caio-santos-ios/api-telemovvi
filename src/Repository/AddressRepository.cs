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
    public class AddressRepository(AppDbContext context) : IAddressRepository
{
    #region READ
    public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<Address> pagination)
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

            List<BsonDocument> results = await context.Addresses.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return new(list);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Items");
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
                new("$project", new BsonDocument
                {
                    {"_id", 0},
                    {"id", new BsonDocument("$toString", "$_id")},
                    {"name", 1},
                    {"description", 1}
                }),
            ];

            BsonDocument? response = await context.Addresses.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
            return result is null ? new(null, 404, "Item não encontrado") : new(result);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Item");
        }
    }
    
    public async Task<ResponseApi<Address?>> GetByIdAsync(string id)
    {
        try
        {
            Address? address = await context.Addresses.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
            return new(address);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Item");
        }
    }
    public async Task<ResponseApi<Address?>> GetByParentIdAsync(string parentId, string parent)
    {
        try
        {
            Address? address = await context.Addresses.Find(x => x.ParentId == parentId && x.Parent == parent && !x.Deleted).FirstOrDefaultAsync();
            return new(address);
        }
        catch
        {
            return new(null, 500, "Falha ao buscar Item");
        }
    }
    
    public async Task<int> GetCountDocumentsAsync(PaginationUtil<Address> pagination)
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

        List<BsonDocument> results = await context.Addresses.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
    }
    #endregion
    
    #region CREATE
    public async Task<ResponseApi<Address?>> CreateAsync(Address address)
    {
        try
        {
            await context.Addresses.InsertOneAsync(address);

            return new(address, 201, "Item criado com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao criar Item");  
        }
    }
    public async Task<ResponseApi<List<Address>?>> CreateManyAsync(List<Address> addresses)
    {
        try
        {
            await context.Addresses.InsertManyAsync(addresses);

            return new(addresses, 201, "Item criado com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao criar Item");  
        }
    }
    #endregion
    
    #region UPDATE
    public async Task<ResponseApi<Address?>> UpdateAsync(Address address)
    {
        try
        {
            await context.Addresses.ReplaceOneAsync(x => x.Id == address.Id, address);

            return new(address, 201, "Item atualizado com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao atualizar Item");
        }
    }
    public async Task<ResponseApi<Address?>> UpdateParentAsync(Address address)
    {
        try
        {
            await context.Addresses.ReplaceOneAsync(x => x.ParentId == address.ParentId && x.Parent == address.Parent, address);

            return new(address, 201, "Item atualizado com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao atualizar Item");
        }
    }
    #endregion
    
    #region DELETE
    public async Task<ResponseApi<Address>> DeleteAsync(string id)
    {
        try
        {
            Address? address = await context.Addresses.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
            if(address is null) return new(null, 404, "Item não encontrado");
            address.Deleted = true;
            address.DeletedAt = DateTime.UtcNow;

            await context.Addresses.ReplaceOneAsync(x => x.Id == id, address);

            return new(address, 204, "Item excluído com sucesso");
        }
        catch
        {
            return new(null, 500, "Falha ao excluír Item");
        }
    }
    #endregion
}
}