using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Library.API.Entities;

namespace Library.API.Services
{
    public interface IBookRepository : IDisposable
    {
        Task<IEnumerable<Book>> GetBooksAsync(Guid authorId);

        Task<Book> GetBookAsync(Guid authorId, Guid bookId);

        void AddBook(Book bookToAdd);

        Task AddBookForAuthorAsync(Guid authorId, Book book);

        void DeleteBook(Book book);

        Task<bool> SaveChangesAsync();
    }
}
