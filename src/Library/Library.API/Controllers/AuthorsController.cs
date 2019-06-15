using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using AutoMapper;
using Library.API.Services;
using Library.API.Models;
using Library.API.Entities;
using Library.API.Helpers;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IUrlHelper _urlHelper;
        private readonly IMapper _mapper;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetAuthors")]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorsAsync([FromQuery] AuthorsResourceParameters parameters)
        {
            var authorsFromRepo = await _libraryRepository.GetAuthorsAsync(parameters);

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

            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            return Ok(authors);
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        public async Task<ActionResult<AuthorDto>> GetAuthorAsync(Guid authorId)
        {
            var authorFromRepo = await _libraryRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
        }

        [HttpPost]
        public async Task<ActionResult<AuthorDto>> CreateAuthorAsync([FromBody] AuthorForCreationDto author)
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

            return CreatedAtRoute("GetAuthor", new { authorId = authorToReturn.Id }, authorToReturn);
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

        [HttpDelete("{authorId}")]
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
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = parameters.PageNumber - 1,
                            pageSize = parameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = parameters.PageNumber + 1,
                            pageSize = parameters.PageSize
                        });
                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = parameters.PageNumber,
                            pageSize = parameters.PageSize
                        });
            }
        }
    }
}
