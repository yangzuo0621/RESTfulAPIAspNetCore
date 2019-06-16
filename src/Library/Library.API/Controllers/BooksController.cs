using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using Library.API.Services;
using Library.API.Models;
using Library.API.Entities;
using Microsoft.AspNetCore.Routing;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;

        public BooksController(ILibraryRepository libraryRepository, LinkGenerator linkGenerator, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _linkGenerator = linkGenerator;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetBooksForAuthor")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksForAuthorAsync(Guid authorId)
        {
            if (!await _libraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _libraryRepository.GetBooksForAuthorAsync(authorId);

            var booksForAuthor = _mapper.Map<IEnumerable<BookDto>>(booksFromRepo);

            booksForAuthor = booksForAuthor.Select(book => CreateLinksForBook(book));

            var wrapper = new LinkedCollectionResourceWrapperDto<BookDto>(booksForAuthor);

            return Ok(CreateLinksForBook(wrapper));
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

            var bookForAuthor = _mapper.Map<BookDto>(bookForAuthorFromRepo);
            return Ok(CreateLinksForBook(bookForAuthor));
        }

        [HttpPost(Name = "CreateBookForAuthor")]
        public async Task<ActionResult<BookDto>> CreateBookForAuthorAsync(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            // As the [ApiController] attribute, the common validation is handled by it, such as model binding.
            // So only custom validation is handled by this code
            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto),
                    "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
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
                CreateLinksForBook(bookToReturn));
        }

        [HttpDelete("{bookId}", Name = "DeleteBookForAuthor")]
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

        [HttpPut("{bookId}", Name = "UpdateBookForAuthor")]
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

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
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

        [HttpPatch("{bookId}", Name = "PartiallyUpdateBookForAuthor")]
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
                #region Upserting with PATCH
                //var bookDto = new BookForUpdateDto();
                //patchDoc.ApplyTo(bookDto);

                //if (bookDto.Description == bookDto.Title)
                //{
                //    ModelState.AddModelError(nameof(BookForUpdateDto),
                //        "The provided description should be different from the title.");
                //}

                //TryValidateModel(bookDto);

                //if (!ModelState.IsValid)
                //{
                //    return new UnprocessableEntityObjectResult(ModelState);
                //}

                //var bookToAdd = _mapper.Map<Book>(bookDto);
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

            var bookToPatch = _mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            patchDoc.ApplyTo(bookToPatch, ModelState);

            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "The provided description should be different from the title.");
            }

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // add validation

            _mapper.Map(bookToPatch, bookForAuthorFromRepo);

            await _libraryRepository.UpdateBookForAuthorAsync(bookForAuthorFromRepo);

            if (!await _libraryRepository.SaveChangesAsync())
            {
                throw new Exception($"Patching book {bookId} for author {authorId} failed on save.");
            }

            return NoContent();
        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            book.Links.Add(new LinkDto(
                href: _linkGenerator.GetUriByRouteValues(HttpContext, "GetBookForAuthor", new { bookId = book.Id }), 
                rel: "self", 
                method: "GET"));

            book.Links.Add(new LinkDto(
              href: _linkGenerator.GetUriByRouteValues(HttpContext, "DeleteBookForAuthor", new { bookId = book.Id }),
              rel: "delte_book",
              method: "DELETE"));

            book.Links.Add(new LinkDto(
                href: _linkGenerator.GetUriByRouteValues(HttpContext, "UpdateBookForAuthor", new { bookId = book.Id }),
                rel: "update_book",
                method: "PUT"));

            book.Links.Add(new LinkDto(
                href: _linkGenerator.GetUriByRouteValues(HttpContext, "PartiallyUpdateBookForAuthor", new { bookId = book.Id }),
                rel: "partially_update_book",
                method: "PATCH"));

            return book;
        }

        private LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBook(
            LinkedCollectionResourceWrapperDto<BookDto> booksWrapper)
        {
            booksWrapper.Links.Add(new LinkDto(
                href: _linkGenerator.GetUriByRouteValues(HttpContext, "GetBooksForAuthor", new { }),
                rel: "self",
                method: "GET"));

            return booksWrapper;
        }
    }
}
