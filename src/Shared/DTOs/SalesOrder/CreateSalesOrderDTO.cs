using System.ComponentModel.DataAnnotations;

namespace api_infor_cell.src.Shared.DTOs
{
    public class CreateSalesOrderDTO : RequestDTO
    {
        // [Required(ErrorMessage = "O Produto é obrigatório.")]
        // [Display(Order = 1)]
        public string ProductId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Vendedor é obrigatório.")]
        [Display(Order = 2)]
        public string SellerId { get; set; } = string.Empty;
        
        // [Required(ErrorMessage = "A Variação é obrigatória.")]
        // [Display(Order = 3)]
        public string VariationId { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Value { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountValue { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public bool CreateItem {get; set;} = false;
        public string CodeVariation { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
    }
}