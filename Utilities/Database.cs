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

	public int Execute(string query, NpgsqlParameter[]? parameters = null)
	{
		using var connection = GetConnection();
		using var command = new NpgsqlCommand(query, connection);

		if (parameters != null)
			command.Parameters.AddRange(parameters);

		return command.ExecuteNonQuery();
	}

	public async Task<int> ExecuteAsync(string query, NpgsqlParameter[]? parameters = null)
	{
		using var connection = await GetConnectionAsync();
		using var command = new NpgsqlCommand(query, connection);

		if (parameters != null)
			command.Parameters.AddRange(parameters);

		return await command.ExecuteNonQueryAsync();
	}

	public T? Query<T>(string query, NpgsqlParameter[]? parameters = null)
	{
		using var connection = GetConnection();
		using var command = new NpgsqlCommand(query, connection);

		if (parameters != null)
			command.Parameters.AddRange(parameters);

		using var reader = command.ExecuteReader();
		if (reader.Read())
		{
			var value = reader[0];
			return (T)Convert.ChangeType(value, typeof(T));
		}

		return default(T);
	}

	public async Task<T?> QueryAsync<T>(string query, NpgsqlParameter[]? parameters = null)
	{
		using var connection = await GetConnectionAsync();
		using var command = new NpgsqlCommand(query, connection);

		if (parameters != null)
			command.Parameters.AddRange(parameters);

		using var reader = await command.ExecuteReaderAsync();
		if (await reader.ReadAsync())
		{
			var value = reader[0];
			return (T)Convert.ChangeType(value, typeof(T));
		}

		return default(T);
	}
}
