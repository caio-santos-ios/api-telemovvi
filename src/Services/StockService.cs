using api_infor_cell.src.Handlers;
using api_infor_cell.src.Interfaces;
using api_infor_cell.src.Models;
using api_infor_cell.src.Models.Base;
using api_infor_cell.src.Shared.DTOs;
using api_infor_cell.src.Shared.Utils;
using AutoMapper;

namespace api_infor_cell.src.Services
{
    public class StockService(IStockRepository repository, ILogApiRepository logApiRepository, IMapper _mapper) : IStockService
    {
        #region READ
        public async Task<PaginationApi<List<dynamic>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<Stock> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> Stocks = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                return new(Stocks.Data, count, pagination.PageNumber, pagination.PageSize);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetByProductIdAggregationAsync(string plan, string company, string productId)
        {
            try
            {
                ResponseApi<List<dynamic>> Stocks = await repository.GetByProductIdAggregationAsync(plan, company, productId);
                return new(Stocks.Data);
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
                ResponseApi<dynamic?> Stock = await repository.GetByIdAggregateAsync(id);
                if(Stock.Data is null) return new(null, 404, "Estoque não encontrada");
                return new(Stock.Data);
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion
        
        #region CREATE
        public async Task<ResponseApi<Stock?>> CreateAsync(CreateStockDTO request)
        {
            try
            {
                Stock stock = _mapper.Map<Stock>(request);
                ResponseApi<long> code = await repository.GetNextCodeAsync(request.Plan, request.Company, request.Store);
                stock.Code = code.Data.ToString().PadLeft(6, '0');
                decimal qtdSerial = request.Quantity;
                stock.QuantityAvailable = request.Quantity;
                
                ResponseApi<List<Stock>> stocks = await repository.GetByProductId(request.ProductId, request.Plan, request.Company, request.Store);
                if(stocks.IsSuccess && stocks.Data is not null)
                {
                    qtdSerial += stocks.Data.Sum(x => x.Quantity);
                };

                string costPart = request.Cost.ToString().PadLeft(7, '0');
                string quantityPart = qtdSerial.ToString().PadLeft(4, '0');
                stock.SerialNumber = $"{costPart}{quantityPart}";

                ResponseApi<Stock?> response = await repository.CreateAsync(stock);

                if(response.Data is null) return new(null, 400, "Falha ao criar Estoque.");

                // await logApiRepository.CreateAsync(new ()
                // {
                //     Plan = request.Plan,
                //     Company = request.Company,
                //     Store = request.Store,
                //     CreatedBy = request.CreatedBy,
                //     Description = request.OriginDescription,
                //     OriginId = stock.Id,
                //     OriginSecondaryId = stock.ProductId,
                //     ResponseMessage = "Estoque movimentado",
                //     Response = "success",
                //     Table = "stock"
                // });

                return new(response.Data, 201, "Estoque criada com sucesso.");
            }
            catch
            { 
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion
        
        #region UPDATE
        public async Task<ResponseApi<Stock?>> UpdateAsync(UpdateStockDTO request)
        {
            try
            {
                ResponseApi<Stock?> StockResponse = await repository.GetByIdAsync(request.Id);
                if(StockResponse.Data is null) return new(null, 404, "Falha ao atualizar");
                
                Stock Stock = _mapper.Map<Stock>(request);
                Stock.UpdatedAt = DateTime.UtcNow;

                ResponseApi<Stock?> response = await repository.UpdateAsync(Stock);
                if(!response.IsSuccess) return new(null, 400, "Falha ao atualizar");
                return new(response.Data, 201, "Atualizado com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        #endregion
        
        #region DELETE
        public async Task<ResponseApi<Stock>> DeleteAllByProductAsync(DeleteDTO request)
        {
            try
            {

                ResponseApi<Stock> stock = await repository.DeleteAllByProductAsync(request);
                if(!stock.IsSuccess) return new(null, 400, stock.Message);
                return new(null, 204, "Excluída com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<Stock>> DeleteAsync(string id)
        {
            try
            {
                ResponseApi<Stock> Stock = await repository.DeleteAsync(id);
                if(!Stock.IsSuccess) return new(null, 400, Stock.Message);
                return new(null, 204, "Excluída com sucesso");
            }
catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion 
    }
}