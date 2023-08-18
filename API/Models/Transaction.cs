//using Nest;
//using System.ComponentModel.DataAnnotations;

//namespace API.Models
//{
//    public class Transaction
//    {
//        public int Id { get; set; }
     
//        public string SenderName { get; set; }
       
//        public string SenderID_NO { get; set; }

//        public string SenderPhone_No { get; set; }
//        public string SenderSrc_Account { get; set; }
//        public string ReceiverName { get; set; }


//        public string ReceiverID_NO { get; set; }

//        public string ReceiverPhone_No { get; set; }

//        public string ReceiverDst_Account { get; set; }

//        public string RouteId { get; set; }

//        public string CategoryDescription { get; set; }
//        public int ChannelType { get; set; }
//        public string ChannelDescription { get; set; }

//        public string reference { get; set; }
//        public string? systemConversationId { get; internal set; }
//        public string? originatorConversationId { get; internal set; }
        

//        [Required]
//        [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive number.")]
//        public int Amount { get; set; }
//        public string? resultCode { get; set; }
//        public string? resultCodeDescription { get; set; }
//        public DateTime Date { get; set; }

//        public bool IsPosted { get; set; }
//    }
//}
