using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

[ApiController]
[Route("ws/signal")]
public class SignalController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentQueue<SignalMessage>>> Queues = new();

    [Authorize]
    [HttpPost("{sessionId}/{toUserId}")]
    public IActionResult Send(string sessionId, string toUserId, [FromBody] SignalMessage msg)
    {
        var session = Queues.GetOrAdd(sessionId, _ => new ConcurrentDictionary<string, ConcurrentQueue<SignalMessage>>());
        var q = session.GetOrAdd(toUserId, _ => new ConcurrentQueue<SignalMessage>());
        q.Enqueue(msg);
        return Ok();
    }

    [Authorize]
    [HttpGet("{sessionId}/poll/{userId}")]
    public IActionResult Poll(string sessionId, string userId)
    {
        var session = Queues.GetOrAdd(sessionId, _ => new ConcurrentDictionary<string, ConcurrentQueue<SignalMessage>>());
        var q = session.GetOrAdd(userId, _ => new ConcurrentQueue<SignalMessage>());

        var list = new List<SignalMessage>();
        while (q.TryDequeue(out var msg)) list.Add(msg);
        return Ok(list);
    }
}

public class SignalMessage
{
    public string From { get; set; }
    public string Type { get; set; }
    public string Payload { get; set; }
}
