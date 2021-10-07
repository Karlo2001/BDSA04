using System;
using Xunit;
using Assignment4;
using Assignment4.Core;
using Assignment4.Entities;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Assignment4.Entities.Tests
{
    public class TaskRepositoryTests
    {
        [Fact]
        public void TaskRepositoryDeleteFunctionDeletesNewStatus()
        {
            var connectionString = GetConnectionString();

            var taskRepo = new TaskRepository(connectionString);

            taskRepo.Delete(1);

            Assert.Equal(6, taskRepo.ReadAll().Count);
        }

        private SqlConnection GetConnectionString()
        {
            return LoadConfiguration().GetConnectionString("ConnectionString");
        }

        static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<TaskRepositoryTests>();

            return builder.Build();
        }
    }
}
