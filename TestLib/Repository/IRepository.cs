using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestApp.Model;

namespace TestApp.Repository
{
    public interface IRepository
    {
        Task<Result<Brand>> CreateBrandAsync(string name);
        Task<Result<Customer>> CreateCustomerAsync(string emailAddress, string firstName, string lastName);
        Task<Result<Offer>> CreateOfferAsync(int productId, string description, decimal price, int numberOfTerms);
        Task<Result<Order>> CreateOrderAsync(int offerId, DateTime startDate, int customerId);
        Task<Result<Product>> CreateProductAsync(string name, bool isActive, string term, int brandId);
        Task<Result<int>> DeleteBrandAsync(int id);
        Task<Result<int>> DeleteCustomerAsync(int id);
        Task<Result<int>> DeleteOfferAsync(int id);
        Task<Result<int>> DeleteOrderAsync(int id);
        Task<Result<int>> DeleteProductAsync(int id);
        Task<IEnumerable<Brand>> GetAllBrandsAsync(int? pageNumber, int pageSize, string name);
        Task<IEnumerable<Customer>> GetAllCustomersAsync(int? pageNumber, int pageSize, string name, string email, int? offerId);
        Task<IEnumerable<Offer>> GetAllOffersAsync(int? pageNumber, int pageSize, string description, int? productId, bool? activeProduct);
        Task<IEnumerable<Order>> GetAllOrdersAsync(int? pageNumber, int pageSize, int? offerId, int? customerId);
        Task<IEnumerable<Product>> GetAllProductsAsync(int? pageNumber, int pageSize, string name, string brand, bool? isActive);
        Task<Brand> GetBrandAsync(int id);
        Task<Customer> GetCustomerAsync(int id);
        Task<Offer> GetOfferAsync(int id);
        Task<Order> GetOrderAsync(int id);
        Task<Product> GetProductAsync(int id);
        Task<Result<Brand>> UpdateBrandAsync(int id, string name);
        Task<Result<Customer>> UpdateCustomerAsync(int id, string emailAddress, string firstName, string lastName);
        Task<Result<Offer>> UpdateOfferAsync(int id, int productId, string description, decimal price, int numberOfTerms);
        Task<Result<Order>> UpdateOrderAsync(int id, bool paid, bool cancelled, string reason);
        Task<Result<Product>> UpdateProductAsync(int id, string name, bool isActive, string term, int brandId);
    }
}