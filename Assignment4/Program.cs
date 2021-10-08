using Assignment4.Core;
using Assignment4.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Assignment4
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var configuration = LoadConfiguration();
            var connectionString = configuration.GetConnectionString("ConnectionString");

            var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>().UseSqlServer(connectionString);

            using var context = new KanbanContext(optionsBuilder.Options);*/

            /*var tag = new List<Tag>();
            var task = new Task { Title = "Cool title", State = State.Active, Tags = tag, Description = "", AssignedTo = null};
            //Apparently tries to insert into "TaskId" column for some reason. Most likely because Tasks is of type List<Task>
            var taga = new Tag {
                Name = "Email@Email.com",
                Tasks = new List<Task>()
            };
            taga.Tasks.Add(task);

            //Console.WriteLine(task);
            context.Tags.Add(taga);
            context.SaveChanges();
            Console.WriteLine(taga);*/


            //Only works if tags in Task is of type string else if of type List<Tag> Invalid object name 'TagTask'
            /*var chars = from c in context.Tasks
                        where c.Id == 1
                        select c.Tags;

            foreach (var c in chars)
            {
                Console.WriteLine(c);
            }*/
        }
        static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>();

            return builder.Build();
        }
    }
}
