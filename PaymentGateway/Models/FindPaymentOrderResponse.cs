
public class OrderLine
{
    public Remitter remitter { get; set; }
    public Recipient recipient { get; set; }
    public TransactionDetails transaction { get; set; }
    public TransactionOutcome transactionOutcome { get; set; }
}

public class Recipient
{
    public string name { get; set; }
    public string address { get; set; }
    public string emailAddress { get; set; }
    public string phoneNumber { get; set; }
    public string mccmnc { get; set; }
    public string idType { get; set; }
    public string idNumber { get; set; }
    public string financialInstitution { get; set; }
    public object institutionIdentifier { get; set; }
    public string primaryAccountNumber { get; set; }
    public int ccy { get; set; }
    public string purpose { get; set; }
    public object country { get; set; }
    public object creditCustomerAccountId { get; set; }
    public object state { get; set; }
    public object city { get; set; }
    public object postalCode { get; set; }
    public object nationality { get; set; }
}

public class Remitter
{
    public string name { get; set; }
    public string address { get; set; }
    public string phoneNumber { get; set; }
    public string idType { get; set; }
    public object idIssuePlace { get; set; }
    public string idNumber { get; set; }
    public object idIssueDate { get; set; }
    public object idExpireDate { get; set; }
    public object nationality { get; set; }
    public string country { get; set; }
    public string ccy { get; set; }
    public string financialInstitution { get; set; }
    public string sourceOfFunds { get; set; }
    public string principalActivity { get; set; }
    public object dateOfBirth { get; set; }
    public object state { get; set; }
    public object city { get; set; }
    public object postalCode { get; set; }
}

public class FindPaymentOrderResponse
{
    public string id { get; set; }
    public string conversationId { get; set; }
    public string originatorConversationId { get; set; }
    public string remarks { get; set; }
    public bool isProcessed { get; set; }
    public List<OrderLine> orderLines { get; set; }
}

public class ThirdPartyPayload
{
    public object thirdPartyResultCode { get; set; }
    public object thirdPartyResultCodeDescription { get; set; }
    public object thirdPartyReceiptNumber { get; set; }
}

public class TransactionDetails
{
    public string routeId { get; set; }
    public double amount { get; set; }
    public string reference { get; set; }
    public string systemTraceAuditNumber { get; set; }
    public int type { get; set; }
    public int channelType { get; set; }
    public object customerAccountNo { get; set; }
    public string routeTransactionTypeId { get; set; }
    public object relationship { get; set; }
    
}

public class TransactionOutcome
{
    public string id { get; set; }
    public double paymentAmount { get; set; }
    public double feeAmount { get; set; }
    public string trackingNumber { get; set; }
    public int transactionStatus { get; set; }
    public string transactionStatusDescription { get; set; }
    public object resultCode { get; set; }
    public object resultCodeDescription { get; set; }
    public ThirdPartyPayload thirdPartyPayload { get; set; }
}

