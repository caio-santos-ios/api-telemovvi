using api_infor_cell.src.Shared.DTOs;

namespace api_telemovvi.src.Shared.DTOs.Subscription;
public class PaymentPlansDTO
{
    public string Event { get; set; } = string.Empty;
    public AsaasPaymentDTO Payment { get; set; } = new();
}