namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for initializing the database with seed data.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initializes the database and seeds default data if this is the first launch.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Forces re-seeding of default data (for testing purposes).
    /// </summary>
    Task SeedDefaultDataAsync();
}
