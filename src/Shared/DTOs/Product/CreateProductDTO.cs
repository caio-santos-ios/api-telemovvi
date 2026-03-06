using System.ComponentModel.DataAnnotations;
using api_infor_cell.src.Models;

namespace api_infor_cell.src.Shared.DTOs
{
    public class CreateProductDTO : RequestDTO
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [Display(Order = 1)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Imei { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A Categoria é obrigatória.")]
        [Display(Order = 2)]
        public string CategoryId { get; set; } = string.Empty;
        
        // [Required(ErrorMessage = "A Marca é obrigatória.")]
        // [Display(Order = 3)]
        public string BrandId { get; set; } = string.Empty;
        public string MoveStock { get; set; } = string.Empty;
        public int QuantityStock { get; set; }
        
        [Required(ErrorMessage = "O Preço é obrigatório.")]
        [Display(Order = 5)]
        public decimal Price { get; set; }
        public decimal PriceDiscount { get; set; }
        public decimal PriceTotal { get; set; }
        
        [Required(ErrorMessage = "O Custo é obrigatório.")]
        [Display(Order = 4)]
        public decimal CostPrice { get; set; }
        public decimal ExpenseCostPrice { get; set; }
        public List<VariationProduct> Variations {get;set;} = [];
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public string DescriptionComplet { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Ean { get; set; } = string.Empty;
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public string SaleWithoutStock { get; set; } = string.Empty;
        public string HasVariations { get; set; } = string.Empty;
        public string HasSerial { get; set; } = string.Empty;
        public string PhysicalLocation { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal AverageCost { get; set; }
        public decimal MinimumPrice { get; set; }
        public decimal Margin { get; set; }
        public string HasDiscount { get; set; } = string.Empty;
        public string Ncm { get; set; } = string.Empty;
        public decimal Cest { get; set; }
        public decimal CfopIn { get; set; }
        public decimal CfopOut { get; set; }
        public string Origin { get; set; } = string.Empty;
        public decimal Cst { get; set; }
        public decimal Icms { get; set; }
        public decimal Pis { get; set; }
        public decimal Cofins { get; set; }
        public decimal Ipi { get; set; }
        public decimal Ibpt { get; set; }
        public string TaxGroup { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
    }
}