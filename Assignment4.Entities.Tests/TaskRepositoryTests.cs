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
    public class TaskRepositoryTests : IDisposable
    {
        private readonly KanbanContext _context;
        private readonly TaskRepository _repo;

        public TaskRepositoryTests()
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
            _repo = new TaskRepository(_context);
        }

        [Fact]
        public void TaskCreateNewTaskReturnsResponseCreated()
        {
            var task = _repo.Create(new TaskCreateDTO{Title = "Task2", AssignedToId = 1, Description = "Desc", Tags = new HashSet<string>()});

            Assert.Equal(Response.Created, task.Response);
        }

        [Fact]
        public void TaskCreateExistingTaskReturnsConflictResponse()
        {
            var response = _repo.Create(new TaskCreateDTO { Title = "Cleaning", AssignedToId = 1, Description = "Desc" });

            Assert.Equal((Response.Conflict, 1), response);
        }

        [Fact]
        public void TaskCreateWithNonExistingUserReturnsBadRequest()
        {
            var tuple = _repo.Create(new TaskCreateDTO {
                    Title = "Cleaning",
                    AssignedToId = -1 // Non-existing user ID
                }
            );
            
            var response = tuple.Item1;
            var taskId = tuple.Item2;

            Assert.Equal(BadRequest, response);
            Assert.Equal(-1, taskId);
        }

        [Fact]
        public void TaskCreateSetsStateToNew()
        {
            var taskId = _repo.Create(new TaskCreateDTO { Title = "New Task", AssignedToId = 1, Description = "Task with State.New", Tags = new HashSet<string>() }).Item2;
            var task = _repo.Read(taskId);

            Assert.Equal(New, task.State);
        }

        [Fact]
        public void TaskCreateSetsDateTimesToUtcNow()
        {
            var taskId = _repo.Create(new TaskCreateDTO { Title = "UtcNow Task", AssignedToId = 1, Description = "Task with DateTime.UtcNow", Tags = new HashSet<string>() }).Item2;
            var task = _repo.Read(taskId);

            Assert.Equal(DateTime.UtcNow, task.Created, precision: TimeSpan.FromSeconds(5));
            Assert.Equal(DateTime.UtcNow, task.StateUpdated, precision: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void TaskReadAllReturnsAllTasks()
        {
            var task1 = new TaskDTO ( 1, "Cleaning", "Billy", new HashSet<string>() { "Not important" }, New );
            var task2 = new TaskDTO ( 2, "Arrange event", "Alice", new HashSet<string>() { "Urgent", "Needs info" }, Active );
            var task3 = new TaskDTO ( 3, "Update FAQ", "Emily", new HashSet<string>() { "Not important" }, Removed );
            var task4 = new TaskDTO ( 4, "Go skiing in Val d'isère", "Charlie", new HashSet<string>() { "Not important" }, Removed );
            var taskList = new List<TaskDTO>() { task1, task2, task3, task4 };

            var tasks = _repo.ReadAll();
            var counter = 0;

            foreach (var task in tasks)
            {
                Assert.Equal(task.Id, taskList[counter].Id);
                Assert.Equal(task.Title, taskList[counter].Title);
                Assert.Equal(task.AssignedToName, taskList[counter].AssignedToName);
                Assert.Equal(task.Tags, taskList[counter].Tags);
                Assert.Equal(task.State, taskList[counter].State);
                counter++;
            }
        }

        [Fact]
        public void TaskReadAllRemovedReturnsAllRemovedTasks()
        {
            var tasks = _repo.ReadAllRemoved();

            foreach (var task in tasks)
            {
                Assert.Equal(Removed, task.State);
            }
        }

        [Theory]
        [InlineData("Urgent")]
        [InlineData("Needs info")]
        [InlineData("Not important")]
        public void TaskReadAllByTagReturnsAllTasksWithTag(string tag)
        {
            var tasks = _repo.ReadAllByTag(tag);

            foreach (var task in tasks)
            {
                Assert.True(ContainsTag(task, tag), "Task should contain tag: " + tag);
            }
        }

        [Theory]
        [InlineData("Billy", 1)]
        [InlineData("Alice", 2)]
        [InlineData("Emily", 3)]
        [InlineData("Charlie", 4)]
        public void TaskReadAllByUserReturnsAllTasksWithUser(string userName, int userId)
        {
            var tasks = _repo.ReadAllByUser(userId);
            
            foreach (var task in tasks)
            {
                Assert.Equal(userName, task.AssignedToName);
            }
        }

        [Fact]
        public void TaskReadReturnsAllTaskDetails()
        {
            var task = _repo.Read(1);
            var tags = new HashSet<string>() { "Not important" };

            Assert.Equal(1, task.Id);
            Assert.Equal("Cleaning", task.Title);
            Assert.Equal("The office and the bathroom needs to be cleaned", task.Description);
            Assert.Equal("Billy", task.AssignedToName);
            Assert.Equal(New, task.State);
            Assert.True(tags.SetEquals(task.Tags));
        }

        [Fact]
        public void TaskReadReturnsNulIfTaskDoesNotExist()
        {
            var task = _repo.Read(404);

            Assert.Null(task);
        }

        [Fact]
        public void UpdateAnExistingTaskReturnsResponseUpdated()
        {
            var updateTask = new TaskUpdateDTO {Title = "Updated title", AssignedToId = 1, Description = "Updated description", Tags = new HashSet<string>() { "Updated" }, Id = 4, State = State.Active};
            var response = _repo.Update(updateTask);
            var task = _repo.Read(4);

            Assert.Equal(Response.Updated, response);
            Assert.Equal(updateTask.Title, task.Title);
            Assert.Equal(updateTask.Description, task.Description);
            Assert.Equal(updateTask.Tags, task.Tags);
            Assert.Equal(updateTask.Id, task.Id);
            Assert.Equal(updateTask.State, task.State);
        }

        [Fact]
        public void TaskUpdateUpdatesState()
        {
            var response = _repo.Update(new TaskUpdateDTO { Id = 1, Title = "Update Task State", AssignedToId = 1, Description = "State changed to State.Closed", Tags = new HashSet<string>(), State = Closed });
            var task = _repo.Read(1);

            Assert.Equal(Closed, task.State);
        }

        [Fact]
        public void TaskUpdateSetsStateUpdatedToUtcNow()
        {
            var response = _repo.Update(new TaskUpdateDTO { Id = 1, Title = "Update Task State", AssignedToId = 1, Description = "State changed to State.Closed", Tags = new HashSet<string>(), State = Closed });
            var task = _repo.Read(1);

            Assert.Equal(DateTime.UtcNow, task.StateUpdated, precision: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void DeleteNewTaskReturnsDeletedResponse()
        {
            var response = _repo.Delete(1);

            Assert.Equal(Deleted, response);
        }


        [Fact]
        public void DeleteRemovedTaskReturnsConflictResponse()
        {
            var response = _repo.Delete(3);

            Assert.Equal(Conflict, response);
        }

        [Fact]
        public void DeleteActiveTaskReturnsUpdatedResponse()
        {
            var response = _repo.Delete(2);

            Assert.Equal(Updated, response);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        
        private bool ContainsTag(TaskDTO task, string tag)
        {
            foreach (var taskTag in task.Tags)
            {
                if (tag == taskTag)
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
