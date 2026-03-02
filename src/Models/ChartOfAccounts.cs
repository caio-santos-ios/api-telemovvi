using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace api_infor_cell.src.Models
{
    public class ChartOfAccounts : ModelMasterBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("code")]
        public string Code { get; set; } = string.Empty;

        [BsonElement("name")]
        [Required(ErrorMessage = "Nome é obrigatório")]
        [Display(Order = 1)]
        public string Name { get; set; } = string.Empty;

        [BsonElement("level")]
        public int Level { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = string.Empty;

        [BsonElement("groupDRE")]
        public string GroupDRE { get; set; } = string.Empty;

        [BsonElement("account")]
        public string Account { get; set; } = string.Empty;
    }
}