using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_infor_cell.src.Models
{
    public class Plan : ModelBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("type")]
        public string Type { get; set; } = string.Empty; 

        [BsonElement("startDate")]
        public DateTime StartDate {get;set;} = DateTime.UtcNow;
        
        [BsonElement("expirationDate")]
        public DateTime ExpirationDate {get;set;} = DateTime.UtcNow;
        
        [BsonElement("status")]
        public string Status { get; set; } = string.Empty; 
    }
}