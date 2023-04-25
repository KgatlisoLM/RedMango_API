using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.Dto;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using RedMango_API.Utility;

namespace RedMango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ApiResponse _response;

        public MenuItemsController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ApiResponse();
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            var menu = await _db.MenuItems.ToListAsync();
            var data = _mapper.Map<List<MenuItem>, List<MenuItemDto>>(menu);
            _response.Result = data;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        [HttpGet("{id:int}", Name = "GetMenuItem")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;    
                return BadRequest(_response);
            }
            MenuItem menuItem = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }
            _response.Result = _mapper.Map<MenuItem, MenuItemDto>(menuItem);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDto menuItemCreateDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemCreateDto.File == null || menuItemCreateDto.File.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest(_response);
                    }
                    var pathUpload = "Content/images/";
                    var fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(menuItemCreateDto.File.FileName);
                    var path = Path.Combine(pathUpload, fileName);
                    await using var fileStream = new FileStream(path, FileMode.Create);
                    await menuItemCreateDto.File.CopyToAsync(fileStream);

                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuItemCreateDto.Name,
                        Price = menuItemCreateDto.Price,
                        Category = menuItemCreateDto.Category,
                        SpecialTag = menuItemCreateDto.SpecialTag,
                        Description = menuItemCreateDto.Description,
                        Image = "images/" + fileName
                    };

                    _db.MenuItems.Add(menuItemToCreate);
                    _db.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new { id = menuItemToCreate.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }

            return _response;
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem([FromForm] MenuItemUpdateDto menuItemUpdateDto, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDto == null || id != menuItemUpdateDto.Id)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);
                    if (menuItemFromDb == null)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    menuItemFromDb.Name = menuItemUpdateDto.Name;
                    menuItemFromDb.Description = menuItemUpdateDto.Description;
                    menuItemFromDb.Category = menuItemUpdateDto.Category;
                    menuItemFromDb.Price = menuItemUpdateDto.Price;
                    menuItemFromDb.SpecialTag = menuItemUpdateDto.SpecialTag;

                    if (menuItemUpdateDto.File != null && menuItemUpdateDto.File.Length > 0)
                    {
                        var imagePath = Path.Combine("Content", menuItemFromDb.Image.TrimStart('/'));

                        if (imagePath != null)
                        {
                            System.IO.File.Delete(imagePath);
                        }

                        var pathUpload = "Content/images/";
                        var fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(menuItemUpdateDto.File.FileName);
                        var path = Path.Combine(pathUpload, fileName);
                        await using var fileStream = new FileStream(path, FileMode.Create);
                        await menuItemUpdateDto.File.CopyToAsync(fileStream);
                        menuItemFromDb.Image = "images/" + fileName;
                    }
                    _db.MenuItems.Update(menuItemFromDb);
                    _db.SaveChanges();
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }

            return _response;
        }


        [HttpDelete("{id:int}")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            try
            {
                if (id == 0)
                {   _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }

                MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);
                if (menuItemFromDb == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }

                var imagePath = Path.Combine("Content", menuItemFromDb.Image.TrimStart('/'));

                if (imagePath != null)
                {
                    System.IO.File.Delete(imagePath);
                }

                _db.MenuItems.Remove(menuItemFromDb);
                _db.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }

            return _response;

        }



    }

}

