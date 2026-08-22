using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_infor_cell.src.Models
{
    public class LogApi : ModelMasterBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        [BsonElement("collection")]
        public string Collection { get; set; } = string.Empty;
        
        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;
        
        [BsonElement("status")]
        public int Status { get; set; }
        
        [BsonElement("time")]
        public double Time { get; set; }
        
        [BsonElement("path")]
        public string Path { get; set; } = string.Empty;
        
        [BsonElement("method")]
        public string Method { get; set; } = string.Empty;
    }
}