using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SavedArticle
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}