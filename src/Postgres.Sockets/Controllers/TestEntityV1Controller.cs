using System.Net.WebSockets;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Postgres.Sockets.Core;
using Postgres.Sockets.Core.Incoming.Commands;
using Postgres.Sockets.Core.Incoming.Queries;
using WebSocketContext = Postgres.Sockets.Core.WebSocketContext;

namespace Postgres.Sockets.Controllers;

[ApiController]
[Route("/v1")]
public class TestEntityV1Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebSocketManager _webSocketManager;

    public TestEntityV1Controller(IMediator mediator, IWebSocketManager webSocketManager)
    {
        _mediator = mediator;
        _webSocketManager = webSocketManager;
    }
    
    /// <summary>
    /// The /ws websocket connect endpoint.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)] 
    [HttpGet("/ws/{ctxVal}")]
    public async Task WebsocketConnect([FromRoute]string ctxVal)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest && Enum.TryParse<WebSocketContextType>(ctxVal, true, out var ctxType))
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socketFinishedTcs = new TaskCompletionSource<WebSocket>();
                        
            await _webSocketManager.AddWebSocketAsync(
                new WebSocketContext(
                    ctxType, 
                    webSocket, 
                    socketFinishedTcs));
            await socketFinishedTcs.Task;    
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    
    /// <summary>
    /// Returns all test entities.
    /// </summary>
    /// <returns><see cref="TestEntitiesResponse"/></returns>
    [HttpGet("testEntities")]
    [ProducesResponseType(typeof(TestEntitiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestEntities()
    {
        var request = new GetTestEntitiesQuery();
        return Ok(await _mediator.Send(request));
    }
    
    /// <summary>
    /// Returns the test entity matching the supplied testEntityId.
    /// </summary>
    /// <param name="testEntityId">The testEntityId of the testEntity to return.</param>
    /// <returns><see cref="TestEntity"/></returns>
    [HttpGet("{testEntityId:int}")]
    [ProducesResponseType(typeof(TestEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestEntity([FromRoute]int testEntityId)
    {
        var request = new GetTestEntityQuery(testEntityId);
        var testEntity = await _mediator.Send(request);
        return testEntity is not null ? Ok(testEntity) : NotFound();
    }
    
    /// <summary>
    /// Inserts a new test entity.
    /// </summary>
    /// <param name="request">The request containing the name of the test entity to create.</param>
    /// <returns><see cref="TestEntity"/></returns>
    [HttpPost]
    [ProducesResponseType(typeof(TestEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Insert([FromBody]TestEntityRequest request)
    {
        var command = request.ToInsertTestEntityCommand();
        var testEntity = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(TestEntity), 
            new{testEntityId = testEntity?.TestEntityId}, 
            testEntity);
    }
    
    /// <summary>
    /// Updates the test entity matching the supplied testEntityId.
    /// </summary>
    /// <param name="testEntityId">The testEntityId of the testEntity to update.</param>
    /// <param name="request">The testEntity name to update to.</param> 
    /// <returns><see cref="TestEntity"/></returns>
    [HttpPatch("{testEntityId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromRoute]int testEntityId, [FromBody]TestEntityRequest request)
    {
        var command = new UpdateTestEntityCommand
        {
            TestEntityId = testEntityId,
            Name = request.Name!
        };
        var updated = await _mediator.Send(command);
        return updated ? NoContent() : NotFound();
    }
    
    /// <summary>
    /// Deletes the test entity matching the supplied testEntityId.
    /// </summary>
    /// <param name="testEntityId">The testEntityId of the test entity to delete.</param>
    /// <returns><see cref="NoContentResult"/></returns>
    [HttpDelete("{testEntityId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([FromRoute]int testEntityId)
    {
        var request = new DeleteTestEntityCommand(testEntityId);
        var deleted = await _mediator.Send(request);
        return deleted ? NoContent() : NotFound();
    }
}