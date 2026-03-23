using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

public static class VoiceWebSocketHandler
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> Sessions = new();

    public static async Task Handle(HttpContext context, string sessionId, string userId)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var participants = Sessions.GetOrAdd(sessionId, _ => new ConcurrentDictionary<string, WebSocket>());
        participants[userId] = webSocket;

        try
        {
            var buffer = new byte[16 * 1024];
            var seg = new ArraySegment<byte>(buffer);

            while (webSocket.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(seg, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                        break;
                    }

                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close) break;

                var messageBytes = ms.ToArray();

                // Broadcast binary message to other participants in the same session
                foreach (var kv in participants)
                {
                    if (kv.Key == userId) continue;
                    var ws = kv.Value;
                    if (ws.State == WebSocketState.Open)
                    {
                        try
                        {
                            await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Binary, true, CancellationToken.None);
                        }
                        catch
                        {
                            // ignore send errors
                        }
                    }
                }
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            if (Sessions.TryGetValue(sessionId, out var part))
            {
                part.TryRemove(userId, out _);
                if (part.IsEmpty) Sessions.TryRemove(sessionId, out _);
            }

            try { if (webSocket.State != WebSocketState.Closed) await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None); } catch { }
            webSocket.Dispose();
        }
    }
}
