namespace Shared
{
    public class PaymentOrderLine
    {
        public RecipientItem recipient { get; set; }
        public TransactionItem transaction { get; set; }

        public RemitterItem remitter { get; set; }
    }

    public class RecipientItem
    {
        public string name { get; set; }
        public string address { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string idType { get; set; }
        public string idNumber { get; set; }
        public string financialInstitution { get; set; }
        public string primaryAccountNumber { get; set; }
        public string? institutionIdentifier { get; set; }
        public string mccmnc { get; set; }
        public int ccy { get; set; }
        public string country { get; set; }
        public string purpose { get; set; }
    }

    public class RemitterItem
    {
        public string name { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string idType { get; set; }
        public string idIssuePlace { get; set; }
        public string idNumber { get; set; }
        public string idIssueDate { get; set; }
        public string idExpireDate { get; set; }
        public string nationality { get; set; }
        public string country { get; set; }
        public int ccy { get; set; }
        public string financialInstitution { get; set; }
        public string sourceOfFunds { get; set; }
        public string principalActivity { get; set; }
        public string dateOfBirth { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
    }


    public class ZamupayRequest
    {
        public string originatorConversationId { get; set; }
        public string paymentNotes { get; set; }
        public List<PaymentOrderLine> paymentOrderLines { get; set; }
    }

    public class TransactionItem
    {
        public string routeId { get; set; }
        public int ChannelType { get; set; }
        public int amount { get; set; }
        public string reference { get; set; }
        public string systemTraceAuditNumber { get; set; }
    }
}
