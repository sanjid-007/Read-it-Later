using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.DTOs;
using Business.Interfaces;
using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
  public class ArticleRepository : IArticleRepository
  {
    private readonly AppDbContext _context;
    public ArticleRepository(AppDbContext context)
    {
      _context = context;
    }
    public async Task AddArticleAsync(ArticleResponse article)
        {
            var articleEntity = new SavedArticle
            {
                Url = article.Url,
                Title = article.Title,
                Content = article.Content,
                CreatedAt = DateTime.UtcNow
            };
            _context.Articles.Add(articleEntity);
            await _context.SaveChangesAsync();
        }

    public async Task<ArticleResponse> GetArticleAsync(int id)
    {
        var articleEntity = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
        if (articleEntity == null)
        {
            return null;
        }

        var articleResponse = new ArticleResponse
        {
            Url = articleEntity.Url,
            Title = articleEntity.Title,
            Content = articleEntity.Content
        };

        return articleResponse;
    }
  }
}