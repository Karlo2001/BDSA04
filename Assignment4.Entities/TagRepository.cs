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
    public class TagRepository : ITagRepository
    {
        private readonly KanbanContext _context;

        public TagRepository(KanbanContext context)
        {
            _context = context;
        }

        // Trying to create a tag which exists already should return Conflict.
        // Trying to update a tag which is non existing should return NotFound

        public (Response Response, int TagId) Create(TagCreateDTO tag)
        {
            var getTag = (from c in _context.Tags
                          where c.Name == tag.Name
                          select c).FirstOrDefault();
            if (getTag == null)
            {
                var tasks = (from c in _context.Tasks
                             select c).ToList();
                var createTask = new List<Task>();
                foreach (var item in tasks)
                {
                    foreach (var itemTag in item.Tags)
                    {
                        if (itemTag.Name == tag.Name)
                        {
                            createTask.Add(item);
                        }
                    }
                }
                var createTag = new Tag {Name = tag.Name, Tasks = createTask};
                _context.Tags.Add(createTag);
                _context.SaveChanges();
                return (Response.Created ,createTag.Id);
            }
            else
            {
                return (Response.Conflict, getTag.Id);
            }
        }
        
        
        public IReadOnlyCollection<TagDTO> ReadAll()
        {
            var tags = (from c in _context.Tags
                        select c).ToList();
            return ConvertToTagDTOs(tags);
        }

        //If a tag is not found return null
        public TagDTO Read(int tagId)
        {
            var tag = (from c in _context.Tags
                       where c.Id == tagId
                       select c).FirstOrDefault();

            if (tag == null)
            {
                return null;
            }
            return new TagDTO (tag.Id, tag.Name);
        }

        // Trying to update a non existing tag should return NotFound
        public Response Update(TagUpdateDTO tag)
        {
            var updateTag = (from c in _context.Tags
                             where c.Id == tag.Id
                             select c).FirstOrDefault();

            if (updateTag == null)
            {
                return Response.NotFound;
            }
            
            updateTag.Name = tag.Name;

            return Response.Updated;
        }

        // Tags which are assigned to a task may only be deleted using the force.
        // Trying to delete a tag in use without the force should return Conflict.
        public Response Delete(int tagId, bool force = false)
        {
            var tag = (from c in _context.Tags
                       where c.Id == tagId
                       select c).FirstOrDefault();
            if (tag == null)
            {
                return Response.NotFound;
            }
            else if (tag.Tasks.Count > 0 && force == true || tag.Tasks.Count == 0)
            {
                _context.Tags.Remove(tag);
                _context.SaveChanges();
                return Response.Deleted;
            }
            return Response.Conflict;

        }

        private IReadOnlyCollection<TagDTO> ConvertToTagDTOs(List<Tag> tags)
        {
            var listDTO = new List<TagDTO>();
            
            foreach (var tag in tags)
            {
                listDTO.Add(new TagDTO(tag.Id, tag.Name));
            }
            return listDTO.AsReadOnly();
        }
    }
}