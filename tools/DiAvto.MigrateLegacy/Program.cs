using DiAvto.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// Переносит общие таблицы из старой БД DiAvto.Web в новую схему.
// Использование:
//   dotnet run --project tools/DiAvto.MigrateLegacy -- <старая.db> <новая.db>
//
// Новая БД создаётся с нуля. Существующий файл не перезаписывается.

if (args.Length < 2)
{
    Console.Error.WriteLine("Использование: DiAvto.MigrateLegacy <source.db> <target.db>");
    return 2;
}

var sourcePath = Path.GetFullPath(args[0]);
var targetPath = Path.GetFullPath(args[1]);

if (!File.Exists(sourcePath))
{
    Console.Error.WriteLine($"Исходная БД не найдена: {sourcePath}");
    return 2;
}

if (File.Exists(targetPath))
{
    Console.Error.WriteLine($"Целевая БД уже существует: {targetPath}");
    Console.Error.WriteLine("Укажите путь к новому файлу, чтобы не перезаписать данные.");
    return 2;
}

Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

var options = new DbContextOptionsBuilder<DiAvtoDbContext>()
    .UseSqlite($"Data Source={targetPath}")
    .Options;

await using (var db = new DiAvtoDbContext(options))
{
    await db.Database.MigrateAsync();
}

Console.WriteLine($"Создана новая схема: {targetPath}");

await using var connection = new SqliteConnection($"Data Source={targetPath}");
await connection.OpenAsync();

await ExecuteAsync(connection, "PRAGMA foreign_keys = OFF;");
await ExecuteAsync(connection, "ATTACH DATABASE $source AS legacy;", ("$source", sourcePath));

var tables = await ReadStringsAsync(
    connection,
    "select name from main.sqlite_master where type = 'table' and name not like 'sqlite_%' and name not like '__EFMigrations%' order by name;");

var copied = 0;
var skipped = 0;

foreach (var table in tables)
{
    if (!await LegacyTableExistsAsync(connection, table))
    {
        skipped++;
        continue;
    }

    var targetColumns = await ReadColumnsAsync(connection, "main", table);
    var sourceColumns = await ReadColumnsAsync(connection, "legacy", table);
    var common = targetColumns
        .Where(x => sourceColumns.Contains(x, StringComparer.OrdinalIgnoreCase))
        .ToList();

    if (common.Count == 0)
    {
        skipped++;
        continue;
    }

    var columnList = string.Join(", ", common.Select(Quote));
    var sql = $"INSERT OR IGNORE INTO main.{Quote(table)} ({columnList}) SELECT {columnList} FROM legacy.{Quote(table)};";

    try
    {
        var rows = await ExecuteAsync(connection, sql);
        copied++;
        Console.WriteLine($"{table}: скопировано {rows} строк (колонок: {common.Count}).");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"{table}: пропущено — {ex.Message}");
        skipped++;
    }
}

await ExecuteAsync(connection, "DETACH DATABASE legacy;");
await ExecuteAsync(connection, "PRAGMA foreign_keys = ON;");

Console.WriteLine($"Готово. Таблиц скопировано: {copied}, пропущено: {skipped}.");
Console.WriteLine("Публичный контент витрины загрузится при первом запуске DiAvto.Site.");
return 0;

static async Task<bool> LegacyTableExistsAsync(SqliteConnection connection, string table)
{
    await using var command = connection.CreateCommand();
    command.CommandText = "select count(*) from legacy.sqlite_master where type = 'table' and name = $name;";
    command.Parameters.AddWithValue("$name", table);
    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt64(result) > 0;
}

static async Task<List<string>> ReadColumnsAsync(SqliteConnection connection, string schema, string table)
{
    var columns = new List<string>();

    await using var command = connection.CreateCommand();
    command.CommandText = $"PRAGMA {schema}.table_info({Quote(table)});";

    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        // hidden = 2/3 — generated/virtual columns, их нельзя вставлять.
        var hidden = reader.FieldCount > 6 ? reader.GetInt32(6) : 0;
        if (hidden == 0)
        {
            columns.Add(reader.GetString(1));
        }
    }

    return columns;
}

static async Task<List<string>> ReadStringsAsync(SqliteConnection connection, string sql)
{
    var result = new List<string>();

    await using var command = connection.CreateCommand();
    command.CommandText = sql;

    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        result.Add(reader.GetString(0));
    }

    return result;
}

static async Task<int> ExecuteAsync(
    SqliteConnection connection,
    string sql,
    params (string Name, object Value)[] parameters)
{
    await using var command = connection.CreateCommand();
    command.CommandText = sql;

    foreach (var parameter in parameters)
    {
        command.Parameters.AddWithValue(parameter.Name, parameter.Value);
    }

    return await command.ExecuteNonQueryAsync();
}

static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";
