using Mediator;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Core.Incoming.Queries;

public sealed record GetTestEntitiesQuery : IRequest<TestEntitiesResponse>;

public sealed class GetTestEntitiesQueryHandler : IRequestHandler<GetTestEntitiesQuery, TestEntitiesResponse>
{
    private readonly ITestEntityRepository _repository;

    public GetTestEntitiesQueryHandler(ITestEntityRepository repository)
    {
        _repository = repository;
    }
    
    public async ValueTask<TestEntitiesResponse> Handle(
        GetTestEntitiesQuery request, CancellationToken cancellationToken)
    {
        var data = await _repository.GetTestEntitiesAsync(cancellationToken);
        return new TestEntitiesResponse
        {
            TestEntities = data.ToTestEntities()
        };
    }
}