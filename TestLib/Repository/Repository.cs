using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TestApp.Model;

namespace TestApp.Repository
{
    public class Repository : IRepository
    {
        private readonly ILogger<Repository> _logger;
        private readonly TestAppContext _dbContext;

        public Repository(TestAppContext dbContext, ILogger<Repository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region Product API
        public async Task<IEnumerable<Product>> GetAllProductsAsync(int? pageNumber, int pageSize, string name, string brand, bool? isActive)
        {
            _logger.LogDebug("GetAllProductsAsync {0} {1} {2} {3}", pageNumber, pageSize, name, brand);

            IQueryable<Product> query = _dbContext.Products.OrderBy(row => row.ProductId); ;

            if (name != null) query = query.Where(row => row.Name.Contains(name));
            if (brand != null) query = query.Where(row => row.Brand.Name.Contains(brand));
            if (isActive.HasValue) query = query.Where(row => row.IsActive == isActive);

            return await PaginatedList<Product>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize);
        }

        public async Task<Product> GetProductAsync(int id)
        {
            _logger.LogDebug("GetProductAsync {0}", id);

            return await _dbContext.Products.FindAsync(id);
        }

        public async Task<Result<Product>> CreateProductAsync(string name, bool isActive, string term, int brandId)
        {
            _logger.LogDebug("CreateProductAsync {0} {1} {2} {3}", name, isActive, term, brandId);

            var brand = await _dbContext.Brands.FindAsync(brandId);

            if (brand == null) return Result<Product>.Fail($"Brand with id = {brandId} does not exsts");

            var product = new Product()
            {
                Name = name,
                IsActive = isActive,
                Term = term,
                BrandId = brandId
            };

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            return Result<Product>.Ok(product);
        }

        public async Task<Result<Product>> UpdateProductAsync(int id, string name, bool isActive, string term, int brandId)
        {
            _logger.LogDebug("UpdateProductAsync {0} {1} {2} {3} {4}", id, name, isActive, term, brandId);

            var brand = await _dbContext.Brands.FindAsync(brandId);
            if (brand == null) return Result<Product>.Fail($"Brand with id = {brandId} does not exsts");

            var product = await _dbContext.Products.FindAsync(id);
            if (product == null) return Result<Product>.Fail($"Product with id = {id} does not exsts");

            product.Name = name;
            product.IsActive = isActive;
            product.Term = term;
            product.Brand = brand;

            _dbContext.Update(product);
            await _dbContext.SaveChangesAsync();

            return Result<Product>.Ok(product);
        }

        public async Task<Result<int>> DeleteProductAsync(int id)
        {
            _logger.LogDebug("DeleteProductAsync {0}", id);

            var product = await _dbContext.Products.FindAsync(id);
            if (product == null) return Result<int>.Ok(id); // does not exists - so OK

            if (await _dbContext.Offers.AnyAsync(row => row.ProductId == id)) return Result<int>.Fail($"Product with id = {id} can't be deleted (referenced by some Offer)");

            _dbContext.Products.Remove(product);
            return Result<int>.Ok(id);
        }

        #endregion

        #region Brand
        public async Task<IEnumerable<Brand>> GetAllBrandsAsync(int? pageNumber, int pageSize, string name)
        {
            _logger.LogDebug("GetAllBrandsAsync {0} {1} {2}", pageNumber, pageSize, name);

            IQueryable<Brand> query = _dbContext.Brands.OrderBy(row => row.BrandId); ;

            if (name != null) query = query.Where(row => row.Name.Contains(name));

            return await PaginatedList<Brand>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize);
        }

        public async Task<Brand> GetBrandAsync(int id)
        {
            _logger.LogDebug("GetBrandAsync {0}", id);

            return await _dbContext.Brands.FindAsync(id);
        }

        public async Task<Result<Brand>> CreateBrandAsync(string name)
        {
            _logger.LogDebug("CreateBrandAsync {0}", name);

            var brand = new Brand() { Name = name };

            _dbContext.Brands.Add(brand);

            await _dbContext.SaveChangesAsync();
            return Result<Brand>.Ok(brand);
        }

        public async Task<Result<Brand>> UpdateBrandAsync(int id, string name)
        {
            _logger.LogDebug("UpdateBrandAsync {0} {1}", id, name);

            var brand = _dbContext.Brands.Find(id);
            if (brand == null) return Result<Brand>.Fail($"Brand with id = {id} does not exsts");

            brand.Name = name;

            _dbContext.Update(brand);
            await _dbContext.SaveChangesAsync();

            return Result<Brand>.Ok(brand);
        }

        public async Task<Result<int>> DeleteBrandAsync(int id)
        {
            _logger.LogDebug("DeleteBrandAsync {0}", id);

            var brand = _dbContext.Brands.Find(id);
            if (brand == null) return Result<int>.Ok(id);

            if (await _dbContext.Products.AnyAsync(row => row.BrandId == id)) return Result<int>.Fail($"Brand with id = {id} can't be deleted (referenced by some Product)");

            _dbContext.Brands.Remove(brand);
            await _dbContext.SaveChangesAsync();

            return Result<int>.Ok(id);
        }
        #endregion

        #region Customer
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync(int? pageNumber, int pageSize, string name, string email, int? offerId)
        {
            _logger.LogDebug("GetAllCustomersAsync {0} {1} {2} {3}", pageNumber, pageSize, name, email);

            IQueryable<Customer> query = _dbContext.Customers.OrderBy(row => row.CustomerId);

            if (name != null) query = query.Where(row => row.FirstName.Contains(name) || row.LastName.Contains(name));
            if (email != null) query = query.Where(row => row.EmailAddress.Contains(email));
            if (offerId.HasValue) query = query.Where(row => row.Orders.Any(order => order.OfferId == offerId));

            return await PaginatedList<Customer>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize);
        }

        public async Task<Customer> GetCustomerAsync(int id)
        {
            _logger.LogDebug("GetCustomerAsync {0}", id);

            return await _dbContext.Customers.FindAsync(id);
        }

        public async Task<Result<Customer>> CreateCustomerAsync(string emailAddress, string firstName, string lastName)
        {
            _logger.LogDebug("CreateCustomerAsync {0} {1} {2}", emailAddress, firstName, lastName);

            var customer = await _dbContext.Customers.FirstOrDefaultAsync(row => row.EmailAddress == emailAddress);
            if (customer != null) return Result<Customer>.Fail($"e-mail {emailAddress} alerady regstered");

            customer = new Customer()
            {
                EmailAddress = emailAddress,
                FirstName = firstName,
                LastName = lastName
            };

            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            return Result<Customer>.Ok(customer);
        }

        public async Task<Result<Customer>> UpdateCustomerAsync(int id, string emailAddress, string firstName, string lastName)
        {
            _logger.LogDebug("UpdateCustomerAsync {0} {1} {2} {3}", id, emailAddress, firstName, lastName);

            var customer = await _dbContext.Customers.FindAsync(id);
            if (customer == null) return Result<Customer>.Fail($"Customer with id = {id} does not exist");

            customer.EmailAddress = emailAddress;
            customer.FirstName = firstName;
            customer.LastName = lastName;

            _dbContext.Update(customer);
            await _dbContext.SaveChangesAsync();

            return Result<Customer>.Ok(customer);
        }

        public async Task<Result<int>> DeleteCustomerAsync(int id)
        {
            _logger.LogDebug("DeleteCustomerAsync {0}", id);

            var customer = await _dbContext.Customers.FindAsync(id);
            if (customer == null) return Result<int>.Ok(id);
            if (await _dbContext.Orders.AnyAsync(row => row.CustomerId == id)) return Result<int>.Fail($"Customer with id = {id} can't be deleted (referenced by some Order)");

            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();

            return Result<int>.Ok(id);
        }
        #endregion

        #region Offer
        public async Task<IEnumerable<Offer>> GetAllOffersAsync(int? pageNumber, int pageSize, string description, int? productId, bool? activeProduct)
        {
            _logger.LogDebug("GetAllOffersAsync {0} {1} {2} {3} {4}", pageNumber, pageSize, description, productId, activeProduct);

            IQueryable<Offer> query = _dbContext.Offers.OrderBy(row => row.OfferId);

            if (description != null) query = query.Where(row => row.Description.Contains(description));
            if (productId.HasValue) query = query.Where(row => row.ProductId == productId);
            if (activeProduct.HasValue) query = query.Where(row => row.Product.IsActive == activeProduct);

            return await PaginatedList<Offer>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize);
        }

        public async Task<Offer> GetOfferAsync(int id)
        {
            _logger.LogDebug("GeOfferAsync {0}", id);

            return await _dbContext.Offers.FindAsync(id);
        }

        public async Task<Result<Offer>> CreateOfferAsync(int productId, string description, decimal price, int numberOfTerms)
        {
            _logger.LogDebug("CreateOfferAsync {0} {1} {2} {3}", productId, description, price, numberOfTerms);

            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null) return Result<Offer>.Fail($"Product with id = {productId} does not exist");

            var offer = new Offer() { ProductId = productId, Description = description, Price = price, NumberOfTerms = numberOfTerms };

            await _dbContext.Offers.AddAsync(offer);
            await _dbContext.SaveChangesAsync();

            return Result<Offer>.Ok(offer);
        }

        public async Task<Result<Offer>> UpdateOfferAsync(int id, int productId, string description, decimal price, int numberOfTerms)
        {
            _logger.LogDebug("UpdateOfferAsync {0} {1} {2} {3} {4}", id, productId, description, price, numberOfTerms);

            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null) return Result<Offer>.Fail($"Product with id = {productId} does not exist");

            var offer = await _dbContext.Offers.FindAsync(id);
            if (offer == null) return Result<Offer>.Fail($"Offer with id = {id} does not exist");

            offer.Product = product;
            offer.Description = description;
            offer.Price = price;
            offer.NumberOfTerms = numberOfTerms;

            _dbContext.Update(offer);
            await _dbContext.SaveChangesAsync();

            return Result<Offer>.Ok(offer);
        }

        public async Task<Result<int>> DeleteOfferAsync(int id)
        {
            _logger.LogDebug("DeleteOfferAsync {0}", id);

            var offer = await _dbContext.Offers.FindAsync(id);
            if (offer == null) return Result<int>.Ok(id);

            if (await _dbContext.Orders.AnyAsync(row => row.OfferId == id)) return Result<int>.Fail($"Offer with id = {id} can't be deleted (referenced by some Order)");

            _dbContext.Offers.Remove(offer);
            await _dbContext.SaveChangesAsync();

            return Result<int>.Ok(id);
        }
        #endregion

        #region Order
        public async Task<IEnumerable<Order>> GetAllOrdersAsync(int? pageNumber, int pageSize, int? offerId, int? customerId)
        {
            _logger.LogDebug("GetAllOrdersAsync {0} {1} {2} {3}", pageNumber, pageSize, offerId, customerId);

            IQueryable<Order> query = _dbContext.Orders.OrderBy(row => row.OrderId); ;

            if (offerId.HasValue) query = query.Where(row => row.OfferId == offerId);
            if (customerId.HasValue) query = query.Where(row => row.CustomerId == customerId);

            return await PaginatedList<Order>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize);
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            _logger.LogDebug("GeOrderAsync {0}", id);

            return await _dbContext.Orders.FindAsync(id);
        }

        public async Task<Result<Order>> CreateOrderAsync(int offerId, DateTime startDate, int customerId)
        {
            _logger.LogDebug("CreateOrderAsync {0} {1} {2}", offerId, startDate, customerId);

            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer == null) return Result<Order>.Fail($"Customer with id = {customerId} does not exist");

            var offer = await _dbContext.Offers.Include(row => row.Product).FirstOrDefaultAsync(row => row.OfferId == offerId);
            if (offer == null) return Result<Order>.Fail($"Offer with id = {offerId} does not exist");
            if (!offer.Product.IsActive) return Result<Order>.Fail($"Offer with id = {offerId} linked to inactive Product");

            var order = _dbContext.Orders.FromSqlRaw("CreateOrder @OfferId, @StartDate, @CustomerId",
                new SqlParameter("@OfferId", offerId),
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@CustomerId", customerId))
                .AsEnumerable()
                .First();

            return Result<Order>.Ok(order);
        }

        public async Task<Result<Order>> UpdateOrderAsync(int id, bool paid, bool cancelled, string reason)
        {
            _logger.LogDebug("UpdateOrderAsync {0} {1} {2} {3}", id, paid, cancelled, reason);

            var order = await _dbContext.Orders.FindAsync(id);
            if (order == null) return Result<Order>.Fail($"Order with id = {id} does not exist");

            order.Cancelled = cancelled;
            order.Paid = paid;
            order.Reason = reason;

            _dbContext.Update(order);
            await _dbContext.SaveChangesAsync();

            return Result<Order>.Ok(order);
        }

        public async Task<Result<int>> DeleteOrderAsync(int id)
        {
            _logger.LogDebug("DeleteOrderAsync {0}", id);

            var order = await _dbContext.Orders.FindAsync(id);
            if (order == null) return Result<int>.Fail($"Order with id = {id} does not exist");

            order.Cancelled = true;

            _dbContext.Update(order);
            await _dbContext.SaveChangesAsync();

            return Result<int>.Ok(id);
        }
        #endregion
    }
}
