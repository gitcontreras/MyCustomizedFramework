using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration;

/// <summary>
/// The one place that actually differs per engine when generating a repository: identifier quoting,
/// parameter placeholders, and how a newly inserted row's generated key is retrieved.
/// </summary>
internal static class SqlDialect
{
    public static SqlFragments Build(
        DatabaseEngine engine,
        string? schema,
        string tableName,
        TableColumn primaryKey,
        IReadOnlyCollection<TableColumn> insertableColumns) =>
        engine switch
        {
            DatabaseEngine.SqlServer => BuildSqlServer(schema, tableName, primaryKey, insertableColumns),
            DatabaseEngine.PostgreSql => BuildPostgreSql(schema, tableName, primaryKey, insertableColumns),
            DatabaseEngine.MySql => BuildMySql(schema, tableName, primaryKey, insertableColumns),
            DatabaseEngine.Oracle => BuildOracle(schema, tableName, primaryKey, insertableColumns),
            _ => throw new ArgumentOutOfRangeException(nameof(engine))
        };

    private static SqlFragments BuildSqlServer(string? schema, string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> columns)
    {
        var qualifiedTable = string.IsNullOrWhiteSpace(schema) ? $"[{tableName}]" : $"[{schema}].[{tableName}]";
        var columnList = string.Join(", ", columns.Select(c => $"[{c.Name}]"));
        var paramList = string.Join(", ", columns.Select(c => $"@{c.Name}"));
        var setList = string.Join(", ", columns.Select(c => $"[{c.Name}] = @{c.Name}"));

        return new SqlFragments(
            $"INSERT INTO {qualifiedTable} ({columnList}) OUTPUT INSERTED.[{primaryKey.Name}] VALUES ({paramList})",
            $"SELECT * FROM {qualifiedTable}",
            $"SELECT * FROM {qualifiedTable} WHERE [{primaryKey.Name}] = @{primaryKey.Name}",
            $"UPDATE {qualifiedTable} SET {setList} WHERE [{primaryKey.Name}] = @{primaryKey.Name}",
            $"DELETE FROM {qualifiedTable} WHERE [{primaryKey.Name}] = @{primaryKey.Name}");
    }

    private static SqlFragments BuildPostgreSql(string? schema, string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> columns)
    {
        var qualifiedTable = string.IsNullOrWhiteSpace(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";
        var columnList = string.Join(", ", columns.Select(c => $"\"{c.Name}\""));
        var paramList = string.Join(", ", columns.Select(c => $"@{c.Name}"));
        var setList = string.Join(", ", columns.Select(c => $"\"{c.Name}\" = @{c.Name}"));

        return new SqlFragments(
            $"INSERT INTO {qualifiedTable} ({columnList}) VALUES ({paramList}) RETURNING \"{primaryKey.Name}\"",
            $"SELECT * FROM {qualifiedTable}",
            $"SELECT * FROM {qualifiedTable} WHERE \"{primaryKey.Name}\" = @{primaryKey.Name}",
            $"UPDATE {qualifiedTable} SET {setList} WHERE \"{primaryKey.Name}\" = @{primaryKey.Name}",
            $"DELETE FROM {qualifiedTable} WHERE \"{primaryKey.Name}\" = @{primaryKey.Name}");
    }

    private static SqlFragments BuildMySql(string? schema, string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> columns)
    {
        // MySQL has no separate "schema" concept - a schema here is another database, addressed as
        // `database`.`table`, which MySQL supports natively when the connecting user has access to it.
        var qualifiedTable = string.IsNullOrWhiteSpace(schema) ? $"`{tableName}`" : $"`{schema}`.`{tableName}`";
        var columnList = string.Join(", ", columns.Select(c => $"`{c.Name}`"));
        var paramList = string.Join(", ", columns.Select(c => $"@{c.Name}"));
        var setList = string.Join(", ", columns.Select(c => $"`{c.Name}` = @{c.Name}"));

        return new SqlFragments(
            $"INSERT INTO {qualifiedTable} ({columnList}) VALUES ({paramList}); SELECT LAST_INSERT_ID();",
            $"SELECT * FROM {qualifiedTable}",
            $"SELECT * FROM {qualifiedTable} WHERE `{primaryKey.Name}` = @{primaryKey.Name}",
            $"UPDATE {qualifiedTable} SET {setList} WHERE `{primaryKey.Name}` = @{primaryKey.Name}",
            $"DELETE FROM {qualifiedTable} WHERE `{primaryKey.Name}` = @{primaryKey.Name}");
    }

    private static SqlFragments BuildOracle(string? schema, string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> columns)
    {
        var qualifiedTable = string.IsNullOrWhiteSpace(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";
        var columnList = string.Join(", ", columns.Select(c => $"\"{c.Name}\""));
        var paramList = string.Join(", ", columns.Select(c => $":{c.Name}"));
        var setList = string.Join(", ", columns.Select(c => $"\"{c.Name}\" = :{c.Name}"));

        return new SqlFragments(
            $"INSERT INTO {qualifiedTable} ({columnList}) VALUES ({paramList}) RETURNING \"{primaryKey.Name}\" INTO :{primaryKey.Name}",
            $"SELECT * FROM {qualifiedTable}",
            $"SELECT * FROM {qualifiedTable} WHERE \"{primaryKey.Name}\" = :{primaryKey.Name}",
            $"UPDATE {qualifiedTable} SET {setList} WHERE \"{primaryKey.Name}\" = :{primaryKey.Name}",
            $"DELETE FROM {qualifiedTable} WHERE \"{primaryKey.Name}\" = :{primaryKey.Name}");
    }
}

internal sealed record SqlFragments(string Insert, string SelectAll, string SelectById, string Update, string Delete);
