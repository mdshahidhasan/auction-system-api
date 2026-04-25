using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;
using Dapper;
using MySqlConnector;
using System.Data;

namespace AuctionSystem.Infra.Services.Data;

public class ProductDataService : IProductDataService
{
    private readonly MySqlConnection _connection;

    public ProductDataService(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<Product> CreateProduct(Product product, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connection;
        var shouldManageConnection = transaction is null;

        try
        {
            if (shouldManageConnection && connection!.State != ConnectionState.Open)
            {
                await ((MySqlConnection)connection).OpenAsync();
            }

            product.CreatedAt = DateTime.UtcNow;

            const string insertSql = @"
                INSERT INTO products
                (
                    CreatedAt,
                    UpdatedAt,
                    UserId,
                    Title,
                    Brand,
                    Location,
                    Description,
                    AuctionStatus,
                    AuctionStarts,
                    AuctionEnds,
                    AskingPrice,
                    StartingPrice,
                    CurrentHighestBid,
                    MinBidIncrement
                )
                VALUES
                (
                    @CreatedAt,
                    @UpdatedAt,
                    @UserId,
                    @Title,
                    @Brand,
                    @Location,
                    @Description,
                    @AuctionStatus,
                    @AuctionStarts,
                    @AuctionEnds,
                    @AskingPrice,
                    @StartingPrice,
                    @CurrentHighestBid,
                    @MinBidIncrement
                );

                SELECT LAST_INSERT_ID();";

            var newId = await connection!.ExecuteScalarAsync<long>(insertSql, product, transaction);
            product.Id = (int)newId;

            return product;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while creating product.", ex);
        }
        finally
        {
            if (shouldManageConnection && connection!.State == ConnectionState.Open)
            {
                await ((MySqlConnection)connection).CloseAsync();
            }
        }
    }

    public async Task<ServiceResult<List<Product>>> GetProducts(ProductSearchModel searchModel)
    {
        try
        {
            await _connection.OpenAsync();

            const string filters = @"
                                WHERE (@AuctionStatus IS NULL OR AuctionStatus = @AuctionStatus)";

            var listSql = $@"
                SELECT *
                FROM products
                {filters}
                ORDER BY Id DESC
                LIMIT @Limit OFFSET @Offset;";

            var countSql = $@"
                SELECT COUNT(1)
                FROM products
                {filters};";

            var parameters = new
            {
                searchModel.AuctionStatus,
                searchModel.Limit,
                searchModel.Offset
            };

            var products = (await _connection.QueryAsync<Product>(listSql, parameters)).ToList();
            var totalCount = await _connection.ExecuteScalarAsync<int>(countSql, parameters);

            return new ServiceResult<List<Product>>
            {
                Code = 200,
                Message = "Products retrieved successfully.",
                Data = products,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving products.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task<ServiceResult<List<Product>>> GetProductsByUserId(int userId, ProductSearchModel searchModel)
    {
        try
        {
            await _connection.OpenAsync();

            const string filters = @"
                WHERE UserId = @UserId
                  AND (@AuctionStatus IS NULL OR AuctionStatus = @AuctionStatus)";

            var listSql = $@"
                SELECT *
                FROM products
                {filters}
                ORDER BY Id DESC
                LIMIT @Limit OFFSET @Offset;";

            var countSql = $@"
                SELECT COUNT(1)
                FROM products
                {filters};";

            var parameters = new
            {
                UserId = userId,
                searchModel.AuctionStatus,
                searchModel.Limit,
                searchModel.Offset
            };

            var products = (await _connection.QueryAsync<Product>(listSql, parameters)).ToList();
            var totalCount = await _connection.ExecuteScalarAsync<int>(countSql, parameters);

            return new ServiceResult<List<Product>>
            {
                Code = 200,
                Message = "Products retrieved successfully.",
                Data = products,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving user products.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task<Product?> GetProductById(int id)
    {
        try
        {
            await _connection.OpenAsync();
            const string sql = "SELECT * FROM products WHERE Id = @productId;";

            return await _connection.QueryFirstOrDefaultAsync<Product>(sql, new { productId = id });
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while retrieving product with ID {id}.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task UpdateProduct(int id, ProductUpdateModel productUpdateModel)
    {
        try
        {
            await _connection.OpenAsync();

            var sql = @"
                UPDATE products
                SET
                    UpdatedAt = @UpdatedAt,
                    Title = COALESCE(@Title, Title),
                    Brand = COALESCE(@Brand, Brand),
                    Location = COALESCE(@Location, Location),
                    Description = COALESCE(@Description, Description),
                    AuctionStarts = COALESCE(@AuctionStarts, AuctionStarts),
                    AuctionEnds = COALESCE(@AuctionEnds, AuctionEnds),
                    AskingPrice = COALESCE(@AskingPrice, AskingPrice),
                    StartingPrice = COALESCE(@StartingPrice, StartingPrice),
                    MinBidIncrement = COALESCE(@MinBidIncrement, MinBidIncrement)
                WHERE Id = @Id;";

            await _connection.ExecuteAsync(sql, new
            {
                Id = id,
                UpdatedAt = DateTime.UtcNow,
                productUpdateModel.Title,
                productUpdateModel.Brand,
                productUpdateModel.Location,
                productUpdateModel.Description,
                productUpdateModel.AuctionStarts,
                productUpdateModel.AuctionEnds,
                productUpdateModel.AskingPrice,
                productUpdateModel.StartingPrice,
                productUpdateModel.MinBidIncrement
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while updating product with ID {id}.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task UpdateCurrentHighestBid(int id, decimal? currentHighestBid, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connection;
        var shouldManageConnection = transaction is null;

        try
        {
            if (shouldManageConnection && connection!.State != ConnectionState.Open)
            {
                await ((MySqlConnection)connection).OpenAsync();
            }

            const string sql = @"
                UPDATE products
                SET
                    CurrentHighestBid = @CurrentHighestBid,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id;";

            await connection!.ExecuteAsync(sql, new
            {
                Id = id,
                CurrentHighestBid = currentHighestBid,
                UpdatedAt = DateTime.UtcNow
            }, transaction);
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while updating highest bid for product with ID {id}.", ex);
        }
        finally
        {
            if (shouldManageConnection && connection!.State == ConnectionState.Open)
            {
                await ((MySqlConnection)connection).CloseAsync();
            }
        }
    }

    public async Task DeleteProduct(int id, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connection;
        var shouldManageConnection = transaction is null;

        try
        {
            if (shouldManageConnection && connection!.State != ConnectionState.Open)
            {
                await ((MySqlConnection)connection).OpenAsync();
            }

            const string sql = "DELETE FROM products WHERE Id = @Id;";

            await connection!.ExecuteAsync(sql, new { Id = id }, transaction);
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while deleting product with ID {id}.", ex);
        }
        finally
        {
            if (shouldManageConnection && connection!.State == ConnectionState.Open)
            {
                await ((MySqlConnection)connection).CloseAsync();
            }
        }
    }
}