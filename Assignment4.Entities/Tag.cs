using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Assignment4.Entities
{
    public class Tag
    {
        [Required()]
        [Index(IsUnique=true)]
        public int Id {get; set;}
        public string Name {get; set;}
        public List<Task> Tasks {get; set;}
    }
}