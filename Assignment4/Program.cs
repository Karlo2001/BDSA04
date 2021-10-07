using System;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Assignment4.Entities;
using System.Collections.Generic;

namespace Assignment4
{
    class Program
    {
        static void Main(string[] args)
        {

            var connectionString = "Server=localhost;Database=Kanban;User Id=sa;Password=MyStrongPassword;";

            var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>().UseSqlServer(connectionString);

            using var context = new KanbanContext(optionsBuilder.Options);


            /* Apparently tries to insert into "TaskId" column for some reason. Most likely because Tasks is of type List<Task>
            var tag = new Tag {
                Name = "Email@Email.com",
                Tasks = "This task, and this task"
            };
            context.Tags.Add(tag);
            context.SaveChanges();
            Console.WriteLine(tag);
            

            //Only works if tags in Task is of type string else if of type List<Tag> Invalid object name 'TagTask'
            /*var chars = from c in context.Tasks
                        where c.Id == 1
                        select c.Tags;

            foreach (var c in chars)
            {
                Console.WriteLine(c);
            }*/
        }
    }
}
