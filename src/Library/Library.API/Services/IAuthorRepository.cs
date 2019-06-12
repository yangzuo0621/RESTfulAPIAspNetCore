using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Library.API.Entities;

namespace Library.API.Services
{
    public interface IAuthorRepository : IDisposable
    {
        Task<bool> AuthorExistsAsync(Guid authorId);

        Task<IEnumerable<Author>> GetAuthorsAsync();

        Task<IEnumerable<Author>> GetAuthorsAsync(IEnumerable<Guid> authorIds);

        Task<Author> GetAuthorAsync(Guid authorId);

        Task AddAuthorAsync(Author author);

        void UpdateAuthor(Author author);

        void DeleteAuthor(Author author);

        Task<bool> SaveChangesAsync();
    }
}
