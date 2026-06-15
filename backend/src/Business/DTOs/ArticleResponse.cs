using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Business.DTOs
{
    public class ArticleResponse
    {
        public int Id { get; set; }
        public string? Url { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public ArticleStatus Status { get; set; }
        
    }
}