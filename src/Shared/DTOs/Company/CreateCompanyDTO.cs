namespace api_infor_cell.src.Shared.DTOs
{
        public class CreateCompanyDTO
        {        
                public string Plan { get; set; } = string.Empty; 
                public string Document { get; set; } = string.Empty; 
                public string CorporateName { get; set; } = string.Empty; 
                public string TradeName { get; set; } = string.Empty;
                public string StateRegistration { get; set; } = string.Empty; 
                public string MunicipalRegistration { get; set; } = string.Empty; 
                public string Email { get; set; } = string.Empty;
                public string Phone { get; set; } = string.Empty;
                public string Whatsapp { get; set; } = string.Empty;
                public string Photo { get; set; } = string.Empty;
                public string Website { get; set; } = string.Empty;
                public string CreatedBy { get; set; } = string.Empty;
        }
}