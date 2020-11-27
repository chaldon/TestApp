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
    public class OfferController : ControllerBase
    {
        private readonly Repository.IRepository _repository;
        private readonly ILogger<OfferController> _logger;
        private readonly IMapper _mapper;
        public OfferController(Repository.IRepository repository, ILogger<OfferController> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates list of Offers with minimalistic pagination and filtering support
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="description">Search substring in Offer description</param>
        /// <param name="productId">Search offers by productId</param>
        /// <param name="activeProduct">Search by active products</param>
        /// <returns>List of Offers</returns>
        /// <response code="200">Returns a list of Offers</response>
        /// <response code="400">If request is failed</response>            
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OfferViewModel>>> GetAllAsync(int pageNumber = 1, int pageSize = 100, string description = null, int? productId = null, bool? activeProduct = null)
        {
            _logger.LogDebug("GetAll {0} {1} {2} {3} {4}", pageNumber, pageSize, description, productId, activeProduct);

            InputValidation(pageNumber, pageSize);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.GetAllOffersAsync(pageNumber, pageSize, description, productId, activeProduct);
            return Ok(result?.Select((Offer) => _mapper.Map<OfferViewModel>(Offer)));
        }

        /// <summary>
        /// Get a single Offer by OfferId
        /// </summary>
        /// <param name="id">OfferId</param>
        /// <returns>Offer</returns>
        /// <response code="200">Returns a Offer</response>
        /// <response code="404">If a Offer is not found</response>            
        [HttpGet("{id:int}", Name = "Offer" + nameof(GetAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OfferViewModel>> GetAsync(int id)
        {
            _logger.LogDebug("Get {0}", id);

            var Offer = await _repository.GetOfferAsync(id);
            if (Offer == null) return NotFound();

            return Ok(_mapper.Map<OfferViewModel>(Offer));
        }

        /// <summary>
        /// Create and store a new Offer
        /// </summary>
        /// <param name="pe">A new Offer</param>
        /// <returns>Offer</returns>
        /// <response code="201">Returns a created Offer</response>
        /// <response code="400">If creation of a Offer is failed</response>            
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType( StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OfferViewModel>> PostAsync([FromBody] CreateUpdateOfferViewModel pe)
        {
            _logger.LogDebug("Post {0} {1} {2} {3}", pe.ProductId, pe.Description, pe.Price, pe.NumberOfTerms);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.CreateOfferAsync(pe.ProductId, pe.Description, pe.Price, pe.NumberOfTerms);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return CreatedAtRoute("Offer" + nameof(GetAsync), new { id = result.Data.OfferId }, _mapper.Map<OfferViewModel>(result.Data));
        }

        /// <summary>
        /// Updates an existing Offer
        /// </summary>
        /// <param name="id">OfferId</param>
        /// <param name="pe">Offer Updates</param>
        /// <returns>Returns an updated Offer</returns>
        /// <response code="200">Returns an updated Offer</response>
        /// <response code="400">If update of Offer is failed</response>    
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OfferViewModel>> PutAsync(int id, CreateUpdateOfferViewModel pe)
        {
            _logger.LogDebug("Put {0} {1} {2} {3}", pe.ProductId, pe.Description, pe.Price, pe.NumberOfTerms);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.UpdateOfferAsync(id, pe.ProductId, pe.Description, pe.Price, pe.NumberOfTerms);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return Ok(_mapper.Map<OfferViewModel>(result.Data));
        }

        /// <summary>
        /// Delete a Offer by OfferId
        /// </summary>
        /// <param name="id">OfferId</param>
        /// <response code="200">Returns OfferId of a deleted Offer</response>
        /// <response code="400">If deletion failed</response>    
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> DeleteAsync(int id)
        {
            _logger.LogDebug("Delete {0}", id);

            var result = await _repository.DeleteOfferAsync(id);
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
