using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TestApp.Models
{
     /// <summary>
     /// Brand ViewModel
     /// </summary>
    public class BrandViewModel
    {
        /// <summary>
        /// BrandId
        /// </summary>
        public int BrandId { get; set; }
        /// <summary>
        /// Brannd name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Date Modified
        /// </summary>
        public DateTime DateModified { get; set; }
    }

    /// <summary>
    /// Customer ViewModel
    /// </summary>
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateModified { get; set; }
    }

    /// <summary>
    /// Offer ViewModel
    /// </summary>
    public class OfferViewModel
    {
        public int OfferId { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int NumberOfTerms { get; set; }
        public DateTime DateModified { get; set; }
    }

    /// <summary>
    /// Order ViewModel
    /// </summary>
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public int OfferId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateModified { get; set; }
        public bool Cancelled { get; set; }
        public string Reason { get; set; }
        public bool Paid { get; set; }
    }

    /// <summary>
    /// Prodicet ViewModel
    /// </summary>
    public class ProductViewModel
    {
        /// <summary>
        /// ProductId
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Product Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Product Status
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Product term (monthly or annualy)
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// Modification Date
        /// </summary>
        public DateTime DateModified { get; set; }
        /// <summary>
        /// Brand Id
        /// </summary>
        public int BrandId { get; set; }
    }

    /// <summary>
    /// Create or Update Product ViewModel
    /// </summary>
    public class CreateUpdateProductViewModel
    {
        /// <summary>
        /// Product Name
        /// </summary>
        [Required(AllowEmptyStrings = false),StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Product term (monthly or annualy)
        /// </summary>
        [Required]
        [DefaultValue(true)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Product term (monthly or annualy)
        /// </summary>
        [Required(AllowEmptyStrings = false), StringLength(50)]
        public string Term { get; set; }

        /// <summary>
        /// Brand Id
        /// </summary>
        [Required]
        public int BrandId { get; set; }
    }

    /// <summary>
    /// Create or Update Brand ViewModel
    /// </summary>
    public class CreateUpdateBrandViewModel
    {
        /// <summary>
        /// Brand name
        /// </summary>
        [Required(AllowEmptyStrings = false), StringLength(50)]
        public string Brand { get; set; }
}

    /// <summary>
    /// Create or Update Customer ViewModel
    /// </summary>
    public class CreateUpdateCustomerViewModel
    {
        [Required(AllowEmptyStrings = false), StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false), StringLength(100)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false), StringLength(100)]
        public string LastName { get; set; }
    }

    /// <summary>
    /// Create or Update Offer ViewModel
    /// </summary>
    public class CreateUpdateOfferViewModel
    {
        [Required]
        public int ProductId { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required,Range(1, int.MaxValue)]
        public int NumberOfTerms { get; set; }
    }

    /// <summary>
    /// Create Order ViewModel
    /// </summary>
    public class CreateOrderViewModel
    {
        [Required]
        public int OfferId { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
    }

    /// <summary>
    /// Update Order ViewModel
    /// </summary>
    public class UpdateOrderViewModel
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public bool Cancelled { get; set; }
        [StringLength(50)]
        public string Reason { get; set; }
        [Required]
        public bool Paid { get; set; }
    }

}
