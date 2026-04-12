namespace QPhising.Application.Features.Setup;

public static class SetupSettingKeys
{
    public const string IsCompleted = "setup.is_completed";
    public const string CompletedAtUtc = "setup.completed_at_utc";
    public const string ValidatedDatabaseConfiguration = "setup.db.validated_configuration";
    public const string PersistedDatabaseConfiguration = "setup.db.persisted_configuration";
    public const string PersistedDatabaseConfigurationSavedAtUtc = "setup.db.persisted_configuration_saved_at_utc";
}
