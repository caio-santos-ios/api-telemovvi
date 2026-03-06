using System.ComponentModel.DataAnnotations;

namespace api_infor_cell.src.Shared.DTOs
{
    public class CreateBudgetDTO : RequestDTO
    {
        public string CustomerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Vendedor é obrigatório.")]
        [Display(Order = 1)]
        public string SellerId { get; set; } = string.Empty;

        public DateTime? Validity { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
