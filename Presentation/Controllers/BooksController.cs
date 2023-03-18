using Entities.DataTransferObjects;
using Entities.Exceptions;
using Entities.Models;
using Entities.RequestFeatures;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Presentation.ActionFilters;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    //[ResponseCache(CacheProfileName="5mins")] //Cachelenebilir özelliğini kazandırdık.
    //[ApiVersion("1.0")]
   // [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 80)]
    [ApiController]
    [Route("api/books")]
    [ServiceFilter(typeof(LogFilterAttribute))]
    [ApiExplorerSettings(GroupName = "v1")]
    public class BooksController : ControllerBase
    {
        private readonly IServiceManager _manager;

        public BooksController(IServiceManager services)
        {
            _manager = services;
        }

        [Authorize]
        [HttpHead]
        [HttpGet]
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        public async Task<IActionResult> GetAllBooksAsync([FromQuery] BookParameters bookParameters)
        {
            var linkParameters = new LinkParameters()
            {
                BookParameters = bookParameters,
                HttpContext = HttpContext
            };
            var result = await _manager.BookService.GetAllBooksAsync(linkParameters, false);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.metaData));
            return result.linkResponse.HasLinks ? Ok(result.linkResponse.LinkedEntities) : Ok(result.linkResponse.ShapedEntities);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetOneBookAsync([FromRoute(Name = "id")] int id)
        {
            var bookFind = await _manager.BookService.GetOneBookByIdAsync(id, false);

            return Ok(bookFind);
        }
        [Authorize]
        [HttpGet("details")]
        public async Task<IActionResult> GetAllBookWithDetailsAsync()
        {
            return Ok(await _manager.
                BookService.
                GetAllBooksWithDetailsAsync(false)) ;
        }

        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> CreateOneBookAsync([FromBody] BookDtoForInsertion book)
        {
            await _manager.BookService.CreateOneBookAsync(book);
            return StatusCode(201, book); //CreatedAtRoute();
        }

        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> UpdateOneBookAsync([FromRoute(Name = "id")] int id, [FromBody] BookDtoForUpdate bookDto)
        {
            await _manager.BookService.UpdateOneBookAsync(id, bookDto, false);
            return NoContent();//204
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllBooksAsync([FromRoute(Name = "id")] int id)
        {
            await _manager.BookService.DeleteOneBookAsync(id, false);
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Editor,Admin")]

        public async Task<IActionResult> PartiallyUpdateOneBookAsync([FromRoute(Name = "id")] int id, [FromBody] JsonPatchDocument<BookDtoForUpdate> bookPatch)
        {
            if (bookPatch is null)
                return BadRequest();

            var result = await _manager.BookService.GetOneBookForPatchAsync(id, false);

            bookPatch.ApplyTo(result.bookDtoForUpdate, ModelState);

            TryValidateModel(result.bookDtoForUpdate);

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            await _manager.BookService.SaveChangesForPatchAsync(result.bookDtoForUpdate, result.book);

            return NoContent();

        }

        [HttpOptions]
        [Authorize]
        public IActionResult GetBooksOptions()
        {
            Response.Headers.Add("Allow", "GET,PUT,PATCH,DELETE,HEAD, OPTIONS");
            return Ok();
        }
    }
}
