using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Library.API.Entities;
using Library.API.Contexts;
using Library.API.Helpers;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository, IDisposable
    {
        private LibraryContext _context;

        public LibraryRepository(LibraryContext context)
        {
            _context = context;
        }

        #region Author
        public async Task<Author> GetAuthorAsync(Guid authorId)
        {
            if (Guid.Empty == authorId)
            {
                throw new ArgumentException(nameof(authorId));
            }

            return await _context.Authors.FirstOrDefaultAsync(a => a.Id == authorId);
        }

        public async Task<IEnumerable<Author>> GetAuthorsAsync()
        {
            return await _context.Authors
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Author>> GetAuthorsAsync(AuthorsResourceParameters parameters)
        {
            return await _context.Authors
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .Skip(parameters.PageSize * (parameters.PageNumber - 1))
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Author>> GetAuthorsAsync(IEnumerable<Guid> authorIds)
        {
            return await _context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .OrderBy(a => a.LastName)
                .ToListAsync();
        }

        public Task AddAuthorAsync(Author author)
        {
            author.Id = Guid.NewGuid();
            _context.Authors.Add(author);

            if (author.Books.Any())
            {
                foreach (var book in author.Books)
                {
                    book.Id = Guid.NewGuid();
                }
            }

            return Task.CompletedTask;
        }

        public Task DeleteAuthorAsync(Author author)
        {
            _context.Authors.Remove(author);
            return Task.CompletedTask;
        }

        public Task UpdateAuthorAsync(Author author)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> AuthorExistsAsync(Guid authorId)
        {
            return await _context.Authors.AnyAsync(a => a.Id == authorId);
        }
        #endregion

        #region Book
        public async Task<IEnumerable<Book>> GetBooksForAuthorAsync(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentException(nameof(authorId));
            }

            return await _context.Books
                .Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<Book> GetBookForAuthorAsync(Guid authorId, Guid bookId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentException(nameof(authorId));
            }

            if (bookId == Guid.Empty)
            {
                throw new ArgumentException(nameof(bookId));
            }

            return await _context.Books
                .Include(b => b.Author)
                .Where(b => b.AuthorId == authorId && b.Id == bookId)
                .FirstOrDefaultAsync();
        }

        public async Task AddBookForAuthorAsync(Guid authorId, Book book)
        {
            var author = await GetAuthorAsync(authorId);
            if (author != null)
            {
                if (book.Id == Guid.Empty)
                {
                    book.Id = Guid.NewGuid();
                }
                author.Books.Add(book);
            }
        }

        public Task UpdateBookForAuthorAsync(Book book)
        {
            return Task.CompletedTask;
        }

        public Task DeleteBookAsync(Book book)
        {
            _context.Books.Remove(book);
            return Task.CompletedTask;
        }
        #endregion

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
