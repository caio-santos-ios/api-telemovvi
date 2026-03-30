using System.Text;
using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Repository;
using api_infor_cell.src.Services;
using api_infor_cell.src.Shared.Validators;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace api_infor_cell.src.Configuration
{
    public static class Build
    {
        public static void AddBuilderConfiguration(this WebApplicationBuilder builder)
        {
            AppDbContext.ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? ""; 
            AppDbContext.DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? ""; 
            bool IsSSL;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IS_SSL")))
            {
                IsSSL = Convert.ToBoolean(Environment.GetEnvironmentVariable("IS_SSL"));
            }
            else
            {
                IsSSL = false;
            }

            AppDbContext.IsSSL = IsSSL;
        }
        public static void AddBuilderAuthentication(this WebApplicationBuilder builder)
        {
            string? SecretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ?? "";
            string? Issuer = Environment.GetEnvironmentVariable("ISSUER") ?? "";
            string? Audience = Environment.GetEnvironmentVariable("AUDIENCE") ?? "";
            
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(SecretKey)
                    )
                };
            });
        }
        public static void AddContext(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<AppDbContext>();
        }
        public static void AddBuilderServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IAuthService, AuthService>();                  
            
            // MASTER DATA
            builder.Services.AddTransient<IPlanService, PlanService>();
            builder.Services.AddTransient<IPlanRepository, PlanRepository>();                       
            builder.Services.AddTransient<ICompanyService, CompanyService>();
            builder.Services.AddTransient<ICompanyRepository, CompanyRepository>();                       
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();    
            builder.Services.AddTransient<IGenericTableService, GenericTableService>();
            builder.Services.AddTransient<IGenericTableRepository, GenericTableRepository>();                       
            builder.Services.AddTransient<IAddressService, AddressService>();
            builder.Services.AddTransient<IAddressRepository, AddressRepository>();                        
            builder.Services.AddTransient<IContactService, ContactService>();
            builder.Services.AddTransient<IContactRepository, ContactRepository>();                        
            builder.Services.AddTransient<IAttachmentService, AttachmentService>();
            builder.Services.AddTransient<IAttachmentRepository, AttachmentRepository>();                  
            builder.Services.AddTransient<ISupplierService, SupplierService>();
            builder.Services.AddTransient<ISupplierRepository, SupplierRepository>();
            builder.Services.AddTransient<IStoreService, StoreService>();
            builder.Services.AddTransient<IStoreRepository, StoreRepository>();
            builder.Services.AddTransient<IStoreService, StoreService>();
            builder.Services.AddTransient<IBrandRepository, BrandRepository>();
            builder.Services.AddTransient<IBrandService, BrandService>();
            builder.Services.AddTransient<IProductService, ProductService>();
            builder.Services.AddTransient<IProductRepository, ProductRepository>();
            builder.Services.AddTransient<ICategoryService, CategoryService>();
            builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
            builder.Services.AddTransient<IEmployeeService, EmployeeService>();
            builder.Services.AddTransient<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddTransient<IFlagService, FlagService>();
            builder.Services.AddTransient<IFlagRepository, FlagRepository>();
            builder.Services.AddTransient<IModelService, ModelService>();
            builder.Services.AddTransient<IModelRepository, ModelRepository>();
            builder.Services.AddTransient<IPaymentMethodService, PaymentMethodService>();
            builder.Services.AddTransient<IPaymentMethodRepository, PaymentMethodRepository>();
            builder.Services.AddTransient<IServiceOrderService, ServiceOrderService>();
            builder.Services.AddTransient<IServiceOrderRepository, ServiceOrderRepository>();
            builder.Services.AddTransient<ISalesOrderService, SalesOrderService>();
            builder.Services.AddTransient<ISalesOrderRepository, SalesOrderRepository>();
            builder.Services.AddTransient<IStockService, StockService>();
            builder.Services.AddTransient<IStockRepository, StockRepository>();
            builder.Services.AddTransient<IBoxService, BoxService>();
            builder.Services.AddTransient<IBoxRepository, BoxRepository>();
            builder.Services.AddTransient<IServiceOrderItemService, ServiceOrderItemService>();
            builder.Services.AddTransient<IServiceOrderItemRepository, ServiceOrderItemRepository>();
            builder.Services.AddTransient<ISalesOrderItemService, SalesOrderItemService>();
            builder.Services.AddTransient<ISalesOrderItemRepository, SalesOrderItemRepository>();
            builder.Services.AddTransient<ICustomerService, CustomerService>();
            builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
            builder.Services.AddTransient<IExchangeService, ExchangeService>();
            builder.Services.AddTransient<IExchangeRepository, ExchangeRepository>();

            builder.Services.AddTransient<IPurchaseOrderService, PurchaseOrderService>();
            builder.Services.AddTransient<IPurchaseOrderRepository, PurchaseOrderRepository>();

            builder.Services.AddTransient<IPurchaseOrderItemService, PurchaseOrderItemService>();
            builder.Services.AddTransient<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();

            builder.Services.AddTransient<ITransferService, TransferService>();
            builder.Services.AddTransient<ITransferRepository, TransferRepository>();

            builder.Services.AddTransient<IVariationService, VariationService>();
            builder.Services.AddTransient<IVariationRepository, VariationRepository>();

            builder.Services.AddTransient<IProfilePermissionService, ProfilePermissionService>();
            builder.Services.AddTransient<IProfilePermissionRepository, ProfilePermissionRepository>();
            
            builder.Services.AddTransient<IAdjustmentService, AdjustmentService>();
            builder.Services.AddTransient<IAdjustmentRepository, AdjustmentRepository>();
            
            builder.Services.AddTransient<ISituationService, SituationService>();
            builder.Services.AddTransient<ISituationRepository, SituationRepository>();
            
            builder.Services.AddTransient<ILogApiRepository, LogApiRepository>();

            builder.Services.AddTransient<IBudgetService, BudgetService>();
            builder.Services.AddTransient<IBudgetRepository, BudgetRepository>();
            builder.Services.AddTransient<IBudgetItemService, BudgetItemService>();
            builder.Services.AddTransient<IBudgetItemRepository, BudgetItemRepository>();

            // FINANCIAL
            builder.Services.AddTransient<IAccountReceivableService, AccountReceivableService>();
            builder.Services.AddTransient<IAccountReceivableRepository, AccountReceivableRepository>();
            builder.Services.AddTransient<IAccountPayableService, AccountPayableService>();
            builder.Services.AddTransient<IAccountPayableRepository, AccountPayableRepository>();
            builder.Services.AddScoped<IChartOfAccountsRepository, ChartOfAccountsRepository>();
            builder.Services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();
            builder.Services.AddScoped<IDreRepository, DreRepository>();
            builder.Services.AddScoped<IDreService, DreService>();
            
            // DASHBOARD
            builder.Services.AddTransient<IDashboardService, DashboardService>();
            builder.Services.AddTransient<IDashboardRepository, DashboardRepository>();


            // Subscription
            builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

            // ECOMMERCE
            builder.Services.AddTransient<IEcommerceService, EcommerceService>();

            // FISCAL
            builder.Services.AddTransient<IFiscalDocumentService, FiscalDocumentService>();
            builder.Services.AddTransient<IFiscalDocumentRepository, FiscalDocumentRepository>();
            builder.Services.AddTransient<ISefazProvider, SefazProvider>();

            // Handlers
            builder.Services.AddTransient<SmsHandler>();
            builder.Services.AddTransient<MailHandler>();
            builder.Services.AddTransient<CloudinaryHandler>();
            builder.Services.AddSingleton<AsaasHandler>();

            // Validator
            builder.Services.AddSingleton<ValidatorPlan>();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            Account account = new(
                Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            );
            builder.Services.AddSingleton(new Cloudinary(account));
        }
    }
}