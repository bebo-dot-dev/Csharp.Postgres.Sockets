using Mediator;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Core.Incoming.Queries;

public sealed record GetTestEntityQuery(int TestEntityId) : IRequest<TestEntity?>;

public sealed class GetTestEntityQueryHandler : IRequestHandler<GetTestEntityQuery, TestEntity?>
{
    private readonly ITestEntityRepository _repository;

    public GetTestEntityQueryHandler(ITestEntityRepository repository)
    {
        _repository = repository;
    }
    
    public async ValueTask<TestEntity?> Handle(
        GetTestEntityQuery request, CancellationToken cancellationToken)
    {
        var testEntityData = await _repository.GetTestEntityAsync(request.TestEntityId, cancellationToken);
        return testEntityData.ToTestEntity();
    }
}