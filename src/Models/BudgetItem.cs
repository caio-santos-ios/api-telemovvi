using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_infor_cell.src.Models
{
    public class BudgetItem : ModelMasterBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("budgetId")]
        public string BudgetId { get; set; } = string.Empty;

        [BsonElement("variationId")]
        public string VariationId { get; set; } = string.Empty;

        [BsonElement("codeVariation")]
        public string CodeVariation { get; set; } = string.Empty;

        [BsonElement("productId")]
        public string ProductId { get; set; } = string.Empty;

        [BsonElement("total")]
        public decimal Total { get; set; }

        [BsonElement("subTotal")]
        public decimal SubTotal { get; set; }

        [BsonElement("value")]
        public decimal Value { get; set; }

        [BsonElement("quantity")]
        public decimal Quantity { get; set; }

        [BsonElement("discountValue")]
        public decimal DiscountValue { get; set; }

        [BsonElement("discountType")]
        public string DiscountType { get; set; } = string.Empty;

        [BsonElement("serial")]
        public string Serial { get; set; } = string.Empty;

        [BsonElement("stockIds")]
        public List<string> StockIds { get; set; } = [];

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;
    }
}
