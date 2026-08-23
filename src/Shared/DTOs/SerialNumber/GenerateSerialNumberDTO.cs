namespace api_infor_cell.src.Shared.DTOs
{
    public class GenerateSerialNumberDTO : RequestDTO
    {
        public string Type { get; set; } = "serial";
        public string? ProductId { get; set; } = string.Empty;
    }
}
