using System;
using System.Collections.Generic;

#nullable disable

namespace TestApp.Model
{
    public partial class Product
    {
        public Product()
        {
            Offers = new HashSet<Offer>();
        }

        public int ProductId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateModified { get; set; }
        public string Term { get; set; }
        public int BrandId { get; set; }

        public virtual Brand Brand { get; set; }
        public virtual ICollection<Offer> Offers { get; set; }
    }
}
