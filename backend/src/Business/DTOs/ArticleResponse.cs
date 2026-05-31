using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.DTOs
{
    public class ArticleResponse
    {
        public required string Url { get; set; }
        public required string Title { get; set; }
        public string? Content { get; set; }
        
    }
}