using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Library.API.Contexts;
using Library.API.Services;
using Library.API.Entities;

namespace Library.API.Tests.UnitTests
{
    public class LibraryRepositoryTests
    {
        #region AddAuthorAsync Test
        [Fact]
        public async Task AddAuthorAsync_Test()
        {
            using (var context = new LibraryContext(Utilities.TestDbContextOptions()))
            using (ILibraryRepository repository = new LibraryRepository(context))
            {
                await repository.AddAuthorAsync(new Author
                {
                    FirstName = "Jens",
                    LastName = "Lapidus",
                    Genre = "Thriller",
                    DateOfBirth = new DateTimeOffset(new DateTime(1974, 5, 24)),
                    Books = new List<Book>
                    {
                        new Book
                        {
                            Title = "Easy Money",
                            Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus"
                        }
                    }
                });
                await repository.SaveChangesAsync();

                var authors = await repository.GetAuthorsAsync();
                var author = authors.ElementAt(0);

                Assert.True(authors.Count() == 1);
                Assert.NotEqual(Guid.Empty, author.Id);
                Assert.Equal("Jens", author.FirstName);
                Assert.Equal("Lapidus", author.LastName);
                Assert.Equal("Thriller", author.Genre);
                Assert.Equal(new DateTimeOffset(new DateTime(1974, 5, 24)), author.DateOfBirth);

                var books = author.Books;
                var book = books.ElementAt(0);

                Assert.True(books.Count() == 1);
                Assert.NotEqual(Guid.Empty, book.Id);
                Assert.Equal("Easy Money", book.Title);
                Assert.Equal("Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus", book.Description);
            }
        }
        #endregion

        #region GetAuthorAsync Test
        [Fact]
        public async Task GetAuthorAsync_WithExistAuthor_Test()
        {
            using (var context = new LibraryContext(Utilities.TestDbContextOptions()))
            using (ILibraryRepository repository = new LibraryRepository(context))
            {
                context.Authors.Add(new Author()
                {
                    Id = new Guid("a1da1d8e-1988-4634-b538-a01709477b77"),
                    FirstName = "Jens",
                    LastName = "Lapidus",
                    Genre = "Thriller",
                    DateOfBirth = new DateTimeOffset(new DateTime(1974, 5, 24)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("1325360c-8253-473a-a20f-55c269c20407"),
                            Title = "Easy Money",
                            Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus."
                        }
                    }
                });
                context.SaveChanges();

                var author = await repository.GetAuthorAsync(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"));

                Assert.NotNull(author);
                Assert.Equal(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), author.Id);
                Assert.Equal("Jens", author.FirstName);
                Assert.Equal("Lapidus", author.LastName);
                Assert.Equal("Thriller", author.Genre);
                Assert.Equal(new DateTimeOffset(new DateTime(1974, 5, 24)), author.DateOfBirth);

                var book = author.Books.ElementAt(0);
                Assert.Equal(Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"), book.Id);
                Assert.Equal("Easy Money", book.Title);
                Assert.Equal("Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus.", book.Description);
            }
        }

        [Fact]
        public async Task GetAuthorAsync_WithNonExistAuthor_Test()
        {
            using (var context = new LibraryContext(Utilities.TestDbContextOptions()))
            using (ILibraryRepository repository = new LibraryRepository(context))
            {
                context.Authors.Add(new Author()
                {
                    Id = new Guid("a1da1d8e-1988-4634-b538-a01709477b77"),
                    FirstName = "Jens",
                    LastName = "Lapidus",
                    Genre = "Thriller",
                    DateOfBirth = new DateTimeOffset(new DateTime(1974, 5, 24)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("1325360c-8253-473a-a20f-55c269c20407"),
                            Title = "Easy Money",
                            Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus."
                        }
                    }
                });
                context.SaveChanges();

                var author = await repository.GetAuthorAsync(Guid.Parse("f74d6899-9ed2-4137-9876-66b070553f8f"));

                Assert.Null(author);
            }
        }

        [Fact]
        public async Task GetAuthorAsync_WithGuidEmpty_Test()
        {
            using (var context = new LibraryContext(Utilities.TestDbContextOptions()))
            using (ILibraryRepository repository = new LibraryRepository(context))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => repository.GetAuthorAsync(Guid.Empty));
            }
        }
        #endregion

        #region AuthorExistsAsync Test
        [Fact]
        public async Task AuthorExistsAsync_Test()
        {
            using (var context = new LibraryContext(Utilities.TestDbContextOptions()))
            using (ILibraryRepository repository = new LibraryRepository(context))
            {
                context.Authors.Add(new Author()
                {
                    Id = new Guid("a1da1d8e-1988-4634-b538-a01709477b77"),
                    FirstName = "Jens",
                    LastName = "Lapidus",
                    Genre = "Thriller",
                    DateOfBirth = new DateTimeOffset(new DateTime(1974, 5, 24)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("1325360c-8253-473a-a20f-55c269c20407"),
                            Title = "Easy Money",
                            Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus. It has been a success in term of sales, and the paperback was the fourth best seller of Swedish novels in 2007."
                        }
                    }
                });
                context.SaveChanges();

                var exist = await repository.AuthorExistsAsync(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"));
                var nonExist = await repository.AuthorExistsAsync(Guid.Parse("f74d6899-9ed2-4137-9876-66b070553f8f"));

                Assert.True(exist);
                Assert.False(nonExist);
            }
        }
        #endregion

        #region DeleteAuthorAsync Test
        [Fact]
        public async Task DeleteAuthorAsync_Test()
        {
            using (var context = new LibraryContext(Utilities.TestDbContextOptions()))
            using (ILibraryRepository repository = new LibraryRepository(context))
            {
                // Arrange
                context.Authors.Add(new Author()
                {
                    Id = new Guid("a1da1d8e-1988-4634-b538-a01709477b77"),
                    FirstName = "Jens",
                    LastName = "Lapidus",
                    Genre = "Thriller",
                    DateOfBirth = new DateTimeOffset(new DateTime(1974, 5, 24)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("1325360c-8253-473a-a20f-55c269c20407"),
                            Title = "Easy Money",
                            Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus."
                        }
                    }
                });
                context.SaveChanges();

                var author = await repository.GetAuthorAsync(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"));
                Assert.NotNull(author);

                // Act
                await repository.DeleteAuthorAsync(author);
                await repository.SaveChangesAsync();

                // Assert
                var NonAuthor = await repository.GetAuthorAsync(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"));
                Assert.Null(NonAuthor);
            }
        }
        #endregion

        #region UpdateAuthorAsync Test
        #endregion

        #region GetAuthorsAsync Test
        #endregion
    }
}
