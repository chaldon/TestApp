using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using TestApp;
using TestApp.Controllers;
using TestApp.Model;
using TestApp.Models;
using TestApp.Repository;

namespace NUnitTestProject
{
    /// <summary>
    /// Unit tests for ProductController, mostly check validation logic for request parameters
    /// </summary>
    public class ProductControllerUnitTests
    {
        Mock<IRepository> repository;
        Mock<ILogger<ProductController>> logger;
        IMapper mapper;

        [SetUp]
        public void Setup()
        {
            repository = new Mock<IRepository>();
            logger = new Mock<ILogger<ProductController>>();
            mapper = new MapperConfiguration(cfg => {
                cfg.AddMaps(typeof(Repository).Assembly);
            }).CreateMapper();
        }

        [Test]
        [TestCase(1, 100)]
        [TestCase(100, 10)]
        public async Task GetAll_Returns_Ok(int pageNumber, int pageSize)
        {
            repository.Setup(m => m.GetAllProductsAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()))
                .Returns( Task.FromResult(Enumerable.Empty<Product>()));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.GetAllAsync(pageNumber, pageSize, null, null, null);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(200, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(int.MinValue, int.MaxValue)]
        [TestCase(1, 0)]
        [TestCase(1, -1)]
        public async Task GetAll_Returns_BadRequest(int pageNumber, int pageSize)
        {
            repository.Setup(m => m.GetAllProductsAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()))
                .Returns(Task.FromResult(Enumerable.Empty<Product>()));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.GetAllAsync(pageNumber, pageSize, null, null, null);

            Assert.IsFalse(controller.ModelState.IsValid, "ModelState is valid");
            Assert.AreEqual(400, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        public async Task Get_Returns_Ok()
        {
            repository.Setup(m => m.GetProductAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Product()));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.GetAsync(1);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(200, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        public async Task Get_Returns_NotFound()
        {
            repository.Setup(m => m.GetProductAsync(It.IsAny<int>()))
                .Returns(Task.FromResult((Product)null));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.GetAsync(1);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(404, (response.Result as NotFoundResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        [TestCase("t1", true,"monthly",1)]
        [TestCase("t2", false, "annually", 1)]
        public async Task Post_Returns_Ok(string name, bool iActive, string term, int brandId)
        {
            repository.Setup(m => m.CreateProductAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult( Result<Product>.Ok(new Product() {ProductId = 0, Name = name, IsActive = iActive, Term = term, BrandId = brandId } )));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.PostAsync(new CreateUpdateProductViewModel() { Name = name, IsActive = iActive, Term = term, BrandId = brandId  });

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(201, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        [TestCase("t1", true, "daily", 1)]
        [TestCase("", true, "annually", 1)]
        [TestCase(null, true, "annually", 1)]
        public async Task Post_Returns_BadRequest(string name, bool iActive, string term, int brandId)
        {
            repository.Setup(m => m.CreateProductAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(Result<Product>.Ok(new Product() { ProductId = 0, Name = name, IsActive = iActive, Term = term, BrandId = brandId })));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.PostAsync(new CreateUpdateProductViewModel() { Name = name, IsActive = iActive, Term = term, BrandId = brandId });

            Assert.IsFalse(controller.ModelState.IsValid, "ModelState is valid");
            Assert.AreEqual(400, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        [TestCase(1, "t1", true, "monthly", 1)]
        [TestCase(2, "t2", false, "annually", 1)]
        public async Task Put_Returns_Ok(int productId, string name, bool iActive, string term, int brandId)
        {
            repository.Setup(m => m.UpdateProductAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(Result<Product>.Ok(new Product() { ProductId = 0, Name = name, IsActive = iActive, Term = term, BrandId = brandId })));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.PutAsync(productId, new CreateUpdateProductViewModel() { Name = name, IsActive = iActive, Term = term, BrandId = brandId });

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(200, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        [TestCase(1, "t1", true, "daily", 1)]
        [TestCase(2, "", true, "annually", 1)]
        [TestCase(3, null, true, "annually", 1)]
        public async Task Put_Returns_BadRequest(int productId, string name, bool iActive, string term, int brandId)
        {
            repository.Setup(m => m.UpdateProductAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(Result<Product>.Ok(new Product() { ProductId = 0, Name = name, IsActive = iActive, Term = term, BrandId = brandId })));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.PutAsync(productId, new CreateUpdateProductViewModel() { Name = name, IsActive = iActive, Term = term, BrandId = brandId });

            Assert.IsFalse(controller.ModelState.IsValid, "ModelState is valid");
            Assert.AreEqual(400, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }

        [Test]
        [TestCase(1)]
        [TestCase(int.MaxValue)]
        public async Task Delete_Returns_Ok(int productId)
        {
            repository.Setup(m => m.DeleteProductAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(Result<int>.Ok(productId)));

            var controller = new ProductController(repository.Object, logger.Object, mapper);

            var response = await controller.DeleteAsync(productId);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is invalid");
            Assert.AreEqual(200, (response.Result as ObjectResult).StatusCode, "Wrong StatusCode returned");
        }
    }
}