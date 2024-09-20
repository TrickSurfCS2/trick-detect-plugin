using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace TrickDetect.Database;

public class DB(string? dbConnectionString)
{
	private readonly string? dbConnectionString = dbConnectionString;

	public NpgsqlConnection GetConnection()
	{
		try
		{
			if (string.IsNullOrEmpty(dbConnectionString))
			{
				throw new ArgumentException("Database connection string cannot be null or empty.", nameof(dbConnectionString));
			}

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
