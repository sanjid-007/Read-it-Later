using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.DTOs;
using Domain.Entities;
namespace Business.Interfaces
{
    public interface IArticleRepository
    {
        Task<ArticleResponse?> AddArticleAsync(ArticleDto article);
        Task<ArticleResponse?> GetArticleAsync(int id);
        Task<List<ArticleResponse>> GetPendingArticlesAsync();
        Task UpdateArticleAsync(int id, ArticleStatus status, string? title = null, string? content = null);
    }
}