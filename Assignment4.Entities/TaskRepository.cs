using Assignment4.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using static Assignment4.Core.Response;
using static Assignment4.Core.State;

namespace Assignment4.Entities
{
    public class TaskRepository : ITaskRepository
    {
        private readonly KanbanContext _context;

        public TaskRepository(KanbanContext context)
        {
            _context = context;
        }

        // Creating a task will set its state to New and Created/StateUpdated to current time in UTC.
        // Create/update task must allow for editing tags.
        // Assigning a user which does not exist should return BadRequest.
        public (Response Response, int TaskId) Create(TaskCreateDTO task) {
            
            User? user = (from c in _context.Users
                          where c.Id == task.AssignedToId
                          select c).FirstOrDefault();

            if (user == null)
            {
                return (Response.BadRequest, -1);
            }

            var tags = from c in _context.Tags
                       where !task.Tags.Contains(c.Name)
                       select c;

            var created = new Task {
                Title = task.Title,
                AssignedTo = user,
                Description = task.Description,
                State = New,
            };

            var checkCreate = (from c in _context.Tasks
                              where c.Title == created.Title
                              select c.Id).FirstOrDefault();

            if (checkCreate == null || checkCreate == 0)
            {
                _context.Tasks.Add(created);

                _context.SaveChanges();

                return (Created, created.Id);
            } 
            else 
            {
                return (Conflict, checkCreate);    
            }
        }

        public IReadOnlyCollection<TaskDTO> ReadAll()
        {
            var tasks = (from t in _context.Tasks
                         select t).ToList();

            return ConvertToTaskDTOs(tasks);
        }

        public IReadOnlyCollection<TaskDTO> ReadAllRemoved() 
        {
            return ReadAllByState(Removed);
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByTag(string tag)
        {
            var tasks = (from c in _context.Tasks
                              select c).ToList();

            var tasksByTag = new List<Task>();
            foreach (var task in tasks)
            {
                if (task.Tags.Contains(new Tag { Name = tag }))
                {
                    tasksByTag.Add(task);
                }
            }

            return ConvertToTaskDTOs(tasksByTag);
        }
        
        public IReadOnlyCollection<TaskDTO> ReadAllByUser(int userId)
        {
            throw new NotImplementedException();
        }
        
        public IReadOnlyCollection<TaskDTO> ReadAllByState(State state)
        {
            var tasksByState = (from c in _context.Tasks
                                where c.State == state
                                select c).ToList();
            
            return ConvertToTaskDTOs(tasksByState);
        }

        public TaskDetailsDTO Read(int taskId)
        {
            var task = from c in _context.Tasks
                       where c.Id == taskId
                       select new TaskDetailsDTO (
                           c.Id,
                           c.Title,
                           c.Description,
                           DateTime.Now,
                           c.AssignedTo.Name,
                           c.Tags.Select(t => t.Name).ToHashSet(),
                           c.State,
                           DateTime.Now
                       );

            return task.FirstOrDefault();
        }

        // Updating the State of a task will change the StateUpdated to current time in UTC.
        // Assigning a user which does not exist should return BadRequest.
        public Response Update(TaskUpdateDTO task)
        {
            var user = (from c in _context.Users
                        where c.Id == task.AssignedToId
                        select c).FirstOrDefault();

            if (user.Id == 0)
            {
                return Response.BadRequest;
            }
            else
            {
                var updateTask = (from c in _context.Tasks
                            where c.Id == task.Id
                            select c).FirstOrDefault();

                updateTask.Title = task.Title;
                updateTask.AssignedTo = user;
                updateTask.State = task.State;
                updateTask.Description = task.Description;
                var tags = (from c in _context.Tags
                            where )
                updateTask.Tags = task.Tags.Select(t => t.Name).ToList().AsReadOnly();
                _context.SaveChanges();
                return Response.Updated;
            }
        }

        // Only tasks with the state New can be deleted from the database.
        // Deleting a task which is Active should set its state to Removed.
        // Deleting a task which is Resolved, Closed, or Removed should return Conflict.
        public Response Delete(int taskId)
        {

            var task = (from c in _context.Tasks
                        where c.Id == taskId
                        select c).FirstOrDefault();

            var userId = (from c in _context.Users
                        where c.Id == task.AssignedTo.Id
                        select c.Id).FirstOrDefault();

            if (task.State == State.New)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();
                return Response.Deleted;
            }
            else if (task.State == State.Active)
            {
                var updateTask = new TaskUpdateDTO{
                    Id = task.Id,
                    Title = task.Title,
                    AssignedToId = userId,
                    State = State.Removed,
                    Description = task.Description,
                    Tags = task.Tags.Select(t => t.Name).ToList().AsReadOnly()
                };
                return Update(updateTask);
            }
            else
            {
                return Response.Conflict;
            }
        }
        
        private IReadOnlyCollection<TaskDTO> ConvertToTaskDTOs(List<Task> tasks)
        {
            var listDTO = new List<TaskDTO>();
            
            foreach (var task in tasks)
            {
                var userName = (from t in _context.Users
                                where t.Id == task.AssignedTo.Id
                                select t.Name).FirstOrDefault();
                listDTO.Add(new TaskDTO(task.Id, task.Title, userName, task.Tags.Select(t => t.Name).ToHashSet(), task.State));
            }

            return listDTO.AsReadOnly();
        }
    }
}
