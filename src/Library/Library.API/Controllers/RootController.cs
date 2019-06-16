using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Library.API.Models;

namespace Library.API.Controllers
{
    [Route("api")]
    public class RootController : ControllerBase
    {
        private readonly LinkGenerator _linkGenerator;

        public RootController(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var links = new List<LinkDto>();

                links.Add(new LinkDto(
                    href: _linkGenerator.GetUriByRouteValues(HttpContext, "GetRoot", new { }),
                    rel: "self",
                    method: "GET"));

                links.Add(new LinkDto(
                    href: _linkGenerator.GetUriByRouteValues(HttpContext, "GetAuthors", new { }),
                    rel: "authors",
                    method: "GET"));

                links.Add(new LinkDto(
                   href: _linkGenerator.GetUriByRouteValues(HttpContext, "CreateAuthor", new { }),
                   rel: "create_authors",
                   method: "POST"));

                return Ok(links);
            }

            return NoContent();
        }
    }
}
