using Assignment4;
using Assignment4.Core;
using Assignment4.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Xunit;
using static Assignment4.Core.Response;
using static Assignment4.Core.State;

namespace Assignment4.Entities.Tests
{
    public class TagRepositoryTests : IDisposable
    {
         private readonly KanbanContext _context;
        private readonly TagRepository _repo;

        public TagRepositoryTests()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>();
            builder.UseSqlite(connection);
            var context = new KanbanContext(builder.Options);
            context.Database.EnsureCreated();

            Tag urgent = new Tag { Name = "Urgent" };
            Tag needsInfo = new Tag { Name = "Needs info" };
            Tag notImportant = new Tag { Name = "Not important" };

            User billy = new User { Name = "Billy", Email = "Billy@Email.com" };
            User alice = new User { Name = "Alice", Email = "Alice@Email.com" };
            User emily = new User { Name = "Emily", Email = "Emily@Email.com" };
            User charlie = new User { Name = "Charlie", Email = "Charlie@Email.com" };

            context.Tags.AddRange(urgent, needsInfo, notImportant);
            context.Users.AddRange(billy, alice, emily, charlie);
            context.Tasks.AddRange(
                new Task { Id = 1, Title = "Cleaning", AssignedTo = billy, Description = "The office and the bathroom needs to be cleaned", State = New, Tags = new List<Tag>() { notImportant }, Created = DateTime.UtcNow, StateUpdated = DateTime.UtcNow },
                new Task { Id = 2, Title = "Arrange event", AssignedTo = alice, Description = "Big company will be arriving. We need to arrange an event for them", State = Active, Tags = new List<Tag>() { urgent, needsInfo }, Created = DateTime.UtcNow, StateUpdated = DateTime.UtcNow },
                new Task { Id = 3, Title = "Update FAQ", AssignedTo = emily, State = Removed, Tags = new List<Tag>() { notImportant }, Created = DateTime.UtcNow, StateUpdated = DateTime.UtcNow },
                new Task { Id = 4, Title = "Go skiing in Val d'isère", AssignedTo = charlie, State = Removed, Tags = new List<Tag>() { notImportant }, Created = DateTime.UtcNow, StateUpdated = DateTime.UtcNow }
            );

            context.SaveChanges();

            _context = context;
            _repo = new TagRepository(_context);
        }

        [Fact]
        public void CreateNewTagReturnsRightIdAndCreatedResponse()
        {
            var tag = new TagCreateDTO {Name = "Created new Tag"};
            var response = _repo.Create(tag);

            Assert.Equal(Response.Created, response.Response);
            Assert.Equal(4, response.TagId);
        }

        [Fact]
        public void CreateExistingTagReturnConflictResponse()
        {
            var tag = new TagCreateDTO {Name = "Urgent"};
            var response = _repo.Create(tag);

            Assert.Equal(Response.Conflict, response.Response);
            Assert.Equal(1, response.TagId);
        }

        [Fact]
        public void ReadAllTagsReturnsAllTags()
        {
            Tag urgent = new Tag { Name = "Urgent" };
            Tag needsInfo = new Tag { Name = "Needs info" };
            Tag notImportant = new Tag { Name = "Not important" };

            var tagList = new List<Tag>() {urgent, needsInfo, notImportant};
            var tags = _repo.ReadAll();
            var counter = 0;

            foreach (var tag in tags)
            {
                Assert.Equal(tagList[counter].Name, tag.Name);
                counter++;
            }
        }

        [Fact]
        public void ReadExistingTagReturnsTag()
        {
            var tag = _repo.Read(1);

            Assert.Equal(new TagDTO(1, "Urgent"), tag);
        }

        [Fact]
        public void ReadNonExistingTagReturnsNull()
        {
            var tag = _repo.Read(4);

            Assert.Null(tag);
        }

        [Fact]
        public void DeleteNonExistingTagReturnsNotFoundResponse()
        {
            var response = _repo.Delete(4);

            Assert.Equal(Response.NotFound, response);
        }

        [Fact]
        public void DeleteExistingTagInUseWithoutForceReturnsConflict()
        {
            var response = _repo.Delete(3);

            Assert.Equal(Response.Conflict, response);
        }

        [Fact]
        public void DeleteExistingTagInUseWithForceReturnsDeletedResponse()
        {
            var response = _repo.Delete(3, true);

            Assert.Equal(Response.Deleted, response);
        }

        [Fact]
        public void UpdateNonExistingTagReturnsNotFoundResponse()
        {
            var response = _repo.Update(new TagUpdateDTO {Id = 4, Name = "Updated"});

            Assert.Equal(Response.NotFound, response);
        }

        [Fact]
        public void UpdateExistingTagReturnsUpdatedResponse()
        {
            var response = _repo.Update(new TagUpdateDTO {Id = 3, Name = "Updated"});

            Assert.Equal(Response.Updated, response);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}