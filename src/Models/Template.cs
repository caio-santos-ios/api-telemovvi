using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_infor_cell.src.Models
{
    public class Template : ModelBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        [BsonElement("code")]
        public string Code {get;set;} = string.Empty;        
        
        [BsonElement("html")]
        public string Html {get;set;} = string.Empty;        
    }
}