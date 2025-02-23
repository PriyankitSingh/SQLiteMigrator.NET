using System.Data.SQLite;
using Dapper;

namespace SqliteDatabaseInstaller;

public class SqliteMigrator
{
    private readonly string _connectionString;
    private readonly string _migrationFolder = "./Migrations";
    
    public SqliteMigrator(string databaseName, string? migrationFolder = null)
    {
        _connectionString = $"Data Source={databaseName}";
        _migrationFolder = migrationFolder ?? _migrationFolder;
        Console.WriteLine($"Migration folder: {_migrationFolder}");
        // Ensure database file exists
        if (!File.Exists(databaseName))
        {
            Console.WriteLine("Creating Database file");
            SQLiteConnection.CreateFile(databaseName);
        }
    }

    public void Migrate()
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        // Ensure Migrations table exists
        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS Migrations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL UNIQUE,
                AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ";
        connection.Execute(createTableSql);

        // Get all migration scripts in order
        var files = Directory.GetFiles(_migrationFolder, "*.sql").OrderBy(f => f).ToList();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            // Check if migration was already applied
            var alreadyApplied = connection.QuerySingleOrDefault<int>(
                "SELECT COUNT(*) FROM Migrations WHERE FileName = @FileName",
                new { FileName = fileName });

            if (alreadyApplied == 0)
            {
                Console.WriteLine($"Applying migration: {fileName}");
                var sql = File.ReadAllText(file);
                connection.Execute(sql);

                // Mark migration as applied
                connection.Execute("INSERT INTO Migrations (FileName) VALUES (@FileName)", new { FileName = fileName });
            }
        }
    }
}