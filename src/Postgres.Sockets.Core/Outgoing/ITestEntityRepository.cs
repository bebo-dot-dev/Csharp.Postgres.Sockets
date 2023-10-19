namespace Postgres.Sockets.Core.Outgoing;

public interface ITestEntityRepository
{
    Task<List<TestEntityData>> GetTestEntitiesAsync(
        CancellationToken cancellationToken);
    
    Task<TestEntityData?> GetTestEntityAsync(
        int testEntityId, CancellationToken cancellationToken);
    
    Task<TestEntityData> InsertTestEntityAsync(
        TestEntityData testEntity, CancellationToken cancellationToken);
    
    Task<bool> UpdateTestEntityAsync(
        TestEntityData testEntity, CancellationToken cancellationToken);
    
    Task<bool> DeleteTestEntityAsync(
        int testEntityId, CancellationToken cancellationToken);

    Task ListenForEntityChangesAsync(CancellationToken cancellationToken);
}