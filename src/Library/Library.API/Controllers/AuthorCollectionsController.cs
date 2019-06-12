using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Library.API.Entities;
using Library.API.Helpers;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    [ApiController]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(IAuthorRepository authorRepository, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollectionAsync(
            [FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if (authorCollection == null)
            {
                return BadRequest();
            }

            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);

            foreach (var author in authorEntities)
            {
                await _authorRepository.AddAuthorAsync(author);
            }

            if (!await _authorRepository.SaveChangesAsync())
            {
                throw new Exception("Creating an author collection failed on save.");
            }

            var authorCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString = string.Join(",", authorCollectionToReturn.Select(a => a.Id));

            return CreatedAtRoute("GetAuthorCollection", new { authorIds = idsAsString }, authorCollectionToReturn);
        }

        // (key1,key2, ...)
        [HttpGet("({authorIds})", Name = "GetAuthorCollection")]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorCollectionAsync(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> authorIds)
        {
            if (authorIds == null)
            {
                return BadRequest();
            }

            var authorEntities = await _authorRepository.GetAuthorsAsync(authorIds);

            if (authorIds.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }
    }
}
