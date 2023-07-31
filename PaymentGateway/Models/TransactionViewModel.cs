using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;



namespace PaymentGateway.Models
{
    public class TransactionViewModel
    {


        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string SenderName { get; set; }
        public string SenderID_NO { get; set; }
        public string SenderPhone_No { get; set; }
        public string SenderSrc_Account { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverID_NO { get; set; }
        public string ReceiverPhone_No { get; set; }
        public string ReceiverDst_Account { get; set; }
       
           
        public string Amount { get; set; }
        public DateTime Date { get; set; }

       
    }
}
