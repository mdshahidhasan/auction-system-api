using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.Services;
using Dapper;
using MySqlConnector;
using System.Data;

namespace AuctionSystem.Infra.Services.Data;

public class ProductPhotoDataService : IProductPhotoDataService
{
    private readonly MySqlConnection _connection;

    public ProductPhotoDataService(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<ProductPhoto> UploadPhotos(ProductPhoto productPhoto, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connection;
        var shouldManageConnection = transaction is null;

        try
        {
            if (shouldManageConnection && connection!.State != ConnectionState.Open)
            {
                await ((MySqlConnection)connection).OpenAsync();
            }

            productPhoto.CreatedAt = DateTime.UtcNow;

            const string sql = @"
                INSERT INTO productphotos
                (
                    CreatedAt,
                    UpdatedAt,
                    ProductId,
                    PhotoOrder,
                    PhotoUrl
                )
                VALUES
                (
                    @CreatedAt,
                    @UpdatedAt,
                    @ProductId,
                    @PhotoOrder,
                    @PhotoUrl
                );

                SELECT LAST_INSERT_ID();";

            var newId = await connection!.ExecuteScalarAsync<long>(sql, productPhoto, transaction);
            productPhoto.Id = (int)newId;

            return productPhoto;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while uploading product photo.", ex);
        }
        finally
        {
            if (shouldManageConnection && connection!.State == ConnectionState.Open)
            {
                await ((MySqlConnection)connection).CloseAsync();
            }
        }
    }

    public async Task<List<ProductPhoto>> GetPhotosByProductId(int productId)
    {
        try
        {
            await _connection.OpenAsync();

            const string sql = "SELECT * FROM productphotos WHERE ProductId = @ProductId ORDER BY PhotoOrder ASC, Id ASC;";
            return (await _connection.QueryAsync<ProductPhoto>(sql, new { ProductId = productId })).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while retrieving photos for product ID {productId}.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task DeletePhotos(int photoId)
    {
        try
        {
            await _connection.OpenAsync();

            const string sql = @"DELETE FROM productphotos WHERE Id = @Id;";

            await _connection.ExecuteAsync(sql, new { Id = photoId });
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while deleting product photo with ID {photoId}.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task DeletePhotosByProductId(int productId, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connection;
        var shouldManageConnection = transaction is null;

        try
        {
            if (shouldManageConnection && connection!.State != ConnectionState.Open)
            {
                await ((MySqlConnection)connection).OpenAsync();
            }

            const string sql = "DELETE FROM productphotos WHERE ProductId = @ProductId;";
            await connection!.ExecuteAsync(sql, new { ProductId = productId }, transaction);
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while deleting photos for product ID {productId}.", ex);
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