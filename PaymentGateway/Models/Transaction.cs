
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


