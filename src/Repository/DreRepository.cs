using api_infor_cell.src.Configuration;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api_infor_cell.src.Repository
{
    // Representa um grupo pai do DRE com seus filhos
    public record DreGrupoDef(string Key, string Label, bool IsParent, string? ParentKey = null);

    public class DreRepository(AppDbContext context) : IDreRepository
    {
        // ── Hierarquia exata dos grupos conforme o frontend ───────────────────────
        // Receitas
        private static readonly List<DreGrupoDef> _gruposReceita = new()
        {
            new("rec_bruta",       "Receita Bruta",                 true),
            new("rec_vendas",      "(+) Receitas de vendas",        false, "rec_bruta"),
            new("rec_fin",         "Receitas Financeiras",          true),
            new("rend_fin",        "(+) Rendimentos financeiros",   false, "rec_fin"),
            new("jur_rec",         "(+) Juros/multas recebidos",    false, "rec_fin"),
            new("desc_rec",        "(+) Descontos recebidos",       false, "rec_fin"),
            new("rec_outras",      "Outras Receitas",               true),
            new("outras_rec_item", "(+) Outras receitas",           false, "rec_outras"),
        };

        // Despesas / deduções
        private static readonly List<DreGrupoDef> _gruposDespesa = new()
        {
            new("deducoes",   "Deduções",                           true),
            new("imp_vendas", "(-) Impostos sobre vendas",          false, "deducoes"),
            new("com_vendas", "(-) Comissões sobre vendas",         false, "deducoes"),
            new("dev_vendas", "(-) Devolução de vendas",            false, "deducoes"),
            new("custos",     "Custos Operacionais",                true),
            new("cpv",        "(-) Custo dos produtos vendidos",    false, "custos"),
            new("desp_op",    "Despesas Operacionais",              true),
            new("desp_adm",   "(-) Despesas administrativas",       false, "desp_op"),
            new("desp_ger",   "(-) Despesas operacionais",          false, "desp_op"),
            new("desp_com",   "(-) Despesas comerciais",            false, "desp_op"),
            new("desp_fin",   "Despesas Financeiras",               true),
            new("emp_div",    "(-) Empréstimos e dívidas",          false, "desp_fin"),
            new("jur_mul",    "(-) Juros/multas pagos",             false, "desp_fin"),
            new("desc_conc",  "(-) Descontos concedidos",           false, "desp_fin"),
            new("tax_ban",    "(-) Taxas/tarifas bancárias",        false, "desp_fin"),
            new("outras",     "Outras Despesas",                    true),
            new("outras_esp", "(-) Outras despesas",                false, "outras"),
        };

        // Grupos cujos filhos somam como RECEITA nos totalizadores
        private static readonly HashSet<string> _gruposReceita_keys = new()
        {
            "rec_bruta", "rec_vendas",
            "rec_fin",   "rend_fin", "jur_rec", "desc_rec",
            "rec_outras","outras_rec_item"
        };

        // Grupos cujos filhos somam como DEDUÇÕES (abate da receita bruta)
        private static readonly HashSet<string> _gruposDeducao = new() { "deducoes", "imp_vendas", "com_vendas", "dev_vendas" };
        // Custos
        private static readonly HashSet<string> _gruposCusto   = new() { "custos",   "cpv" };
        // Despesas operacionais
        private static readonly HashSet<string> _gruposDespOp  = new() { "desp_op",  "desp_adm", "desp_ger", "desp_com" };
        // Despesas financeiras
        private static readonly HashSet<string> _gruposDespFin = new() { "desp_fin", "emp_div",  "jur_mul", "desc_conc", "tax_ban" };
        // Outras despesas
        private static readonly HashSet<string> _gruposOutraDesp = new() { "outras",  "outras_esp" };

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
                string dateField = regime == "caixa" ? "paymentDate" : "dueDate";

                // ── Contas do plano de contas com groupDRE preenchido ─────────────────
                var accountsFilter = Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.And(
                    Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Eq("plan",    planId),
                    Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Eq("company", companyId),
                    Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Eq("deleted", false),
                    Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Ne("groupDRE", ""),
                    Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Ne("groupDRE", BsonNull.Value),
                    Builders<global::api_infor_cell.src.Models.ChartOfAccounts>.Filter.Ne("groupDRE", "none")
                );

                var accounts = await context.ChartOfAccounts
                    .Find(accountsFilter).SortBy(a => a.Code).ToListAsync();

                // ── Meses do período ──────────────────────────────────────────────────
                var meses = new List<(DateTime Inicio, string Label)>();
                var cur = new DateTime(startDate.Year, startDate.Month, 1);
                var fimPeriodo = new DateTime(endDate.Year, endDate.Month,
                    DateTime.DaysInMonth(endDate.Year, endDate.Month), 23, 59, 59);

                while (cur <= fimPeriodo)
                {
                    meses.Add((cur, cur.ToString("MMM/yyyy")));
                    cur = cur.AddMonths(1);
                }

                var mesesLabels = meses.Select(m => m.Label).ToList();

                if (accounts.Count == 0)
                    return new(new
                    {
                        periodo = new { inicio = startDate.ToString("dd/MM/yyyy"), fim = endDate.ToString("dd/MM/yyyy"), regime },
                        meses   = mesesLabels,
                        secoes  = new { receitas = 0, despesas = 0},
                        totalizadores = new { },
                        indicadores   = new { margemBruta = 0, margemLiquida = 0 }
                    });

                // ── Pipeline de agregação por conta + mês ─────────────────────────────
                // BsonDocument BuildPipeline(BsonDocument matchExtra) => new BsonDocument();

                List<BsonDocument> AggPipeline(BsonDocument extraMatch) => new()
                {
                    new("$match", new BsonDocument(extraMatch)
                    {
                        { "deleted",  false },
                        { "plan",     planId },
                        { "company",  companyId },
                        { "store",    storeId },
                        { dateField,  new BsonDocument { { "$gte", startDate }, { "$lte", fimPeriodo } } }
                    }),
                    new("$group", new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "account", "$chartOfAccountId" },
                                { "year",    new BsonDocument("$year",  $"${dateField}") },
                                { "month",   new BsonDocument("$month", $"${dateField}") }
                            }
                        },
                        { "total", new BsonDocument("$sum", new BsonDocument("$toDouble", "$amount")) }
                    })
                };

                var receivablesRaw = await context.AccountsReceivable
                    .Aggregate<BsonDocument>(AggPipeline(new BsonDocument()))
                    .ToListAsync();

                var payablesRaw = await context.AccountsPayable
                    .Aggregate<BsonDocument>(AggPipeline(new BsonDocument()))
                    .ToListAsync();

                // ── Dicionário [accountId|yyyy-MM] = valor ────────────────────────────
                var valMap = new Dictionary<string, decimal>();

                void MapDocs(List<BsonDocument> docs)
                {
                    foreach (var doc in docs)
                    {
                        var id = doc["_id"].AsBsonDocument;
                        if (!id.Contains("account") || id["account"].IsBsonNull) continue;
                        string accId = id["account"].AsString;
                        int y = id["year"].AsInt32;
                        int m = id["month"].AsInt32;
                        decimal v = (decimal)doc["total"].ToDouble();
                        string key = $"{accId}|{y}-{m:D2}";
                        valMap[key] = valMap.GetValueOrDefault(key) + v;
                    }
                }

                MapDocs(receivablesRaw);
                MapDocs(payablesRaw);

                decimal GetVal(string accountId, DateTime mesInicio) =>
                    valMap.GetValueOrDefault($"{accountId}|{mesInicio:yyyy-MM}");

                // ── Agrupar contas por groupDRE ───────────────────────────────────────
                var contasPorGrupo = accounts
                    .GroupBy(a => (a.GroupDRE ?? "").Trim())
                    .ToDictionary(g => g.Key, g => g.ToList());

                // ── Helper: montar subgrupo filho ────────────────────────────────────
                Dictionary<string, decimal> MesDic() => meses.ToDictionary(m => m.Label, _ => 0m);

                object MontarSubGrupo(DreGrupoDef def)
                {
                    var linhas    = new List<object>();
                    var totalMes  = MesDic();

                    if (contasPorGrupo.TryGetValue(def.Key, out var contas))
                    {
                        foreach (var conta in contas)
                        {
                            var valMes = MesDic();
                            decimal tot = 0;
                            foreach (var (ini, label) in meses)
                            {
                                decimal v = GetVal(conta.Id, ini);
                                valMes[label] = v;
                                tot += v;
                                totalMes[label] += v;
                            }
                            linhas.Add(new { id = conta.Id, code = conta.Code, name = conta.Name, valoresMes = valMes, total = tot });
                        }
                    }

                    return new
                    {
                        key      = def.Key,
                        label    = def.Label,
                        isParent = false,
                        linhas,
                        totalMes,
                        total    = totalMes.Values.Sum()
                    };
                }

                // ── Helper: montar grupo pai com filhos ───────────────────────────────
                object MontarGrupoPai(DreGrupoDef pai, List<DreGrupoDef> filhos)
                {
                    var subGrupos  = filhos.Select(f => MontarSubGrupo(f)).ToList();
                    var totalMes   = MesDic();

                    // Total pai = soma de todos os filhos
                    foreach (dynamic sg in subGrupos)
                        foreach (var (_, label) in meses)
                            totalMes[label] += (decimal)sg.totalMes[label];

                    return new
                    {
                        key       = pai.Key,
                        label     = pai.Label,
                        isParent  = true,
                        subGrupos,
                        totalMes,
                        total     = totalMes.Values.Sum()
                    };
                }

                // ── Montar seções: Receitas e Despesas ────────────────────────────────
                // Agrupando pais com seus filhos
                object MontarSecao(List<DreGrupoDef> defs)
                {
                    var pais  = defs.Where(d => d.IsParent).ToList();
                    var items = new List<object>();

                    foreach (var pai in pais)
                    {
                        var filhos = defs.Where(d => d.ParentKey == pai.Key).ToList();
                        items.Add(MontarGrupoPai(pai, filhos));
                    }

                    return items;
                }

                var gruposReceitaResult  = (List<object>)MontarSecao(_gruposReceita);
                var gruposDespesaResult  = (List<object>)MontarSecao(_gruposDespesa);

                // ── Acumuladores para os totalizadores ────────────────────────────────
                var recBrutaMes      = MesDic();
                var recFinMes        = MesDic();
                var outrasRecMes     = MesDic();
                var deducoesMes      = MesDic();
                var custosMes        = MesDic();
                var despOpMes        = MesDic();
                var despFinMes       = MesDic();
                var outrasDespMes    = MesDic();

                // Somar filhos nos acumuladores corretos
                void AcumularFilhos(List<DreGrupoDef> filhos, Dictionary<string, decimal> acum)
                {
                    foreach (var f in filhos)
                    {
                        if (!contasPorGrupo.TryGetValue(f.Key, out var contas)) continue;
                        foreach (var conta in contas)
                            foreach (var (ini, label) in meses)
                                acum[label] += GetVal(conta.Id, ini);
                    }
                }

                AcumularFilhos(_gruposReceita.Where(d => d.ParentKey == "rec_bruta").ToList(),   recBrutaMes);
                AcumularFilhos(_gruposReceita.Where(d => d.ParentKey == "rec_fin").ToList(),     recFinMes);
                AcumularFilhos(_gruposReceita.Where(d => d.ParentKey == "rec_outras").ToList(),  outrasRecMes);
                AcumularFilhos(_gruposDespesa.Where(d => d.ParentKey == "deducoes").ToList(),    deducoesMes);
                AcumularFilhos(_gruposDespesa.Where(d => d.ParentKey == "custos").ToList(),      custosMes);
                AcumularFilhos(_gruposDespesa.Where(d => d.ParentKey == "desp_op").ToList(),     despOpMes);
                AcumularFilhos(_gruposDespesa.Where(d => d.ParentKey == "desp_fin").ToList(),    despFinMes);
                AcumularFilhos(_gruposDespesa.Where(d => d.ParentKey == "outras").ToList(),      outrasDespMes);

                // ── Calcular totalizadores por mês ────────────────────────────────────
                var receitaTotalMes  = MesDic(); // rec_bruta + rec_fin + outras_rec
                var receitaLiqMes    = MesDic(); // receitaTotal - deducoes
                var lucroBrutoMes    = MesDic(); // receitaLiq - custos
                var resultOpMes      = MesDic(); // lucroBruto - despOp
                var resultFinMes     = MesDic(); // resultOp - despFin
                var resultAntesMes   = MesDic(); // resultFin - outrasDespesas + outrasReceitas  (EBT)
                var lucroLiquidoMes  = MesDic(); // resultAntes (sem IR/CS neste modelo)

                foreach (var (_, label) in meses)
                {
                    receitaTotalMes[label] = recBrutaMes[label] + recFinMes[label] + outrasRecMes[label];
                    receitaLiqMes[label]   = recBrutaMes[label] - deducoesMes[label];
                    lucroBrutoMes[label]   = receitaLiqMes[label] - custosMes[label];
                    resultOpMes[label]     = lucroBrutoMes[label] - despOpMes[label];
                    resultFinMes[label]    = resultOpMes[label]   - despFinMes[label];
                    resultAntesMes[label]  = resultFinMes[label]  - outrasDespMes[label] + outrasRecMes[label] + recFinMes[label];
                    lucroLiquidoMes[label] = resultAntesMes[label];
                }

                decimal TotSum(Dictionary<string, decimal> d) => d.Values.Sum();

                decimal totalRecBruta  = TotSum(recBrutaMes);
                decimal totalLucroBruto = TotSum(lucroBrutoMes);
                decimal totalLucroLiq   = TotSum(lucroLiquidoMes);

                dynamic result = new
                {
                    periodo = new
                    {
                        inicio = startDate.ToString("dd/MM/yyyy"),
                        fim    = endDate.ToString("dd/MM/yyyy"),
                        regime
                    },
                    meses = mesesLabels,
                    secoes = new
                    {
                        receitas  = gruposReceitaResult,
                        despesas  = gruposDespesaResult
                    },
                    totalizadores = new
                    {
                        receitaBruta   = new { mes = recBrutaMes,      total = totalRecBruta },
                        deducoes       = new { mes = deducoesMes,       total = TotSum(deducoesMes) },
                        receitaLiquida = new { mes = receitaLiqMes,     total = TotSum(receitaLiqMes) },
                        custos         = new { mes = custosMes,         total = TotSum(custosMes) },
                        lucroBruto     = new { mes = lucroBrutoMes,     total = totalLucroBruto },
                        despesasOp     = new { mes = despOpMes,         total = TotSum(despOpMes) },
                        resultadoOp    = new { mes = resultOpMes,       total = TotSum(resultOpMes) },
                        despesasFin    = new { mes = despFinMes,        total = TotSum(despFinMes) },
                        resultadoFin   = new { mes = resultFinMes,      total = TotSum(resultFinMes) },
                        outrasDespesas = new { mes = outrasDespMes,     total = TotSum(outrasDespMes) },
                        outrasReceitas = new { mes = outrasRecMes,      total = TotSum(outrasRecMes) },
                        receitasFinanc = new { mes = recFinMes,         total = TotSum(recFinMes) },
                        lucroLiquido   = new { mes = lucroLiquidoMes,   total = totalLucroLiq }
                    },
                    indicadores = new
                    {
                        margemBruta   = totalRecBruta > 0 ? Math.Round(totalLucroBruto / totalRecBruta * 100, 2) : 0m,
                        margemLiquida = totalRecBruta > 0 ? Math.Round(totalLucroLiq   / totalRecBruta * 100, 2) : 0m
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