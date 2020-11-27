using System;
using System.Collections.Generic;

#nullable disable

namespace TestApp.Model
{
    public partial class Brand
    {
        public Brand()
        {
            Products = new HashSet<Product>();
        }

        public int BrandId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
