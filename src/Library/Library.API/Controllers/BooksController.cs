using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
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
        public readonly ILibraryRepository _libraryRepository;
        public readonly IMapper _mapper;

        public BooksController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksForAuthorAsync(Guid authorId)
        {
            if (!await _libraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _libraryRepository.GetBooksForAuthorAsync(authorId);
            return Ok(_mapper.Map<IEnumerable<BookDto>>(booksFromRepo));
        }

        [HttpGet("{bookId}", Name = "GetBookForAuthor")]
        public async Task<ActionResult<BookDto>> GetBookForAuthorAsync(Guid authorId, Guid bookId)
        {
            if (!await _libraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = await _libraryRepository.GetBookForAuthorAsync(authorId, bookId);
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

            if (!await _libraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookEntity = _mapper.Map<Book>(book);
            await _libraryRepository.AddBookForAuthorAsync(authorId, bookEntity);

            if (!await _libraryRepository.SaveChangesAsync())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save.");
            }

            var bookToReturn = _mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute(
                "GetBookForAuthor",
                new { authorId = authorId, bookId = bookToReturn.Id },
                bookToReturn);
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteBookForAuthorAsync(Guid authorId, Guid bookId)
        {
            if (!await _libraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = await _libraryRepository.GetBookForAuthorAsync(authorId, bookId);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            await _libraryRepository.DeleteBookAsync(bookForAuthorFromRepo);

            if (!await _libraryRepository.SaveChangesAsync())
            {
                throw new Exception($"Deleting book {bookId} for author {authorId} failed on save.");
            }

            return NoContent();
        }

        [HttpPut("{bookId}")]
        public async Task<ActionResult> UpdateBookForAuthorAsync(
            Guid authorId, Guid bookId, [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!await _libraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = await _libraryRepository.GetBookForAuthorAsync(authorId, bookId);

            if (bookForAuthorFromRepo == null)
            {
                #region Upserting with HTTP PUT
                //var bookToAdd = _mapper.Map<Book>(book);
                //bookToAdd.Id = bookId;

                //await _libraryRepository.AddBookForAuthorAsync(authorId, bookToAdd);

                //if (!await _libraryRepository.SaveChangesAsync())
                //{
                //    throw new Exception($"Upserting book {bookId} for author {authorId} failed on save.");
                //}

                //var bookToReturn = _mapper.Map<BookDto>(bookToAdd);

                //return CreatedAtRoute("GetBookForAuthor",
                //    new { authorId = authorId, bookId = bookToReturn.Id },
                //    bookToReturn);
                #endregion

                return NotFound();
            }

            await _libraryRepository.UpdateBookForAuthorAsync(bookForAuthorFromRepo);

            _mapper.Map(book, bookForAuthorFromRepo);

            if (!await _libraryRepository.SaveChangesAsync())
            {
                throw new Exception($"Update book {bookId} for author {authorId} failed on save.");
            }

            return NoContent();
        }

        [HttpPatch("{bookId}")]
        public async Task<IActionResult> PartiallyUpdateBookForAuthorAsync(
            Guid authorId, Guid bookId, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!await _libraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = await _libraryRepository.GetBookForAuthorAsync(authorId, bookId);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookToPatch = _mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            patchDoc.ApplyTo(bookToPatch);

            // add validation

            _mapper.Map(bookToPatch, bookForAuthorFromRepo);

            await _libraryRepository.UpdateBookForAuthorAsync(bookForAuthorFromRepo);

            if (!await _libraryRepository.SaveChangesAsync())
            {
                throw new Exception($"Patching book {bookId} for author {authorId} failed on save.");
            }

            return NoContent();
        }
    }
}
