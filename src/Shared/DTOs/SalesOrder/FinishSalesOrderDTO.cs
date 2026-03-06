using System.ComponentModel.DataAnnotations;

namespace api_infor_cell.src.Shared.DTOs
{
    public class FinishSalesOrderDTO : RequestDTO
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Forma de Pagamento é obrigatória.")]
        [Display(Order = 1)]
        public string PaymentMethodId { get; set; } = string.Empty;
        public decimal NumberOfInstallments { get; set; }
        public decimal Freight { get; set; }
        public string Currier { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
    }
}
