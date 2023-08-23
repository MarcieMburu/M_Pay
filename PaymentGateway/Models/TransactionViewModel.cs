using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Security.Policy;

namespace PaymentGateway.Models
{
    public class TransactionViewModel
    {


        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string SenderName { get; set; }
        [Display(Name = "Identification Number")]
        public string SenderID_NO { get; set; }

        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\+?254\d{9}$", ErrorMessage = "Invalid phone number format. Use format: +254XXXXXXXXX.")]
        public string SenderPhone_No { get; set; }
        [Display(Name = "Source Of Funds")]
        public string SenderSrc_Account { get; set; }
        [Display(Name = "Name")]
        public string ReceiverName { get; set; }

        [Display(Name = "Identification Number")]

        public string ReceiverID_NO { get; set; }
        [Display(Name = "Phone Number")]

        public string ReceiverPhone_No { get; set; }
        [Display(Name = "Funds Destination")]

        public string ReceiverDst_Account { get; set; }

        public string RouteId { get; set; }

        public string CategoryDescription { get; set; }
        public int ChannelType { get; set; }
        public string ChannelDescription { get; set; }
       
        [Display(Name = "Reference")]
        public string reference { get; set; }
        public string? systemConversationId { get;  set; }
        public string originatorConversationId { get;  set; }
        public string systemTraceAuditNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive number.")]
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

