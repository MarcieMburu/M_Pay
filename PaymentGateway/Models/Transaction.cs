
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;


namespace PaymentGateway.Models
{
    public class Transaction
    {
        [Key]
        
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public Sender Sender { get; set; }
        public Receiver Receiver { get; set; }
        public string RouteId { get; set; }
        public string CategoryDescription { get; set; }
        public int ChannelType { get; set; }
        public string ChannelDescription { get; set; }
        public string systemTraceAuditNumber { get;  set; }
      
        public string? systemConversationId { get;  set; }
        public string originatorConversationId { get;  set; }
        public string reference { get; set; }
        public int Amount { get; set; }
        public string? resultCode { get; set; }
        public string? resultCodeDescription { get; set; }
        public DateTime Date { get; set; }
        public bool IsPosted { get; set; }
        public string CreatedBy { get; set; }

        public int transactionStatus { get; set; }
        public string? transactionStatusDescription { get; set; }
        public bool IsStatusUpdated { get; set; }
    }
    
}


