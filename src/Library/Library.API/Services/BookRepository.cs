﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Contexts;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Services
{
    public class BookRepository : IBookRepository, IDisposable
    {
        private LibraryContext _context;

        public BookRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(Guid authorId)
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

        public async Task<Book> GetBookAsync(Guid authorId, Guid bookId)
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

        public void AddBook(Book bookToAdd)
        {
            if (bookToAdd == null)
            {
                throw new ArgumentNullException(nameof(bookToAdd));
            }

            _context.Add(bookToAdd);
        }

        public async Task AddBookForAuthorAsync(Guid authorId, Book book)
        {
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == authorId);
            if (book.Id == Guid.Empty)
            {
                book.Id = Guid.NewGuid();
            }
            author.Books.Add(book);
        }

        public async Task<bool> SaveChangesAsync()
        {
            // return true if 1 or more entities were changed
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
