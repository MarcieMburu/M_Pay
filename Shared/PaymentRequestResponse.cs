namespace Shared
{
    public class Message
    {
        public string appDomainName { get; set; }
        public string systemConversationId { get; set; }
        public string originatorConversationId { get; set; }
        public string remarks { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class PaymentRequestResponse
    {
        public Message message { get; set; }

    }
}