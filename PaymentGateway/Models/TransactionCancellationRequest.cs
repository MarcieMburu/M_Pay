namespace PaymentGateway.Models
{
    public class PaymentOrderline
    {
        public string originatorConversationId { get; set; }
        public int transactionStatus { get; set; }
        public string transactionStatusDescription { get; set; }
        public string resultCode { get; set; }
        public string resultCodeDescription { get; set; }
    }

    public class TransactionCancellationRequest
    {
        public List<PaymentOrderline> paymentOrderlines { get; set; }
    }
  
}
