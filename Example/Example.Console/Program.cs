using SqliteDatabaseInstaller;

var databaseName = "../../../appdata.db"; // Change this to any file name
var migrationFolder = Path.GetFullPath("Migrations");
var migrator = new SqliteMigrator(databaseName, migrationFolder);
migrator.Migrate();