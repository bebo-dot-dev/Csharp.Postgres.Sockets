using Mediator;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Core.Incoming.Commands;

public sealed record UpdateTestEntityCommand : TestEntity, ICommand<bool>;

public sealed class UpdateTestEntityCommandHandler : ICommandHandler<UpdateTestEntityCommand, bool>
{
    private readonly ITestEntityRepository _repository;

    public UpdateTestEntityCommandHandler(ITestEntityRepository repository)
    {
        _repository = repository;
    }
    
    public async ValueTask<bool> Handle(
        UpdateTestEntityCommand command, CancellationToken cancellationToken)
    {
        var testEntityData = new TestEntityData { TestEntityId = command.TestEntityId, Name = command.Name! };
        return await _repository.UpdateTestEntityAsync(testEntityData, cancellationToken);
    }
}