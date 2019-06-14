using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Xunit;
using Moq;
using AutoMapper;
using Library.API.Entities;
using Library.API.Services;
using Library.API.Controllers;
using Library.API.Profiles;
using Library.API.Models;

namespace Library.API.Tests.UnitTests
{
    public class BooksControllerTests
    {
        #region [HttpGet] GetBooksForAuthorAsync Test
        [Fact]
        public async Task GetBooksForAuthorAsync_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.GetBooksForAuthorAsync(authorId))
                .ReturnsAsync(GetTestAuthorsData().FirstOrDefault(a => a.Id == authorId).Books);
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(true);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.GetBooksForAuthorAsync(authorId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<BookDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnBookDtos = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);
            Assert.NotEmpty(returnBookDtos);
            Assert.Collection(returnBookDtos,
                item =>
                {
                    Assert.Equal(new Guid("1325360c-8253-473a-a20f-55c269c20407"), item.Id);
                    Assert.Equal("Easy Money", item.Title);
                    Assert.Equal("Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus.", item.Description);
                }
            );
        }

        [Fact]
        public async Task GetBooksForAuthorAsync_ReturnsNotFound_Test()
        {
            // Arrange
            var fakeId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(fakeId))
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.GetBooksForAuthorAsync(fakeId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<BookDto>>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
        #endregion

        #region [HttpGet] GetBookForAuthorAsync Test
        [Fact]
        public async Task GetBookForAuthorAsync_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookId = Guid.Parse("1325360c-8253-473a-a20f-55c269c20407");
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
                .ReturnsAsync(
                    GetTestAuthorsData().FirstOrDefault(a => a.Id == authorId)
                    .Books.FirstOrDefault(b => b.Id == bookId)
                );

            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.GetBookForAuthorAsync(authorId, bookId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BookDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnBookDto = Assert.IsType<BookDto>(okResult.Value);
            Assert.Equal(new Guid("1325360c-8253-473a-a20f-55c269c20407"), returnBookDto.Id);
            Assert.Equal("Easy Money", returnBookDto.Title);
            Assert.Equal("Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus.", returnBookDto.Description);
        }

        [Fact]
        public async Task GetBookForAuthorAsync_AuthorNotExist_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.GetBookForAuthorAsync(authorId, bookId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BookDto>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetBookForAuthorAsync_BookNotExist_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
               .ReturnsAsync((Book)null);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.GetBookForAuthorAsync(authorId, bookId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BookDto>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
        #endregion

        #region [HttpPost] CreateBookForAuthorAsync Test
        [Fact]
        public async Task CreateBookForAuthorAsync_Test()
        {
            // Arrange
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();

            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookForCreationDto = new BookForCreationDto { Title = "The Book", Description = "The Description..." };
            var bookEntity = mapper.Map<Book>(bookForCreationDto);

            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(true);
            mockRepo.Setup(repo => repo.AddBookForAuthorAsync(authorId, bookEntity))
                .Returns(Task.CompletedTask);
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(true);
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.CreateBookForAuthorAsync(authorId, book: bookForCreationDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BookDto>>(result);
            Assert.IsType<CreatedAtRouteResult>(actionResult.Result);
        }

        [Fact]
        public async Task CreateBookForAuthorAsync_ReturnsBadRequest_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.CreateBookForAuthorAsync(authorId, book: null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BookDto>>(result);
            Assert.IsType<BadRequestResult>(actionResult.Result);
        }

        [Fact]
        public async Task CreateBookForAuthorAsync_AuthorNotExist_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.CreateBookForAuthorAsync(authorId, book: new BookForCreationDto());

            // Assert
            var actionResult = Assert.IsType<ActionResult<BookDto>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task CreateBookForAuthorAsync_ThrowException_Test()
        {
            // Arrange
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();

            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookForCreationDto = new BookForCreationDto { Title = "The Book", Description = "The Description..." };
            var bookEntity = mapper.Map<Book>(bookForCreationDto);

            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(true);
            mockRepo.Setup(repo => repo.AddBookForAuthorAsync(authorId, bookEntity))
                .Returns(Task.CompletedTask);
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(false);
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await Assert.ThrowsAsync<Exception>(() => controller.CreateBookForAuthorAsync(authorId, book: bookForCreationDto));

            // Assert
            Assert.Equal($"Creating a book for author {authorId} failed on save.", result.Message);
        }
        #endregion

        #region [HttpDelete] DeleteBookForAuthorAsync Test
        [Fact]
        public async Task DeleteBookForAuthorAsync_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
               .ReturnsAsync(new Book());
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(true);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.DeleteBookForAuthorAsync(authorId, bookId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteBookForAuthorAsync_AuthorNotExist_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.DeleteBookForAuthorAsync(authorId, bookId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteBookForAuthorAsync_BookNotExist_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
               .ReturnsAsync((Book)null);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.DeleteBookForAuthorAsync(authorId, bookId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteBookForAuthorAsync_ThrowException_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
               .ReturnsAsync(new Book());
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await Assert.ThrowsAsync<Exception>(() => controller.DeleteBookForAuthorAsync(authorId, bookId));

            // Assert
            Assert.Equal($"Deleting book {bookId} for author {authorId} failed on save.", result.Message);
        }
        #endregion

        #region [HttpPut] UpdateBookForAuthor Test
        [Fact]
        public async Task UpdateBookForAuthorAsync_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookId = Guid.Parse("1325360c-8253-473a-a20f-55c269c20407");
            var book = GetTestAuthorsData().FirstOrDefault(a => a.Id == authorId)
                .Books.FirstOrDefault(b => b.Id == bookId);
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
                .ReturnsAsync(book);
            mockRepo.Setup(repo => repo.UpdateBookForAuthorAsync(book))
                .Returns(Task.CompletedTask);
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(true);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);
            var bookForUpdateDto = new BookForUpdateDto();

            // Act 
            var result = await controller.UpdateBookForAuthorAsync(authorId, bookId, bookForUpdateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateBookForAuthorAsync_ReturnsBadRequest_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.UpdateBookForAuthorAsync(authorId, bookId, book: null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateBookForAuthorAsync_AuthorNotExist_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.UpdateBookForAuthorAsync(authorId, bookId, book: new BookForUpdateDto());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateBookForAuthorAsync_BookNotExist_Test()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
               .ReturnsAsync((Book)null);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act 
            var result = await controller.UpdateBookForAuthorAsync(authorId, bookId, book: new BookForUpdateDto());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateBookForAuthorAsync_ThrowException_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookId = Guid.Parse("1325360c-8253-473a-a20f-55c269c20407");
            var book = GetTestAuthorsData().FirstOrDefault(a => a.Id == authorId)
                .Books.FirstOrDefault(b => b.Id == bookId);
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
                .ReturnsAsync(book);
            mockRepo.Setup(repo => repo.UpdateBookForAuthorAsync(book))
                .Returns(Task.CompletedTask);
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);
            var bookForUpdateDto = new BookForUpdateDto();

            // Act 
            var result = await Assert.ThrowsAsync<Exception>(() => controller.UpdateBookForAuthorAsync(authorId, bookId, bookForUpdateDto));

            // Assert
            Assert.Equal($"Update book {bookId} for author {authorId} failed on save.", result.Message);
        }
        #endregion

        #region [HttpPatch] PartiallyUpdateBookForAuthorAsync Test
        [Fact]
        public async Task PartiallyUpdateBookForAuthorAsync_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookId = Guid.Parse("1325360c-8253-473a-a20f-55c269c20407");
            var patchDoc = new JsonPatchDocument<BookForUpdateDto>();
            var book = GetTestAuthorsData().FirstOrDefault(a => a.Id == authorId)
                .Books.FirstOrDefault(b => b.Id == bookId);
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
                .ReturnsAsync(book);
            mockRepo.Setup(repo => repo.UpdateBookForAuthorAsync(book))
                .Returns(Task.CompletedTask)
                .Verifiable();
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(true);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);
            var bookForUpdateDto = new BookForUpdateDto();

            // Act 
            var result = await controller.PartiallyUpdateBookForAuthorAsync(authorId, bookId, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);
            mockRepo.Verify();
        }

        [Fact]
        public async Task PartiallyUpdateBookForAuthorAsync_ReturnsBadRequest_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookId = Guid.Parse("1325360c-8253-473a-a20f-55c269c20407");
            var mockRepo = new Mock<ILibraryRepository>();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act
            var result = await controller.PartiallyUpdateBookForAuthorAsync(authorId, bookId, patchDoc:null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PartiallyUpdateBookForAuthorAsync_AuthorNotExist_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookId = Guid.Parse("1325360c-8253-473a-a20f-55c269c20407");
            var patchDoc = new JsonPatchDocument<BookForUpdateDto>();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act
            var result = await controller.PartiallyUpdateBookForAuthorAsync(authorId, bookId, patchDoc);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PartiallyUpdateBookForAuthorAsync_BookNotExist_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookId = Guid.Parse("1325360c-8253-473a-a20f-55c269c20407");
            var patchDoc = new JsonPatchDocument<BookForUpdateDto>();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .ReturnsAsync(true);
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
                .ReturnsAsync((Book)null)
                .Verifiable();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);

            // Act
            var result = await controller.PartiallyUpdateBookForAuthorAsync(authorId, bookId, patchDoc);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            mockRepo.Verify();
        }

        [Fact]
        public async Task PartiallyUpdateBookForAuthorAsync_ThrowException_Test()
        {
            // Arrange
            var authorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var bookId = Guid.Parse("1325360c-8253-473a-a20f-55c269c20407");
            var patchDoc = new JsonPatchDocument<BookForUpdateDto>();
            var book = GetTestAuthorsData().FirstOrDefault(a => a.Id == authorId)
                .Books.FirstOrDefault(b => b.Id == bookId);
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(authorId))
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.GetBookForAuthorAsync(authorId, bookId))
                .ReturnsAsync(book);
            mockRepo.Setup(repo => repo.UpdateBookForAuthorAsync(book))
                .Returns(Task.CompletedTask)
                .Verifiable();
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new BookProfile())).CreateMapper();
            var controller = new BooksController(mockRepo.Object, mapper);
            var bookForUpdateDto = new BookForUpdateDto();

            // Act 
            var result = await Assert.ThrowsAsync<Exception>(() => controller.PartiallyUpdateBookForAuthorAsync(authorId, bookId, patchDoc));

            // Assert
            Assert.Equal($"Patching book {bookId} for author {authorId} failed on save.", result.Message);
            mockRepo.Verify();
        }
        #endregion

        #region Test data
        private IEnumerable<Author> GetTestAuthorsData()
        {
            #region authors data
            var authors = new List<Author>
            {
                new Author()
                {
                    Id = new Guid("25320c5e-f58a-4b1f-b63a-8ee07a840bdf"),
                    FirstName = "Stephen",
                    LastName = "King",
                    Genre = "Horror",
                    DateOfBirth = new DateTimeOffset(new DateTime(1947, 9, 21)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("c7ba6add-09c4-45f8-8dd0-eaca221e5d93"),
                            Title = "The Shining",
                            Description = "The Shining is a horror novel by American author Stephen King. Published in 1977, it is King's third published novel and first hardback bestseller: the success of the book firmly established King as a preeminent author in the horror genre. "
                        },
                        new Book()
                        {
                            Id = new Guid("a3749477-f823-4124-aa4a-fc9ad5e79cd6"),
                            Title = "Misery",
                            Description = "Misery is a 1987 psychological horror novel by Stephen King. This novel was nominated for the World Fantasy Award for Best Novel in 1988, and was later made into a Hollywood film and an off-Broadway play of the same name."
                        },
                        new Book()
                        {
                            Id = new Guid("70a1f9b9-0a37-4c1a-99b1-c7709fc64167"),
                            Title = "It",
                            Description = "It is a 1986 horror novel by American author Stephen King. The story follows the exploits of seven children as they are terrorized by the eponymous being, which exploits the fears and phobias of its victims in order to disguise itself while hunting its prey. 'It' primarily appears in the form of a clown in order to attract its preferred prey of young children."
                        },
                        new Book()
                        {
                            Id = new Guid("60188a2b-2784-4fc4-8df8-8919ff838b0b"),
                            Title = "The Stand",
                            Description = "The Stand is a post-apocalyptic horror/fantasy novel by American author Stephen King. It expands upon the scenario of his earlier short story 'Night Surf' and outlines the total breakdown of society after the accidental release of a strain of influenza that had been modified for biological warfare causes an apocalyptic pandemic which kills off the majority of the world's human population."
                        }
                    }
                },
                new Author()
                {
                    Id = new Guid("76053df4-6687-4353-8937-b45556748abe"),
                    FirstName = "George",
                    LastName = "RR Martin",
                    Genre = "Fantasy",
                    DateOfBirth = new DateTimeOffset(new DateTime(1948, 9, 20)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("447eb762-95e9-4c31-95e1-b20053fbe215"),
                            Title = "A Game of Thrones",
                            Description = "A Game of Thrones is the first novel in A Song of Ice and Fire, a series of fantasy novels by American author George R. R. Martin. It was first published on August 1, 1996."
                        },
                        new Book()
                        {
                            Id = new Guid("bc4c35c3-3857-4250-9449-155fcf5109ec"),
                            Title = "The Winds of Winter",
                            Description = "Forthcoming 6th novel in A Song of Ice and Fire."
                        },
                        new Book()
                        {
                            Id = new Guid("09af5a52-9421-44e8-a2bb-a6b9ccbc8239"),
                            Title = "A Dance with Dragons",
                            Description = "A Dance with Dragons is the fifth of seven planned novels in the epic fantasy series A Song of Ice and Fire by American author George R. R. Martin."
                        }
                    }
                },
                new Author()
                {
                    Id = new Guid("412c3012-d891-4f5e-9613-ff7aa63e6bb3"),
                    FirstName = "Neil",
                    LastName = "Gaiman",
                    Genre = "Fantasy",
                    DateOfBirth = new DateTimeOffset(new DateTime(1960, 11, 10)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("9edf91ee-ab77-4521-a402-5f188bc0c577"),
                            Title = "American Gods",
                            Description = "American Gods is a Hugo and Nebula Award-winning novel by English author Neil Gaiman. The novel is a blend of Americana, fantasy, and various strands of ancient and modern mythology, all centering on the mysterious and taciturn Shadow."
                        }
                    }
                },
                new Author()
                {
                    Id = new Guid("578359b7-1967-41d6-8b87-64ab7605587e"),
                    FirstName = "Tom",
                    LastName = "Lanoye",
                    Genre = "Various",
                    DateOfBirth = new DateTimeOffset(new DateTime(1958, 8, 27)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid("01457142-358f-495f-aafa-fb23de3d67e9"),
                            Title = "Speechless",
                            Description = "Good-natured and often humorous, Speechless is at times a 'song of curses', as Lanoye describes the conflicts with his beloved diva of a mother and her brave struggle with decline and death."
                        }
                    }
                },
                new Author()
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
                            Description = "The Hitchhiker's Guide to the Galaxy is the first of five books in the Hitchhiker's Guide to the Galaxy comedy science fiction 'trilogy' by Douglas Adams."
                        }
                    }
                },
                new Author()
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
                }
            };
            #endregion

            return authors;
        }
        #endregion
    }
}
