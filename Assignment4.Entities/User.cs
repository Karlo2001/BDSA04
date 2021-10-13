using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Assignment4.Entities
{
    public class User
    {
        public int Id {get; set;}
        
        [Required]
        [StringLength(100)]
        public string Name {get; set;}

        [Required]
        [StringLength(100)]
        public string Email {get; set;}

        public List<Task> Tasks {get; set;}
    }
}
