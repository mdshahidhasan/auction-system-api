using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;
using Dapper;
using MySqlConnector;
using System.Data;

namespace AuctionSystem.Infra.Services.Data;

public class UserDataService : IUserDataService
{
    private readonly MySqlConnection _connection;
    public UserDataService(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<User> CreateUser(User user, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connection;
        var shouldManageConnection = transaction is null;

        try
        {
            if (shouldManageConnection && connection!.State != ConnectionState.Open)
            {
                await ((MySqlConnection)connection).OpenAsync();
            }

            user.CreatedAt = DateTime.UtcNow;

            const string sql = @"
                INSERT INTO users
                (
                    CreatedAt,
                    UpdatedAt,
                    FirstName,
                    LastName,
                    Email,
                    Password,
                    Role,
                    ContactNumber,
                    Location,
                    PhotoPath,
                    IsActive,
                    IsVerified
                )
                VALUES
                (
                    @CreatedAt,
                    @UpdatedAt,
                    @FirstName,
                    @LastName,
                    @Email,
                    @Password,
                    @Role,
                    @ContactNumber,
                    @Location,
                    @PhotoPath,
                    @IsActive,
                    @IsVerified
                );

                SELECT LAST_INSERT_ID();";

            var newId = await connection!.ExecuteScalarAsync<long>(sql, user, transaction);
            user.Id = (int)newId;

            return user;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while creating user.", ex);
        }
        finally
        {
            if (shouldManageConnection && connection!.State == ConnectionState.Open)
            {
                await ((MySqlConnection)connection).CloseAsync();
            }
        }
    }

    public async Task<ServiceResult<List<User>>> GetUsers(UserSearchModel searchModel)
    {
        try
        {
            await _connection.OpenAsync();

            var filters = @"
                WHERE (@Email IS NULL OR Email LIKE CONCAT('%', @Email, '%'))
                  AND (@Role IS NULL OR Role = @Role)
                  AND (@IsActive IS NULL OR IsActive = @IsActive)
                  AND (@IsVerified IS NULL OR IsVerified = @IsVerified)";

            var listSql = $@"
                SELECT *
                FROM users
                {filters}
                ORDER BY Id DESC
                LIMIT @Limit OFFSET @Offset;";

            var countSql = $@"
                SELECT COUNT(1)
                FROM users
                {filters};";

            var parameters = new
            {
                searchModel.Email,
                searchModel.Role,
                searchModel.IsActive,
                searchModel.IsVerified,
                Limit = searchModel.Limit,
                Offset = searchModel.Offset
            };

            var users = (await _connection.QueryAsync<User>(listSql, parameters)).ToList();
            var totalCount = await _connection.ExecuteScalarAsync<int>(countSql, parameters);

            return new ServiceResult<List<User>>
            {
                Code = 200,
                Message = "Users retrieved successfully.",
                Data = users,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving users.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task<User?> GetUserById(int id)
    {
        try
        {
            await _connection.OpenAsync();
            string sql = "SELECT * FROM users WHERE Id = @userId;";
            return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { userId = id });
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while retrieving the user with ID {id}.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task<User?> GetUserByEmail(string email, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connection;
        var shouldManageConnection = transaction is null;

        try
        {
            if (shouldManageConnection && connection!.State != ConnectionState.Open)
            {
                await ((MySqlConnection)connection).OpenAsync();
            }

            const string sql = "SELECT * FROM users WHERE Email = @Email LIMIT 1;";
            return await connection!.QueryFirstOrDefaultAsync<User>(sql, new { Email = email }, transaction);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving user by email.", ex);
        }
        finally
        {
            if (shouldManageConnection && connection!.State == ConnectionState.Open)
            {
                await ((MySqlConnection)connection).CloseAsync();
            }
        }
    }

    public async Task UpdateUser(User user)
    {
        try
        {
            await _connection.OpenAsync();
            user.UpdatedAt = DateTime.UtcNow;

            string sql = @"
                UPDATE users
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Role = @Role,
                    ContactNumber = @ContactNumber,
                    Location = @Location,
                    PhotoPath = @PhotoPath,
                    IsActive = @IsActive,
                    IsVerified = @IsVerified,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id;";

            await _connection.ExecuteAsync(sql, user);
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while updating the user.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task DeleteUser(int userId)
    {
        try
        {
            await _connection.OpenAsync();
            string sql = "DELETE FROM users WHERE Id = @Id";
            await _connection.ExecuteAsync(sql, new { Id = userId });
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while deleting the user with ID {userId}.", ex);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
}