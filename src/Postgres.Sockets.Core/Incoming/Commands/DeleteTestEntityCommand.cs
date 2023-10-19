using Mediator;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Core.Incoming.Commands;

public sealed record DeleteTestEntityCommand(int TestEntityId) : ICommand<bool>;

public sealed class DeleteTestEntityCommandHandler : ICommandHandler<DeleteTestEntityCommand, bool>
{
    private readonly ITestEntityRepository _repository;

    public DeleteTestEntityCommandHandler(ITestEntityRepository repository)
    {
        _repository = repository;
    }
    
    public async ValueTask<bool> Handle(
        DeleteTestEntityCommand command, CancellationToken cancellationToken)
    {
        return await _repository.DeleteTestEntityAsync(
            command.TestEntityId, cancellationToken);
    }
}