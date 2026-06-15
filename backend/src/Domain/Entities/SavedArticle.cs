using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SavedArticle
    {
        public int Id { get; set; }
        public string? Url { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public ArticleStatus Status { get; set; }

    }
    public enum ArticleStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }
}