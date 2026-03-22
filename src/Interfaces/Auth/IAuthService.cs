using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Responses;
using api_infor_cell.src.Shared.DTOs;

namespace api_infor_cell.src.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseApi<AuthResponse>> LoginAsync(LoginDTO request);
        Task<ResponseApi<dynamic>> RegisterAsync(RegisterDTO request);
        Task<ResponseApi<dynamic>> ConfirmAccountAsync(ConfirmAccountDTO request);
        Task<ResponseApi<dynamic>> NewCodeConfirmAsync(NewCodeConfirmDTO request);
        Task<ResponseApi<AuthResponse>> RefreshTokenAsync(string token, string planId);
        Task<ResponseApi<User>> ResetPasswordAsync(ResetPasswordDTO request);
        Task<ResponseApi<User>> RequestForgotPasswordAsync(ForgotPasswordDTO request);
        Task<ResponseApi<User>> ResetPassordForgotAsync(ResetPasswordDTO request);
    }
}