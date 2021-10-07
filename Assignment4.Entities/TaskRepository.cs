using Assignment4.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

// TODO: Set Created/StateUpdated to current time in UTC (Business rule 2.4)
// TODO: Updating the State of a task will change the StateUpdated to current time in UTC (Business rule 2.6)
namespace Assignment4.Entities
{
    public class TaskRepository : ITaskRepository
    {
        private readonly SqlConnection _connection;

        public TaskRepository(SqlConnection connection)
        {
            _connection = connection;
        }

        public (Response Response, int TaskId) Create(TaskCreateDTO task) {
            var cmdText = @"INSERT Tasks (Title, AssignedToId, Description, State, Tags)
                            VALUES (@Title, @AssignedToId, @Description, @State, @Tags);
                            SELECT SCOPE_IDENTITY()";
            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@Title", task.Title);
            command.Parameters.AddWithValue("@AssignedToId", task.AssignedToId);
            command.Parameters.AddWithValue("@Description", task.Description);
            command.Parameters.AddWithValue("@State", State.New);
            command.Parameters.AddWithValue("@Tags", task.Tags);

            OpenConnection();

            var id = (int) command.ExecuteScalar();

            CloseConnection();

            return (Response.Created, id);
        }

        public IReadOnlyCollection<TaskDTO> ReadAll()
        {
            var cmdText = @"SELECT * FROM Tasks";
            using var command = new SqlCommand(cmdText, _connection);

            OpenConnection();

            using var reader = command.ExecuteReader();
            List<TaskDTO> tasks = new List<TaskDTO>();

            while (reader.Read())
            {
                tasks.Add(createTaskDTO(reader));
            }

            CloseConnection();

            return tasks;
        }

        public IReadOnlyCollection<TaskDTO> ReadAllRemoved() 
        {
            var cmdText = @"SELECT * FROM Tasks WHERE State = @State";
            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@State", State.Removed);

            OpenConnection();

            using var reader = command.ExecuteReader();
            List<TaskDTO> tasks = new List<TaskDTO>();

            while (reader.Read())
            {
                tasks.Add(createTaskDTO(reader));
            }

            CloseConnection();

            return tasks;
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByTag(string tag)
        {
            throw new NotImplementedException();
        }
        
        public IReadOnlyCollection<TaskDTO> ReadAllByUser(int userId)
        {
            var cmdText = @"SELECT * FROM Tasks WHERE Id = @ID";
            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@ID", userId);

            OpenConnection();

            using var reader = command.ExecuteReader();
            List<TaskDTO> tasks = new List<TaskDTO>();

            while (reader.Read())
            {
                tasks.Add(createTaskDTO(reader));
            }

            CloseConnection();

            return tasks;
        }
        
        public IReadOnlyCollection<TaskDTO> ReadAllByState(State state)
        {
            var cmdText = @"SELECT * FROM Tasks WHERE State = @State";
            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@State", state);

            OpenConnection();

            using var reader = command.ExecuteReader();
            List<TaskDTO> tasks = new List<TaskDTO>();

            while (reader.Read())
            {
                tasks.Add(createTaskDTO(reader));
            }

            CloseConnection();

            return tasks;
        }
        
        public TaskDetailsDTO Read(int taskId)
        {
            var cmdText = @"SELECT * FROM Tasks WHERE Id = @ID";
            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@ID", taskId);

            OpenConnection();

            using var reader = command.ExecuteReader();
            var task = reader.Read() ? new TaskDetailsDTO(
                reader.GetInt32("Id"),
                reader.GetString("Title"),
                reader.GetString("Description"),
                DateTime.Parse(reader.GetString("Created")),  // DateTime, where does this come from??
                reader.GetString("Assigned_To"),
                reader.GetString("Tags").Split(", "),
                parseState(reader.GetString("State")),
                DateTime.Parse(reader.GetString("StateUpdated")) // DateTime, where does this come from??
            ) : null;

            CloseConnection();

            // return task;
            // TaskDetailsDTO(int Id, string Title, string Description, DateTime Created, string AssignedToName, IReadOnlyCollection<string> Tags, State State, DateTime StateUpdated)
            throw new NotImplementedException();
        }

        public Response Update(TaskUpdateDTO task)
        {
            throw new NotImplementedException();
        }

        public Response Delete(int taskId)
        {
            var cmdText = @"DELETE Tasks WHERE Id = @Id AND State = 'New'";

            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@Id", taskId);

            OpenConnection();

            command.ExecuteNonQuery();

            CloseConnection();

            // return Response.Deleted;
            throw new NotImplementedException();
        }
        
        private void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        private void CloseConnection()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        private TaskDTO createTaskDTO(SqlDataReader reader)
        {
            return new TaskDTO (
                reader.GetInt32("Id"),
                reader.GetString("Title"),
                reader.GetString("Assigned_To"),
                reader.GetString("Tags").Split(", "),
                parseState(reader.GetString("State"))
            );
        }

        private State parseState(string state)
        {
            return (State) Enum.Parse(typeof(State), state, true);
        }
    }
}
