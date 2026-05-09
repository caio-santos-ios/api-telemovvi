using api_infor_cell.src.Models;

namespace api_infor_cell.src.Responses
{
    public class AuthResponse
    {
        public string Token {get;set;} = string.Empty; 
        public string RefreshToken  {get;set;} = string.Empty; 
        public string Name {get;set;} = string.Empty; 
        public string Email {get;set;} = string.Empty; 
        public bool Admin {get; set;}
        public bool Master {get; set;}
        public string Id {get;set;} = string.Empty; 
        public string Photo {get;set;} = string.Empty; 
        public string LogoCompany {get;set;} = string.Empty; 
        public string NameCompany {get;set;} = string.Empty; 
        public string NameStore {get;set;} = string.Empty; 
        public string Plan {get;set;} = string.Empty; 
        public string TypeUser {get;set;} = string.Empty; 
        public string TypePlan {get;set;} = string.Empty; 
        public bool SubscriberPlan {get;set;} = false;
        public DateTime ExpirationDate {get;set;} = DateTime.UtcNow;
        public List<Module> Modules {get;set;} = [];
        public List<Company> Companies {get;set;} = [];
    }
}