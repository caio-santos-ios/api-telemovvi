using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class ProductService(IProductRepository repository, IStockService stockService, IMapper _mapper) : IProductService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Product> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> Products = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(Products.Data, count, pagination.PageNumber, pagination.PageSize);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<PaginationApi<List<dynamic>>> GetAutocompleteAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Product> pagination = new(request.QueryParams);

                ResponseApi<List<dynamic>> products = await repository.GetAutocompleteAsync(pagination);
                return new(products.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Product> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> products = await repository.GetSelectAsync(pagination);
                return new(products.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        
        public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> Product = await repository.GetByIdAggregateAsync(id);
                if(Product.Data is null) return new(null, 404, "Produto não encontrado");
                return new(Product.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<Product?>> CreateAsync(CreateProductDTO request)
        {
            try
            {
                Product product = _mapper.Map<Product>(request);
                ResponseApi<long> code = await repository.GetNextCodeAsync(request.Company, request.Store);
                product.Code = code.Data.ToString().PadLeft(6, '0');
                ResponseApi<Product?> response = await repository.CreateAsync(product);

                if(response.Data is null) return new(null, 400, "Falha ao criar Produto.");
                return new(response.Data, 201, "Produto criado com sucesso.");
            }
            catch(Exception ex)
            { 
                System.Console.WriteLine(ex.Message);
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<Product?>> UpdateAsync(UpdateProductDTO request)
        {
            try
            {
                ResponseApi<Product?> productResponse = await repository.GetByIdAsync(request.Id);
                if(productResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                Product product = _mapper.Map<Product>(request);
                product.UpdatedAt = DateTime.UtcNow;
                product.CreatedAt = productResponse.Data.CreatedAt;
                product.Code = productResponse.Data.Code;

                ResponseApi<Product?> response = await repository.UpdateAsync(product);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
                return new(response.Data, 201, "Atualizado com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<Product?>> UpdateStockAsync(UpdateProductDTO request)
        {
            try
            {
                ResponseApi<Product?> productResponse = await repository.GetByIdAsync(request.Id);
                if(productResponse.Data is null) return new(null, 404, "Falha ao atualizar");

                productResponse.Data.Variations = request.Variations;
                productResponse.Data.VariationsCode = request.VariationsCode;
                
                ResponseApi<Product?> response = await repository.UpdateAsync(productResponse.Data);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

                if(productResponse.Data.MoveStock == "yes")
                {

                    await stockService.DeleteAllByProductAsync(new () 
                    { 
                        DeletedBy = request.UpdatedBy, 
                        Id = request.Id,
                        Plan = request.Plan,
                        Company = request.Company,
                        Store = request.Store
                    });

                    // if(request.HasVariations == "yes")
                    // {
                    //     foreach (VariationProduct variation in productResponse.Data.Variations)
                    //     {
                    //         await stockService.CreateAsync(new ()
                    //         {
                    //             Company = request.Company,
                    //             Store = request.Store,
                    //             Plan = request.Plan,
                    //             Quantity = variation.Stock,
                    //             Barcode = variation.Barcode,
                    //             CreatedBy = request.CreatedBy,
                    //             ProductId = productResponse.Data.Id,
                    //             Variation = variation,
                    //             Cost = productResponse.Data.Cost,
                    //             Price = productResponse.Data.Price,
                    //             Origin = "Movimentado por Produto",
                    //             VariationsCode = request.VariationsCode
                    //         });
                    //     };
                    // }
                    // else 
                    // {
                    //     await stockService.CreateAsync(new ()
                    //     {
                    //         Company = request.Company,
                    //         Store = request.Store,
                    //         Plan = request.Plan,
                    //         Quantity = request.QuantityStock,
                    //         Barcode = "",
                    //         CreatedBy = request.CreatedBy,
                    //         ProductId = productResponse.Data.Id,
                    //         Variation = new(),
                    //         Cost = productResponse.Data.Cost,
                    //         Price = productResponse.Data.Price,
                    //         Origin = "Movimentado por Produto",
                    //         VariationsCode = []
                    //     });
                    // }
                }

                return new(response.Data, 201, "Atualizado com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        #endregion
        
        #region DELETE
        public async Task<ResponseApi<Product>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<Product> Product = await repository.DeleteAsync(id);
                if(!Product.IsSuccess) return new(null, 400, Product.Message);
                return new(null, 204, "Excluído com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion 
    }
}