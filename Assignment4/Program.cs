using System;
using Microsoft.Extensions.Configuration;

namespace Assignment4
{
    class Program
    {
        static void Main(string[] args)
        {
            var Configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            var connectionString = Configuration.GetConnectionString("ConnectionString");
        }
    }
}
