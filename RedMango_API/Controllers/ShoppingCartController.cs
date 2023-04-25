using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.Dto;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ApiResponse _response;

        public ShoppingCartController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ApiResponse();
        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
        {
            try
            {
                  ShoppingCart shoppingCart;

                if (string.IsNullOrEmpty(userId))
                {
                    shoppingCart = new();
                }
                else
                {
                   shoppingCart =  _db.ShoppingCarts
                                       .Include(x => x.CartItems)
                                       .ThenInclude(x => x.MenuItems)
                                       .FirstOrDefault(x => x.UserId == userId);
                }

                if (shoppingCart == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }

                if (shoppingCart.CartItems != null && shoppingCart.CartItems.Count > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(x => x.Quantity * x.MenuItems.Price);
                }

                var data = _mapper.Map<ShoppingCart, ShoppingCartDto>(shoppingCart);
                _response.Result = data;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQty)
        {

            ShoppingCart shoppingCart = await _db.ShoppingCarts.Include(x =>x.CartItems).FirstOrDefaultAsync(x => x.UserId == userId);
            MenuItem menuItem = _db.MenuItems.FirstOrDefault(x =>x.Id == menuItemId);
            if(menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest();
            }

            if(shoppingCart == null && updateQty > 0)
            {
                // create a shopping cart & add cart item
                ShoppingCart newCart = new() { UserId = userId };
                _db.ShoppingCarts.Add(newCart);
                _db.SaveChanges();

                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQty,
                    ShoppingCartId = newCart.Id,
                    MenuItems = null
                };
                _db.CartItems.Add(newCartItem);
                _db.SaveChanges();
            }
            else
            {
                //shopping cart exists 
                CartItem cartItemInCart = shoppingCart.CartItems.FirstOrDefault(x =>x.MenuItemId == menuItemId);
                if(cartItemInCart == null)
                {
                    // item does not exist in current cart

                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQty,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItems = null
                    };

                    _db.CartItems.Add(newCartItem);
                    _db.SaveChanges();
                }
                else
                {
                    //item already exist in the cart and we have to update quantity
                    int newQuantity = cartItemInCart.Quantity + updateQty;
                    if(updateQty == 0 || newQuantity <= 0)
                    {
                        // remove cart item from cart and if it is the only item then remove cart 
                        _db.CartItems.Remove(cartItemInCart);

                        if(shoppingCart.CartItems.Count() == 1)
                        {
                            _db.ShoppingCarts.Remove(shoppingCart);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        cartItemInCart.Quantity = newQuantity;
                        _db.SaveChanges();
                    }

                }

            }

            return _response;
        }
    }
}
