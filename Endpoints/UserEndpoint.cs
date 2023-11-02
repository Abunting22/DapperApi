using AspNetCoreApi.Models;
using AspNetCoreApi.Services;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Reflection.Metadata.Ecma335;

namespace AspNetCoreApi.Endpoints
{
    public static class UserEndpoint
    {
        public static void MapUserEndpoint(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("accounts");

            group.MapGet("", async(SqlConnectionFactory sqlConnectionFactory) =>
            {
                using var connection = sqlConnectionFactory.Create();

                const string sql = "SELECT * FROM Accounts";

                var users = await connection.QueryAsync<User>(sql);

                return Results.Ok(users);
            });

            group.MapGet("{id}", async(int id, SqlConnectionFactory sqlConnectionFactory) =>
            {
                using var connection = sqlConnectionFactory.Create();

                const string sql = """
                    SELECT Id, FirstName, LastName
                    FROM Accounts
                    WHERE Id = @AccountId
                    """;

                var user = await connection.QuerySingleOrDefaultAsync<User>(
                    sql,
                    new {AccountId = id});

                return user is not null ? Results.Ok(user) : Results.NotFound();
            });

            group.MapPost("", async (User user, SqlConnectionFactory sqlConnectionFactory) =>
            {
                using var connection = sqlConnectionFactory.Create();

                const string sql = """
                    INSERT INTO Accounts (FirstName, LastName)
                    VALUES (@FirstName, @LastName)
                    """;

                await connection.ExecuteAsync(sql, user);

                return Results.Ok();
            });

            group.MapPut("{id}", async (int id, User user, SqlConnectionFactory sqlConnectionFactory) =>
            {
                using var connection = sqlConnectionFactory.Create();

                user.Id = id;

                const string sql = """
                    UPDATE Accounts
                    SET FirstName = @FirstName,
                        LastName = @LastName
                    WHERE Id = @AccountId
                    """;

                await connection.ExecuteAsync(sql, user);

                return Results.Ok();
            });

            group.MapDelete("{id}", async (int id, SqlConnectionFactory sqlConnectionFactory) =>
            {
                using var connection = sqlConnectionFactory.Create();

                const string sql = "DELETE FROM Accounts WHERE Id = @AccountId";

                await connection.ExecuteAsync(sql, new { AccountId = id });

                return Results.NoContent();
            });
        }
    }
}
