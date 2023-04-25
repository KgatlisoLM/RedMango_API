using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.Dto;
using RedMango_API.Utility;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ApiResponse _response;

        public OrderController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ApiResponse();
        }


        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId, string searchString, string status)
        {
            try
            {
                var orderHeaders = await _db.OrderHeaders
                                    .Include(x => x.OrderDetails)
                                    .ThenInclude(x => x.MenuItems)
                                    .OrderByDescending(x => x.OrderHeaderId).ToListAsync();

                if(!string.IsNullOrEmpty(userId))
                {
                    var order = orderHeaders.Where(x => x.AppUserId == userId).ToList();
                    var data = _mapper.Map<List<OrderHeader>, List<OrderHeaderDto>>(order);
                    _response.Result = data;
                }
                else
                {
                    var data = _mapper.Map<List<OrderHeader>, List<OrderHeaderDto>>(orderHeaders);
                    _response.Result = data;
                }
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrder(int id)
        {
            try
            {
                if(id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }


                var orderHeaders = _db.OrderHeaders
                                    .Include(x => x.OrderDetails)
                                    .ThenInclude(x => x.MenuItems)
                                    .Where(x => x.OrderHeaderId == id).ToList();

                if (orderHeaders == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                else
                {
                    var data = _mapper.Map<List<OrderHeader>, List<OrderHeaderDto>>(orderHeaders);
                    _response.Result = data;
                }
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }



       [HttpPost] 
       public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDto orderHeaderDto)
       {    
            try
            {
                OrderHeader order = new()
                {
                    AppUserId = orderHeaderDto.AppUserId,
                    PickupEmail = orderHeaderDto.PickupEmail,
                    PickupName = orderHeaderDto.PickupName,
                    PickupPhoneNumber = orderHeaderDto.PickupPhoneNumber,
                    OrderTotal = orderHeaderDto.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentId = orderHeaderDto.StripePaymentIntentId,
                    TotalItems = orderHeaderDto.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeaderDto.Status) ? SD.status_pending : orderHeaderDto.Status, 
                };

                if (ModelState.IsValid)
                {
                    _db.OrderHeaders.Add(order);
                  await  _db.SaveChangesAsync();

                    foreach (var orderDetailDto in orderHeaderDto.OrderDetailsDto)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDto.ItemName,
                            MenuItemId = orderDetailDto.MenuItemId,
                            Price = orderDetailDto.Price,
                            quantity = orderDetailDto.quantity,

                        };

                        _db.OrderDetails.Add(orderDetails);
                    }
                    await  _db.SaveChangesAsync();
                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.StatusCode = HttpStatusCode.Created;
                    return Ok(_response);
                }
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false; 
                _response.ErrorMessages  =
                     new List<string>() { ex.ToString() };
            }
            return _response;
       }


       [HttpPut("{id:int}")]
       public async Task<ActionResult<ApiResponse>> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDto orderHeaderUpdate)
       {
            try
            {
                if(orderHeaderUpdate == null || id != orderHeaderUpdate.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();    
                }
                OrderHeader orderFromDb = await _db.OrderHeaders.FirstOrDefaultAsync(x => x.OrderHeaderId == id);

                if(orderFromDb == null) 
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(); 
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdate.PickupName))
                {
                    orderFromDb.PickupName = orderHeaderUpdate.PickupName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdate.PickupPhoneNumber))
                {
                    orderFromDb.PickupPhoneNumber = orderHeaderUpdate.PickupPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdate.PickupEmail))
                {
                    orderFromDb.PickupEmail = orderHeaderUpdate.PickupEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdate.Status))
                {
                    orderFromDb.Status = orderHeaderUpdate.Status;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdate.StripePaymentIntentId))
                {
                    orderFromDb.StripePaymentIntentId = orderHeaderUpdate.StripePaymentIntentId;
                }
                _db.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.ToString() };
            }

            return _response;
       }


    }
}
