using System.ComponentModel.DataAnnotations;

namespace api_infor_cell.src.Shared.DTOs
{
    public class CreateBudgetItemDTO : RequestDTO
    {
        [Required(ErrorMessage = "O Produto é obrigatório.")]
        [Display(Order = 1)]
        public string ProductId { get; set; } = string.Empty;
        public string BudgetId { get; set; } = string.Empty;
        public string VariationId { get; set; } = string.Empty;
        public string CodeVariation { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Value { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountValue { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public string StockId { get; set; } = string.Empty;
    }
}
