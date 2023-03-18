using Entities.LinkModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class RootController : ControllerBase
    {
        private readonly LinkGenerator _linkGenerator;

        public RootController(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        [HttpGet(Name = "GetRoot")]
        public async Task<IActionResult> GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType.Contains("application/vnd.btkakademi.apiroot"))
            {
                var link = new List<Link>()
                {
                    new Link()
                    {
                        HRef=_linkGenerator.GetUriByName(HttpContext,nameof(GetRoot),new{}),
                        Rel="_self",
                        Method="GET"
                    },
                    new Link()
                    {
                        HRef=_linkGenerator.GetUriByName(HttpContext,nameof(BooksController.GetAllBooksAsync),new{}),
                        Rel="book",
                        Method="GET"
                    },
                    new Link()
                    {
                        HRef=_linkGenerator.GetUriByName(HttpContext,nameof(BooksController.CreateOneBookAsync),new{}),
                        Rel="_self",
                        Method="POST"
                    }
                };
                return Ok(link);
            }
            return NoContent(); //204
        }
    }
}
