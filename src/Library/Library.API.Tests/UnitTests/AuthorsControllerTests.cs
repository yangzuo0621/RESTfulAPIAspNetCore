using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using AutoMapper;
using Library.API.Services;
using Library.API.Entities;
using Library.API.Controllers;
using Library.API.Profiles;
using Library.API.Models;
using Library.API.Helpers;

namespace Library.API.Tests.UnitTests
{
    public class AuthorsControllerTests
    {
        #region [HttpGet] GetAuthorsAsync Test
        [Fact]
        public async Task GetAuthorsAsync_Test()
        {
            // Arrange
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.GetAuthorsAsync())
                .ReturnsAsync(GetTestAuthorsData());
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.GetAuthorsAsync(new AuthorsResourceParameters());

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<AuthorDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnAuthorDtos = Assert.IsAssignableFrom<IEnumerable<AuthorDto>>(okResult.Value);
        }
        #endregion

        #region [HttpGet] GetAuthorAsync Test
        [Fact]
        public async Task GetAuthorAsync_ReturnsAuthorDto_Test()
        {
            // Arrange
            var mockRepo = new Mock<ILibraryRepository>();
            var testAuthorId = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            mockRepo.Setup(repo => repo.GetAuthorAsync(testAuthorId))
                .ReturnsAsync(GetTestAuthorsData().FirstOrDefault(a => a.Id == testAuthorId));
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.GetAuthorAsync(testAuthorId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthorDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnAuthorDtos = Assert.IsType<AuthorDto>(okResult.Value);
            Assert.Equal("Jens Lapidus", returnAuthorDtos.Name);
            Assert.Equal("Thriller", returnAuthorDtos.Genre);
            Assert.Equal(new DateTimeOffset(new DateTime(1974, 5, 24)).GetCurrentAge(), returnAuthorDtos.Age);
        }

        [Fact]
        public async Task GetAuthorAsync_ReturnsNotFound_Test()
        {
            // Arrange
            var mockRepo = new Mock<ILibraryRepository>();
            var testAuthorId = Guid.Parse("25320c5e-f58a-5bbf-b63a-8ee07a840bdf");
            mockRepo.Setup(repo => repo.GetAuthorAsync(testAuthorId))
                .ReturnsAsync((Author)null);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.GetAuthorAsync(testAuthorId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthorDto>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
        #endregion

        #region [HttpPost] CreateAuthorAsync Test
        [Fact]
        public async Task CreateAuthorAsync_ReturnsNewlyCreatedAuthorDto_Test()
        {
            // Arrange
            Guid id = Guid.Parse("3fd2e030-7626-4d7f-987e-a8a16c40068e");
            string firstName = "James";
            string lastName = "Ellroy";
            string dateOfBirth = "1948-03-04T00:00:00";
            string genre = "Thriller";

            var authorForCreationDto = new AuthorForCreationDto
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = new DateTimeOffset(DateTime.Parse(dateOfBirth)),
                Genre = genre
            };

            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();

            var newAuthor = mapper.Map<Author>(authorForCreationDto);
            newAuthor.Id = id;

            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.FromResult(true));
            mockRepo.Setup(repo => repo.UpdateAuthorAsync(newAuthor))
                .Returns(Task.FromResult(newAuthor));

            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.CreateAuthorAsync(authorForCreationDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthorDto>>(result);
            var createResult = Assert.IsType<CreatedAtRouteResult>(actionResult.Result);
            var returnAuthorDto = Assert.IsType<AuthorDto>(createResult.Value);
            mockRepo.Verify();
            Assert.Equal($"{firstName} {lastName}", returnAuthorDto.Name);
            Assert.Equal(genre, returnAuthorDto.Genre);
            Assert.Equal(new DateTimeOffset(DateTime.Parse(dateOfBirth)).GetCurrentAge(), returnAuthorDto.Age);
        }

        [Fact]
        public async Task CreateAuthorAsync_ThrowException_Test()
        {
            // Arrange
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var authorForCreationDto = new AuthorForCreationDto();
            var author = mapper.Map<Author>(authorForCreationDto);
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(false);
            mockRepo.Setup(repo => repo.AddAuthorAsync(author))
                .Returns(Task.CompletedTask);

            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await Assert.ThrowsAsync<Exception>(() => controller.CreateAuthorAsync(authorForCreationDto));

            // Assert
            Assert.Equal("Creating an author failed on save.", result.Message);
            mockRepo.Verify();
        }

        [Fact]
        public async Task CreateAuthorAsync_ReturnsBadRequest_Test()
        {
            // Arrange
            var mockRepo = new Mock<ILibraryRepository>();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.CreateAuthorAsync((AuthorForCreationDto)null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthorDto>>(result);
            Assert.IsType<BadRequestResult>(actionResult.Result);
        }
        #endregion

        #region [HttpPost] BlockAuthorCreationAsync Test
        [Fact]
        public async Task BlockAuthorCreationAsync_ReturnsNotFound_Test()
        {
            // Arrange
            var fakeId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(fakeId))
                .ReturnsAsync(false);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.BlockAuthorCreationAsync(fakeId);

            // Arrange
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task BlockAuthorCreationAsync_ReturnsStatus409Conflict_Test()
        {
            // Arrange
            var fakeId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.AuthorExistsAsync(fakeId))
                .ReturnsAsync(true);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.BlockAuthorCreationAsync(fakeId);

            // Arrange
            var status = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(409, status.StatusCode);
        }
        #endregion

        #region [HttpDelete] DeleteAuthorAsync Test
        [Fact]
        public async Task DeleteAuthorAsync_ReturnsNotFound_Test()
        {
            // Arrange
            var fakeId = Guid.NewGuid();
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.GetAuthorAsync(fakeId))
                .ReturnsAsync((Author) null);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.DeleteAuthorAsync(fakeId);

            // Arrange
            var status = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAuthorAsync_ThrowException_Test()
        {
            // Arrange
            var id = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var author = GetTestAuthorsData().FirstOrDefault(a => a.Id == id);
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.GetAuthorAsync(id))
                .ReturnsAsync(author);
            mockRepo.Setup(repo => repo.DeleteAuthorAsync(author))
                .Returns(Task.CompletedTask);
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.FromResult(false));
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await Assert.ThrowsAsync<Exception>(() => controller.DeleteAuthorAsync(id));

            // Arrange
            Assert.Equal($"Deleting author {id} failed on save.", result.Message);
            mockRepo.Verify();
        }

        [Fact]
        public async Task DeleteAuthorAsync_ReturnsNoContent_Test()
        {
            // Arrange
            var id = Guid.Parse("a1da1d8e-1988-4634-b538-a01709477b77");
            var author = GetTestAuthorsData().FirstOrDefault(a => a.Id == id);
            var mockRepo = new Mock<ILibraryRepository>();
            mockRepo.Setup(repo => repo.GetAuthorAsync(id))
                .ReturnsAsync(author);
            mockRepo.Setup(repo => repo.DeleteAuthorAsync(author))
                .Returns(Task.CompletedTask);
            mockRepo.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.FromResult(true));
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AuthorProfile())).CreateMapper();
            var controller = new AuthorsController(mockRepo.Object, mapper);

            // Act
            var result = await controller.DeleteAuthorAsync(id);

            // Arrange
            Assert.IsType<NoContentResult>(result);
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
                            Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus. It has been a success in term of sales, and the paperback was the fourth best seller of Swedish novels in 2007."
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
