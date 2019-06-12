using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Library.API.Services;
using Library.API.Models;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
        {
            var authorsFromRepo = await _authorRepository.GetAuthorsAsync();
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            return Ok(authors);
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
        {
            var authorFromRepo = await _authorRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
        }

        [HttpPost]
        public async Task<ActionResult<AuthorDto>> CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = _mapper.Map<Author>(author);

            await _authorRepository.AddAuthorAsync(authorEntity);

            if (!await _authorRepository.SaveChangesAsync())
            {
                throw new Exception("Creating an author failed on save.");
                // return StatusCode(500, "A problem happened with handling your request.");
            }

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new { authorId = authorToReturn.Id }, authorToReturn);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> BlockAuthorCreation(Guid id)
        {
            if (await _authorRepository.AuthorExistsAsync(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{authorId}")]
        public async Task<IActionResult> DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = await _authorRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _authorRepository.DeleteAuthor(authorFromRepo);

            if (!await _authorRepository.SaveChangesAsync())
            {
                throw new Exception($"Deleting author {authorId} failed on save.");
            }

            return NoContent();
        }
    }
}
