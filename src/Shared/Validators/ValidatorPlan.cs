using api_infor_cell.src.Configuration;
using api_infor_cell.src.Models.Base;
using MongoDB.Driver;

namespace api_infor_cell.src.Shared.Validators;
public class ValidatorPlan(AppDbContext context)
{
    public async Task<ResponseApi<dynamic>> ValidatorConfigurationPlan(string planId, string planType, string collection)
    {
        string actualPlanType = planType;
        if (!string.IsNullOrEmpty(planId))
        {
            var planDb = await context.Plans.Find(x => x.Id == planId).FirstOrDefaultAsync();
            if (planDb != null && !string.IsNullOrEmpty(planDb.Type))
            {
                actualPlanType = planDb.Type;
            }
        }

        if("companies".Equals(collection))
        {
            long quantityCompanies = await context.Companies.Find(x => !x.Deleted && x.Plan == planId).CountDocumentsAsync();

            if(!VerifyQuantityCompanies(actualPlanType, quantityCompanies + 1)) return new(null, 400, $"Seu plano não permite ter {quantityCompanies + 1} Empresas.");
        }
        
        if("stores".Equals(collection))
        {
            long quantityStores = await context.Stores.Find(x => !x.Deleted && x.Plan == planId).CountDocumentsAsync();
            
            if(!VerifyQuantityStores(actualPlanType, quantityStores + 1)) return new(null, 400, $"Seu plano não permite ter {quantityStores + 1} Lojas.");
        }
        
        if("users".Equals(collection))
        {
            long quantityUsers = await context.Users.Find(x => !x.Deleted && x.Plan == planId && !x.Admin).CountDocumentsAsync();

            if(!VerifyQuantityUsers(actualPlanType, quantityUsers + 1)) return new(null, 400, $"Seu plano não permite ter {quantityUsers + 1} Usuários.");
        }
        
        return new(null, 200);
    }

    #region FUNCTIONS
    public static bool VerifyQuantityCompanies(string planType, long quantityCompanies)
    {
        switch(planType?.ToLower()?.Trim()) 
        {
            case "free":
                if(quantityCompanies > 1) return false;
                return true;

            case "bronze":
                if(quantityCompanies > 3) return false;
                return true;

            case "prata":
                if(quantityCompanies > 4) return false;
                return true;
                
            case "ouro":
                if(quantityCompanies > 5) return false;
                return true;

            case "platina":
                if(quantityCompanies > 6) return false;
                return true;
            
            default:
                return false;
        }
    }
    public static bool VerifyQuantityStores(string planType, long quantityStores)
    {
        switch(planType?.ToLower()?.Trim()) 
        {
            case "free":
                if(quantityStores > 1) return false;
                return true;

            case "bronze":
                if(quantityStores > 3) return false;
                return true;

            case "prata":
                if(quantityStores > 4) return false;
                return true;
                
            case "ouro":
                if(quantityStores > 5) return false;
                return true;

            case "platina":
                if(quantityStores > 6) return false;
                return true;
            
            default:
                return false;
        }
    }
    public static bool VerifyQuantityUsers(string planType, long quantityUsers)
    {
        switch(planType?.ToLower()?.Trim()) 
        {
            case "free":
                if(quantityUsers > 1) return false;
                return true;

            case "bronze":
                if(quantityUsers > 3) return false;
                return true;

            case "prata":
                if(quantityUsers > 4) return false;
                return true;
                
            case "ouro":
                if(quantityUsers > 5) return false;
                return true;

            case "platina":
                if(quantityUsers > 6) return false;
                return true;
            
            default:
                return false;
        }
    }
    #endregion
}