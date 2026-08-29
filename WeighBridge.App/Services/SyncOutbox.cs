using Microsoft.Data.Sqlite;

namespace WeightBridgeApp.Services;

/// <summary>
/// Transactional outbox for Hub push sync. Call inside the same SQLite transaction as the business write.
/// </summary>
public static class SyncOutbox
{
    public const string OperationUpsert = "Upsert";

    public static void Enqueue(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string entityType,
        string entityKey,
        string operation = OperationUpsert)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (string.IsNullOrWhiteSpace(entityKey))
            throw new ArgumentException("Entity key is required.", nameof(entityKey));

        var enqueuedUtc = DateTime.UtcNow.ToString("o");

        using var updateCommand = connection.CreateCommand();
        updateCommand.Transaction = transaction;
        updateCommand.CommandText = """
            UPDATE sync_outbox
            SET EnqueuedUtc = $EnqueuedUtc,
                Operation = $Operation,
                Status = 'Pending',
                RetryCount = 0,
                LastError = NULL
            WHERE EntityType = $EntityType
              AND EntityKey = $EntityKey
              AND Status IN ('Pending', 'Failed');
            """;
        updateCommand.Parameters.AddWithValue("$EnqueuedUtc", enqueuedUtc);
        updateCommand.Parameters.AddWithValue("$Operation", operation);
        updateCommand.Parameters.AddWithValue("$EntityType", entityType.Trim());
        updateCommand.Parameters.AddWithValue("$EntityKey", entityKey.Trim());

        var updated = updateCommand.ExecuteNonQuery();
        if (updated > 0)
            return;

        using var insertCommand = connection.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = """
            INSERT INTO sync_outbox
            (EntityType, EntityKey, Operation, Status, RetryCount, EnqueuedUtc)
            VALUES
            ($EntityType, $EntityKey, $Operation, 'Pending', 0, $EnqueuedUtc);
            """;
        insertCommand.Parameters.AddWithValue("$EntityType", entityType.Trim());
        insertCommand.Parameters.AddWithValue("$EntityKey", entityKey.Trim());
        insertCommand.Parameters.AddWithValue("$Operation", operation);
        insertCommand.Parameters.AddWithValue("$EnqueuedUtc", enqueuedUtc);
        insertCommand.ExecuteNonQuery();
    }
}
