using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Bid;
using Dapper;
using MySqlConnector;
using System.Data;

namespace AuctionSystem.Infra.Services.Data;

public class BidDataService : IBidDataService
{
    private readonly MySqlConnection _connection;

    public BidDataService(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<Bid> CreateBid(Bid bid, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connection;
        var shouldManageConnection = transaction is null;

        try
        {
            if (shouldManageConnection && connection!.State != ConnectionState.Open)
            {
                await ((MySqlConnection)connection).OpenAsync();
            }

            bid.CreatedAt = DateTime.UtcNow;

            const string insertSql = @"
                INSERT INTO bids
                (
                    CreatedAt,
                    UpdatedAt,
                    UserId,
                    Amount,
                    ProductId
                )
                VALUES
                (
                    @CreatedAt,
                    @UpdatedAt,
                    @UserId,
                    @Amount,
                    @ProductId
                );

                SELECT LAST_INSERT_ID();";

            var newId = await connection!.ExecuteScalarAsync<long>(insertSql, bid, transaction);
            bid.Id = (int)newId;

            return bid;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while creating bid.", ex);
        }
        finally
        {
            if (shouldManageConnection && connection!.State == ConnectionState.Open)
            {
                await ((MySqlConnection)connection).CloseAsync();
            }
        }
    }

    public async Task<ServiceResult<List<Bid>>> GetBids(BidSearchModel searchModel)
    {
        try
        {
            await _connection.OpenAsync();

            const string filters = @"
                                WHERE (@ProductId = 0 OR ProductId = @ProductId)
                                    AND (@UserId IS NULL OR UserId = @UserId)";

            var listSql = $@"
                SELECT *
                FROM bids
                {filters}
                ORDER BY Id DESC
                LIMIT @Limit OFFSET @Offset;";

            var countSql = $@"
                SELECT COUNT(1)
                FROM bids
                {filters};";

            var parameters = new
            {
                searchModel.ProductId,
                searchModel.UserId,
                searchModel.Limit,
                searchModel.Offset
            };

            var bids = (await _connection.QueryAsync<Bid>(listSql, parameters)).ToList();
            var totalCount = await _connection.ExecuteScalarAsync<int>(countSql, parameters);

            return new ServiceResult<List<Bid>>
            {
                Code = 200,
                Message = "Bids retrieved successfully.",
                Data = bids,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving bids.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task<Bid?> GetBidById(int id)
    {
        try
        {
            await _connection.OpenAsync();

            const string sql = "SELECT * FROM bids WHERE Id = @Id LIMIT 1;";

            return await _connection.QueryFirstOrDefaultAsync<Bid>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while retrieving bid with ID {id}.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task<decimal?> GetHighestBidAmount(int productId, int? excludeBidId = null, IDbTransaction? transaction = null)
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
                SELECT MAX(Amount)
                FROM bids
                WHERE ProductId = @ProductId
                  AND (@ExcludeBidId IS NULL OR Id <> @ExcludeBidId);";

            return await connection!.ExecuteScalarAsync<decimal?>(sql, new
            {
                ProductId = productId,
                ExcludeBidId = excludeBidId
            }, transaction);
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while retrieving highest bid for product with ID {productId}.", ex);
        }
        finally
        {
            if (shouldManageConnection && connection!.State == ConnectionState.Open)
            {
                await ((MySqlConnection)connection).CloseAsync();
            }
        }
    }

    public async Task<List<MyBidReadModel>> GetMyBids(int userId, IDbTransaction? transaction = null)
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
                SELECT
                    b.Id AS BidId,
                    b.Amount AS MyBidAmount,
                    b.CreatedAt,
                    p.Id AS ProductId,
                    p.Title AS ProductTitle,
                    p.Brand AS ProductBrand,
                    (
                        SELECT pp.PhotoUrl
                        FROM productphotos pp
                        WHERE pp.ProductId = p.Id
                        ORDER BY pp.PhotoOrder ASC, pp.Id ASC
                        LIMIT 1
                    ) AS PrimaryPhotoUrl,
                    p.CurrentHighestBid,
                    p.MinBidIncrement,
                    p.AuctionEnds,
                    p.AuctionStatus
                FROM bids b
                INNER JOIN products p ON p.Id = b.ProductId
                WHERE b.UserId = @UserId
                ORDER BY b.CreatedAt DESC;";

            var rows = await connection!.QueryAsync<MyBidReadModel>(sql, new { UserId = userId }, transaction);
            return rows.ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while retrieving bids for user with ID {userId}.", ex);
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