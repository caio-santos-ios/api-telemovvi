namespace api_infor_cell.src.Shared.DTOs
{
    public class SaveEcommerceConfigDTO : RequestDTO
    {
        public string StoreName { get; set; } = string.Empty;
        public string StoreDescription { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;
        public bool Enabled { get; set; } = false;
        public string PrimaryColor { get; set; } = "#7C3AED";
        public bool ShippingEnabled { get; set; } = false;
        public decimal ShippingFixedPrice { get; set; } = 0;
        public decimal ShippingFreeAbove { get; set; } = 0;
        public string ShippingDescription { get; set; } = string.Empty;
    }

    public class EcommerceRegisterDTO
    {
        public string Plan { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
    }

    public class EcommerceLoginDTO
    {
        public string Plan { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class EcommerceCheckoutDTO
    {
        public string Plan { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string BillingType { get; set; } = string.Empty; // PIX, BOLETO, CREDIT_CARD
        public List<EcommerceCheckoutItem> Items { get; set; } = [];
        public EcommerceAddressDTO ShippingAddress { get; set; } = new();

        // Cartão de crédito (opcional)
        public string? CardHolderName { get; set; }
        public string? CardNumber { get; set; }
        public string? CardExpiryMonth { get; set; }
        public string? CardExpiryYear { get; set; }
        public string? CardCvv { get; set; }
    }

    public class EcommerceCheckoutItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string StockId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class EcommerceAddressDTO
    {
        public string ZipCode { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Complement { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}
