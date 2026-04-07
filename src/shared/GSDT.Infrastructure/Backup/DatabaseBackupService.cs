
namespace GSDT.Infrastructure.Backup;

/// <summary>
/// Executes SQL Server BACKUP DATABASE via ADO.NET.
/// Called by Hangfire background job — never from HTTP request thread.
/// NĐ53 compliance: backup files stored on configured path with timestamp.
/// </summary>
public sealed class DatabaseBackupService(
    BackupDbContext db,
    IConfiguration configuration,
    ILogger<DatabaseBackupService> logger)
{
    /// <summary>Execute full database backup and update BackupRecord status.</summary>
    public async Task ExecuteBackupAsync(Guid recordId)
    {
        var record = await db.BackupRecords.FindAsync(recordId);
        if (record is null) return;

        record.MarkInProgress();
        await db.SaveChangesAsync();

        try
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' not configured");

            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            var backupDir = configuration["Backup:Directory"] ?? "/backups";
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{databaseName}_{timestamp}.bak";
            var filePath = Path.Combine(backupDir, fileName);

            // Ensure backup directory exists
            Directory.CreateDirectory(backupDir);

            // Execute BACKUP DATABASE command
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var sql = $"BACKUP DATABASE [{databaseName}] TO DISK = @FilePath WITH FORMAT, INIT, COMPRESSION";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@FilePath", filePath);
            command.CommandTimeout = 600; // 10 minutes for large databases

            await command.ExecuteNonQueryAsync();

            // Get file size
            var fileInfo = new FileInfo(filePath);
            var fileSize = fileInfo.Exists ? fileInfo.Length : 0;

            record.MarkCompleted(filePath, fileSize);
            logger.LogInformation("Backup completed: {FilePath} ({Size} bytes)", filePath, fileSize);
        }
        catch (Exception ex)
        {
            record.MarkFailed(ex.Message);
            logger.LogError(ex, "Backup failed for record {RecordId}", recordId);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>Execute restore drill: backup → restore to temp DB → verify → drop temp DB.</summary>
    public async Task ExecuteRestoreDrillAsync(Guid recordId)
    {
        var record = await db.BackupRecords.FindAsync(recordId);
        if (record is null) return;

        record.MarkInProgress();
        await db.SaveChangesAsync();

        try
        {
            // Step 1: Create a fresh backup first
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' not configured");

            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            var backupDir = configuration["Backup:Directory"] ?? "/backups";
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupFile = Path.Combine(backupDir, $"{databaseName}_drill_{timestamp}.bak");
            var tempDbName = $"{databaseName}_RestoreDrill_{timestamp}";

            Directory.CreateDirectory(backupDir);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Backup
            var backupSql = $"BACKUP DATABASE [{databaseName}] TO DISK = @FilePath WITH FORMAT, INIT, COMPRESSION";
            await using (var cmd = new SqlCommand(backupSql, connection))
            {
                cmd.Parameters.AddWithValue("@FilePath", backupFile);
                cmd.CommandTimeout = 600;
                await cmd.ExecuteNonQueryAsync();
            }

            // Restore to temp DB
            var restoreSql = $@"RESTORE DATABASE [{tempDbName}] FROM DISK = @FilePath
                WITH MOVE '{databaseName}' TO @DataFile,
                     MOVE '{databaseName}_log' TO @LogFile,
                     REPLACE";
            var dataFile = Path.Combine(backupDir, $"{tempDbName}.mdf");
            var logFile = Path.Combine(backupDir, $"{tempDbName}_log.ldf");

            await using (var cmd = new SqlCommand(restoreSql, connection))
            {
                cmd.Parameters.AddWithValue("@FilePath", backupFile);
                cmd.Parameters.AddWithValue("@DataFile", dataFile);
                cmd.Parameters.AddWithValue("@LogFile", logFile);
                cmd.CommandTimeout = 600;
                await cmd.ExecuteNonQueryAsync();
            }

            // Verify — check table count in restored DB
            var verifySql = $"SELECT COUNT(*) FROM [{tempDbName}].sys.tables";
            await using (var cmd = new SqlCommand(verifySql, connection))
            {
                var tableCount = (int)(await cmd.ExecuteScalarAsync())!;
                logger.LogInformation("Restore drill verified: {TableCount} tables in temp DB", tableCount);
            }

            // Cleanup — drop temp DB
            var dropSql = $"DROP DATABASE [{tempDbName}]";
            await using (var cmd = new SqlCommand(dropSql, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            var fileInfo = new FileInfo(backupFile);
            record.MarkCompleted(backupFile, fileInfo.Exists ? fileInfo.Length : 0);
            logger.LogInformation("Restore drill completed successfully");
        }
        catch (Exception ex)
        {
            record.MarkFailed(ex.Message);
            logger.LogError(ex, "Restore drill failed for record {RecordId}", recordId);
        }

        await db.SaveChangesAsync();
    }
}
