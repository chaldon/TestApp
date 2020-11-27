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
    public class CustomerController : ControllerBase
    {
        private readonly Repository.IRepository _repository;
        private readonly ILogger<CustomerController> _logger;
        private readonly IMapper _mapper;
        public CustomerController(Repository.IRepository repository, ILogger<CustomerController> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates list of Customers with minimalistic pagination and filtering support
        /// </summary>
        /// <param name="pageNumber">Starting position in a list of Customers (ordered by CustomerId), zero-based</param>
        /// <param name="pageSize">Max rows returned</param>
        /// <param name="name">Search substring in a Name</param>
        /// <param name="email">Search substring in an e-mail</param>
        /// <param name="offerId">Search by OfferId</param>
        /// <returns>List of Customers</returns>
        /// <response code="200">Returns a list of Customer</response>
        /// <response code="400">If request is failed</response>  
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CustomerViewModel>>> GetAllAsync(int pageNumber = 1, int pageSize = 100, string name = null, string email = null, int? offerId = null)
        {
            _logger.LogDebug("GetAll {0} {1} {2} {3}", pageNumber, pageSize, name, email, offerId);

            InputValidation(pageNumber, pageSize);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.GetAllCustomersAsync(pageNumber, pageSize, name, email, offerId);
            return Ok(result?.Select((customer) => _mapper.Map<CustomerViewModel>(customer)));
        }

        /// <summary>
        /// Get a single Customer by CustomerId
        /// </summary>
        /// <param name="id">CustomerId</param>
        /// <returns>Customer</returns>
        /// <response code="200">Returns a Customer</response>
        /// <response code="404">If a Customer is not found</response>            
        [HttpGet("{id:int}", Name = "Customer" + nameof(GetAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerViewModel>> GetAsync(int id)
        {
            _logger.LogDebug("Get {0}", id);

            var customer = await _repository.GetCustomerAsync(id);
            if (customer == null) return NotFound();

            return Ok(_mapper.Map<CustomerViewModel>(customer));
        }

        /// <summary>
        /// Create and store a new Customer
        /// </summary>
        /// <param name="pe">A new Customer</param>
        /// <returns>Customer</returns>
        /// <response code="201">Returns a created Customer</response>
        /// <response code="400">If creation of a Customer is failed</response>            
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerViewModel>> PostAsync([FromBody] CreateUpdateCustomerViewModel pe)
        {
            _logger.LogDebug("Post {0} {1} {2}", pe.EmailAddress, pe.FirstName, pe.LastName);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.CreateCustomerAsync(pe.EmailAddress, pe.FirstName, pe.LastName);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return CreatedAtRoute("Customer" + nameof(GetAsync), new { id = result.Data.CustomerId }, _mapper.Map<CustomerViewModel>(result.Data));
        }

        /// <summary>
        /// Updates an existing Customer
        /// </summary>
        /// <param name="id">CustomerId</param>
        /// <param name="pe">Customer Updates</param>
        /// <returns>Returns an updated Customer</returns>
        /// <response code="200">Returns an updated Customer</response>
        /// <response code="400">If update of Customer is failed</response>    
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerViewModel>> PutAsync(int id, CreateUpdateCustomerViewModel pe)
        {
            _logger.LogDebug("Put {0} {1} {2} {3}", id, pe.EmailAddress, pe.FirstName, pe.LastName);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.UpdateCustomerAsync(id, pe.EmailAddress, pe.FirstName, pe.LastName);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return Ok(_mapper.Map<CustomerViewModel>(result.Data));
        }

        /// <summary>
        /// Delete a Customer by CustomerId
        /// </summary>
        /// <param name="id">CustomerId</param>
        /// <response code="200">Returns CustomerId of a deleted Customer</response>
        /// <response code="400">If deletion failed</response>    
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> DeleteAsync(int id)
        {
            _logger.LogDebug("Delete {0}", id);

            var result = await _repository.DeleteCustomerAsync(id);
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
