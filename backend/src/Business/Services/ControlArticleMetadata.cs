using Business.DTOs;
using Business.Interfaces;
using Domain.Entities;
namespace Business.Services
{
    public class ControlArticleMetadata : IControlArticleMetadata
    {
        private readonly IArticleRepository _articleRepository;
        public ControlArticleMetadata(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }
        public Task<ArticleResponse> GetArticleAsync(int id)
        {
            return _articleRepository.GetArticleAsync(id);
        }

        public async Task<ArticleResponse> SaveArticleAsync(ArticleDto article)
        {
           var articleResponse =  await _articleRepository.AddArticleAsync(article);
            return articleResponse;
        }
    }
}