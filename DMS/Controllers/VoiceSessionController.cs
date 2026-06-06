using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/voice")]
public class VoiceSessionController : ControllerBase
{
    private static readonly Dictionary<string, VoiceSession> Sessions = new();

    [HttpPost("create")]
    [Authorize]
    public IActionResult CreateSession()
    {
        var id = Guid.NewGuid().ToString();
        var session = new VoiceSession { Id = id, CreatedAt = DateTime.UtcNow };
        Sessions[id] = session;
        return Ok(new { sessionId = id });
    }

    [HttpPost("{sessionId}/join")]
    [Authorize]
    public IActionResult JoinSession(string sessionId, [FromBody] JoinSessionRequest req)
    {
        if (!Sessions.TryGetValue(sessionId, out var session))
            return NotFound("Sesja nie istnieje.");

        lock (session)
        {
            if (!session.Participants.Contains(req.UserId))
                session.Participants.Add(req.UserId);
        }

        return Ok(new { sessionId = sessionId });
    }

    [HttpPost("{sessionId}/upload-audio")]
    [Authorize]
    public IActionResult UploadAudio(string sessionId, [FromBody] AudioChunkRequest chunk)
    {
        if (!Sessions.TryGetValue(sessionId, out var session))
            return NotFound("Sesja nie istnieje.");

        lock (session)
        {
            session.Chunks.Add(new AudioChunk { SenderId = chunk.SenderId, Data = chunk.Base64Data, Timestamp = DateTime.UtcNow });
            if (session.Chunks.Count > 200) session.Chunks.RemoveAt(0);
        }

        return Ok();
    }

    [HttpGet("{sessionId}/poll/{userId}")]
    [Authorize]
    public IActionResult Poll(string sessionId, string userId)
    {
        if (!Sessions.TryGetValue(sessionId, out var session))
            return NotFound("Sesja nie istnieje.");

        List<AudioChunk> result;
        lock (session)
        {
            result = session.Chunks.Where(c => c.SenderId != userId).ToList();
            session.Chunks.Clear();
        }

        return Ok(result);
    }
}

public class VoiceSession
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Participants { get; } = new();
    public List<AudioChunk> Chunks { get; } = new();
}

public class AudioChunk
{
    public string SenderId { get; set; }
    public string Data { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AudioChunkRequest
{
    public string SenderId { get; set; }
    public string Base64Data { get; set; }
}

public class JoinSessionRequest
{
    public string UserId { get; set; }
}
