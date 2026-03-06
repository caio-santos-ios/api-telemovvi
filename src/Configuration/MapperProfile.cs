using AutoMapper;
using api_infor_cell.src.Models;
using api_infor_cell.src.Shared.DTOs;



namespace api_infor_cell.src.Configuration
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {            
            #region MASTER DATA

            CreateMap<CreatePlanDTO, Plan>().ReverseMap();
            CreateMap<UpdatePlanDTO, Plan>().ReverseMap();

            CreateMap<CreateCompanyDTO, Company>().ReverseMap();
            CreateMap<UpdateCompanyDTO, Company>().ReverseMap();
            
            CreateMap<CreateGenericTableDTO, GenericTable>().ReverseMap();
            CreateMap<UpdateGenericTableDTO, GenericTable>().ReverseMap();
            
            CreateMap<CreateAddressDTO, Address>().ReverseMap();
            CreateMap<UpdateAddressDTO, Address>().ReverseMap();
            
            CreateMap<CreateContactDTO, Contact>().ReverseMap();
            CreateMap<UpdateContactDTO, Contact>().ReverseMap();

            CreateMap<CreateAttachmentDTO, Attachment>().ReverseMap();
            CreateMap<UpdateAttachmentDTO, Attachment>().ReverseMap();      

            CreateMap<CreateSupplierDTO, Supplier>().ReverseMap();
            CreateMap<UpdateSupplierDTO, Supplier>().ReverseMap(); 

            CreateMap<CreateStoreDTO, Store>().ReverseMap();
            CreateMap<UpdateStoreDTO, Store>().ReverseMap();

            CreateMap<CreateBrandDTO, Brand>().ReverseMap();
            CreateMap<UpdateBrandDTO, Brand>().ReverseMap();

            CreateMap<CreateProductDTO, Product>().ReverseMap();
            CreateMap<UpdateProductDTO, Product>().ReverseMap();

            CreateMap<CreateCategoryDTO, Category>().ReverseMap();
            CreateMap<UpdateCategoryDTO, Category>().ReverseMap();

            CreateMap<CreateEmployeeDTO, Employee>().ReverseMap();
            CreateMap<UpdateEmployeeDTO, Employee>().ReverseMap();

            CreateMap<CreateFlagDTO, Flag>().ReverseMap();
            CreateMap<UpdateFlagDTO, Flag>().ReverseMap();

            CreateMap<CreateModelDTO, Model>().ReverseMap();
            CreateMap<UpdateModelDTO, Model>().ReverseMap();

            CreateMap<CreateServiceOrderDTO, ServiceOrder>().ReverseMap();
            CreateMap<UpdateServiceOrderDTO, ServiceOrder>().ReverseMap();

            CreateMap<CreateSalesOrderDTO, SalesOrder>().ReverseMap();
            CreateMap<UpdateSalesOrderDTO, SalesOrder>().ReverseMap();
            
            CreateMap<CreateSalesOrderItemDTO, SalesOrderItem>().ReverseMap();
            CreateMap<UpdateSalesOrderItemDTO, SalesOrderItem>().ReverseMap();

            CreateMap<CreateStockDTO, Stock>().ReverseMap();
            CreateMap<UpdateStockDTO, Stock>().ReverseMap();

            CreateMap<CreateBoxDTO, Box>().ReverseMap();
            CreateMap<UpdateBoxDTO, Box>().ReverseMap();

            CreateMap<CreateServiceOrderItemDTO, ServiceOrderItem>().ReverseMap();
            CreateMap<UpdateServiceOrderItemDTO, ServiceOrderItem>().ReverseMap();


            CreateMap<CreateCustomerDTO, Customer>().ReverseMap();
            CreateMap<UpdateCustomerDTO, Customer>().ReverseMap();

            CreateMap<CreateExchangeDTO, Exchange>().ReverseMap();
            CreateMap<UpdateExchangeDTO, Exchange>().ReverseMap();

            CreateMap<CreatePurchaseOrderDTO, PurchaseOrder>().ReverseMap();
            CreateMap<UpdatePurchaseOrderDTO, PurchaseOrder>().ReverseMap();
            CreateMap<CreatePurchaseOrderItemDTO, PurchaseOrderItem>().ReverseMap();
            CreateMap<UpdatePurchaseOrderItemDTO, PurchaseOrderItem>().ReverseMap();

            CreateMap<CreateTransferDTO, Transfer>().ReverseMap();
            CreateMap<UpdateTransferDTO, Transfer>().ReverseMap();
            
            CreateMap<CreateVariationDTO, Variation>().ReverseMap();
            CreateMap<UpdateVariationDTO, Variation>().ReverseMap();

            CreateMap<CreateProfilePermissionDTO, ProfilePermission>().ReverseMap();
            CreateMap<UpdateProfilePermissionDTO, ProfilePermission>().ReverseMap();
            
            CreateMap<CreateAdjustmentDTO, Adjustment>().ReverseMap();
            CreateMap<UpdateAdjustmentDTO, Adjustment>().ReverseMap();
            
            CreateMap<CreateSituationDTO, Situation>().ReverseMap();
            CreateMap<UpdateSituationDTO, Situation>().ReverseMap();

            CreateMap<CreateSituationDTO, Situation>().ReverseMap();
            CreateMap<UpdateUserDTO, Employee>().ReverseMap();
            CreateMap<UpdateUserDTO, User>().ReverseMap();

            CreateMap<CreateBudgetDTO, Budget>().ReverseMap();
            CreateMap<UpdateBudgetDTO, Budget>().ReverseMap();
            CreateMap<CreateBudgetItemDTO, BudgetItem>().ReverseMap();
            CreateMap<UpdateBudgetItemDTO, BudgetItem>().ReverseMap();
            #endregion

            #region FINANCIAL
            CreateMap<CreateAccountReceivableDTO, AccountReceivable>().ReverseMap();
            CreateMap<UpdateAccountReceivableDTO, AccountReceivable>().ReverseMap();

            CreateMap<CreateAccountPayableDTO, AccountPayable>().ReverseMap();
            CreateMap<UpdateAccountPayableDTO, AccountPayable>().ReverseMap();

            CreateMap<CreatePaymentMethodDTO, PaymentMethod>().ReverseMap();
            CreateMap<UpdatePaymentMethodDTO, PaymentMethod>().ReverseMap();
            #endregion

            #region  FISCAL
            
            #endregion
        }
    }

    
}