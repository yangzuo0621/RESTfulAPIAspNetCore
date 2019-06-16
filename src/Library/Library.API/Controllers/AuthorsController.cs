﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;

        public AuthorsController(
            ILibraryRepository libraryRepository, 
            LinkGenerator linkGenerator, 
            IMapper mapper, 
            IPropertyMappingService propertyMappingService)
        {
            _libraryRepository = libraryRepository;
            _linkGenerator = linkGenerator;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
        }

        [HttpGet(Name = "GetAuthors")]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorsAsync([FromQuery] AuthorsResourceParameters parameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(parameters.OrderBy))
            {
                return BadRequest();
            }

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
                    return _linkGenerator.GetUriByRouteValues(HttpContext, "GetAuthors",
                        new
                        {
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
                            orderBy = parameters.OrderBy,
                            searchQuery = parameters.SearchQuery,
                            genre = parameters.Genre,
                            pageNumber = parameters.PageNumber + 1,
                            pageSize = parameters.PageSize
                        });
                default:
                    return _linkGenerator.GetUriByRouteValues(HttpContext, "GetAuthors",
                        new
                        {
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
    }
}
