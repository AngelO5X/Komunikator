using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/messages")]
public class PrivateMessagesController : ControllerBase
{
    private readonly DmsDbContext _context;

    public PrivateMessagesController(DmsDbContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrivateMessageRequest request)
    {
        var message = new PrivateMessage
        {
            SenderUUID = request.SenderUUID,
            ReceiverUUID = request.ReceiverUUID,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.PrivateMessages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new { MessageId = message.MessageId });
    }

    [HttpGet("{userA:guid}/{userB:guid}")]
    public async Task<IActionResult> GetConversation(Guid userA, Guid userB)
    {
        var msgs = await _context.PrivateMessages
            .Where(m => (m.SenderUUID == userA && m.ReceiverUUID == userB) || (m.SenderUUID == userB && m.ReceiverUUID == userA))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new {
                m.MessageId,
                m.SenderUUID,
                m.ReceiverUUID,
                m.Content,
                m.CreatedAt
            })
            .ToListAsync();

        return Ok(msgs);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(Guid userId)
    {
        var msgs = await _context.PrivateMessages
            .Where(m => m.SenderUUID == userId || m.ReceiverUUID == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new {
                m.MessageId,
                m.SenderUUID,
                m.ReceiverUUID,
                m.Content,
                m.CreatedAt
            })
            .ToListAsync();

        return Ok(msgs);
    }
}
