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
        [Fact]
        public async Task UpdateAuthorAsync_Test()
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
                author.FirstName = "Jens U";
                author.LastName = "Lapidus U";
                author.Genre = "Thriller U";
                author.DateOfBirth = new DateTimeOffset(new DateTime(1980, 1, 1));

                await repository.UpdateAuthorAsync(author);
                await repository.SaveChangesAsync();
                author = null;

                var updataAuthor = await repository.GetAuthorAsync(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"));
                Assert.Equal("Jens U", updataAuthor.FirstName);
                Assert.Equal("Lapidus U", updataAuthor.LastName);
                Assert.Equal("Thriller U", updataAuthor.Genre);
                Assert.Equal(new DateTimeOffset(new DateTime(1980, 1, 1)), updataAuthor.DateOfBirth);
            }
        }
        #endregion

        #region GetAuthorsAsync Test
        [Fact]
        public async Task GetAuthorsAsync_WithNoAugument_Test()
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

                // Act
                var authors = await repository.GetAuthorsAsync();

                // Assert
                Assert.IsAssignableFrom<IEnumerable<Author>>(authors);
                Assert.True(authors.Count() == 1);
                Assert.Collection(authors, item => Assert.Equal(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), item.Id));
            }
        }

        [Fact]
        public async Task GetAuthorsAsync_WithIdsAugument_Test()
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
                context.Authors.Add(new Author()
                {
                    Id = new Guid("f74d6899-9ed2-4137-9876-66b070553f8f"),
                    FirstName = "Douglas",
                    LastName = "Adams",
                    Genre = "Science fiction",
                    DateOfBirth = new DateTimeOffset(new DateTime(1952, 3, 11)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("e57b605f-8b3c-4089-b672-6ce9e6d6c23f"),
                            Title = "The Hitchhiker's Guide to the Galaxy",
                            Description = "The Hitchhiker's Guide to the Galaxy"
                        }
                    }
                });
                context.SaveChanges();

                // Act
                var ids = new Guid[] { Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), Guid.Parse("f74d6899-9ed2-4137-9876-66b070553f8f") };
                var authors = await repository.GetAuthorsAsync(ids);

                // Assert
                Assert.IsAssignableFrom<IEnumerable<Author>>(authors);
                Assert.True(authors.Count() == 2);
                Assert.Collection(
                    authors, 
                    item => Assert.Equal(Guid.Parse("f74d6899-9ed2-4137-9876-66b070553f8f"), item.Id),
                    item => Assert.Equal(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), item.Id)
                );
            }
        }
        #endregion

        #region GetBooksForAuthorAsync Test
        [Fact]
        public async Task GetBooksForAuthorAsync_WithExsitAuthor_Test()
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

                var books = await repository.GetBooksForAuthorAsync(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"));

                Assert.NotNull(books);
                Assert.IsAssignableFrom<IEnumerable<Book>>(books);
                Assert.Collection(
                    books, 
                    bookItem => {
                        Assert.Equal(Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"), bookItem.Id);
                        Assert.Equal("Easy Money", bookItem.Title);
                        Assert.Equal("Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus.", bookItem.Description);
                    }
                );
            }
        }

        [Fact]
        public async Task GetBooksForAuthorAsync_WithNonExsitAuthor_Test()
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

                var books = await repository.GetBooksForAuthorAsync(Guid.Parse("f74d6899-9ed2-4137-9876-66b070553f8f"));

                Assert.IsAssignableFrom<IEnumerable<Book>>(books);
                Assert.Empty(books);
            }
        }

        [Fact]
        public async Task GetBooksForAuthorAsync_WithEmptyGuid_Test()
        {
            using (var context = new LibraryContext(Utilities.TestDbContextOptions()))
            using (ILibraryRepository repository = new LibraryRepository(context))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => repository.GetBooksForAuthorAsync(Guid.Empty));

            }
        }
        #endregion

        #region GetBookForAuthorAsync Test
        [Fact]
        public async Task GetBookForAuthorAsync_WithExistAuthorAndBook_Test()
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

                var book = await repository.GetBookForAuthorAsync(
                    Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"));

                Assert.NotNull(book);
                Assert.IsType<Book>(book);
                Assert.Equal(Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"), book.Id);
                Assert.Equal("Easy Money", book.Title);
                Assert.Equal("Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus.", book.Description);
            }
        }

        [Fact]
        public async Task GetBookForAuthorAsync_WithNonExistAuthorOrBook_Test()
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

                var book1 = await repository.GetBookForAuthorAsync(
                    Guid.Parse("f74d6899-9ed2-4137-9876-66b070553f8f"), Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"));
                var book2 = await repository.GetBookForAuthorAsync(
                    Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), Guid.Parse("e57b605f-8b3c-4089-b672-6ce9e6d6c23f"));
                var book3 = await repository.GetBookForAuthorAsync(
                    Guid.Parse("f74d6899-9ed2-4137-9876-66b070553f8f"), Guid.Parse("e57b605f-8b3c-4089-b672-6ce9e6d6c23f"));

                Assert.Null(book1);
                Assert.Null(book2);
            }
        }

        [Fact]
        public async Task GetBookForAuthorAsync_WithEmptyGuid_Test()
        {
            using (var context = new LibraryContext(Utilities.TestDbContextOptions()))
            using (ILibraryRepository repository = new LibraryRepository(context))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => repository.GetBookForAuthorAsync(Guid.Empty, Guid.Parse("1325360c-8253-473a-a20f-55c269c20407")));
                await Assert.ThrowsAsync<ArgumentException>(() => repository.GetBookForAuthorAsync(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), Guid.Empty));
                await Assert.ThrowsAsync<ArgumentException>(() => repository.GetBookForAuthorAsync(Guid.Empty, Guid.Empty));
            }
        }
        #endregion

        #region AddBookForAuthorAsync Test
        [Fact]
        public async Task AddBookForAuthorAsync_WithGuidBook_Test()
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
                    DateOfBirth = new DateTimeOffset(new DateTime(1974, 5, 24))
                });
                context.SaveChanges();

                await repository.AddBookForAuthorAsync(
                    authorId: Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), 
                    book: new Book
                    {
                        Id = new Guid("1325360c-8253-473a-a20f-55c269c20407"),
                        Title = "Easy Money",
                        Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus."
                });
                await repository.SaveChangesAsync();

                var book = await repository.GetBookForAuthorAsync(
                    Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"), 
                    Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"));

                Assert.NotNull(book);
                Assert.Equal(Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"), book.Id);
                Assert.Equal("Easy Money", book.Title);
                Assert.Equal("Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus.", book.Description);
            }
        }

        [Fact]
        public async Task AddBookForAuthorAsync_WithoutGuidBook_Test()
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
                    DateOfBirth = new DateTimeOffset(new DateTime(1974, 5, 24))
                });
                context.SaveChanges();

                await repository.AddBookForAuthorAsync(
                    authorId: Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"),
                    book: new Book
                    {
                        Title = "Easy Money",
                        Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus."
                    });
                await repository.SaveChangesAsync();

                var books = await repository.GetBooksForAuthorAsync(Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"));

                Assert.Collection(books, bookItem => {
                    Assert.Equal("Easy Money", bookItem.Title);
                    Assert.Equal("Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus.", bookItem.Description);
                });
            }
        }
        #endregion

        #region UpdateBookForAuthorAsync Test
        [Fact]
        public async Task UpdateBookForAuthorAsync_Test()
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

                var book = await repository.GetBookForAuthorAsync(
                    authorId: Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"),
                    bookId: Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"));
                book.Title = "Easy Money U";
                book.Description = "Test Description";

                await repository.UpdateBookForAuthorAsync(book);
                await repository.SaveChangesAsync();
                book = null;

                var updateBook = await repository.GetBookForAuthorAsync(
                    authorId: Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"),
                    bookId: Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"));
                Assert.NotNull(updateBook);
                Assert.Equal(Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"), updateBook.Id);
                Assert.Equal("Easy Money U", updateBook.Title);
                Assert.Equal("Test Description", updateBook.Description);
            }
        }
        #endregion

        #region DeleteBookAsync Test
        [Fact]
        public async Task DeleteBookAsync_Test()
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

                var book = await repository.GetBookForAuthorAsync(
                    authorId: Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"),
                    bookId: Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"));

                await repository.DeleteBookAsync(book);
                await repository.SaveChangesAsync();

                var book1 = await repository.GetBookForAuthorAsync(
                    authorId: Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77"),
                    bookId: Guid.Parse("1325360c-8253-473a-a20f-55c269c20407"));
                Assert.Null(book1);
            }
        }
        #endregion
    }
}
