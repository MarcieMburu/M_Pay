
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
        public string systemTraceAuditNumber { get; set; }
        public string? systemConversationId { get; internal set; }
        public string originatorConversationId { get; internal set; }
        public string reference { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        
    }
    
}


