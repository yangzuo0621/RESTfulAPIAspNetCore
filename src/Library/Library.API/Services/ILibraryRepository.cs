using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Library.API.Entities;
using Library.API.Helpers;

namespace Library.API.Services
{
    public interface ILibraryRepository : IDisposable
    {
        #region Author
        Task<Author> GetAuthorAsync(Guid authorId);

        Task<IEnumerable<Author>> GetAuthorsAsync();

        Task<PagedList<Author>> GetAuthorsAsync(AuthorsResourceParameters authorsResourceParameters);

        Task<IEnumerable<Author>> GetAuthorsAsync(IEnumerable<Guid> authorIds);

        Task AddAuthorAsync(Author author);

        Task DeleteAuthorAsync(Author author);

        Task UpdateAuthorAsync(Author author);

        Task<bool> AuthorExistsAsync(Guid authorId);
        #endregion

        #region Book
        Task<IEnumerable<Book>> GetBooksForAuthorAsync(Guid authorId);

        Task<Book> GetBookForAuthorAsync(Guid authorId, Guid bookId);

        Task AddBookForAuthorAsync(Guid authorId, Book book);

        Task UpdateBookForAuthorAsync(Book book);

        Task DeleteBookAsync(Book book);
        #endregion

        Task<bool> SaveChangesAsync();
    }
}
