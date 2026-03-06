using api_infor_cell.src.Models;
using MongoDB.Driver;
using Twilio.TwiML.Voice;

namespace api_infor_cell.src.Configuration
{
    public class AppDbContext
    {
        public static string? ConnectionString { get; set; }
        public static string? DatabaseName { get; set; }
        public static bool IsSSL { get; set; }
        private IMongoDatabase Database { get; }

        public AppDbContext()
        {
            try
            {
                MongoClientSettings mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
                if (IsSSL)
                {
                    mongoClientSettings.SslSettings = new SslSettings
                    {
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                    };
                }
                
                var mongoClient = new MongoClient(mongoClientSettings);
                Database = mongoClient.GetDatabase(DatabaseName);
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to connect to database. Error: {ex.Message}");
            }
        }

        #region MASTER DATA
        public IMongoCollection<Plan> Plans
        {
            get { return Database.GetCollection<Plan>("plans"); }
        }
        public IMongoCollection<Company> Companies
        {
            get { return Database.GetCollection<Company>("companies"); }
        }
        public IMongoCollection<User> Users
        {
            get { return Database.GetCollection<User>("users"); }
        }
        public IMongoCollection<GenericTable> GenericTables
        {
            get { return Database.GetCollection<GenericTable>("generic_tables"); }
        }
        public IMongoCollection<Address> Addresses
        {
            get { return Database.GetCollection<Address>("addresses"); }
        }
        public IMongoCollection<Contact> Contacts
        {
            get { return Database.GetCollection<Contact>("contacts"); }
        }
        public IMongoCollection<Attachment> Attachments
        {
            get { return Database.GetCollection<Attachment>("attachments"); }
        }
         public IMongoCollection<Supplier> Suppliers
        {
            get { return Database.GetCollection<Supplier>("suppliers"); }
        }
         public IMongoCollection<Store> Stores
        {
            get { return Database.GetCollection<Store>("stores"); }
        }
         public IMongoCollection<Brand> Brands   
        {
            get { return Database.GetCollection<Brand>("brands"); }
        }
        public IMongoCollection<Product> Products
        {
           get { return Database.GetCollection<Product>("products"); }
        }
        public IMongoCollection<Category> Categories
        {
            get { return Database.GetCollection<Category>("categories"); }
        }
        public IMongoCollection<Employee> Employees
        {
             get { return Database.GetCollection<Employee>("employees"); }
        }
        public IMongoCollection<Flag> Flags
        {
            get { return Database.GetCollection<Flag>("flags"); }
        }
        public IMongoCollection<Model> Models
        {
            get { return Database.GetCollection<Model>("groups"); }
        }
        public IMongoCollection<ServiceOrder> ServiceOrders
        {
            get { return Database.GetCollection<ServiceOrder>("service_orders"); }
        }
        public IMongoCollection<SalesOrder> SalesOrders
        {
            get { return Database.GetCollection<SalesOrder>("sales_orders"); }
        }
        public IMongoCollection<Stock> Stocks
        {
            get { return Database.GetCollection<Stock>("stock"); }
        }
        public IMongoCollection<Box> Boxes
        {
            get { return Database.GetCollection<Box>("boxes"); }
        }
        public IMongoCollection<ServiceOrderItem> ServiceOrderItems
        {
            get { return Database.GetCollection<ServiceOrderItem>("service_order_items"); }
        }
        public IMongoCollection<SalesOrderItem> SalesOrderItems
        {
            get { return Database.GetCollection<SalesOrderItem>("sales_order_items"); }
        }
        public IMongoCollection<Customer> Customers
        {
            get { return Database.GetCollection<Customer>("customers"); }
        }
        public IMongoCollection<Exchange> Exchanges
        {
            get { return Database.GetCollection<Exchange>("exchanges"); }
        }
        public IMongoCollection<PurchaseOrder> PurchaseOrders
        {
            get { return Database.GetCollection<PurchaseOrder>("purchase_orders"); }
        }
        public IMongoCollection<PurchaseOrderItem> PurchaseOrderItems
        {
            get { return Database.GetCollection<PurchaseOrderItem>("purchase_order_items"); }
        }
        public IMongoCollection<Transfer> Transfers
        {
            get { return Database.GetCollection<Transfer>("transfers"); }
        }       
        
        public IMongoCollection<Variation> Variations
        {
            get { return Database.GetCollection<Variation>("variations"); }
        }
        public IMongoCollection<ProfilePermission> ProfilePermissions
        {
            get { return Database.GetCollection<ProfilePermission>("profile_permissions"); }
        }       
        public IMongoCollection<Adjustment> Adjustments
        {
            get { return Database.GetCollection<Adjustment>("adjustments"); }
        }  
        public IMongoCollection<LogApi> LogApis
        {
            get { return Database.GetCollection<LogApi>("logs"); }
        }       
        public IMongoCollection<Situation> Situations
        {
            get { return Database.GetCollection<Situation>("situations"); }
        }       
        
        public IMongoCollection<Budget> Budgets => Database.GetCollection<Budget>("budgets");
        public IMongoCollection<BudgetItem> BudgetItems => Database.GetCollection<BudgetItem>("budget_items");
        #endregion

        #region FINANCIAL
        public IMongoCollection<PaymentMethod> PaymentMethods
        {
            get { return Database.GetCollection<PaymentMethod>("payment_methods"); }
        }
        public IMongoCollection<AccountReceivable> AccountsReceivable
        {
            get { return Database.GetCollection<AccountReceivable>("accounts_receivable"); }
        }     
        public IMongoCollection<AccountPayable> AccountsPayable
        {
            get { return Database.GetCollection<AccountPayable>("accounts_payable"); }
        }
        public IMongoCollection<ChartOfAccounts> ChartOfAccounts => Database.GetCollection<ChartOfAccounts>("chart_of_accounts");
        #endregion

        #region PLAN
        public IMongoCollection<Subscription> Subscriptions
        {
            get { return Database.GetCollection<Subscription>("subscriptions"); }
        }
        #endregion

        #region FISCAL
        public IMongoCollection<FiscalDocument> FiscalDocuments => Database.GetCollection<FiscalDocument>("fiscal_documents");

        public IMongoCollection<FiscalEvent> FiscalEvents => Database.GetCollection<FiscalEvent>("fiscal_events");

        public IMongoCollection<FiscalConfig> FiscalConfigs => Database.GetCollection<FiscalConfig>("fiscal_configs");

        #endregion
    }
}

