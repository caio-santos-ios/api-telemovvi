using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;

namespace api_infor_cell.src.Interfaces
{
    public interface IEcommerceService
    {
        // Config
        Task<ResponseApi<EcommerceConfig?>> GetConfigAsync(string plan, string company, string store);
        Task<ResponseApi<EcommerceConfig?>> SaveConfigAsync(SaveEcommerceConfigDTO request);

        // Produtos públicos
        Task<ResponseApi<List<dynamic>>> GetPublicCategoriesAsync(string plan, string company, string store);
        Task<ResponseApi<List<dynamic>>> GetPublicProductsAsync(string plan, string company, string store, string? search, string? categoryId, string? subcategory = null);

        // Clientes da loja
        Task<ResponseApi<dynamic>> RegisterCustomerAsync(EcommerceRegisterDTO request);
        Task<ResponseApi<dynamic>> LoginCustomerAsync(EcommerceLoginDTO request);

        // Pedidos
        Task<ResponseApi<EcommerceOrder?>> CheckoutAsync(EcommerceCheckoutDTO request);
        Task<ResponseApi<dynamic>> GetOrderByIdAsync(string orderId, string customerId);
        Task<ResponseApi<string>> HandlePaymentWebhookAsync(string paymentId, string status);
    }
}
