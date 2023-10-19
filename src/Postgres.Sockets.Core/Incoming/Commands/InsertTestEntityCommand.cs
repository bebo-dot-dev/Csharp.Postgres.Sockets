using Mediator;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Core.Incoming.Commands;

public sealed record InsertTestEntityCommand : TestEntityRequest, ICommand<TestEntity?>;

public sealed class InsertTestEntityCommandHandler : ICommandHandler<InsertTestEntityCommand, TestEntity?>
{
    private readonly ITestEntityRepository _repository;

    public InsertTestEntityCommandHandler(ITestEntityRepository repository)
    {
        _repository = repository;
    }
    
    public async ValueTask<TestEntity?> Handle(
        InsertTestEntityCommand command, CancellationToken cancellationToken)
    {
        var testEntityData = new TestEntityData { Name = command.Name! };
        testEntityData = await _repository.InsertTestEntityAsync(testEntityData, cancellationToken);
        return testEntityData.ToTestEntity();
    }
}