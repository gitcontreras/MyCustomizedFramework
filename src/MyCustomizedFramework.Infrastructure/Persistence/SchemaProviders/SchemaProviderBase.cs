using System.Data;
using System.Data.Common;
using Dapper;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

/// <summary>
/// Template method base class: each engine only supplies a <see cref="DbConnectionStringBuilder"/> built from
/// <see cref="DatabaseConnectionDetails"/> plus its metadata queries; the connection lifecycle and row mapping
/// are shared here. Building the connection string via a typed builder (instead of string concatenation)
/// avoids malformed or unsafe connection strings, and every metadata query is parameterized (not
/// string-interpolated) to avoid SQL injection through a caller-supplied table name.
/// </summary>
internal abstract class SchemaProviderBase : ISchemaProvider
{
    public async Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
        DatabaseConnectionDetails connection,
        CancellationToken cancellationToken = default)
    {
        using var dbConnection = CreateConnection(connection);

        var command = new CommandDefinition(TablesQuery, cancellationToken: cancellationToken);
        var rows = await dbConnection.QueryAsync<TableRow>(command);

        return rows
            .Select(row => DatabaseTable.Create(row.Schema, row.Name))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<TableColumn>> GetColumnsAsync(
        DatabaseConnectionDetails connection,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        using var dbConnection = CreateConnection(connection);

        var columnsCommand = new CommandDefinition(
            ColumnsQuery,
            new { TableName = tableName },
            cancellationToken: cancellationToken);
        var columnRows = await dbConnection.QueryAsync<ColumnRow>(columnsCommand);

        var primaryKeyCommand = new CommandDefinition(
            PrimaryKeyColumnsQuery,
            new { TableName = tableName },
            cancellationToken: cancellationToken);
        var primaryKeyNames = (await dbConnection.QueryAsync<string>(primaryKeyCommand))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return columnRows
            .OrderBy(row => row.OrdinalPosition)
            .Select(row =>
            {
                var isNullable = row.IsNullable != 0;
                return TableColumn.Create(
                    row.Name,
                    MapToCSharpType(row.DataType, isNullable),
                    isNullable,
                    primaryKeyNames.Contains(row.Name),
                    row.OrdinalPosition);
            })
            .ToArray();
    }

    protected abstract IDbConnection CreateConnection(DatabaseConnectionDetails connection);

    protected abstract string TablesQuery { get; }

    protected abstract string ColumnsQuery { get; }

    protected abstract string PrimaryKeyColumnsQuery { get; }

    protected abstract string MapToCSharpType(string dataType, bool isNullable);

    protected static string AsNullable(string csharpType, bool isNullable) =>
        isNullable ? $"{csharpType}?" : csharpType;

    protected sealed record TableRow(string Schema, string Name);

    /// <summary>
    /// <see cref="IsNullable"/> is an int (0/1), not bool: SQL Server/MySQL/Oracle's CASE expressions and
    /// Dapper's constructor-based materialization don't reliably coerce int -&gt; bool, so every engine's
    /// ColumnsQuery must project a real 0/1 here and the base class converts it to bool afterwards.
    /// </summary>
    protected sealed record ColumnRow(string Name, string DataType, int IsNullable, int OrdinalPosition);
}
