
using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string SenderName { get; set; }
        public string SenderIDNO { get; set; }
        public string SenderPhoneNo { get; set; }
        public string SenderAccount { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverIDNO { get; set; }
        public string ReceiverPhoneNo { get; set; }
        public string ReceiverAccount { get; set; }
        public string Amount { get; set; }
        public DateTime Date { get; set; }

    }
    public class TransactionViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string SenderName { get; set; }
        public string SenderIDNO { get; set; }
        public string SenderPhoneNo { get; set; }
        public string SenderAccount { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverIDNO { get; set; }
        public string ReceiverPhoneNo { get; set; }
        public string ReceiverAccount { get; set; }
        public string Amount { get; set; }
        public DateTime Date { get; set; }
    }
}


