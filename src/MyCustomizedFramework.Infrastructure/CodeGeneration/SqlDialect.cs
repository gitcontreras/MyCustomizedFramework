using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration;

/// <summary>
/// The one place that actually differs per engine when generating a repository: identifier quoting,
/// parameter placeholders, and how a newly inserted row's generated key is retrieved.
/// </summary>
internal static class SqlDialect
{
    public static SqlFragments Build(DatabaseEngine engine, string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> insertableColumns) =>
        engine switch
        {
            DatabaseEngine.SqlServer => BuildSqlServer(tableName, primaryKey, insertableColumns),
            DatabaseEngine.PostgreSql => BuildPostgreSql(tableName, primaryKey, insertableColumns),
            DatabaseEngine.MySql => BuildMySql(tableName, primaryKey, insertableColumns),
            DatabaseEngine.Oracle => BuildOracle(tableName, primaryKey, insertableColumns),
            _ => throw new ArgumentOutOfRangeException(nameof(engine))
        };

    private static SqlFragments BuildSqlServer(string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> columns)
    {
        var columnList = string.Join(", ", columns.Select(c => $"[{c.Name}]"));
        var paramList = string.Join(", ", columns.Select(c => $"@{c.Name}"));
        var setList = string.Join(", ", columns.Select(c => $"[{c.Name}] = @{c.Name}"));

        return new SqlFragments(
            $"INSERT INTO [{tableName}] ({columnList}) OUTPUT INSERTED.[{primaryKey.Name}] VALUES ({paramList})",
            $"SELECT * FROM [{tableName}]",
            $"SELECT * FROM [{tableName}] WHERE [{primaryKey.Name}] = @{primaryKey.Name}",
            $"UPDATE [{tableName}] SET {setList} WHERE [{primaryKey.Name}] = @{primaryKey.Name}",
            $"DELETE FROM [{tableName}] WHERE [{primaryKey.Name}] = @{primaryKey.Name}");
    }

    private static SqlFragments BuildPostgreSql(string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> columns)
    {
        var columnList = string.Join(", ", columns.Select(c => $"\"{c.Name}\""));
        var paramList = string.Join(", ", columns.Select(c => $"@{c.Name}"));
        var setList = string.Join(", ", columns.Select(c => $"\"{c.Name}\" = @{c.Name}"));

        return new SqlFragments(
            $"INSERT INTO \"{tableName}\" ({columnList}) VALUES ({paramList}) RETURNING \"{primaryKey.Name}\"",
            $"SELECT * FROM \"{tableName}\"",
            $"SELECT * FROM \"{tableName}\" WHERE \"{primaryKey.Name}\" = @{primaryKey.Name}",
            $"UPDATE \"{tableName}\" SET {setList} WHERE \"{primaryKey.Name}\" = @{primaryKey.Name}",
            $"DELETE FROM \"{tableName}\" WHERE \"{primaryKey.Name}\" = @{primaryKey.Name}");
    }

    private static SqlFragments BuildMySql(string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> columns)
    {
        var columnList = string.Join(", ", columns.Select(c => $"`{c.Name}`"));
        var paramList = string.Join(", ", columns.Select(c => $"@{c.Name}"));
        var setList = string.Join(", ", columns.Select(c => $"`{c.Name}` = @{c.Name}"));

        return new SqlFragments(
            $"INSERT INTO `{tableName}` ({columnList}) VALUES ({paramList}); SELECT LAST_INSERT_ID();",
            $"SELECT * FROM `{tableName}`",
            $"SELECT * FROM `{tableName}` WHERE `{primaryKey.Name}` = @{primaryKey.Name}",
            $"UPDATE `{tableName}` SET {setList} WHERE `{primaryKey.Name}` = @{primaryKey.Name}",
            $"DELETE FROM `{tableName}` WHERE `{primaryKey.Name}` = @{primaryKey.Name}");
    }

    private static SqlFragments BuildOracle(string tableName, TableColumn primaryKey, IReadOnlyCollection<TableColumn> columns)
    {
        var columnList = string.Join(", ", columns.Select(c => $"\"{c.Name}\""));
        var paramList = string.Join(", ", columns.Select(c => $":{c.Name}"));
        var setList = string.Join(", ", columns.Select(c => $"\"{c.Name}\" = :{c.Name}"));

        return new SqlFragments(
            $"INSERT INTO \"{tableName}\" ({columnList}) VALUES ({paramList}) RETURNING \"{primaryKey.Name}\" INTO :{primaryKey.Name}",
            $"SELECT * FROM \"{tableName}\"",
            $"SELECT * FROM \"{tableName}\" WHERE \"{primaryKey.Name}\" = :{primaryKey.Name}",
            $"UPDATE \"{tableName}\" SET {setList} WHERE \"{primaryKey.Name}\" = :{primaryKey.Name}",
            $"DELETE FROM \"{tableName}\" WHERE \"{primaryKey.Name}\" = :{primaryKey.Name}");
    }
}

internal sealed record SqlFragments(string Insert, string SelectAll, string SelectById, string Update, string Delete);
