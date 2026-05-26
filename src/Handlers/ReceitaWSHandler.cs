using System.Text.Json;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.Utils;

namespace api_telemovvi.src.Handlers
{
    public class ReceitaWSHandler
    {
        private HttpClient httpClient = new();
        public async Task<ResponseApi<Address?>> GetAddressByCNPJ(string cnpj, string parent)
        {
            try
            {
                cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

                HttpRequestMessage request = new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://receitaws.com.br/v1/cnpj/{cnpj}"),
                    Headers =
                    {
                        { "Accept", "application/json" },
                    },
                };

                Address address = new();

                using (var response = await httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    string body = await response.Content.ReadAsStringAsync();

                    Dictionary<string, dynamic>? obj = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(body);

                    if(obj is not null)
                    {

                        if(obj.ContainsKey("logradouro")) address.Street = obj["logradouro"].ToString();
                        if(obj.ContainsKey("municipio")) address.City = obj["municipio"].ToString();
                        if(obj.ContainsKey("bairro")) address.Neighborhood = obj["bairro"].ToString();
                        if(obj.ContainsKey("uf")) address.State = obj["uf"].ToString();
                        if(obj.ContainsKey("numero")) address.Number = obj["numero"].ToString();
                    }
                }

                return new(address);
            }
            catch
            {
                return new(null, 500, "Falha ao buscar endereço");
            }
        }
    }
}