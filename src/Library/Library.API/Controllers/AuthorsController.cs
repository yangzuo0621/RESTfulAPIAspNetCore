using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using AutoMapper;
using Library.API.Services;
using Library.API.Models;
using Library.API.Entities;
using Library.API.Helpers;
using System.Collections;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ITypeHelperService _typeHelperService;

        public AuthorsController(
            ILibraryRepository libraryRepository, 
            LinkGenerator linkGenerator, 
            IMapper mapper, 
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;
            _linkGenerator = linkGenerator;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
        }

        [HttpGet(Name = "GetAuthors")]
        public async Task<ActionResult> GetAuthorsAsync(
            [FromQuery] AuthorsResourceParameters parameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(parameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<AuthorDto>(parameters.Fields))
            {
                return BadRequest();
            }

            var authorsFromRepo = await _libraryRepository.GetAuthorsAsync(parameters);

            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = authorsFromRepo.TotalCount,
                    pageSize = authorsFromRepo.PageSize,
                    currentPage = authorsFromRepo.CurrentPage,
                    totalPages = authorsFromRepo.TotalPages,
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateLinksForAuthor(parameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);

                var shapedAuthors = authors.ShapeData(parameters.Fields);

                var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
                {
                    var authorAsDictionary = author as IDictionary<string, object>;
                    var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], parameters.Fields);

                    authorAsDictionary.Add("links", authorLinks);

                    return authorAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = shapedAuthorsWithLinks,
                    links = links
                };

                return Ok(linkedCollectionResource);
            } 
            else
            {
                var previousPageLink = authorsFromRepo.HasPrevious ?
                    CreateAuthorsResourceUri(parameters, ResourceUriType.PreviousPage) : null;

                var nextPageLink = authorsFromRepo.HasNext ?
                    CreateAuthorsResourceUri(parameters, ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    totalCount = authorsFromRepo.TotalCount,
                    pageSize = authorsFromRepo.PageSize,
                    currentPage = authorsFromRepo.CurrentPage,
                    totalPages = authorsFromRepo.TotalPages,
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                return Ok(authors.ShapeData(parameters.Fields));
            }
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        public async Task<ActionResult> GetAuthorAsync(Guid authorId, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var authorFromRepo = await _libraryRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            var author = _mapper.Map<AuthorDto>(authorFromRepo);

            var links = CreateLinksForAuthor(authorId, fields);

            var linkedResourceToReturn = author.ShapeData(fields) as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthor")]
        public async Task<ActionResult> CreateAuthorAsync([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = _mapper.Map<Author>(author);

            await _libraryRepository.AddAuthorAsync(authorEntity);

            if (!await _libraryRepository.SaveChangesAsync())
            {
                throw new Exception("Creating an author failed on save.");
                // return StatusCode(500, "A problem happened with handling your request.");
            }

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor", new { authorId = linkedResourceToReturn["Id"] }, linkedResourceToReturn);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> BlockAuthorCreationAsync(Guid id)
        {
            if (await _libraryRepository.AuthorExistsAsync(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{authorId}", Name = "DeleteAuthor")]
        public async Task<IActionResult> DeleteAuthorAsync(Guid authorId)
        {
            var authorFromRepo = await _libraryRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            await _libraryRepository.DeleteAuthorAsync(authorFromRepo);

            if (!await _libraryRepository.SaveChangesAsync())
            {
                throw new Exception($"Deleting author {authorId} failed on save.");
            }

            return NoContent();
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters parameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _linkGenerator.GetUriByRouteValues(HttpContext, "GetAuthors",
                        new
                        {
                            fields = parameters.Fields,
                            orderBy = parameters.OrderBy,
                            searchQuery = parameters.SearchQuery,
                            genre = parameters.Genre,
                            pageNumber = parameters.PageNumber - 1,
                            pageSize = parameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _linkGenerator.GetUriByRouteValues(HttpContext, "GetAuthors",
                        new
                        {
                            fields = parameters.Fields,
                            orderBy = parameters.OrderBy,
                            searchQuery = parameters.SearchQuery,
                            genre = parameters.Genre,
                            pageNumber = parameters.PageNumber + 1,
                            pageSize = parameters.PageSize
                        });
                case ResourceUriType.Current:
                default:
                    return _linkGenerator.GetUriByRouteValues(HttpContext, "GetAuthors",
                        new
                        {
                            fields = parameters.Fields,
                            orderBy = parameters.OrderBy,
                            searchQuery = parameters.SearchQuery,
                            genre = parameters.Genre,
                            pageNumber = parameters.PageNumber,
                            pageSize = parameters.PageSize
                        });
            }
        }

        //private string CreateAuthorsResourceUri(AuthorsResourceParameters parameters, ResourceUriType type)
        //{
        //    switch (type)
        //    {
        //        case ResourceUriType.PreviousPage:
        //            return Url.Link("GetAuthors",
        //                new
        //                {
        //                    pageNumber = parameters.PageNumber - 1,
        //                    pageSize = parameters.PageSize
        //                });
        //        case ResourceUriType.NextPage:
        //            return Url.Link("GetAuthors",
        //                new
        //                {
        //                    pageNumber = parameters.PageNumber + 1,
        //                    pageSize = parameters.PageSize
        //                });
        //        default:
        //            return Url.Link("GetAuthors",
        //                new
        //                {
        //                    pageNumber = parameters.PageNumber,
        //                    pageSize = parameters.PageSize
        //                });
        //    }
        //}

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(
                    href: _linkGenerator.GetUriByRouteValues(HttpContext, "GetAuthor", new { authorId = authorId }),
                    rel: "self",
                    method: "GET"));
            }
            else
            {
                links.Add(new LinkDto(
                    href: _linkGenerator.GetUriByRouteValues(HttpContext, "GetAuthor", new { authorId = authorId, fields = fields }),
                    rel: "self",
                    method: "GET"));
            }

            links.Add(new LinkDto(
                href: _linkGenerator.GetUriByRouteValues(HttpContext, "DeleteAuthor", new { authorId = authorId }),
                rel: "delete_author",
                method: "DELETE"));

            links.Add(new LinkDto(
                href: _linkGenerator.GetUriByRouteValues(HttpContext, "CreateBookForAuthor", new { authorId = authorId }),
                rel: "create_book_for_author",
                method: "POST"));

            links.Add(new LinkDto(
               href: _linkGenerator.GetUriByRouteValues(HttpContext, "GetBooksForAuthor", new { authorId = authorId }),
               rel: "books",
               method: "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(
            AuthorsResourceParameters paramters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(
                href: CreateAuthorsResourceUri(paramters, ResourceUriType.Current),
                rel: "self",
                method: "GET"));

            if (hasNext)
            {
                links.Add(new LinkDto(
                    href: CreateAuthorsResourceUri(paramters, ResourceUriType.NextPage),
                    rel: "nextPage",
                    method: "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(
                   href: CreateAuthorsResourceUri(paramters, ResourceUriType.PreviousPage),
                   rel: "previousPage",
                   method: "GET"));
            }

            return links;
        }
    }
}
