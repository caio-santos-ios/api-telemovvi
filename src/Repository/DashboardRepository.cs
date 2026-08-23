using api_infor_cell.src.Configuration;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api_infor_cell.src.Repository
{
    public class DashboardRepository(AppDbContext context) : IDashboardRepository
    {
        public async Task<ResponseApi<dynamic>> GetCardsAsync(string plan, string company, string store)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime startOfMonth = new(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime endOfMonth = startOfMonth.AddMonths(1);
                DateTime startOfPrevMonth = startOfMonth.AddMonths(-1);

                var filterSalesMonth = Builders<SalesOrder>.Filter.Gte(x => x.CreatedAt, startOfMonth) &
                                       Builders<SalesOrder>.Filter.Lt(x => x.CreatedAt, endOfMonth) &
                                       Builders<SalesOrder>.Filter.Eq(x => x.Plan, plan) &
                                       Builders<SalesOrder>.Filter.Eq(x => x.Company, company) &
                                       Builders<SalesOrder>.Filter.Eq(x => x.Deleted, false);

                if (store != "all") filterSalesMonth &= Builders<SalesOrder>.Filter.Eq(x => x.Store, store);

                var filterSalesPrevMonth = Builders<SalesOrder>.Filter.Gte(x => x.CreatedAt, startOfPrevMonth) &
                                           Builders<SalesOrder>.Filter.Lt(x => x.CreatedAt, startOfMonth) &
                                           Builders<SalesOrder>.Filter.Eq(x => x.Plan, plan) &
                                           Builders<SalesOrder>.Filter.Eq(x => x.Company, company) &
                                           Builders<SalesOrder>.Filter.Eq(x => x.Deleted, false);

                if (store != "all") filterSalesPrevMonth &= Builders<SalesOrder>.Filter.Eq(x => x.Store, store);

                var salesMonth = await context.SalesOrders.Find(filterSalesMonth).Project(x => new { x.Total, x.Status }).ToListAsync();
                var salesPrevMonth = await context.SalesOrders.Find(filterSalesPrevMonth).Project(x => new { x.Total }).ToListAsync();
                decimal totalSales = salesMonth.Sum(s => s.Total);
                decimal prevTotalSales = salesPrevMonth.Sum(s => s.Total);

                var filterStock = Builders<Product>.Filter.Eq(x => x.Plan, plan) &
                                  Builders<Product>.Filter.Eq(x => x.Company, company) &
                                  Builders<Product>.Filter.Eq(x => x.Deleted, false);

                var stockData = await context.Products.Find(filterStock).Project(x => new { x.PriceTotal, x.QuantityStock }).ToListAsync();

                var filterCustMonth = Builders<Customer>.Filter.Gte(x => x.CreatedAt, startOfMonth) &
                                      Builders<Customer>.Filter.Lt(x => x.CreatedAt, endOfMonth) &
                                      Builders<Customer>.Filter.Eq(x => x.Deleted, false) &
                                      Builders<Customer>.Filter.Eq(x => x.Plan, plan) &
                                      Builders<Customer>.Filter.Eq(x => x.Company, company);

                if (store != "all") filterCustMonth &= Builders<Customer>.Filter.Eq(x => x.Store, store);

                var custMonth = await context.Customers.CountDocumentsAsync(filterCustMonth);

                var filterPrevMonth = Builders<Customer>.Filter.Gte(x => x.CreatedAt, startOfPrevMonth) &
                                      Builders<Customer>.Filter.Lt(x => x.CreatedAt, startOfMonth) &
                                      Builders<Customer>.Filter.Eq(x => x.Deleted, false) &
                                      Builders<Customer>.Filter.Eq(x => x.Plan, plan) &
                                      Builders<Customer>.Filter.Eq(x => x.Company, company);

                if (store != "all") filterPrevMonth &= Builders<Customer>.Filter.Eq(x => x.Store, store);

                var custPrevMonth = await context.Customers.CountDocumentsAsync(filterPrevMonth);

                var filterReceivable = Builders<AccountReceivable>.Filter.Eq(x => x.Deleted, false) &
                                       Builders<AccountReceivable>.Filter.Eq(x => x.Plan, plan) &
                                       Builders<AccountReceivable>.Filter.Eq(x => x.Company, company);

                if (store != "all") filterReceivable &= Builders<AccountReceivable>.Filter.Eq(x => x.Store, store);

                List<AccountReceivable> accountsReceivable = await context.AccountsReceivable.Find(filterReceivable).ToListAsync();

                var filterPayable = Builders<AccountPayable>.Filter.Eq(x => x.Deleted, false) &
                                    Builders<AccountPayable>.Filter.Eq(x => x.Plan, plan) &
                                    Builders<AccountPayable>.Filter.Eq(x => x.Company, company);

                if (store != "all") filterPayable &= Builders<AccountPayable>.Filter.Eq(x => x.Store, store);

                List<AccountPayable> accountsPayable = await context.AccountsPayable.Find(filterPayable).ToListAsync();

                DateTime todayDate = new(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

                dynamic obj = new
                {
                    sales = new
                    {
                        totalMonth = (double)totalSales,
                        countMonth = salesMonth.Count,
                        growthPercent = CalculateGrowth(totalSales, prevTotalSales),
                        openOrders = salesMonth.Count(x => x.Status != "Finalizado")
                    },
                    stock = new
                    {
                        totalValue = (double)stockData.Sum(x => x.PriceTotal),
                        totalItems = (int)stockData.Sum(x => x.QuantityStock)
                    },
                    customers = new
                    {
                        countMonth = (int)custMonth,
                        growthPercent = CalculateGrowth(custMonth, custPrevMonth)
                    },
                    accountsReceivable = new
                    {
                        openAmount = (double)accountsReceivable.Where(x => x.Status == "open").Sum(x => x.Amount),
                        openCount = accountsReceivable.Count(x => x.Status == "open"),
                        overdueAmount = (double)accountsReceivable.Where(x => x.DueDate.Date < todayDate && x.Status == "open").Sum(x => x.Amount),
                        overdueCount = accountsReceivable.Count(x => x.DueDate.Date < todayDate && x.Status == "open"),
                        totalAmount = (double)accountsReceivable.Sum(x => x.Amount),
                        totalCount = accountsReceivable.Count
                    },
                    accountsPayable = new
                    {
                        openAmount = (double)accountsPayable.Where(x => x.Status == "open").Sum(x => x.Amount),
                        openCount = accountsPayable.Count(x => x.Status == "open"),
                        overdueAmount = (double)accountsPayable.Where(x => x.DueDate.Date < todayDate && x.Status == "open").Sum(x => x.Amount),
                        overdueCount = accountsPayable.Count(x => x.DueDate.Date < todayDate && x.Status == "open"),
                        totalAmount = (double)accountsPayable.Sum(x => x.Amount),
                        totalCount = accountsPayable.Count
                    }
                };

                return new ResponseApi<dynamic>(obj);
            }
            catch
            {
                return new ResponseApi<dynamic>(null, 500, "Erro ao carregar cards.");
            }
        }

        public async Task<ResponseApi<dynamic>> GetRecentOrdersAsync(string plan, string company, string store)
        {
            try
            {
                var filter = Builders<SalesOrder>.Filter.Eq(x => x.Deleted, false) &
                             Builders<SalesOrder>.Filter.Eq(x => x.Plan, plan) &
                             Builders<SalesOrder>.Filter.Eq(x => x.Company, company);

                if (store != "all") filter &= Builders<SalesOrder>.Filter.Eq(x => x.Store, store);

                var recent = await context.SalesOrders.Find(filter)
                    .SortByDescending(x => x.CreatedAt)
                    .Limit(5)
                    .ToListAsync();

                var customerIds = recent.Select(x => x.CustomerId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                var sellerIds = recent.Select(x => x.SellerId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

                var customers = await context.Customers.Find(x => customerIds.Contains(x.Id)).ToListAsync();
                var users = await context.Users.Find(x => sellerIds.Contains(x.Id)).ToListAsync();
                var employees = await context.Employees.Find(x => sellerIds.Contains(x.Id)).ToListAsync();
                var employeeUserIds = employees.Select(e => e.UserId).Where(uid => !string.IsNullOrEmpty(uid)).ToList();
                var employeeUsers = employeeUserIds.Count > 0 ? await context.Users.Find(x => employeeUserIds.Contains(x.Id)).ToListAsync() : [];

                var result = recent.Select(x =>
                {
                    string custName = customers.FirstOrDefault(c => c.Id == x.CustomerId)?.TradeName ?? "Ao consumidor";
                    var user = users.FirstOrDefault(u => u.Id == x.SellerId);
                    var emp = employees.FirstOrDefault(e => e.Id == x.SellerId);
                    var empUser = emp is not null ? employeeUsers.FirstOrDefault(u => u.Id == emp.UserId) : null;
                    string sellerName = user?.Name ?? empUser?.Name ?? "Vendedor";

                    return new
                    {
                        id = x.Id,
                        code = x.Code,
                        customerName = custName,
                        sellerName = sellerName,
                        total = (double)x.Total,
                        status = x.Status,
                        createdAt = x.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                }).ToList();

                return new ResponseApi<dynamic>(result);
            }
            catch
            {
                return new ResponseApi<dynamic>(null, 500, "Erro ao carregar pedidos recentes.");
            }
        }

        public async Task<ResponseApi<dynamic>> GetMonthlySalesAsync(string plan, string company, string store)
        {
            try
            {
                int year = DateTime.UtcNow.Year;
                DateTime startOfYear = new(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime endOfYear = new(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                var totals = new double[12];
                var counts = new int[12];

                var filter = Builders<SalesOrder>.Filter.Gte(x => x.CreatedAt, startOfYear) &
                             Builders<SalesOrder>.Filter.Lt(x => x.CreatedAt, endOfYear) &
                             Builders<SalesOrder>.Filter.Eq(x => x.Deleted, false) &
                             Builders<SalesOrder>.Filter.Eq(x => x.Plan, plan) &
                             Builders<SalesOrder>.Filter.Eq(x => x.Company, company);

                if (store != "all") filter &= Builders<SalesOrder>.Filter.Eq(x => x.Store, store);

                var salesYear = await context.SalesOrders.Find(filter).Project(x => new { x.Total, x.CreatedAt }).ToListAsync();

                for (int i = 1; i <= 12; i++)
                {
                    var monthSales = salesYear.Where(s => s.CreatedAt.Month == i).ToList();
                    totals[i - 1] = (double)monthSales.Sum(s => s.Total);
                    counts[i - 1] = monthSales.Count;
                }

                return new ResponseApi<dynamic>(new { totals, counts });
            }
            catch
            {
                return new ResponseApi<dynamic>(null, 500, "Erro ao carregar vendas mensais.");
            }
        }

        public async Task<ResponseApi<dynamic>> GetMonthlyTargetAsync(string plan, string company, string store)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime startM = new(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime endM = startM.AddMonths(1);
                DateTime startP = startM.AddMonths(-1);
                DateTime startToday = new(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

                var filterCur = Builders<SalesOrder>.Filter.Gte(x => x.CreatedAt, startM) &
                                Builders<SalesOrder>.Filter.Lt(x => x.CreatedAt, endM) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Deleted, false) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Plan, plan) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Company, company);

                if (store != "all") filterCur &= Builders<SalesOrder>.Filter.Eq(x => x.Store, store);

                var curS = await context.SalesOrders.Find(filterCur).Project(x => x.Total).ToListAsync();

                var filterPre = Builders<SalesOrder>.Filter.Gte(x => x.CreatedAt, startP) &
                                Builders<SalesOrder>.Filter.Lt(x => x.CreatedAt, startM) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Deleted, false) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Plan, plan) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Company, company);

                if (store != "all") filterPre &= Builders<SalesOrder>.Filter.Eq(x => x.Store, store);

                var preS = await context.SalesOrders.Find(filterPre).Project(x => x.Total).ToListAsync();

                var filterTod = Builders<SalesOrder>.Filter.Gte(x => x.CreatedAt, startToday) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Deleted, false) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Plan, plan) &
                                Builders<SalesOrder>.Filter.Eq(x => x.Company, company);

                if (store != "all") filterTod &= Builders<SalesOrder>.Filter.Eq(x => x.Store, store);

                var todS = await context.SalesOrders.Find(filterTod).Project(x => x.Total).ToListAsync();

                decimal cur = curS.Sum();
                decimal pre = preS.Sum();
                decimal tod = todS.Sum();

                return new ResponseApi<dynamic>(new
                {
                    currentMonth = (double)cur,
                    previousMonth = (double)pre,
                    today = (double)tod,
                    growthPercent = CalculateGrowth(cur, pre)
                });
            }
            catch
            {
                return new ResponseApi<dynamic>(null, 500, "Erro ao carregar metas.");
            }
        }

        private static double CalculateGrowth(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return (double)Math.Round(((current - previous) / previous) * 100, 2);
        }
    }
}