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
        
        public int SelectedRouteId { get; set; }
        public int SelectedCategoryId { get; set; }
        public int SelectedChannelTypeId { get; set; }
        public List<Route> TransactionRoutes { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> ChannelTypes { get; set; }

        public string Amount { get; set; }
        public DateTime Date { get; set; }

       
    }
}
