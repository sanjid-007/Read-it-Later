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
        public async Task<ArticleResponse?> AddArticleAsync(ArticleDto article)
        {
            var articleEntity = new SavedArticle
            {
                Url = article.Url,
                CreatedAt = DateTime.UtcNow,
                Status = ArticleStatus.Pending
            };
            _context.Articles.Add(articleEntity);
            await _context.SaveChangesAsync();
            return new ArticleResponse
            {
                Id = articleEntity.Id,
                Url = articleEntity.Url,
                Title = articleEntity.Title,
                Content = articleEntity.Content,
                Status = articleEntity.Status
            };
        }

        public async Task<ArticleResponse?> GetArticleAsync(int id)
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
                Content = articleEntity.Content,
                Status = articleEntity.Status,
                Id = articleEntity.Id
            };

            return articleResponse;
        }

        public async Task<List<ArticleResponse>> GetPendingArticlesAsync(int batchSize)
        {
            return await _context.Articles
                .Where(a => a.Status == ArticleStatus.Pending)
                .OrderBy(a => a.CreatedAt)
                .Take(batchSize)
                .Select(a => new ArticleResponse
                {
                    Id = a.Id,
                    Url = a.Url,
                    Title = a.Title,
                    Content = a.Content,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task UpdateArticleAsync(int id, ArticleStatus status, string? title = null, string? content = null)
        {
            var articleEntity = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (articleEntity == null)
            {
                return;
            }

            articleEntity.Status = status;
            if (title != null)
            {
                articleEntity.Title = title;
            }
            if (content != null)
            {
                articleEntity.Content = content;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> ConvertProcessingToPendingOnStartupAsync()
        {
            var processingArticles = await _context.Articles
                .Where(a => a.Status == ArticleStatus.Processing)
                .ToListAsync();

            foreach (var article in processingArticles)
            {
                article.Status = ArticleStatus.Pending;
            }

            await _context.SaveChangesAsync();
            return processingArticles.Count;
        }

        public async Task<bool> TryClaimAsync(int id)
        {
            int rowsAffected = await _context.Articles
                .Where(a => a.Id == id && a.Status == ArticleStatus.Pending)   
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, ArticleStatus.Processing));

            return rowsAffected == 1;
        }
    }
}