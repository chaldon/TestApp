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
    public class BrandController : ControllerBase
    {
        private readonly Repository.IRepository _repository;
        private readonly ILogger<ProductController> _logger;
        private readonly IMapper _mapper;
        public BrandController(Repository.IRepository repository, ILogger<ProductController> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates list of Brands with minimalistic pagination and filtering support
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="brand">Search substring in Brand</param>
        /// <returns>List of Brand</returns>
        /// <response code="200">Returns a list of Brands</response>
        /// <response code="400">If request is failed</response>            
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<BrandViewModel>>> GetAllAsync(int pageNumber = 1, int pageSize = 100, string brand = null)
        {
            _logger.LogDebug("GetAll {0} {1} {2}", pageNumber, pageSize,  brand);

            InputValidation(pageNumber, pageSize);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.GetAllBrandsAsync(pageNumber, pageSize, brand);
            return Ok(result?.Select((b) => _mapper.Map<BrandViewModel>(b)));
        }

        /// <summary>
        /// Get a single Brand by BrandtId
        /// </summary>
        /// <param name="id">BrandId</param>
        /// <returns>Brand</returns>
        /// <response code="200">Returns a Brand</response>
        /// <response code="404">If a Brand is not found</response>            
        [HttpGet("{id:int}", Name = "Brand"+nameof(GetAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BrandViewModel>> GetAsync(int id)
        {
            _logger.LogDebug("Get {0}", id);

            var brand = await _repository.GetBrandAsync(id);
            if (brand == null) return NotFound();

            return Ok(_mapper.Map<BrandViewModel>(brand));
        }

        /// <summary>
        /// Create and store a new Brand
        /// </summary>
        /// <param name="pe">A new Brand</param>
        /// <returns>Product</returns>
        /// <response code="201">Returns a created Brand</response>
        /// <response code="400">If creation of a Brand is failed</response>            
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BrandViewModel>> PostAsync([FromBody] CreateUpdateBrandViewModel pe)
        {
            _logger.LogDebug("Post {0}", pe.Brand);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.CreateBrandAsync(pe.Brand);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return CreatedAtRoute("Brand" + nameof(GetAsync), new { id = result.Data.BrandId }, _mapper.Map<BrandViewModel>(result.Data));
        }

        /// <summary>
        /// Updates an existing Brand
        /// </summary>
        /// <param name="id">BrandId</param>
        /// <param name="pe">Brand Updates</param>
        /// <returns>Returns an updated Brand</returns>
        /// <response code="200">Returns an updated Brand</response>
        /// <response code="400">If update of Brand is failed</response>    
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BrandViewModel>> PutAsync(int id, CreateUpdateBrandViewModel pe)
        {
            _logger.LogDebug("Put {0} {1}", id, pe.Brand);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.UpdateBrandAsync(id, pe.Brand);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return Ok(_mapper.Map<BrandViewModel>(result.Data));
        }

        /// <summary>
        /// Delete a Brand by BrandId
        /// </summary>
        /// <param name="id">BrandId</param>
        /// <response code="200">Returns BrandId of a deleted Brand</response>
        /// <response code="400">If deletion failed</response>    
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> DeleteAsync(int id)
        {
            _logger.LogDebug("Delete {0}", id);

            var result = await _repository.DeleteBrandAsync(id);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return Ok(result.Data);
        }

        private void InputValidation(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) ModelState.AddModelError(string.Empty, "pageNumber should be >= 1");
            if (pageSize <= 0) ModelState.AddModelError(string.Empty, "pageSize should be >= 1");
        }
    }
}
