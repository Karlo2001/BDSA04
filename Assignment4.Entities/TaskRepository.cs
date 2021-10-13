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

        public (Response Response, int TaskId) Create(TaskCreateDTO task) {
            User user = (from u in _context.Users
                         where u.Id == task.AssignedToId
                         select u).FirstOrDefault();

            if (user == null)
            {
                return (Response.BadRequest, -1);
            }

            var tags = from t in _context.Tags
                       where !task.Tags.Contains(t.Name)
                       select t;

            var created = new Task {
                Title = task.Title,
                AssignedTo = user,
                Description = task.Description,
                State = New,
                Created = DateTime.UtcNow,
                StateUpdated = DateTime.UtcNow
            };

            var checkCreate = (from c in _context.Tasks
                               where c.Title == created.Title
                               select c.Id).FirstOrDefault();

            if (checkCreate == null || checkCreate == 0)
            {
                 foreach (var item in task.Tags)
                {
                    var findTag = (from c in _context.Tags
                                   where c.Name == item
                                   select c).FirstOrDefault();
                    if (findTag == null)
                    {
                        var tag = new Tag {Name = item, Tasks = new List<Task>() { created }};
                        _context.Tags.Add(tag);
                        created.Tags.Add(tag);
                    }
                    else 
                    {
                        created.Tags.Add(findTag);
                    }
                }

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
            var user = (from c in _context.Users
                        where c.Id == userId
                        select c).FirstOrDefault();
            
            if (user.Id == 0)
            {
                return new List<TaskDTO>().AsReadOnly();
            }

            var tasks = (from t in _context.Tasks select t).ToList();
            var taskList = new List<Task>();
            foreach (var task in tasks)
            {
                if (task.AssignedTo.Id == userId)
                {
                    taskList.Add(task);
                }
            }

            return ConvertToTaskDTOs(taskList);
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
                           c.Created,
                           c.AssignedTo.Name,
                           c.Tags.Select(t => t.Name).ToHashSet(),
                           c.State,
                           c.StateUpdated
                       );

            return task.FirstOrDefault();
        }

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
                updateTask.StateUpdated = DateTime.UtcNow;

                var tags = (from c in _context.Tags select c).ToList();
                var updatedTags = new List<Tag>();

                foreach (var item in tags)
                {
                    if (task.Tags.Contains(item.Name))
                    {
                        updatedTags.Add(item);
                        task.Tags.Remove(item.Name);
                    }
                }

                if (task.Tags.Count > 0)
                {
                    foreach (var item in task.Tags)
                    {
                        var tag = new Tag {Name = item, Tasks = new List<Task>() { updateTask }};
                        updatedTags.Add(tag);
                        _context.Tags.Add(tag);
                        _context.SaveChanges();
                    }
                }

                updateTask.Tags = updatedTags;
                _context.SaveChanges();

                return Response.Updated;
            }
        }

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
                    Tags = task.Tags.Select(t => t.Name).ToList()
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
