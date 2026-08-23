using System.Security.Cryptography;
using System.Text;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;

namespace api_infor_cell.src.Services
{
    public class SerialNumberService(ISerialNumberRepository repository) : ISerialNumberService
    {
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<SerialNumber> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> serialNumbers = await repository.GetAllAsync(pagination);
                return new(serialNumbers.Data, 0, pagination.PageNumber, pagination.PageSize);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<dynamic?>> GenerateAsync(GenerateSerialNumberDTO request)
        {
            try
            {
                string type = string.IsNullOrEmpty(request.Type) ? "serial" : request.Type.ToLower();
                string code = "";
                bool isUnique = false;
                int attempts = 0;

                while (!isUnique && attempts < 50)
                {
                    attempts++;
                    code = type == "barcode" ? GenerateRandomBarcode() : GenerateRandomSerial();
                    bool exists = await repository.ExistsCodeAsync(request.Plan, request.Company, code);
                    if (!exists) isUnique = true;
                }

                SerialNumber serialNumber = new()
                {
                    Code = code,
                    Type = type,
                    ProductId = request.ProductId ?? "",
                    Company = request.Company,
                    Store = request.Store,
                    Plan = request.Plan,
                    CreatedBy = request.CreatedBy,
                    UpdatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Active = true,
                    Deleted = false
                };

                ResponseApi<SerialNumber?> created = await repository.CreateAsync(serialNumber);
                if (!created.IsSuccess || created.Data is null)
                {
                    return new(null, 400, "Falha ao gerar código.");
                }

                return new(new { code = serialNumber.Code, type = serialNumber.Type, id = serialNumber.Id }, 200, "Código gerado com sucesso.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        private static string GenerateRandomSerial()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var stringChars = new char[12];
            var randomBytes = new byte[12];
            RandomNumberGenerator.Fill(randomBytes);

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[randomBytes[i] % chars.Length];
            }

            return new string(stringChars);
        }

        private static string GenerateRandomBarcode()
        {
            var randomBytes = new byte[12];
            RandomNumberGenerator.Fill(randomBytes);
            StringBuilder sb = new("789");
            for (int i = 0; i < 9; i++)
            {
                sb.Append(randomBytes[i] % 10);
            }

            string first12 = sb.ToString();
            int checkDigit = CalculateEan13CheckDigit(first12);
            return $"{first12}{checkDigit}";
        }

        private static int CalculateEan13CheckDigit(string digits)
        {
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int d = digits[i] - '0';
                sum += (i % 2 == 0) ? d : d * 3;
            }
            int mod = sum % 10;
            return (mod == 0) ? 0 : 10 - mod;
        }
    }
}
