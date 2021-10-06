using Assignment4.Core;
using System;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Assignment4.Entities
{
    public class TaskRepository : ITaskRepository
    {
        private readonly SqlConnection _connection;

        public TaskRepository(SqlConnection connection)
        {
            _connection = connection;
        }
        
        public IReadOnlyCollection<TaskDTO> All()
        {
            return new List<TaskDTO>();
        }

        public int Create(TaskDTO task)
        {
            var cmdText = @"INSERT Task (Title, State)
                            VALUES (@Title, @State);
                            SELECT SCOPE_IDENTITY()";

            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@Title", task.Title);
            command.Parameters.AddWithValue("@State", task.State);

            OpenConnection();

            var id = command.ExecuteScalar();

            CloseConnection();

            return (int) id;
        }

        public void Delete(int taskId)
        {
            var cmdText = @"DELETE Task WHERE Id = @Id";

            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@Id", taskId);

            OpenConnection();

            command.ExecuteNonQuery();

            CloseConnection();
        }

        public TaskDetailsDTO FindById(int id)
        {
            var cmdText = @"SELECT Task WHERE Id = @Id";

            using var command = new SqlCommand(cmdText, _connection);

            command.Parameters.AddWithValue("@Id", id);

            OpenConnection();

            using var reader = command.ExecuteReader();

            var task = reader.Read()
                ? new TaskDetailsDTO
                {
                    Id = reader.GetInt32("Id"),
                    Title = reader.GetString("Title"),
                    Description = reader.GetString("Description"),
                    AssignedToId = reader.GetInt32("AssignedToId"),
                    AssignedToName = reader.GetString("AssignedToName"),
                    AssignedToEmail = reader.GetString("AssignedToEmail"),
                    State = (State) Enum.Parse(typeof(State), reader.GetString("State"), true),
                    Tags = GetTags(reader)
                }
                : null;

            CloseConnection();

            return task;
        }

        public void Update(TaskDTO task)
        {

        }

        public void Dispose()
        {

        }

        public IEnumerable<string> GetTags(SqlDataReader reader)
        {
            var list = new List<string>();

            while (reader.Read()) {
                list.Add(reader.GetString("Tags"));
            }

            return list;
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
    }
}
