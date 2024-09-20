using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Threading.Tasks;

namespace TrickDetect.Database;

public class DB(string? dbConnectionString)
{
	private readonly string? dbConnectionString = dbConnectionString;

	public NpgsqlConnection GetConnection()
	{
		try
		{
			var connection = new NpgsqlConnection(dbConnectionString);
			connection.Open();
			return connection;
		}
		catch (Exception ex)
		{
			TrickDetect._logger?.LogCritical($"Unable to connect to database: {ex.Message}");
			throw;
		}
	}

	public async Task<NpgsqlConnection> GetConnectionAsync()
	{
		try
		{
			var connection = new NpgsqlConnection(dbConnectionString);
			await connection.OpenAsync();
			return connection;
		}
		catch (Exception ex)
		{
			TrickDetect._logger?.LogCritical($"Unable to connect to database: {ex.Message}");
			throw;
		}
	}

	public bool CheckDatabaseConnection()
	{
		using var connection = GetConnection();

		try
		{
			return connection.FullState == ConnectionState.Open;
		}
		catch
		{
			return false;
		}
	}

	public int Execute(string query, object? parameters = null)
	{
		using var connection = GetConnection();
		return connection.Execute(query, parameters);
	}

	public async Task<int> ExecuteAsync(string query, object? parameters = null)
	{
		using var connection = await GetConnectionAsync();
		return await connection.ExecuteAsync(query, parameters);
	}

	public T? QuerySingle<T>(string query, object? parameters = null)
	{
		using var connection = GetConnection();
		return connection.QuerySingleOrDefault<T>(query, parameters);
	}

	public async Task<T?> QuerySingleAsync<T>(string query, object? parameters = null)
	{
		using var connection = await GetConnectionAsync();
		return await connection.QuerySingleOrDefaultAsync<T>(query, parameters);
	}

	public IEnumerable<T> Query<T>(string query, object? parameters = null)
	{
		using var connection = GetConnection();
		return connection.Query<T>(query, parameters);
	}

	public async Task<IEnumerable<T>> QueryAsync<T>(string query, object? parameters = null)
	{
		using var connection = await GetConnectionAsync();
		return await connection.QueryAsync<T>(query, parameters);
	}
}
