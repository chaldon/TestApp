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
    public class OrderController : ControllerBase
    {
        private readonly Repository.IRepository _repository;
        private readonly ILogger<OrderController> _logger;
        private readonly IMapper _mapper;
        public OrderController(Repository.IRepository repository, ILogger<OrderController> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates list of Orders with minimalistic pagination and filtering support
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="offerId">Search by OfferId</param>
        /// <param name="customerId">Search by CustomerId</param>
        /// <returns>List of Orders</returns>
        /// <response code="200">Returns a list of Order</response>
        /// <response code="400">If request is failed</response>            
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrderViewModel>>> GetAllAsync(int pageNumber = 1, int pageSize = 100, int? offerId = null, int? customerId = null)
        {
            _logger.LogDebug("GetAll {0} {1} {2} {3}", pageNumber, pageSize, offerId, customerId);

            InputValidation(pageNumber, pageSize);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.GetAllOrdersAsync(pageNumber, pageSize, offerId, customerId);
            return Ok(result?.Select((Order) => _mapper.Map<OrderViewModel>(Order)));
        }

        /// <summary>
        /// Get a single Order by OrderId
        /// </summary>
        /// <param name="id">OrderId</param>
        /// <returns>Order</returns>
        /// <response code="200">Returns a Order</response>
        /// <response code="404">If a Order is not found</response>            
        [HttpGet("{id:int}", Name = "Order" + nameof(GetAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderViewModel>> GetAsync(int id)
        {
            _logger.LogDebug("Get {0}", id);

            var Order = await _repository.GetOrderAsync(id);
            if (Order == null) return NotFound();

            return Ok(_mapper.Map<OrderViewModel>(Order));
        }

        /// <summary>
        /// Create and store a new Order. Order created with Cancelled and Paid attributes equals to false
        /// </summary>
        /// <param name="pe">A new Order</param>
        /// <returns>Order</returns>
        /// <response code="201">Returns a created Order</response>
        /// <response code="400">If creation of a Order is failed</response>            
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderViewModel>> PostAsync([FromBody] CreateOrderViewModel pe)
        {
            _logger.LogDebug("Post {0} {1} {2}", pe.OfferId, pe.StartDate, pe.CustomerId);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.CreateOrderAsync(pe.OfferId, pe.StartDate, pe.CustomerId);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return CreatedAtRoute("Order" + nameof(GetAsync), new { id = result.Data.OrderId }, _mapper.Map<OrderViewModel>(result.Data));
        }

        /// <summary>
        /// Updates an existing Order. API limit updates to a three properties - Cancelled, Reason (of cancellation) and Paid.
        /// The idea behind that is that order should be written "in stone" and all you can do with it is to update few properties only.
        /// </summary>
        /// <param name="id">OrderId</param>
        /// <param name="pe">Order Updates</param>
        /// <returns>Returns an updated Order</returns>
        /// <response code="200">Returns an updated Order</response>
        /// <response code="400">If update of Order is failed</response>    
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderViewModel>> PutAsync(int id, UpdateOrderViewModel pe)
        {
            _logger.LogDebug("Put {0} {1} {2} {3}", id, pe.Paid, pe.Cancelled, pe.Reason);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.UpdateOrderAsync(id, pe.Paid, pe.Cancelled, pe.Reason);
            if (!result) return BadRequest(new ProblemDetails() { Detail = result.Message });

            return Ok(_mapper.Map<OrderViewModel>(result.Data));
        }

        /// <summary>
        /// Cancel a Order by OrderId
        /// </summary>
        /// <param name="id">OrderId</param>
        /// <response code="200">Returns OrderId of a cancelled Order</response>
        /// <response code="400">If deletion failed</response>    
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> DeleteAsync(int id)
        {
            _logger.LogDebug("Delete {0}", id);

            var result = await _repository.DeleteOrderAsync(id);
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
