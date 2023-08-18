using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public Sender Sender { get; set; }
        public Receiver Receiver { get; set; }

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
