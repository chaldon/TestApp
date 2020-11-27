using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApp.Controllers;
using TestApp.Model;
using TestApp.Models;
using TestApp.Repository;

namespace NUnitTestProject
{
    /// <summary>
    /// Integration tests for the ProductController and Repository and Database (using in-memory db)
    /// </summary>
    class ProductControllerIntegrationTests
    {
        TestAppContext context;
        Mock<ILogger<ProductController>> logger;
        Mock<ILogger<Repository>> repositoryLogger;
        IMapper mapper;

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<ProductController>>();
            repositoryLogger = new Mock<ILogger<Repository>>();
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(Repository).Assembly);
            }).CreateMapper();

            var builder = new DbContextOptionsBuilder<TestAppContext>();
            var options = builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            context = new TestAppContext(options);
            // Seed with test values
            context.Brands.Add(new Brand() { BrandId = 1, Name = "US" });
            context.Products.Add(new Product() { ProductId = 1, Name = "Test product", BrandId = 1, IsActive = true, Term = "monthly" });
            context.Products.Add(new Product() { ProductId = 2, Name = "Test product", BrandId = 1, IsActive = true, Term = "monthly" });
            context.Offers.Add(new Offer() { OfferId = 1, NumberOfTerms = 10, Description = "Test offer", Price = 1.0M, ProductId = 1 });
            context.Customers.Add(new Customer() { CustomerId = 1, EmailAddress = "test@test.com", FirstName = "fn", LastName = "ln" });
            context.Orders.Add(new Order() { OrderId = 1, OfferId = 1, CustomerId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(5) });
            context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            context?.Dispose();
        }

        [Test]
        [TestCase(1, 100, null, null, null)]
        [TestCase(1, 10, "est", null, null)]
        [TestCase(1, 10, null, "US", null)]
        [TestCase(1, 10, null, null, true)]
        [TestCase(1, 10, "est", "US", true)]
        public async Task GetAll_Returns_Ok(int pageNumber, int pageSize, string name, string brand, bool? isActive)
        {
            var repository = new Repository(context, repositoryLogger.Object);
            var controller = new ProductController(repository, logger.Object, mapper);

            var response = await controller.GetAllAsync(pageNumber, pageSize, null, null, null);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(200, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");

            var productList = ((IEnumerable<ProductViewModel>)((response.Result as ObjectResult).Value)).ToList();

            Assert.AreEqual(2, productList.Count);

            ProductViewModel productViewModel = productList.First(row => row.ProductId == 1);

            Assert.AreEqual("Test product", productViewModel.Name);
            Assert.AreEqual("monthly", productViewModel.Term);
            Assert.AreEqual(1, productViewModel.ProductId);
            Assert.AreEqual(true, productViewModel.IsActive);
            Assert.AreEqual(1, productViewModel.BrandId);
        }

        [Test]
        public async Task Get_Returns_Ok()
        {
            var repository = new Repository(context, repositoryLogger.Object);
            var controller = new ProductController(repository, logger.Object, mapper);

            var response = await controller.GetAsync(1);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(200, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        public async Task Get_Returns_NotFound()
        {
            var repository = new Repository(context, repositoryLogger.Object);
            var controller = new ProductController(repository, logger.Object, mapper);

            var response = await controller.GetAsync(100);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(404, (response.Result as NotFoundResult).StatusCode, "Wrong StatusCode returned");
        }


        [Test]
        [TestCase("t1", true, "monthly", 1)]
        [TestCase("t2", false, "annually", 1)]
        public async Task Post_Returns_Ok(string name, bool iActive, string term, int brandId)
        {
            var repository = new Repository(context, repositoryLogger.Object);
            var controller = new ProductController(repository, logger.Object, mapper);

            var response = await controller.PostAsync(new CreateUpdateProductViewModel() { Name = name, IsActive = iActive, Term = term, BrandId = brandId });

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(201, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");

            var productViewModel = (ProductViewModel)((response.Result as ObjectResult).Value); //Check what is returned

            Assert.AreEqual(name, productViewModel.Name);
            Assert.AreEqual(term, productViewModel.Term);
            Assert.AreEqual(iActive, productViewModel.IsActive);
            Assert.AreEqual(brandId, productViewModel.BrandId);

            var product = await context.Products.FindAsync(productViewModel.ProductId); // Check what is in a database

            Assert.AreEqual(name, product.Name);
            Assert.AreEqual(term, product.Term);
            Assert.AreEqual(iActive, product.IsActive);
            Assert.AreEqual(brandId, product.BrandId);
        }

        [Test]
        [TestCase("t2", false, "annually", int.MaxValue)]
        public async Task Post_Returns_BadRequest(string name, bool iActive, string term, int brandId)
        {
            var repository = new Repository(context, repositoryLogger.Object);
            var controller = new ProductController(repository, logger.Object, mapper);

            var response = await controller.PostAsync(new CreateUpdateProductViewModel() { Name = name, IsActive = iActive, Term = term, BrandId = brandId });

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(400, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        [TestCase(-1, Description = "Delete non-existing Product")]
        [TestCase(2)]
        public async Task Delete_Returns_Ok(int productId)
        {
            var repository = new Repository(context, repositoryLogger.Object);
            var controller = new ProductController(repository, logger.Object, mapper);

            var response = await controller.DeleteAsync(productId);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(200, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        [TestCase(1, Description = "Product is referenced by order")]
        public async Task Delete_Returns_BadRequest(int productId)
        {
            var repository = new Repository(context, repositoryLogger.Object);
            var controller = new ProductController(repository, logger.Object, mapper);

            var response = await controller.DeleteAsync(productId);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(400, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }
    }
}
