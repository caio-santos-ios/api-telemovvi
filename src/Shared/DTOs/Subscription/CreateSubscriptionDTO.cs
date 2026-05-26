namespace api_infor_cell.src.Shared.DTOs
{
    public class CreateSubscriptionDTO : RequestDTO
    {
        /// <summary>Tipo do plano: Bronze, Prata, Ouro, Platina</summary>
        public string PlanType { get; set; } = string.Empty;

        /// <summary>Forma de pagamento: PIX, BOLETO, CREDIT_CARD, DEBIT_CARD</summary>
        public string BillingType { get; set; } = string.Empty;

        // Dados do cartão (somente para CREDIT_CARD / DEBIT_CARD)
        public string? CardHolderName { get; set; }
        public string? CardNumber { get; set; }
        public string? CardExpiryMonth { get; set; }
        public string? CardExpiryYear { get; set; }
        public string? CardCvv { get; set; }
    }

    public class AsaasWebhookDTO
    {
        public string Event { get; set; } = string.Empty;
        public AsaasPaymentDTO Payment { get; set; } = new();
    }

    public class AsaasPaymentDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Subscription { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string BillingType { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string InvoiceUrl { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
    }
}