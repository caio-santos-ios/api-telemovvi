namespace api_telemovvi.src.Shared.DTOs.Subscription;
public class SignaturePlansDTO
{
    public string Event { get; set; } = string.Empty;
    public AsaasSignaturePlansDTO Subscription { get; set; } = new();
}

public class AsaasSignaturePlansDTO
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Subscription { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string BillingType { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
}