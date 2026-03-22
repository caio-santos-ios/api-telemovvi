using api_infor_cell.src.Configuration;
using api_infor_cell.src.Models.Base;
using MongoDB.Driver;

namespace api_infor_cell.src.Shared.Validators;
public class ValidatorPlan(AppDbContext context)
{
    public async Task<ResponseApi<dynamic>> ValidatorConfigurationPlan(string planId, string planType)
    {
        long quantityCompanies = await context.Companies.Find(x => !x.Deleted && x.Plan == planId).CountDocumentsAsync();

        if(!VerifyQuantityCompanies(planType, quantityCompanies + 1)) return new(null, 400, $"Seu plano não permite ter {quantityCompanies + 1} Empresas.");
        
        long quantityStores = await context.Stores.Find(x => !x.Deleted && x.Plan == planId).CountDocumentsAsync();
        
        if(!VerifyQuantityStores(planType, quantityStores + 1)) return new(null, 400, $"Seu plano não permite ter {quantityStores + 1} Lojas.");
        
        // FIX 7: excluir o admin titular da contagem de colaboradores para nao bloquear antes do limite real
        long quantityUsers = await context.Users.Find(x => !x.Deleted && x.Plan == planId && !x.Admin).CountDocumentsAsync();

        if(!VerifyQuantityUsers(planType, quantityUsers + 1)) return new(null, 400, $"Seu plano não permite ter {quantityUsers + 1} Usuários.");
        
        return new(null, 200);
    }

    #region FUNCTIONS
    public static bool VerifyQuantityCompanies(string planType, long quantityCompanies)
    {
        switch(planType) 
        {
            case "free":
                if(quantityCompanies > 1) return false;
                return true;

            case "Bronze":
                if(quantityCompanies > 3) return false;
                return true;

            case "Prata":
                if(quantityCompanies > 4) return false;
                return true;
                
            case "Ouro":
                if(quantityCompanies > 5) return false;
                return true;

            case "Platina":
                if(quantityCompanies > 6) return false;
                return true;
            
            default:
                return false;
        }
    }
    public static bool VerifyQuantityStores(string planType, long quantityStores)
    {
        switch(planType) 
        {
            case "free":
                if(quantityStores > 1) return false;
                return true;

            case "Bronze":
                if(quantityStores > 3) return false;
                return true;

            case "Prata":
                if(quantityStores > 4) return false;
                return true;
                
            case "Ouro":
                if(quantityStores > 5) return false;
                return true;

            case "Platina":
                if(quantityStores > 6) return false;
                return true;
            
            default:
                return false;
        }
    }
    public static bool VerifyQuantityUsers(string planType, long quantityUsers)
    {
        switch(planType) 
        {
            case "free":
                if(quantityUsers > 1) return false;
                return true;

            case "Bronze":
                if(quantityUsers > 3) return false;
                return true;

            case "Prata":
                if(quantityUsers > 4) return false;
                return true;
                
            case "Ouro":
                if(quantityUsers > 5) return false;
                return true;

            case "Platina":
                if(quantityUsers > 6) return false;
                return true;
            
            default:
                return false;
        }
    }
    #endregion
} 