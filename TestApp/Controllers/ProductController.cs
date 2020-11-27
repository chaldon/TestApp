using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly Repository.IRepository _repository;
        private readonly ILogger<ProductController> _logger;
        private readonly IMapper _mapper;
        public ProductController(Repository.IRepository repository, ILogger<ProductController> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates list of Products with minimalistic pagination and filtering support
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="name">Search substring in Product name</param>
        /// <param name="brand">Search substring in Brand</param>
        /// <param name="isActive">Filter by is active</param>
        /// <returns>List of Products</returns>
        /// <response code="200">Returns a list of Products</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ProductViewModel>>> GetAllAsync(int pageNumber = 1, int pageSize = 100, string name = null, string brand = null, bool? isActive = null)
        {
            _logger.LogDebug("GetAll {0} {1} {2} {3} {4}", pageNumber, pageSize, name, brand, isActive);

            InputValidation(pageNumber, pageSize);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.GetAllProductsAsync(pageNumber, pageSize, name, brand, isActive);
            return Ok(result?.Select((product) => _mapper.Map<ProductViewModel>(product)));
        }

        /// <summary>
        /// Get a single Product by ProductId
        /// </summary>
        /// <param name="id">ProductId</param>
        /// <returns>Product</returns>
        /// <response code="200">Returns a Product</response>
        /// <response code="404">If a Product is not found</response>            
        [HttpGet("{id:int}", Name = "Product" + nameof(GetAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductViewModel>> GetAsync(int id)
        {
            _logger.LogDebug("Get {0}", id);

            var product = await _repository.GetProductAsync(id);
            if (product == null) return NotFound();

            return Ok(_mapper.Map<ProductViewModel>(product));
        }

        /// <summary>
        /// Create and store a new Product
        /// </summary>
        /// <param name="pe">A new Product</param>
        /// <returns>Product</returns>
        /// <response code="201">Returns a created Product</response>
        /// <response code="400">If creation of a Product is failed</response>            
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductViewModel>> PostAsync(CreateUpdateProductViewModel pe)
        {
            _logger.LogDebug("Post {0} {1} {2} {3}", pe.Name, pe.IsActive, pe.Term, pe.BrandId);

            InputValidation(pe);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.CreateProductAsync(pe.Name, pe.IsActive, pe.Term, pe.BrandId);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return CreatedAtRoute("Product" + nameof(GetAsync), new { id = result.Data.ProductId }, _mapper.Map<ProductViewModel>(result.Data));
        }

        /// <summary>
        /// Updates an existing Product
        /// </summary>
        /// <param name="id">ProductId</param>
        /// <param name="pe">Product Updates</param>
        /// <returns>Returns an updated Product</returns>
        /// <response code="200">Returns an updated Product</response>
        /// <response code="400">If update of Product is failed</response>    
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductViewModel>> PutAsync(int id, CreateUpdateProductViewModel pe)
        {
            _logger.LogDebug("Put {0} {1} {2} {3} {4}", id, pe.Name, pe.IsActive, pe.Term, pe.BrandId);

            InputValidation(pe);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.UpdateProductAsync(id, pe.Name, pe.IsActive, pe.Term, pe.BrandId);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return Ok(_mapper.Map<ProductViewModel>(result.Data));
        }

        /// <summary>
        /// Delete a Product by ProductId
        /// </summary>
        /// <param name="id">ProductId</param>
        /// <response code="200">Returns ProductId of a deleted Product</response>
        /// <response code="400">If deletion failed</response>    
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> DeleteAsync(int id)
        {
            _logger.LogDebug("Delete {0}", id);

            var result = await _repository.DeleteProductAsync(id);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return Ok(result.Data);
        }

        /// <summary>
        /// It's possible to validate input values using DataAnnotation &amp; ModelState mechanism
        /// however I choose simpler way here
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        private void InputValidation(CreateUpdateProductViewModel pe)
        {
            if (!(pe.Term == "annually" || pe.Term == "monthly")) ModelState.AddModelError(string.Empty, "Term allowed values annually or monthly");
            if( string.IsNullOrEmpty(pe.Name)) ModelState.AddModelError(string.Empty, "Name should not be empty");
        }

        private void InputValidation(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) ModelState.AddModelError(string.Empty, "pageNumber should be >= 1");
            if (pageSize <= 0) ModelState.AddModelError(string.Empty, "pageSize should be >= 1");
        }
    }
}
