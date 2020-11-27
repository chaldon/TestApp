using System;
using System.Collections.Generic;

#nullable disable

namespace TestApp.Model
{
    public partial class Order
    {
        public int OrderId { get; set; }
        public int OfferId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public int CustomerId { get; set; }
        public bool Cancelled { get; set; }
        public bool Paid { get; set; }
        public string Reason { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Offer Offer { get; set; }
    }
}
