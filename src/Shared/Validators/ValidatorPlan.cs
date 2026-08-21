using api_infor_cell.src.Configuration;
using api_infor_cell.src.Models.Base;
using MongoDB.Driver;

namespace api_infor_cell.src.Shared.Validators;
public class ValidatorPlan(AppDbContext context)
{
    public async Task<ResponseApi<dynamic>> ValidatorConfigurationPlan(string planId, string planType, string collection)
    {
        if (string.IsNullOrEmpty(planId)) return new(null, 200);

        string actualPlanType = planType;
        var planDb = await context.Plans.Find(x => x.Id == planId).FirstOrDefaultAsync();
        if (planDb != null && !string.IsNullOrEmpty(planDb.Type))
        {
            actualPlanType = planDb.Type;
        }

        if (IsUnlimitedPlan(actualPlanType)) return new(null, 200);

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
    private static bool IsUnlimitedPlan(string? planType)
    {
        if (string.IsNullOrEmpty(planType)) return false;
        string type = planType.ToLower().Trim();
        return type == "platina" || type == "platinum" || type == "master" || type == "pro" || type == "unlimited" || type == "ilimitado";
    }

    public static bool VerifyQuantityCompanies(string planType, long quantityCompanies)
    {
        if (IsUnlimitedPlan(planType)) return true;

        switch(planType?.ToLower()?.Trim()) 
        {
            case "free":
                return quantityCompanies <= 1;

            case "bronze":
                return quantityCompanies <= 3;

            case "prata":
                return quantityCompanies <= 5;
                
            case "ouro":
                return quantityCompanies <= 10;

            case "platina":
            case "platinum":
                return true;
            
            default:
                return true;
        }
    }

    public static bool VerifyQuantityStores(string planType, long quantityStores)
    {
        if (IsUnlimitedPlan(planType)) return true;

        switch(planType?.ToLower()?.Trim()) 
        {
            case "free":
                return quantityStores <= 1;

            case "bronze":
                return quantityStores <= 5;

            case "prata":
                return quantityStores <= 10;
                
            case "ouro":
                return quantityStores <= 20;

            case "platina":
            case "platinum":
                return true;
            
            default:
                return true;
        }
    }

    public static bool VerifyQuantityUsers(string planType, long quantityUsers)
    {
        if (IsUnlimitedPlan(planType)) return true;

        switch(planType?.ToLower()?.Trim()) 
        {
            case "free":
                return quantityUsers <= 2;

            case "bronze":
                return quantityUsers <= 5;

            case "prata":
                return quantityUsers <= 10;
                
            case "ouro":
                return quantityUsers <= 25;

            case "platina":
            case "platinum":
                return true;
            
            default:
                return true;
        }
    }
    #endregion
}