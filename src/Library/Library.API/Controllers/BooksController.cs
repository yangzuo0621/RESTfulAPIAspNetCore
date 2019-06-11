using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Services;
using Library.API.Models;
using Library.API.Entities;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        public readonly IBookRepository _bookRepository;
        public readonly IAuthorRepository _authorRepository;
        public readonly IMapper _mapper;

        public BooksController(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksForAuthorAsync(Guid authorId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _bookRepository.GetBooksAsync(authorId);
            return Ok(_mapper.Map<IEnumerable<BookDto>>(booksFromRepo));
        }

        [HttpGet("{bookId}", Name = "GetBookForAuthor")]
        public async Task<ActionResult<BookDto>> GetBookForAuthorAsync(Guid authorId, Guid bookId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BookDto>(bookForAuthorFromRepo));
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> CreateBookForAuthorAsync(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookEntity = _mapper.Map<Book>(book);
            await _bookRepository.AddBookForAuthorAsync(authorId, bookEntity);

            if(!await _bookRepository.SaveChangesAsync())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save.");
            }

            var bookToReturn = _mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute(
                "GetBookForAuthor",
                new { authorId = authorId, bookId = bookToReturn.Id },
                bookToReturn);
        }
    }
}
