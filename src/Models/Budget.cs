using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_infor_cell.src.Models
{
    public class Budget : ModelMasterBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("code")]
        public string Code { get; set; } = string.Empty;

        [BsonElement("customerId")]
        public string CustomerId { get; set; } = string.Empty;

        [BsonElement("sellerId")]
        public string SellerId { get; set; } = string.Empty;

        [BsonElement("total")]
        public decimal Total { get; set; }

        [BsonElement("subTotal")]
        public decimal SubTotal { get; set; }

        [BsonElement("quantity")]
        public decimal Quantity { get; set; }

        [BsonElement("discountValue")]
        public decimal DiscountValue { get; set; }

        [BsonElement("discountType")]
        public string DiscountType { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        [BsonElement("validity")]
        public DateTime? Validity { get; set; }

        [BsonElement("notes")]
        public string Notes { get; set; } = string.Empty;
    }
}
