using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_infor_cell.src.Models
{
    public class EcommerceConfig : ModelMasterBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("storeName")]
        public string StoreName { get; set; } = string.Empty;

        [BsonElement("storeDescription")]
        public string StoreDescription { get; set; } = string.Empty;

        [BsonElement("logoUrl")]
        public string LogoUrl { get; set; } = string.Empty;

        [BsonElement("bannerUrl")]
        public string BannerUrl { get; set; } = string.Empty;

        [BsonElement("enabled")]
        public bool Enabled { get; set; } = false;

        [BsonElement("primaryColor")]
        public string PrimaryColor { get; set; } = "#7C3AED";

        // Frete
        [BsonElement("shippingEnabled")]
        public bool ShippingEnabled { get; set; } = false;

        [BsonElement("shippingFixedPrice")]
        public decimal ShippingFixedPrice { get; set; } = 0;

        [BsonElement("shippingFreeAbove")]
        public decimal ShippingFreeAbove { get; set; } = 0;

        [BsonElement("shippingDescription")]
        public string ShippingDescription { get; set; } = string.Empty;
    }

    public class EcommerceCustomer : ModelBase
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

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("document")]
        public string Document { get; set; } = string.Empty;

        [BsonElement("asaasCustomerId")]
        public string AsaasCustomerId { get; set; } = string.Empty;

        [BsonElement("address")]
        public EcommerceAddress Address { get; set; } = new();
    }

    public class EcommerceAddress
    {
        [BsonElement("zipCode")]
        public string ZipCode { get; set; } = string.Empty;

        [BsonElement("street")]
        public string Street { get; set; } = string.Empty;

        [BsonElement("number")]
        public string Number { get; set; } = string.Empty;

        [BsonElement("complement")]
        public string Complement { get; set; } = string.Empty;

        [BsonElement("neighborhood")]
        public string Neighborhood { get; set; } = string.Empty;

        [BsonElement("city")]
        public string City { get; set; } = string.Empty;

        [BsonElement("state")]
        public string State { get; set; } = string.Empty;
    }

    public class EcommerceOrder : ModelBase
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

        [BsonElement("code")]
        public string Code { get; set; } = string.Empty;

        [BsonElement("customerId")]
        public string CustomerId { get; set; } = string.Empty;

        [BsonElement("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [BsonElement("items")]
        public List<EcommerceOrderItem> Items { get; set; } = [];

        [BsonElement("subtotal")]
        public decimal Subtotal { get; set; }

        [BsonElement("shipping")]
        public decimal Shipping { get; set; }

        [BsonElement("total")]
        public decimal Total { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "PENDING"; // PENDING, PAID, CANCELLED

        [BsonElement("billingType")]
        public string BillingType { get; set; } = string.Empty;

        [BsonElement("asaasPaymentId")]
        public string AsaasPaymentId { get; set; } = string.Empty;

        [BsonElement("paymentUrl")]
        public string PaymentUrl { get; set; } = string.Empty;

        [BsonElement("pixQrCode")]
        public string PixQrCode { get; set; } = string.Empty;

        [BsonElement("pixQrCodeImage")]
        public string PixQrCodeImage { get; set; } = string.Empty;

        [BsonElement("identificationField")]
        public string IdentificationField { get; set; } = string.Empty;

        [BsonElement("shippingAddress")]
        public EcommerceAddress ShippingAddress { get; set; } = new();
    }

    public class EcommerceOrderItem
    {
        [BsonElement("productId")]
        public string ProductId { get; set; } = string.Empty;

        [BsonElement("productName")]
        public string ProductName { get; set; } = string.Empty;

        [BsonElement("stockId")]
        public string StockId { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public decimal Quantity { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("total")]
        public decimal Total { get; set; }
    }
}
