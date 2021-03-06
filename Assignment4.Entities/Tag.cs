using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Assignment4.Entities
{
    public class Tag
    {
        public int Id {get; set;}

        [Required]
        [StringLength(50)]
        public string Name {get; set;}

        public List<Task> Tasks {get; set;}
    }
}