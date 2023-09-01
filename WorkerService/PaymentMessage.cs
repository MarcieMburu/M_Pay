using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService
{
    public class PaymentMessage
    {
     
        public int Id { get; set; }
     
        public string SenderName { get; set; }
        
        public string SenderID_NO { get; set; }

      
        public string SenderPhone_No { get; set; }
     
        public string SenderSrc_Account { get; set; }
     
        public string ReceiverName { get; set; }

      

        public string ReceiverID_NO { get; set; }
       
        public string ReceiverPhone_No { get; set; }
        
        public string ReceiverDst_Account { get; set; }

        public string RouteId { get; set; }

        public string CategoryDescription { get; set; }
        public int ChannelType { get; set; }
        public string ChannelDescription { get; set; }

       
        public string reference { get; set; }
        public string? systemConversationId { get; internal set; }
        public string? originatorConversationId { get; internal set; }

        public int Amount { get; set; }
        public string? resultCode { get; set; }
        public string? resultCodeDescription { get; set; }
        public DateTime Date { get; set; }

        public bool IsPosted { get; set; }
    }
}
