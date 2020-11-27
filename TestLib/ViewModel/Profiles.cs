using AutoMapper;
using TestApp.Model;

namespace TestApp.Models
{
    /// <summary>
    /// AutoMapper support 
    /// </summary>
    public class BrandProfile: Profile
    {
        public BrandProfile()
        {
            CreateMap<Brand, BrandViewModel>();
            CreateMap<BrandViewModel, Brand>();
        }
    }

    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerViewModel>();
            CreateMap<CustomerViewModel, Customer>();
        }
    }

    public class OfferProfile : Profile
    {
        public OfferProfile()
        {
            CreateMap<Offer, OfferViewModel>();
            CreateMap<OfferViewModel, Offer>();
        }
    }

    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderViewModel>();
            CreateMap<OrderViewModel, Order>();
        }
    }

    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductViewModel>();
            CreateMap<ProductViewModel, Product>();
        }
    }
}
