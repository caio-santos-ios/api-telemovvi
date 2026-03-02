using api_infor_cell.src.Configuration;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api_infor_cell.src.Repository
{
    public class DreRepository(AppDbContext context) : IDreRepository
    {
        public async Task<ResponseApi<dynamic>> GenerateAsync(
            string planId,
            string companyId,
            string storeId,
            DateTime startDate,
            DateTime endDate,
            string regime)
        {
            try
            {
                // Define campo de data conforme regime
                string dateField = regime == "caixa" ? "paymentDate" : "dueDate";

                // ── Buscar contas do plano ────────────────────────────────────
                FilterDefinition<global::api_infor_cell.src.Models.ChartOfAccounts> accountsFilter = 
                    Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.And(
                        Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Eq("plan", planId),
                        Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Eq("company", companyId),
                        Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Eq("deleted", false),
                        Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Eq("showInDre", true)
                    );

                List<global::api_infor_cell.src.Models.ChartOfAccounts> accounts = 
                    await context.ChartOfAccounts.Find(accountsFilter).ToListAsync();

                var accountsDict = accounts.ToDictionary(a => a.Id);

                // ── Buscar contas a receber (receitas) ────────────────────────
                List<BsonDocument> receivablesPipeline = new()
                {
                    new("$match", new BsonDocument
                    {
                        { "deleted",  false },
                        { "plan",     planId },
                        { "company",  companyId },
                        { "store",    storeId },
                        { dateField,  new BsonDocument { { "$gte", startDate }, { "$lte", endDate } } }
                    }),
                    new("$group", new BsonDocument
                    {
                        { "_id",   "$chartOfAccountsId" },
                        { "total", new BsonDocument("$sum", new BsonDocument("$toDouble", "$amount")) }
                    })
                };

                List<BsonDocument> receivables = await context.AccountsReceivable
                    .Aggregate<BsonDocument>(receivablesPipeline)
                    .ToListAsync();

                // ── Buscar contas a pagar (despesas) ──────────────────────────
                List<BsonDocument> payablesPipeline = new()
                {
                    new("$match", new BsonDocument
                    {
                        { "deleted",  false },
                        { "plan",     planId },
                        { "company",  companyId },
                        { "store",    storeId },
                        { dateField,  new BsonDocument { { "$gte", startDate }, { "$lte", endDate } } }
                    }),
                    new("$group", new BsonDocument
                    {
                        { "_id",   "$chartOfAccountsId" },
                        { "total", new BsonDocument("$sum", new BsonDocument("$toDouble", "$amount")) }
                    })
                };

                List<BsonDocument> payables = await context.AccountsPayable
                    .Aggregate<BsonDocument>(payablesPipeline)
                    .ToListAsync();

                // ── Mapear valores por conta ──────────────────────────────────
                var accountValues = new Dictionary<string, decimal>();

                foreach (var doc in receivables)
                {
                    string accountId = doc["_id"].AsString;
                    decimal value = (decimal)doc["total"].ToDouble();
                    accountValues[accountId] = value;
                }

                foreach (var doc in payables)
                {
                    string accountId = doc["_id"].AsString;
                    decimal value = (decimal)doc["total"].ToDouble();
                    accountValues[accountId] = value;
                }

                // ── Agrupar por categoria DRE ─────────────────────────────────
                var categories = new Dictionary<string, decimal>
                {
                    { "receita_bruta",             0 },
                    { "deducoes",                  0 },
                    { "cmv",                       0 },
                    { "despesas_administrativas",  0 },
                    { "despesas_comerciais",       0 },
                    { "despesas_financeiras",      0 },
                    { "impostos",                  0 }
                };

                foreach (var account in accounts)
                {
                    // if (accountValues.TryGetValue(account.Id, out decimal value) && !string.IsNullOrEmpty(account.DreCategory))
                    // {
                    //     if (categories.ContainsKey(account.DreCategory))
                    //     {
                    //         categories[account.DreCategory] += value;
                    //     }
                    // }
                }

                // ── Calcular DRE ──────────────────────────────────────────────
                decimal receitaBruta      = categories["receita_bruta"];
                decimal deducoes          = categories["deducoes"];
                decimal receitaLiquida    = receitaBruta - deducoes;
                decimal cmv               = categories["cmv"];
                decimal lucroBruto        = receitaLiquida - cmv;
                decimal despesasAdm       = categories["despesas_administrativas"];
                decimal despesasComercial = categories["despesas_comerciais"];
                decimal despesasFinanc    = categories["despesas_financeiras"];
                decimal despesasOper      = despesasAdm + despesasComercial + despesasFinanc;
                decimal resultadoOper     = lucroBruto - despesasOper; // EBITDA
                decimal impostos          = categories["impostos"];
                decimal lucroLiquido      = resultadoOper - impostos;

                decimal margemBruta       = receitaBruta > 0 ? (lucroBruto / receitaBruta) * 100 : 0;
                decimal margemLiquida     = receitaBruta > 0 ? (lucroLiquido / receitaBruta) * 100 : 0;

                dynamic result = new
                {
                    periodo = new
                    {
                        inicio = startDate.ToString("dd/MM/yyyy"),
                        fim    = endDate.ToString("dd/MM/yyyy"),
                        regime
                    },
                    valores = new
                    {
                        receitaBruta,
                        deducoes,
                        receitaLiquida,
                        cmv,
                        lucroBruto,
                        despesasOperacionais = new
                        {
                            administrativas = despesasAdm,
                            comerciais      = despesasComercial,
                            financeiras     = despesasFinanc,
                            total           = despesasOper
                        },
                        resultadoOperacional = resultadoOper,
                        impostos,
                        lucroLiquido
                    },
                    indicadores = new
                    {
                        margemBruta    = Math.Round(margemBruta, 2),
                        margemLiquida  = Math.Round(margemLiquida, 2)
                    }
                };

                return new(result);
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao gerar DRE: {ex.Message}");
            }
        }
    }
}