using Postgres.Sockets.Core.Incoming.Commands;
using Postgres.Sockets.Core.Outgoing;
using Riok.Mapperly.Abstractions;

namespace Postgres.Sockets.Core;

[Mapper]
public static partial class MapperExtensions
{
    public static partial List<TestEntity> ToTestEntities(this List<TestEntityData> input);
    
    public static partial TestEntity? ToTestEntity(this TestEntityData? input);
    
    public static partial InsertTestEntityCommand ToInsertTestEntityCommand(this TestEntityRequest input);
}