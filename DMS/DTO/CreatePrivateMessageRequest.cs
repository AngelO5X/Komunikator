using System;

public class CreatePrivateMessageRequest
{
    public string Content { get; set; }

    public Guid ReceiverUUID { get; set; }

    public Guid SenderUUID { get; set; }
}
