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
        public (Response Response, int TagId) Create(TagCreateDTO tag)
        {
            Console.WriteLine("Here");
            throw new NotImplementedException();
        }
        public IReadOnlyCollection<TagDTO> ReadAll()
        {
            throw new NotImplementedException();
        }
        public TagDTO Read(int tagId)
        {
            throw new NotImplementedException();
        }
        public Response Update(TagUpdateDTO tag)
        {
            throw new NotImplementedException();
        }
        public Response Delete(int tagId, bool force = false)
        {
            throw new NotImplementedException();
        }
    }
}