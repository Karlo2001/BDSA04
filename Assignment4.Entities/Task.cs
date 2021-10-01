using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Assignment4.Entities
{
    public class Task
    {
        public int Id {get; set;}

        [Required]
        [StringLength(100)]
        public string Title {get; set;}
        public User AssignedTo {get; set;}

        [Optional]
        public string Description {get; set;}

        [Required]
        public State State {get; set;}
        public List<Tag> Tags {get; set;}
    }

    public enum State
    {
        New,
        Active,
        Resolved,
        Closed,
        Removed,
    }
}
