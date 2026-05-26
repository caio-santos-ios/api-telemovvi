using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_infor_cell.src.Models
{
    public class SubscriptionInvoice : ModelBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("plan")]
        public string Plan { get; set; } = string.Empty;

        [BsonElement("company")]
        public string Company { get; set; } = string.Empty;

        [BsonElement("store")]
        public string Store { get; set; } = string.Empty;

        [BsonElement("asaasCustomerId")]
        public string AsaasCustomerId { get; set; } = string.Empty;

        [BsonElement("asaasSubscriptionId")]
        public string AsaasSubscriptionId { get; set; } = string.Empty;

        [BsonElement("asaasPaymentId")]
        public string AsaasPaymentId { get; set; } = string.Empty;

        [BsonElement("planType")]
        public string PlanType { get; set; } = string.Empty;

        [BsonElement("billingType")]
        public string BillingType { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "PENDING";

        [BsonElement("value")]
        public decimal Value { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("paymentDate")]
        public DateTime? PaymentDate { get; set; }

        [BsonElement("dueDate")]
        public DateTime DueDate { get; set; }

        [BsonElement("expirationDate")]
        public DateTime? ExpirationDate { get; set; }

        [BsonElement("paymentUrl")]
        public string PaymentUrl { get; set; } = string.Empty;

        [BsonElement("identificationField")]
        public string IdentificationField { get; set; } = string.Empty;

        [BsonElement("pixQrCode")]
        public string PixQrCode { get; set; } = string.Empty;

        [BsonElement("pixQrCodeImage")]
        public string PixQrCodeImage { get; set; } = string.Empty;

        [BsonElement("invoiceUrl")]
        public string InvoiceUrl { get; set; } = string.Empty;
        
        [BsonElement("invoiceNumber")]
        public string InvoiceNumber { get; set; } = string.Empty;
    }
}