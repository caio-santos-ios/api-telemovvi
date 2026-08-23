using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_infor_cell.src.Controllers
{
    [Route("api/serial-numbers")]
    [ApiController]
    public class SerialNumberController(ISerialNumberService service) : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            PaginationApi<List<dynamic>> response = await service.GetAllAsync(new(Request.Query));
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [Authorize]
        [HttpGet("generate")]
        public async Task<IActionResult> Generate([FromQuery] string type = "serial", [FromQuery] string? productId = null)
        {
            string plan = User.FindFirst("plan")?.Value ?? "";
            string company = User.FindFirst("company")?.Value ?? "";
            string store = User.FindFirst("store")?.Value ?? "";

            GenerateSerialNumberDTO request = new()
            {
                Type = type,
                ProductId = productId,
                Company = company,
                Store = store,
                Plan = plan,
                CreatedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
            };

            ResponseApi<dynamic?> response = await service.GenerateAsync(request);
            return StatusCode(response.StatusCode, new { response.Result });
        }
    }
}
