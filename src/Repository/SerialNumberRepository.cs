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
    public class SerialNumberRepository(AppDbContext context) : ISerialNumberRepository
    {
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<SerialNumber> pagination)
        {
            try
            {
                List<BsonDocument> pipeline =
                [
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),
                ];

                List<BsonDocument> results = await context.SerialNumbers.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Seriais");
            }
        }

        public async Task<ResponseApi<SerialNumber?>> GetByIdAsync(string id)
        {
            try
            {
                SerialNumber serialNumber = await context.SerialNumbers.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return new(serialNumber);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar Serial");
            }
        }

        public async Task<bool> ExistsCodeAsync(string plan, string company, string code)
        {
            try
            {
                bool existsInSerialNumbers = await context.SerialNumbers.Find(x => x.Plan == plan && x.Company == company && x.Code == code && !x.Deleted).AnyAsync();
                if (existsInSerialNumbers) return true;

                bool existsInProducts = await context.Products.Find(x => x.Plan == plan && x.Company == company && (x.Code == code || x.Ean == code || x.Imei == code) && !x.Deleted).AnyAsync();
                if (existsInProducts) return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ResponseApi<SerialNumber?>> CreateAsync(SerialNumber serialNumber)
        {
            try
            {
                await context.SerialNumbers.InsertOneAsync(serialNumber);
                return new(serialNumber);
            }
            catch
            {
                return new(null, 500, "Falha ao registrar Serial");
            }
        }
    }
}
