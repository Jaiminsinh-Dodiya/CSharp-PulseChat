using System;

namespace PulseChatServer.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }  // "text" or "image"
        public string ImagePath { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
